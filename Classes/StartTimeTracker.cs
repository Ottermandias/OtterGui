using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Dalamud.Interface;
using ImGuiNET;
using OtterGui.Raii;
using System.Threading;

namespace OtterGui.Classes;

public class StartTimeTracker<T> where T : unmanaged, Enum
{
    private readonly Stopwatch[] _timers =
#if DEBUG
        Enum.GetValues<T>().Select(e => new Stopwatch()).ToArray();
#else
        Array.Empty<Monitor>();
#endif

    public struct TimingStopper : IDisposable
    {
        private readonly Stopwatch _watch;

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        internal TimingStopper(StartTimeTracker<T> manager, T type)
        {
            _watch = manager._timers[Unsafe.As<T, int>(ref type)];
            _watch.Start();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void Dispose()
            => _watch.Stop();
    }

#if DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public TimingStopper Measure(T timingType)
        => new(this, timingType);
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public IDisposable? Measure(T timingType)
        => null;
#endif

#if DEBUG
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Measure(T timingType, Action action)
    {
        using var t = Measure(timingType);
        action();
    }
#else
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Measure(T _, Action action)
        => action();
#endif


    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Start(T timingType)
        => _timers[Unsafe.As<T, int>(ref timingType)].Start();

    [Conditional("DEBUG")]
    [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
    public void Stop(T timingType)
        => _timers[Unsafe.As<T, int>(ref timingType)].Stop();

    [Conditional("DEBUG")]
    public void Draw(string label, Func<T, string> toNames)
    {
        using var id    = ImRaii.PushId(label);
        using var table = ImRaii.Table("##startTimeTable", 2, ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.RowBg);

        ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.None, 200 * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn("Time", ImGuiTableColumnFlags.None, 200 * ImGuiHelpers.GlobalScale);
        ImGui.TableHeadersRow();

        foreach (var (e, timer) in Enum.GetValues<T>().Zip(_timers))
        {
            ImGuiUtil.DrawTableColumn(toNames(e));
            var time = timer.ElapsedTicks * 1000.0 / Stopwatch.Frequency;
            ImGui.TableNextColumn();
            ImGuiUtil.RightAlign($"{time:F4}");
        }
    }
}
