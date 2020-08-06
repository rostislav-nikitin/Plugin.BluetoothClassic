namespace BluetoothClassic.Droid
{
    using BluetoothClassic.Base;
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BluetoothTransmitException : BluetoothDataTransferUnitException
    {
        public Memory<byte> Buffer { get; }

        public BluetoothTransmitException()
        {
        }

        public BluetoothTransmitException(string message) : this(message, null, null)
        {
        }

        public BluetoothTransmitException(string message, Exception innerException) : this(message, innerException, null)
        {
        }

        public BluetoothTransmitException(string message, Exception innerException, Memory<byte> buffer) : base(message, innerException)
        {
            this.Buffer = buffer;
        }

        protected BluetoothTransmitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}