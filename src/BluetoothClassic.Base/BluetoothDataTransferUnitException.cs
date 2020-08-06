namespace BluetoothClassic.Base
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class BluetoothDataTransferUnitException : Exception
    {
        public BluetoothDataTransferUnitException()
        {
        }

        public BluetoothDataTransferUnitException(string message) : base(message)
        {
        }

        public BluetoothDataTransferUnitException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected BluetoothDataTransferUnitException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}