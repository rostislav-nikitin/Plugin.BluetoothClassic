namespace BluetoothClassic.Base
{
    using System;
    using System.Threading.Tasks;

    public delegate void Recived (object sender, RecivedEventArgs recivedEventArgs);

    public interface IBluetoothConnection : IDisposable
    {
        Task ConnectAsync();

        Task SendAsync(Memory<byte> buffer);

        event Recived OnRecived;
    }
}