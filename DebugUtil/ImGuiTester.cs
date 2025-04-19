using System.Reflection.Metadata;
using ImGuiNET;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using OtterGui.Raii;
using OtterGui.Text;

namespace OtterGui.DebugUtil;

/// <summary> A test panel to enter ImGui code during runtime and have it evaluated. </summary>
/// <remarks> Currently an issue due to being unable to add assemblies as required. </remarks>
public class ImGuiTester
{
    private string _code = string.Empty;

    private readonly HashSet<Assembly> _assemblies = [typeof(ImGui).Assembly, typeof(ImGuiTester).Assembly];
    private readonly HashSet<string>   _imports    = ["ImGuiNET", "OtterGui.Raii", "OtterGui.Text"];

    private readonly Lock                     _lock = new();
    private          Script?                  _script;
    private          Task?                    _compilationTask;
    private          CancellationTokenSource? _cancelSource;

    private readonly Stopwatch _watch = new();

    public unsafe void Draw()
    {
        var width = (ImGui.GetContentRegionAvail().X - ImGui.GetStyle().ItemSpacing.X) / 2;
        using (var child = ImUtf8.Child("code"u8, new Vector2(width, ImGui.GetContentRegionAvail().Y), true))
        {
            if (child)
            {
                if (ImUtf8.InputMultiLine("##input"u8, ref _code, ImGui.GetContentRegionAvail()))
                {
                    _watch.Restart();
                    _cancelSource?.Cancel();
                }
                else if (_watch.IsRunning)
                {
                    if (_watch.ElapsedMilliseconds > 1000)
                    {
                        _watch.Reset();
                        _cancelSource?.Cancel();
                        _cancelSource = new CancellationTokenSource();
                        var code  = new string(_code);
                        var token = _cancelSource.Token;
                        _compilationTask = Task.Run(() =>
                        {
                            using var loader  = new InteractiveAssemblyLoader();
                            var       options = ScriptOptions.Default.WithAllowUnsafe(true);
                            lock (_assemblies)
                            {
                                foreach (var assembly in _assemblies)
                                {
                                    if (assembly is { IsDynamic: false, Location.Length: > 0 })
                                    {
                                        loader.RegisterDependency(assembly);
                                        options = options.AddReferences(assembly);
                                    }
                                    else if (assembly.TryGetRawMetadata(out var blob, out var length))
                                    {
                                        var moduleMetaData   = ModuleMetadata.CreateFromMetadata((nint)blob, length);
                                        var assemblyMetaData = AssemblyMetadata.Create(moduleMetaData);
                                        var r                = assemblyMetaData.GetReference();
                                        loader.RegisterDependency(assembly);
                                        options = options.AddReferences(r);
                                    }
                                }
                            }


                            lock (_imports)
                            {
                                options = options.AddImports(_imports.ToArray());
                            }

                            var script = CSharpScript.Create(code, options, null, loader);
                            script.Compile(token);
                            lock (_lock)
                            {
                                _script = script;
                            }
                        }, token);
                    }
                    else
                    {
                        var text = $"{1000 - _watch.ElapsedMilliseconds}";
                        var size = ImUtf8.CalcTextSize(text);
                        ImGui.SetCursorPos(ImGui.GetWindowContentRegionMax() - size);
                        ImUtf8.Text(text);
                    }
                }
            }
        }

        ImGui.SameLine();
        using (var child = ImUtf8.Child("exec"u8, new Vector2(width, ImGui.GetContentRegionAvail().Y), true))
        {
            if (child)
                lock (_lock)
                {
                    if (_script is not null)
                        try
                        {
                            if (!_script.RunAsync().Wait(1000))
                                throw new Exception("Running Code took more than a second.");
                        }
                        catch (Exception ex)
                        {
                            using (ImRaii.PushColor(ImGuiCol.Text, 0xFF0000FF))
                            {
                                ImUtf8.TextWrapped(ex.ToString());
                            }
                        }
                    else
                        switch (_compilationTask)
                        {
                            case null: ImUtf8.Text("No script available."); break;
                            case { IsFaulted: true }:
                                using (ImRaii.PushColor(ImGuiCol.Text, 0xFF0000FF))
                                {
                                    ImUtf8.TextWrapped(_compilationTask.Exception.ToString());
                                }

                                break;
                            case { IsCompleted: false }:
                                var radius    = 100 * ImUtf8.GlobalScale;
                                var thickness = (int)(20 * ImUtf8.GlobalScale);
                                ImUtf8.Spinner("waiting"u8, radius, thickness, ImGui.GetColorU32(ImGuiCol.Text));

                                break;
                        }
                }
        }
    }

    public ImGuiTester AddAssemblies(params IEnumerable<Type> types)
        => AddAssemblies(types.Select(t => t.Assembly));

    public ImGuiTester AddAssemblies(params IEnumerable<Assembly> assemblies)
    {
        lock (_assemblies)
        {
            foreach (var a in assemblies)
                _assemblies.Add(a);
        }

        return this;
    }

    public ImGuiTester RemoveAssemblies(params IEnumerable<Type> types)
        => RemoveAssemblies(types.Select(t => t.Assembly));

    public ImGuiTester RemoveAssemblies(params IEnumerable<Assembly> assemblies)
    {
        lock (_assemblies)
        {
            foreach (var a in assemblies)
                _assemblies.Remove(a);
        }

        return this;
    }

    public ImGuiTester AddImports(params IEnumerable<string> imports)
    {
        lock (_imports)
        {
            foreach (var i in imports)
                _imports.Add(i);
        }

        return this;
    }

    public ImGuiTester RemoveImports(params IEnumerable<string> imports)
    {
        lock (_imports)
        {
            foreach (var i in imports)
                _imports.Remove(i);
        }

        return this;
    }
}
