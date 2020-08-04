namespace BluetoothClassic.Base
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BluetoothReciveException : BluetoothTransmitterException
    {
        public BluetoothReciveException()
        {
        }

        public BluetoothReciveException(string message) : base(message)
        {
        }

        public BluetoothReciveException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BluetoothReciveException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}