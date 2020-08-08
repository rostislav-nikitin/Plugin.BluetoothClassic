namespace Plugin.BluetoothClassic.Abstractions
{
    using System;
    using System.Threading;

    /// <summary>
    /// The delegate that can be used on a connection state changed.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="StateChangedEventArgs"/> instance.</param>
    public delegate void StateChanged(object sender, StateChangedEventArgs stateChangedEventArgs);

    /// <summary>
    /// The delegate that can be used on a data trnasmitted.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="TransmittedEventArgs"/> instance.</param>
    public delegate void Transmitted(object sender, TransmittedEventArgs transmittedEventArgs);

    /// <summary>
    /// The delegate that can be used on a data recived.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="RecivedEventArgs"/> instance.</param>
    public delegate void Recived (object sender, RecivedEventArgs recivedEventArgs);

    /// <summary>
    /// The delegate that can be used on an error occured.
    /// </summary>
    /// <param name="sender">The parameter that represents a sender.</param>
    /// <param name="stateChangedEventArgs">The parameter that represents a <see cref="ThreadExceptionEventArgs"/> instance.</param>
    public delegate void Error(object sender, ThreadExceptionEventArgs threadExceptionEventArgs);


    /// <summary>
    /// The interface that represents a managed connection between a current bluetooth adapter and the remote bluetooth device.
    /// Types that implements this interface can be used for the long time connections. 
    /// Managed means that it contains the internal manager that control a connection state, reconnect it if required and do other tasks at the background.
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
        /// The method that do not transmits a data immediately but adds it into the transmit queue to be transmitted on a connection will be in the <see cref="ConnectionState.Connected"/> state.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte{int}"/> with data to transmit.</param>
        void Transmit(Memory<byte> buffer);

        /// <summary>
        /// The method that do not transmits a data immediately but adds it into the transmit queue to be transmitted on a connection will be in the <see cref="ConnectionState.Connected"/> state.
        /// </summary>
        /// <param name="buffer">The parameter that represents a <see cref="byte{int}"/> with a data to transmit.</param>
        /// <param name="offset">The parameter that represents a transmit buffer offset.</param>
        /// <param name="count">The parameter that represents a count of the bytes to transmit.</param>
        void Transmit(byte[] buffer, int offset, int count);

        /// <summary>
        /// The event that raises on a connection state changed.
        /// </summary>
        event StateChanged OnStateChanged;
        /// <summary>
        /// The event that raises on a data transmitted.
        /// </summary>
        event Transmitted OnTransmitted;

        /// <summary>
        /// The event that raises on a data recived.
        /// </summary>
        event Recived OnRecived;

        /// <summary>
        /// The event tht raises on a connection and data transfer errors.
        /// </summary>
        event Error OnError;
    }
}