namespace BluetoothClassic.Base
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BluetoothTransmitterException : Exception
    {
        public BluetoothTransmitterException()
        {
        }

        public BluetoothTransmitterException(string message) : base(message)
        {
        }

        public BluetoothTransmitterException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BluetoothTransmitterException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}