using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MoreDrawingColor.Patches;

[HarmonyPatch(typeof(NMapDrawings), "CreateLineForPlayer")]
[HarmonyPatch([typeof(Player), typeof(bool)])]
public class DrawingColorPatch
{
    static void Postfix(NMapDrawings __instance, Line2D __result, Player player, bool isErasing)
    {
        if (isErasing) return;

        var netService = Traverse.Create(__instance)
            .Field("_netService")
            .GetValue<INetGameService>();

        // 本地玩家：使用选中的颜色
        if (netService != null && player.NetId == netService.NetId)
        {
            __result.DefaultColor = ColorConfig.SelectedColor;
            PlayerColorManager.SetPlayerColor(player.NetId, ColorConfig.SelectedColor);
            Log.Debug($"[DrawingColorPatch] Set local player {player.NetId} color: {ColorConfig.SelectedColor}");
        }
        // 远程玩家：从 PlayerColorManager 获取该玩家的颜色
        else
        {
            var playerColor = PlayerColorManager.GetPlayerColor(player.NetId);
            if (playerColor.HasValue)
            {
                __result.DefaultColor = playerColor.Value;
                Log.Debug($"[DrawingColorPatch] Set remote player {player.NetId} color: {playerColor.Value}");
            }
        }
    }
}