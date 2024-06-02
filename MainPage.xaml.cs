namespace MyTrace
{
    using Microsoft.Maui.Controls;

    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new BarcodeScanner());
        }
    }
}
