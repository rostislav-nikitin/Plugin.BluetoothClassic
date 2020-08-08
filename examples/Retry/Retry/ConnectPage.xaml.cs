namespace Retry
{
    using Plugin.BluetoothClassic.Abstractions;
    using System;
    using System.Threading;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConnectPage : ContentPage
    {
        private const int TransmitBufferSizeDefault = 1;
        private const int ReciveBufferSizeDefault = 16;

        private const int BufferOffsetDefault = 0;

        public ConnectPage()
        {
            InitializeComponent();
        }

        private async void btnTransmitSingleDigit_Clicked(object sender, EventArgs e)
        {
            var remoteDevice = (BluetoothDeviceModel)BindingContext;
            if (remoteDevice != null)
            {
                using(var connection = App.BluetoothAdapter.CreateConnectionAsync(remoteDevice))
                {
                    if (await connection.RetryConnectAsync(retriesCount: 2))
                    {

                        byte[] buffer = new byte[TransmitBufferSizeDefault] { (byte)stepperDigit.Value };
                        try
                        {
                            await connection.RetryTransmitAsync(buffer, BufferOffsetDefault, buffer.Length);
                        }
                        catch (Exception exception)
                        {
                            await DisplayAlert("Exception", exception.Message, "Close");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Exception", "Can not connect.", "Close");
                    }
                }
            }
        }

        private async void btnReciveSingleDigit_Clicked(object sender, EventArgs e)
        {
            var remoteDevice = (BluetoothDeviceModel)BindingContext;
            if (remoteDevice != null)
            {
                using (var connection = App.BluetoothAdapter.CreateConnectionAsync(remoteDevice))
                {
                    if (await connection.RetryConnectAsync())
                    {
                        byte[] buffer = new byte[ReciveBufferSizeDefault];
                        try
                        {
                            var count = await connection.ReciveAsync(buffer, BufferOffsetDefault, buffer.Length);
                            if (count > 0)
                            {
                                stepperDigit.Value = buffer[count - 1];
                            }
                        }
                        catch (Exception exception)
                        {
                            await DisplayAlert("Exception", exception.Message, "Close");
                        }
                    }
                    else
                    {
                        await DisplayAlert("Exception", "Can not connect.", "Close");
                    }
                }
            }
        }
    }
}