namespace DigitExample
{
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
        }

        private void CurrentBluetoothConnection_OnRecived(object sender, BluetoothClassic.Base.RecivedEventArgs recivedEventArgs)
        {
            DigitViewModel model = (DigitViewModel)BindingContext;

            if (model != null)
            {
                for (int index = 0; index < recivedEventArgs.Buffer.Length; index++)
                {
                    byte value = recivedEventArgs.Buffer.ToArray()[index];
                    model.Digit = value;
                }
            }
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DigitViewModel model = (DigitViewModel)BindingContext;

            if (model != null && App.CurrentBluetoothConnection.Connected)
            {
                App.CurrentBluetoothConnection.SendAsync(new Memory<byte>(new byte[] { model.Digit } ));
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.CurrentBluetoothConnection.OnRecived += CurrentBluetoothConnection_OnRecived;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            App.CurrentBluetoothConnection.OnRecived -= CurrentBluetoothConnection_OnRecived;
        }
    }
}