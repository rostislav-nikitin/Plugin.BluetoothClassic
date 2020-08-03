using BluetoothClassic.Base;
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
    public partial class SelectBluetoothDevicePage : ContentPage
    {
        private readonly IBluetoothAdapter _bluetoothAdapter;

        public SelectBluetoothDevicePage()
        {
            _bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();

            InitializeComponent();
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (_bluetoothAdapter.Enabled)
            {
                btnDisableBluetooth.IsEnabled = true;
                btnEnableBluetooth.IsEnabled = false;
                lvBluetoothBoundedDevices.ItemsSource = _bluetoothAdapter.BondedDevices;
            }
            else
            {
                btnDisableBluetooth.IsEnabled = false;
                btnEnableBluetooth.IsEnabled = true;
                lvBluetoothBoundedDevices.ItemsSource = null;
            }
        }

        private void btnEnableBluetooth_Clicked(object sender, EventArgs e)
        {
            _bluetoothAdapter.Enable();
            RefreshUI();

        }

        private void btnDisableBluetooth_Clicked(object sender, EventArgs e)
        {
            
            _bluetoothAdapter.Disable();
            RefreshUI();
        }

        private async void lvBluetoothBoundedDevices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            BluetoothDeviceModel bluetoothDeviceModel = e.SelectedItem as BluetoothDeviceModel;

            if (bluetoothDeviceModel != null)
            {
                var connection = _bluetoothAdapter.CreateConnection(bluetoothDeviceModel);
                await connection.ConnectAsync();
                App.CurrentBluetoothConnection = connection;

                await Navigation.PushAsync(new SynchronizeDigitPage());

                /*DisplayAlert("Bluetooth device selected",
                    $"Name: \"{bluetoothDeviceModel.Name}\"\n \"Address:{bluetoothDeviceModel.Address}\".",
                    "Close");*/
            }
        }

    }
}