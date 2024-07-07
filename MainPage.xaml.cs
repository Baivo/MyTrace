using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;

namespace MyTrace
{
    public partial class MainPage : ContentPage
    {
        private readonly IServiceProvider _serviceProvider;

        public MainPage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BarcodeBtn.FontAttributes = FontAttributes.Bold;
            BarcodeBtn.FontSize = 18;
        }

        private async void OnButtonClicked(object sender, EventArgs e)
        {
            var barcodeScanner = _serviceProvider.GetRequiredService<BarcodeScanner>();
            await Navigation.PushAsync(barcodeScanner);
        }
    }
}
