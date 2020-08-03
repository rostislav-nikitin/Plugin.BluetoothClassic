namespace BluetoothClassic.Base
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BluetoothConnectionException : Exception
    {
        public BluetoothConnectionException()
        {
        }

        public BluetoothConnectionException(string message) : base(message)
        {
        }

        public BluetoothConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BluetoothConnectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}