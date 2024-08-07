using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using ImGuiNET;
using OtterGui.Log;
using OtterGui.Text;

namespace OtterGui.Classes;

public class Notification : MessageService.IMessage
{
    public NotificationType NotificationType { get; }

    public string NotificationMessage { get; }

    public uint NotificationDuration { get; init; }

    private readonly string     _content2;
    private readonly Exception? _ex;

    public Notification(string content, NotificationType type, uint duration = 5000)
    {
        NotificationType     = type;
        NotificationMessage  = content;
        _content2            = string.Empty;
        _ex                  = null;
        NotificationDuration = duration;
    }

    public Notification(Exception ex, string content1, string content2, NotificationType type, uint duration = 5000)
    {
        NotificationType     = type;
        NotificationMessage  = content1;
        _content2            = content2;
        _ex                  = ex;
        NotificationDuration = duration;
    }

    public string LogMessage
        => _ex != null ? $"[{NotificationType}] {_content2}:\n{_ex}" : $"[{NotificationType}] {NotificationMessage}";

    public SeString ChatMessage
        => SeString.Empty;

    public string PrintMessage
        => NotificationMessage;

    public string PrintTooltip
        => _ex?.ToString() ?? string.Empty;
}

public class MessageService(Logger log, IUiBuilder builder, IChatGui chat, INotificationManager notificationManager)
    : IReadOnlyDictionary<DateTime, MessageService.IMessage>
{
    public interface IMessage
    {
        public NotificationType NotificationType
            => NotificationType.None;

        public string Message
            => string.Empty;

        public string NotificationMessage
            => Message;

        public string NotificationTitle
            => NotificationType.ToString();

        public uint NotificationDuration
            => 3000;

        public string LogMessage
            => Message;

        public SeString ChatMessage
            => Message;


        public Logger.LogLevel LogLevel
            => NotificationType switch
            {
                NotificationType.None    => Logger.LogLevel.Verbose,
                NotificationType.Success => Logger.LogLevel.Verbose,
                NotificationType.Warning => Logger.LogLevel.Information,
                NotificationType.Error   => Logger.LogLevel.Error,
                NotificationType.Info    => Logger.LogLevel.Debug,
                _                        => Logger.LogLevel.Verbose,
            };

        public string PrintMessage
            => Message;

        public string PrintTooltip
            => Message;
    }

    public readonly Logger               Log                 = log;
    public readonly IUiBuilder           Builder             = builder;
    public readonly INotificationManager NotificationManager = notificationManager;
    public readonly IChatGui             Chat                = chat;

    private readonly SortedDictionary<DateTime, IMessage> _messages   = [];
    private          DateTime                             _deleteTime = DateTime.MinValue;

    private readonly int      _lastTaggedMessageCLeanCycle = 128;
    private readonly TimeSpan _lastTaggedMessageMaxAge     = TimeSpan.FromMinutes(5);
    private          int      _taggedMessageCleanCounter;

    private readonly ConcurrentDictionary<string, (DateTime LastMessage, IMessage Message)> _taggedMessages = [];

    /// <summary> Print a message with a tag only if it has not been sent within <seealso cref="_lastTaggedMessageMaxAge"/>. </summary>
    /// <param name="tag"> The tag to compare messages by. </param>
    /// <param name="message"> The message. </param>
    public void AddTaggedMessage(string tag, IMessage message)
    {
        CleanTaggedMessages(true);

        // Don't warn twice for the same tag.
        if (_taggedMessages.TryGetValue(tag, out _))
            return;

        var time = AddMessage(message);
        _taggedMessages[tag] = (time, message);
    }

    public DateTime AddMessage(IMessage message, bool doPrint = true, bool doNotify = true, bool doLog = true, bool doChat = false)
    {
        var time         = DateTime.UtcNow;
        var printMessage = message.PrintMessage;
        if (doPrint && printMessage.Length > 0)
            lock (_messages)
            {
                while (!_messages.TryAdd(time, message))
                    time = time.AddTicks(1);
            }

        var logMessage = message.LogMessage;
        if (doLog && logMessage.Length > 0)
            Log.Message(message.LogLevel, message.LogMessage);

        var notificationMessage = message.NotificationMessage;
        if (doNotify && notificationMessage.Length > 0)
            NotificationManager.AddNotification(new Dalamud.Interface.ImGuiNotification.Notification()
            {
                Content         = message.NotificationMessage,
                Title           = message.NotificationTitle,
                Type            = message.NotificationType,
                Minimized       = false,
                InitialDuration = TimeSpan.FromMilliseconds(message.NotificationDuration),
            });

        var chatMessage = message.ChatMessage;
        if (doChat && chatMessage.Payloads.Count > 0)
            Chat.Print(chatMessage);
        return time;
    }

    /// <summary> Cleans up all tagged messages that happened long enough ago. </summary>
    /// <param name="force"> If this is false, it only cleans up sporadically. </param>
    public void CleanTaggedMessages(bool force)
    {
        if (!force && ++_taggedMessageCleanCounter >= _lastTaggedMessageCLeanCycle)
        {
            _taggedMessageCleanCounter = 0;
            return;
        }

        var expiredDate = DateTime.UtcNow - _lastTaggedMessageMaxAge;
        foreach (var (key, value) in _taggedMessages)
        {
            if (value.Item1 <= expiredDate && _taggedMessages.TryRemove(key, out var pair))
                _messages.Remove(pair.LastMessage);
        }
    }

    public void Draw()
    {
        var buttonSize = new Vector2(ImGui.GetFrameHeight());
        _deleteTime = DateTime.MinValue;

        using var table = ImUtf8.Table("errors"u8, 5, ImGuiTableFlags.RowBg);
        ImUtf8.TableSetupColumn("##del"u8,   ImGuiTableColumnFlags.WidthFixed, buttonSize.X);
        ImUtf8.TableSetupColumn("Time"u8,    ImGuiTableColumnFlags.WidthFixed, ImUtf8.CalcTextSize("00:00:00.0000"u8).X);
        ImUtf8.TableSetupColumn("##icon"u8,  ImGuiTableColumnFlags.WidthFixed, buttonSize.X);
        ImUtf8.TableSetupColumn("##multi"u8, ImGuiTableColumnFlags.WidthFixed, buttonSize.X);
        ImUtf8.TableSetupColumn("Message"u8, ImGuiTableColumnFlags.WidthStretch);

        ImGui.TableHeadersRow();
        ImGui.TableNextRow();


        lock (_messages)
        {
            var height    = ImGui.GetFrameHeightWithSpacing();
            var skips     = ImGuiClip.GetNecessarySkips(height);
            var remainder = ImGuiClip.ClippedDraw(_messages.WithIndex(), skips, PrintMessage, _messages.Count);
            ImGuiClip.DrawEndDummy(remainder, height);

            if (_deleteTime != DateTime.MinValue)
            {
                _messages.Remove(_deleteTime);
                _deleteTime = DateTime.MinValue;
            }
        }
    }

    private void PrintMessage(ValueTuple<KeyValuePair<DateTime, IMessage>, int> message)
    {
        using var id = ImRaii.PushId(message.Item2);
        ImGui.TableNextColumn();
        if (ImUtf8.IconButton(FontAwesomeIcon.Trash, "Remove this from the list."u8))
            _deleteTime = message.Item1.Key;

        ImGui.TableNextColumn();
        ImUtf8.TextFrameAligned($"{message.Item1.Key.ToLocalTime():HH:mm:ss.fff}");

        ImGui.TableNextColumn();
        using (ImRaii.PushFont(UiBuilder.IconFont))
        {
            var (icon, color) = message.Item1.Value.NotificationType switch
            {
                NotificationType.None    => (FontAwesomeIcon.None, 0u),
                NotificationType.Success => (FontAwesomeIcon.CheckCircle, 0xFF40FF40),
                NotificationType.Warning => (FontAwesomeIcon.ExclamationCircle, 0xFF40FFFF),
                NotificationType.Error   => (FontAwesomeIcon.TimesCircle, 0xFF4040FF),
                NotificationType.Info    => (FontAwesomeIcon.QuestionCircle, 0xFFFF4040),
                _                        => (FontAwesomeIcon.None, 0u),
            };
            using var c = ImRaii.PushColor(ImGuiCol.Text, color);
            ImGui.AlignTextToFramePadding();
            ImUtf8.Icon(icon);
        }

        var text      = message.Item1.Value.PrintMessage;
        var firstLine = text;
        var newLine   = text.IndexOf('\n');
        var tooltip   = message.Item1.Value.PrintTooltip;

        if (newLine >= 0)
        {
            firstLine = text[..newLine];
            tooltip   = $"{text[(newLine + 1)..]}\n\n{tooltip}";
        }

        ImGui.TableNextColumn();
        if (tooltip.Length > 0)
        {
            ImGui.AlignTextToFramePadding();
            ImGuiComponents.HelpMarker(text);
        }

        ImGui.TableNextColumn();
        ImUtf8.TextFrameAligned(firstLine);
        ImUtf8.HoverTooltip(tooltip);
    }

    public IEnumerator<KeyValuePair<DateTime, IMessage>> GetEnumerator()
        => _messages.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => GetEnumerator();

    public int Count
        => _messages.Count;

    public bool ContainsKey(DateTime key)
        => _messages.ContainsKey(key);

    public bool TryGetValue(DateTime key, [NotNullWhen(true)] out IMessage? value)
        => _messages.TryGetValue(key, out value);

    public IMessage this[DateTime key]
        => _messages[key];

    public IEnumerable<DateTime> Keys
        => _messages.Keys;

    public IEnumerable<IMessage> Values
        => _messages.Values;
}
