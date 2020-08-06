using Plugin.BluetoothClassic.Abstractions;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Digit
{
    public partial class App : Application
    {
        public static IBluetoothConnection CurrentBluetoothConnection { get; internal set; }

        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new SelectBluetoothRemoteDevicePage());
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
