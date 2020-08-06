namespace DigitExample
{
    using BluetoothClassic.Base;
    using System;
    using System.Threading.Tasks;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectBluetoothRemoteDevicePage : ContentPage
    {
        private readonly IBluetoothAdapter _bluetoothAdapter;

        public SelectBluetoothRemoteDevicePage()
        {
            _bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            InitializeComponent();
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

        protected override async void OnAppearing()
        {
            RefreshUI();
            await DisconnectIfConnectedAsync();
        }
        private async Task DisconnectIfConnectedAsync()
        {
            if (App.CurrentBluetoothConnection != null)
            {
                try
                {
                    App.CurrentBluetoothConnection.Dispose();
                }
                catch (Exception exception)
                {
                    await DisplayAlert("Error", exception.Message, "Close");
                }
            }
        }

        private async void lvBluetoothBoundedDevices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            BluetoothDeviceModel bluetoothDeviceModel = e.SelectedItem as BluetoothDeviceModel;
            lvBluetoothBoundedDevices.SelectedItem = null;

            if (bluetoothDeviceModel != null)
            {
                var connected = await TryConnect(bluetoothDeviceModel);
                if (connected)
                {
                    await Navigation.PushAsync(new DigitPage());
                }
            }
        }

        private async Task<bool> TryConnect(BluetoothDeviceModel bluetoothDeviceModel)
        {
            const bool Connected = true;
            const bool NotConnected = false;


            var connection = _bluetoothAdapter.CreateConnection(bluetoothDeviceModel);
            try
            {
                connection.Connect();
                App.CurrentBluetoothConnection = connection;

                return Connected; 
            }
            catch (BluetoothConnectionException exception)
            {
                await DisplayAlert("Connection error",
                    $"Can not connect to the device: {bluetoothDeviceModel.Name}({bluetoothDeviceModel.Address}).\n" +
                        $"Exception: \"{exception.Message}\"\n" +
                        "Please, try another one.",
                    "Close");

                return NotConnected;
            }
            catch (Exception exception)
            {
                await DisplayAlert("Generic error", exception.Message, "Close");

                return NotConnected;
            }

        }
    }
}