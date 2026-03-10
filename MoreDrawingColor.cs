using MegaCrit.Sts2.Core.Modding;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;

namespace MoreDrawingColor;

[ModInitializer(nameof(Init))]
public class MoreDrawingColor
{
    private static Harmony? _harmony;

    public static void Init()
    {
        _harmony = new Harmony("moe.gensoukyo.more_drawing_color");
        _harmony.PatchAll();
        Log.Debug("Mod initialized.");
    }
}