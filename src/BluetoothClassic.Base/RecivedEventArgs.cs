namespace BluetoothClassic.Base
{
    using System;

    public class RecivedEventArgs
    {
        public RecivedEventArgs(Memory<byte> buffer)
        {
            Buffer = buffer;
        }

        public Memory<byte> Buffer { get; }
    }
}