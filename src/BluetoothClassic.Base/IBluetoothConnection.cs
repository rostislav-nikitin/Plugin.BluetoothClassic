namespace BluetoothClassic.Base
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate void Recived (object sender, RecivedEventArgs recivedEventArgs);

    public delegate void Error(object sender, ThreadExceptionEventArgs recivedEventArgs);

    public interface IBluetoothConnection : IDisposable
    {
        Task ConnectAsync();

        Task SendAsync(Memory<byte> buffer);

        event Recived OnRecived;

        event Error OnError;
    }
}