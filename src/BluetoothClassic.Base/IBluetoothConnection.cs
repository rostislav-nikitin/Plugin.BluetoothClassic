namespace BluetoothClassic.Base
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public delegate void StateChanged(object sender, StateChangedEventArgs stateChangedEventArgs);
    public delegate void Transmitted(object sender, TransmittedEventArgs transmittedEventArgs);
    public delegate void Recived (object sender, RecivedEventArgs recivedEventArgs);
    public delegate void Error(object sender, ThreadExceptionEventArgs threadExceptionEventArgs);

    public interface IBluetoothConnection : IDisposable
    {
        ConnectionState ConnectionState { get; }

        void Connect();

        void Transmit(Memory<byte> buffer);

        event StateChanged OnStateChanged;
        event Transmitted OnTransmitted;
        event Recived OnRecived;
        event Error OnError;
    }
}