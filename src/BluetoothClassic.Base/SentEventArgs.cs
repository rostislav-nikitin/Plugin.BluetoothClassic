namespace BluetoothClassic.Base
{
    using System;

    public class SentEventArgs : DataExchangeEventArgsBase
    {
        public SentEventArgs(Memory<byte> buffer) : base(buffer)
        {
        }
    }
}
