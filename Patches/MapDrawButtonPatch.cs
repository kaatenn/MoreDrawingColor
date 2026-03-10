using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Screens.Map;
using MegaCrit.Sts2.Core.Logging;

namespace MoreDrawingColor.Patches;

[HarmonyPatch(typeof(NMapDrawButton), "_Ready")]
public class MapDrawButtonPatch
{

    private const string IconPath = "res://icons/color_picker.svg";

    static void Postfix(NMapDrawButton __instance)
    {
        Log.Debug("[MapDrawButtonPatch] _Ready Postfix called!");

        var toolHolder = Traverse.Create(__instance)
            .Field("_drawingToolHolder")
            .GetValue<Control>();

        if (toolHolder == null)
        {
            Log.Error("[MapDrawButtonPatch] _drawingToolHolder is null!");
            return;
        }

        Log.Debug($"[MapDrawButtonPatch] toolHolder type: {toolHolder.GetType().Name}, ChildCount before: {toolHolder.GetChildCount()}");

        // 创建调色盘按钮
        var pickerButton = CreateColorPickerButton();
        toolHolder.CallDeferred(Node.MethodName.AddChild, pickerButton);

        // 创建预设颜色网格（3x3）
        var presetColorContainer = CreatePresetColorGrid();
        toolHolder.CallDeferred(Node.MethodName.AddChild, presetColorContainer);

        Log.Debug("[MapDrawButtonPatch] Added color picker button and preset color grid");
    }

    /// <summary>
    /// 创建调色盘按钮
    /// </summary>
    private static Control CreateColorPickerButton()
    {
        const int btnSize = 60;
        const int iconSize = 40;  // 图标大小

        var button = new Button
        {
            CustomMinimumSize = new Vector2(btnSize, btnSize),
            TooltipText = "打开调色盘"
        };

        var iconTexture = GD.Load<Texture2D>(IconPath);
        if (iconTexture != null)
        {
            // 创建容器来居中图标
            var container = new CenterContainer
            {
                CustomMinimumSize = new Vector2(btnSize, btnSize)
            };

            var iconRect = new TextureRect
            {
                Texture = iconTexture,
                CustomMinimumSize = new Vector2(iconSize, iconSize),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.Scale
            };

            container.AddChild(iconRect);
            button.Flat = true;
            button.AddChild(container);
        }
        else
        {
            button.Text = "🎨";
            Log.Warn("[MapDrawButtonPatch] Failed to load color_picker_icon.svg, using emoji fallback");
        }

        button.Pressed += () =>
        {
            Log.Debug("[MapDrawButtonPatch] Color picker button pressed");
            ShowColorPicker(button);
        };

        return button;
    }

    /// <summary>
    /// 显示调色盘弹窗
    /// </summary>
    private static void ShowColorPicker(Control owner)
    {
        var popup = new PopupPanel
        {
            Size = new Vector2I(250, 300)  // 缩小尺寸
        };

        var colorPicker = new ColorPicker
        {
            Color = ColorConfig.SelectedColor
        };

        colorPicker.ColorChanged += (color) =>
        {
            ColorConfig.SelectedColor = color;
            Log.Debug($"[MapDrawButtonPatch] Color picker changed: {color}");
        };

        popup.CloseRequested += () =>
        {
            popup.QueueFree();
            Log.Debug($"[MapDrawButtonPatch] Color picker closed, final color: {ColorConfig.SelectedColor}");
        };

        popup.AddChild(colorPicker);

        var globalPos = (Vector2I)owner.GlobalPosition;
        var screenSize = owner.GetTree().Root.Size;

        var rightPos = globalPos + new Vector2I((int)owner.Size.X, 0);

        if (rightPos.X + popup.Size.X > screenSize.X)
        {
            var leftPos = globalPos + new Vector2I(-popup.Size.X, 0);
            if (leftPos.X < 0)
            {
                popup.Position = globalPos + new Vector2I(0, -popup.Size.Y);
            }
            else
            {
                popup.Position = leftPos;
            }
        }
        else
        {
            popup.Position = rightPos;
        }

        owner.GetTree().Root.AddChild(popup);
        popup.Popup();

        Log.Debug("[MapDrawButtonPatch] Color picker shown");
    }

    /// <summary>
    /// 创建预设颜色网格（3x3）
    /// 如果认为不美观，可以直接删除此函数的调用
    /// </summary>
    private static Control CreatePresetColorGrid()
    {
        const int btnSize = 16;

        // 预设颜色列表 - 3x3 = 9 个颜色
        Color[] colors = [
            Colors.Black,
            Colors.Red,
            Colors.Blue,
            Colors.Green,
            Colors.Yellow,
            Colors.Magenta,
            Colors.Cyan,
            Colors.White,
            Colors.Orange
        ];

        var colorContainer = new GridContainer
        {
            Columns = 3
        };

        foreach (var color in colors)
        {
            var panel = new PanelContainer
            {
                CustomMinimumSize = new Vector2(btnSize + 2, btnSize + 2)
            };

            var styleBox = new StyleBoxFlat
            {
                BgColor = Colors.Transparent,
                BorderWidthLeft = 1,
                BorderWidthTop = 1,
                BorderWidthRight = 1,
                BorderWidthBottom = 1,
                BorderColor = new Color(0.5f, 0.5f, 0.5f, 0.5f)
            };
            panel.AddThemeStyleboxOverride("panel", styleBox);

            var btn = new ColorRect
            {
                Color = color,
                CustomMinimumSize = new Vector2(btnSize, btnSize),
                Position = new Vector2(1, 1)
            };

            // 闭包捕获当前颜色
            var capturedColor = color;
            btn.GuiInput += (inputEvent) =>
            {
                if (inputEvent is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                {
                    ColorConfig.SelectedColor = capturedColor;
                    Log.Debug($"[MapDrawButtonPatch] Selected preset color: {capturedColor}");
                    inputEvent.Call("AcceptEvent");
                }
            };

            panel.AddChild(btn);
            colorContainer.AddChild(panel);
        }

        return colorContainer;
    }
}