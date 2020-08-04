namespace BluetoothClassic.Droid
{
    using BluetoothClassic.Base;
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BluetoothSendException : BluetoothTransmitterException
    {
        public Memory<byte> Buffer { get; }

        public BluetoothSendException()
        {
        }

        public BluetoothSendException(string message) : this(message, null, null)
        {
        }

        public BluetoothSendException(string message, Exception innerException) : this(message, innerException, null)
        {
        }

        public BluetoothSendException(string message, Exception innerException, Memory<byte> buffer) : base(message, innerException)
        {
            this.Buffer = buffer;
        }

        protected BluetoothSendException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}