using Dalamud.Interface.ImGuiNotification;
using ImGuiNET;
using OtterGui.Classes;
using OtterGui.Raii;
using OtterGui.Text;
using OtterGui.Widgets;
using OtterGuiInternal.Enums;

namespace OtterGui.Custom;

public static class CustomGui
{
    public const uint DiscordColor     = 0xFFDA8972;
    public const uint ReniColorButton  = 0xFFCC648D;
    public const uint ReniColorHovered = 0xFFB070B0;
    public const uint ReniColorActive  = 0xFF9070E0;

    /// <summary> Draw a button to open the official discord server. </summary>
    public static void DrawDiscordButton(MessageService message, float width)
    {
        const string address = @"https://discord.gg/kVva7DHV4r";
        using var    color   = ImRaii.PushColor(ImGuiCol.Button, DiscordColor);

        DrawLinkButton(message, "Join Discord for Support", address, width, $"Open {address}");
    }

    /// <summary> Draw the button that opens the ReniGuide. </summary>
    public static void DrawGuideButton(MessageService message, float width)
    {
        const string address = @"https://reniguide.info/";
        using var color = ImRaii.PushColor(ImGuiCol.Button, ReniColorButton)
            .Push(ImGuiCol.ButtonHovered, ReniColorHovered)
            .Push(ImGuiCol.ButtonActive,  ReniColorActive);

        DrawLinkButton(message, "Beginner's Guides", address, width,
            $"Open {address}\nImage and text based guides for most functionality of Penumbra made by Serenity.\n"
          + "Not directly affiliated and potentially, but not usually out of date.");
    }

    /// <summary> Draw a button that opens an address in the browser. </summary>
    public static void DrawLinkButton(MessageService message, string text, string address, float width, string? tooltip = null)
    {
        if (ImGui.Button(text, new Vector2(width, 0)))
            try
            {
                var process = new ProcessStartInfo(address)
                {
                    UseShellExecute = true,
                };
                Process.Start(process);
            }
            catch
            {
                message.NotificationMessage($"Could not open {text} at {address} in external browser", NotificationType.Error);
            }

        if (tooltip != null)
            ImGuiUtil.HoverTooltip(tooltip);
    }

    public static void DrawKofiPatreonButton(MessageService message, Vector2 size)
    {
        const string kofiAddress    = "https://ko-fi.com/ottermandias";
        const string patreonAddress = "https://www.patreon.com/Ottermandias";
        var          half           = size with { X = size.X / 2 };

        switch (ToggleButton.SplitButton((ImGuiId)5, new ToggleButton.SplitButtonData()
                {
                    Label      = "Ko-Fi"u8,
                    Active     = 0xFF5B5EFFu,
                    Background = 0xFFFFC313u,
                    Hovered    = ImGui.GetColorU32(ImGuiCol.ButtonHovered),
                    Tooltip =
                        "Open Ottermandias' Ko-Fi at https://ko-fi.com/ottermandias in your browser.\n\nAny donations made are entirely voluntary and will not yield any preferential treatment or benefits beyond making Otter happy."u8,
                }, new ToggleButton.SplitButtonData()
                {
                    Label      = "Patreon"u8,
                    Active     = 0xFF492C00u,
                    Hovered    = ImGui.GetColorU32(ImGuiCol.ButtonHovered),
                    Background = 0xFF5467F7u,
                    Tooltip =
                        "Open Ottermandias' Patreon at https://www.patreon.com/Ottermandias in your browser.\n\nAny donations made are entirely voluntary and will not yield any preferential treatment or benefits beyond making Otter happy."u8,
                }, size, MixColors(0xFFFFC313u, 0xFF5467F7u)))
        {
            case 1:
                try
                {
                    var process = new ProcessStartInfo("https://ko-fi.com/ottermandias")
                    {
                        UseShellExecute = true,
                    };
                    Process.Start(process);
                }
                catch
                {
                    message.NotificationMessage($"Could not open Ko-Fi link at {kofiAddress} in external browser", NotificationType.Error);
                }

                break;
            case 2:
                try
                {
                    var process = new ProcessStartInfo(patreonAddress)
                    {
                        UseShellExecute = true,
                    };
                    Process.Start(process);
                }
                catch
                {
                    message.NotificationMessage($"Could not open Patreon link at {patreonAddress} in external browser", NotificationType.Error);
                }

                break;
        }


        uint MixColors(uint x, uint y)
        {
            var r = ((x & 0xFF) + (y & 0xFF)) / 2;
            var g = (((x >> 8) & 0xFF) + ((y >> 8) & 0xFF)) / 2;
            var b = (((x >> 16) & 0xFF) + ((y >> 16) & 0xFF)) / 2;
            return r | (g << 8) | (b << 16) | 0xFF000000u;
        }
    }
}
