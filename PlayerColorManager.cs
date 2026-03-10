using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;

namespace MoreDrawingColor;

public static class PlayerColorManager
{
    // 存储每个玩家的颜色选择
    private static readonly Dictionary<ulong, Color> _playerColors = new();

    public static void SetPlayerColor(ulong playerId, Color color)
    {
        _playerColors[playerId] = color;
        Log.Debug($"[PlayerColorManager] Set player {playerId} color: {color}");
    }

    public static Color? GetPlayerColor(ulong playerId)
    {
        return _playerColors.TryGetValue(playerId, out var color) ? color : null;
    }

    public static Color GetOrCreatePlayerColor(ulong playerId)
    {
        if (!_playerColors.ContainsKey(playerId))
        {
            // 默认黑色
            _playerColors[playerId] = Colors.Black;
        }
        return _playerColors[playerId];
    }
}
