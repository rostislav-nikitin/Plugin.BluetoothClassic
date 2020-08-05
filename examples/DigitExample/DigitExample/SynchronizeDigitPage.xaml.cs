namespace DigitExample
{
    using BluetoothClassic.Base;
    using DigitExample.ViewModel;
    using System;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SynchronizeDigitPage : ContentPage
    {
        public SynchronizeDigitPage()
        {
            InitializeComponent();

            DigitViewModel model = (DigitViewModel)BindingContext;
            model.PropertyChanged += Model_PropertyChanged;

            App.CurrentBluetoothConnection.OnRecived += CurrentBluetoothConnection_OnRecived;
            App.CurrentBluetoothConnection.OnError += CurrentBluetoothConnection_OnError;
        }

        ~SynchronizeDigitPage()
        {
            App.CurrentBluetoothConnection.OnRecived -= CurrentBluetoothConnection_OnRecived;
            App.CurrentBluetoothConnection.OnError -= CurrentBluetoothConnection_OnError;
        }

        private void CurrentBluetoothConnection_OnRecived(object sender, BluetoothClassic.Base.RecivedEventArgs recivedEventArgs)
        {
            DigitViewModel model = (DigitViewModel)BindingContext;

            if (model != null)
            {
                model.SetReciving();

                for (int index = 0; index < recivedEventArgs.Buffer.Length; index++)
                {
                    byte value = recivedEventArgs.Buffer.ToArray()[index];
                    model.Digit = value;
                }

                model.SetRecived();
            }
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            SendCurrentDigit();
        }

        private void CurrentBluetoothConnection_OnError(object sender, System.Threading.ThreadExceptionEventArgs errorEventArgs)
        {
            if(errorEventArgs.Exception is BluetoothTransmitterException)
            {
                SendCurrentDigit();
            }
        }

        private void SendCurrentDigit()
        {
            DigitViewModel model = (DigitViewModel)BindingContext;
            if (model != null && !model.Reciving)
            {
                App.CurrentBluetoothConnection.Send(new Memory<byte>(new byte[] { model.Digit }));
            }
        }

    }
}