using DigitExample.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DigitExample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SynchronizeDigitPage : ContentPage
    {
        public SynchronizeDigitPage()
        {
            InitializeComponent();
            App.CurrentBluetoothConnection.OnRecived += CurrentBluetoothConnection_OnRecived;

            DigitViewModel model = (DigitViewModel)BindingContext;
            model.PropertyChanged += Model_PropertyChanged;
        }

        private void CurrentBluetoothConnection_OnRecived(object sender, BluetoothClassic.Base.RecivedEventArgs recivedEventArgs)
        {
            DigitViewModel model = (DigitViewModel)BindingContext;

            if (model != null)
            {
                var length = recivedEventArgs.Buffer.Length;
                if (length > 0)
                {
                    model.Digit = recivedEventArgs.Buffer.ToArray()[length - 1];
                }
            }
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            DigitViewModel model = (DigitViewModel)BindingContext;
            if (model != null)
            {
                App.CurrentBluetoothConnection.SendAsync(new Memory<byte>(new byte[] { model.Digit } ));
            }
        }


        /*
        private void sliderDigit_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            App.CurrentBluetoothConnection.SendAsync(new Memory<byte>(new byte[] { (byte)e.NewValue }));
        }

        private void stepperDigit_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            App.CurrentBluetoothConnection.SendAsync(new Memory<byte>(new byte[] { (byte)e.NewValue }));
        }*/

        protected override bool OnBackButtonPressed()
        {
            const bool BackAccepted = true;

            App.CurrentBluetoothConnection.OnRecived -= CurrentBluetoothConnection_OnRecived;

            return BackAccepted;
        }
    }
}