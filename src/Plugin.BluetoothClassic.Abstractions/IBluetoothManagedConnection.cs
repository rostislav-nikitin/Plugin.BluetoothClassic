namespace Plugin.BluetoothClassic.Abstractions
{
    using System;
    using System.Threading;

    /// <summary>
    /// The delegate that can be used on connection state changed.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="StateChangedEventArgs"/> instance.</param>
    public delegate void StateChanged(object sender, StateChangedEventArgs stateChangedEventArgs);
    /// <summary>
    /// The delegate that can be used on data trnasmitted.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="TransmittedEventArgs"/> instance.</param>
    public delegate void Transmitted(object sender, TransmittedEventArgs transmittedEventArgs);
    /// <summary>
    /// The delegate that can be used on data recived.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="RecivedEventArgs"/> instance.</param>
    public delegate void Recived (object sender, RecivedEventArgs recivedEventArgs);
    /// <summary>
    /// The delegate that can be used on error occured.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="ThreadExceptionEventArgs"/> instance.</param>
    public delegate void Error(object sender, ThreadExceptionEventArgs threadExceptionEventArgs);


    /// <summary>
    /// The interface that represents a managed connection between a current bluetooth adapter and the remote bluetooth device.
    /// Types that implements this interface have to manage connection state internally by the connection manager.
    /// </summary>
    public interface IBluetoothManagedConnection : IDisposable
    {
        /// <summary>
        /// The property that represents a current connection state.
        /// </summary>
        ConnectionState ConnectionState { get; }

        /// <summary>
        /// The methods that begins connection process. In not creates connection immediately, notify connection manager to create it as soon as possible.
        /// Use <see cref="Recived"/> event to listen to any state chagnes and particullary for the <see cref="ConnectionState.Connected"/> state.
        /// </summary>
        void Connect();

        /// <summary>
        /// The method do not transmits data immediately but adds if to the transmit queue to be transmitted on connection will be in a <see cref="ConnectionState.Connected"/> state.
        /// </summary>
        /// <param name="buffer"></param>
        void Transmit(Memory<byte> buffer);

        /// <summary>
        /// The event that raises on connection state changed.
        /// </summary>
        event StateChanged OnStateChanged;
        /// <summary>
        /// The event that raises on data transmitted.
        /// </summary>
        event Transmitted OnTransmitted;

        /// <summary>
        /// The event that raises on data recived.
        /// </summary>
        event Recived OnRecived;

        /// <summary>
        /// The event tht raises on connection errors.
        /// </summary>
        event Error OnError;
    }
}