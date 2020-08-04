namespace BluetoothClassic.Base
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate void Sent(object sender, SentEventArgs recivedEventArgs);
    public delegate void Recived (object sender, RecivedEventArgs recivedEventArgs);

    public delegate void Error(object sender, ThreadExceptionEventArgs recivedEventArgs);

    public interface IBluetoothConnection : IDisposable
    {
        Task ConnectAsync();

        void Send(Memory<byte> buffer);

        event Sent OnSent;
        event Recived OnRecived;

        event Error OnError;
    }
}