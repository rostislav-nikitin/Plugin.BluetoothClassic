using System.Runtime.Serialization;

namespace Plugin.BluetoothClassic.Abstractions
{
    public enum ConnectionState
    {
        [EnumMember(Value = "Created...")]
        Created,
        [EnumMember(Value ="Initializing...")]
        Initializing,
        [EnumMember(Value = "Connecting...")]
        Connecting,
        [EnumMember(Value = "Connected")]
        Connected,
        [EnumMember(Value = "ErrorHappend")]
        ErrorHappend,
        [EnumMember(Value = "Reconnecting...")]
        Reconnecting,
        [EnumMember(Value = "Disconnecting...")]
        Disconnecting,
        [EnumMember(Value = "Disconnected")]
        Disconnected,
        [EnumMember(Value = "Disposing...")]
        Disposing,
        [EnumMember(Value = "Disposed")]
        Disposed
    }
}