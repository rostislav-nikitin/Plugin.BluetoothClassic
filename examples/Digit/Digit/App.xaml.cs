namespace Digit
{
    using Plugin.BluetoothClassic.Abstractions;
    using Xamarin.Forms;

    public partial class App : Application
    {
        public static IBluetoothManagedConnection CurrentBluetoothConnection { get; internal set; }

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
