using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Plugin;
using Dalamud.Utility;
using OtterGui.Log;

namespace OtterGui.Classes;

public class ChatService
{
    private readonly Logger    _log;
    private readonly UiBuilder _uiBuilder;

    public ChatService(Logger log, DalamudPluginInterface pi)
    {
        _log       = log;
        _uiBuilder = pi.UiBuilder;
    }

    public void NotificationMessage(string content, string? title = null, NotificationType type = NotificationType.None)
    {
        var logLevel = type switch
        {
            NotificationType.None    => Logger.LogLevel.Information,
            NotificationType.Success => Logger.LogLevel.Information,
            NotificationType.Warning => Logger.LogLevel.Warning,
            NotificationType.Error   => Logger.LogLevel.Error,
            NotificationType.Info    => Logger.LogLevel.Information,
            _                        => Logger.LogLevel.Debug,
        };
        _uiBuilder.AddNotification(content, title, type);
        _log.Message(logLevel, title.IsNullOrEmpty() ? content : $"[{title}] {content}");
    }
}

public static class SeStringBuilderExtensions
{
    public const ushort Green  = 504;
    public const ushort Yellow = 31;
    public const ushort Red    = 534;
    public const ushort Blue   = 517;
    public const ushort White  = 1;
    public const ushort Purple = 541;

    public static SeStringBuilder AddText(this SeStringBuilder sb, string text, int color, bool brackets = false)
        => sb.AddUiForeground((ushort)color).AddText(brackets ? $"[{text}]" : text).AddUiForegroundOff();

    public static SeStringBuilder AddGreen(this SeStringBuilder sb, string text, bool brackets = false)
        => sb.AddText(text, Green, brackets);

    public static SeStringBuilder AddYellow(this SeStringBuilder sb, string text, bool brackets = false)
        => sb.AddText(text, Yellow, brackets);

    public static SeStringBuilder AddRed(this SeStringBuilder sb, string text, bool brackets = false)
        => sb.AddText(text, Red, brackets);

    public static SeStringBuilder AddBlue(this SeStringBuilder sb, string text, bool brackets = false)
        => sb.AddText(text, Blue, brackets);

    public static SeStringBuilder AddWhite(this SeStringBuilder sb, string text, bool brackets = false)
        => sb.AddText(text, White, brackets);

    public static SeStringBuilder AddPurple(this SeStringBuilder sb, string text, bool brackets = false)
        => sb.AddText(text, Purple, brackets);

    public static SeStringBuilder AddCommand(this SeStringBuilder sb, string command, string description)
        => sb.AddText("    》 ")
            .AddBlue(command)
            .AddText($" - {description}");

    public static SeStringBuilder AddInitialPurple(this SeStringBuilder sb, string word, bool withComma = true)
        => sb.AddPurple($"[{word[0]}]")
            .AddText(withComma ? $"{word[1..]}, " : word[1..]);
}
