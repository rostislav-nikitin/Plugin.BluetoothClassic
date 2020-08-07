using Plugin.BluetoothClassic.Abstractions;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Retry
{
    public partial class App : Application
    {
        public static IBluetoothAdapter BluetoothAdapter { get; private set; }

        public App()
        {
            InitializeComponent();
            InitializeBluetoothAdapter();
            MainPage = new NavigationPage(new SelectDevicePage());
        }

        private void InitializeBluetoothAdapter()
        {
            var bluetoothAdapter = DependencyService.Resolve<IBluetoothAdapter>();
            App.BluetoothAdapter = bluetoothAdapter;
        }

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
