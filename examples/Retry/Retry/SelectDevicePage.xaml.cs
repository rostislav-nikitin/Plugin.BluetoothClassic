using Plugin.BluetoothClassic.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Retry
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SelectDevicePage : ContentPage
    {
        public SelectDevicePage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            lvBondedDevices.ItemsSource = App.BluetoothAdapter.BondedDevices;
        }

        private async void lvBondedDevices_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            var model = (BluetoothDeviceModel)e.SelectedItem;
            if (model != null)
            {
                await Navigation.PushAsync(new ConnectPage() { BindingContext = model });
                lvBondedDevices.SelectedItem = null;
            }
        }
    }
}