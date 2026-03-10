using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MoreDrawingColor.Messages;

namespace MoreDrawingColor.Patches;

/// <summary>
/// Patch BeginLineLocal 来同步颜色到网络
/// </summary>
[HarmonyPatch(typeof(NMapDrawings), "BeginLineLocal")]
[HarmonyPatch([typeof(Vector2), typeof(DrawingMode?)])]
public class ColorSyncPatch
{
    static void Postfix(NMapDrawings __instance, Vector2 position, DrawingMode? overrideDrawingMode)
    {
        var netService = Traverse.Create(__instance)
            .Field("_netService")
            .GetValue<INetGameService>();

        if (netService == null) return;

        // 发送颜色同步消息
        var colorMessage = new PlayerColorChangedMessage
        {
            Color = ColorConfig.SelectedColor
        };

        netService.SendMessage(colorMessage);
        Log.Debug($"[ColorSyncPatch] Sent color sync message: {ColorConfig.SelectedColor}");
    }
}

/// <summary>
/// 在 NMapDrawings.Initialize 中注册颜色消息处理器
/// </summary>
[HarmonyPatch(typeof(NMapDrawings), "Initialize")]
public class ColorMessageHandlerPatch
{
    static void Postfix(NMapDrawings __instance, INetGameService netService)
    {
        netService.RegisterMessageHandler<PlayerColorChangedMessage>((message, senderId) =>
        {
            // 接收到颜色同步消息，更新 PlayerColorManager
            PlayerColorManager.SetPlayerColor(senderId, message.Color);
            Log.Debug($"[ColorMessageHandlerPatch] Received color sync from player {senderId}: {message.Color}");
        });

        Log.Debug("[ColorMessageHandlerPatch] Registered PlayerColorChangedMessage handler");
    }
}
