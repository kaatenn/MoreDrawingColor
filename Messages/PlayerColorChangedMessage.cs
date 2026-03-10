using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;

namespace MoreDrawingColor.Messages;

/// <summary>
/// 玩家颜色变更消息
/// </summary>
public class PlayerColorChangedMessage : INetMessage
{
    public Color Color;

    // INetMessage 必需属性
    public bool ShouldBroadcast => true;  // 广播给所有玩家
    public NetTransferMode Mode => NetTransferMode.Reliable;  // 可靠传输
    public LogLevel LogLevel => LogLevel.Debug;  // 日志级别

    public void Serialize(PacketWriter writer)
    {
        writer.WriteFloat(Color.R);
        writer.WriteFloat(Color.G);
        writer.WriteFloat(Color.B);
        writer.WriteFloat(Color.A);
    }

    public void Deserialize(PacketReader reader)
    {
        Color = new Color(
            reader.ReadFloat(),
            reader.ReadFloat(),
            reader.ReadFloat(),
            reader.ReadFloat()
        );
    }
}
