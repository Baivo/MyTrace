namespace MyTrace
{
    using ZXing.Net.Maui;
    using ZXing.Net.Maui.Controls;
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Maui.Controls;

    public partial class BarcodeScanner : ContentPage
    {
        private bool _isScanning = true;

        public BarcodeScanner()
        {
            InitializeComponent();

            if (barcodeView != null)
            {
                barcodeView.Options = new BarcodeReaderOptions
                {
                    Formats = BarcodeFormats.OneDimensional,
                    AutoRotate = true,
                    Multiple = true
                };
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _isScanning = true; // Re-enable scanning when the page appears
            barcodeView.IsDetecting = true; // Reactivate the barcode scanner
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            HideDebugMessage();
            HideErrorMessage();
            barcodeView.IsDetecting = false; // Deactivate the barcode scanner to save resources
        }

        protected async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (!_isScanning)
                return;

            try
            {
                _isScanning = false; // Stop scanning for barcodes

                string? resultCode = null;

                var first = e.Results?.FirstOrDefault();
                if (first is not null)
                {
                    resultCode = first.Value;
                    // Update Label on UI thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (ResultLabel != null)
                        {
                            ResultLabel.Text = $"Barcode: {first.Value}";
                        }
                    });
                }

                // Perform navigation after updating the UI
                if (resultCode != null && Navigation != null)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        ShowDebugMessage("Navigating to ProductPage...");
                        await Navigation.PushAsync(new ProductPage(resultCode));
                    });
                }
                else
                {
                    _isScanning = true; // Re-enable scanning if navigation failed
                    if (resultCode == null)
                    {
                        ShowDebugMessage("resultCode is null.");
                    }

                    if (Navigation == null)
                    {
                        ShowErrorMessage("Navigation is null.");
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Exception in BarcodesDetected: {ex}");
                _isScanning = true; // Re-enable scanning in case of an error
            }
        }

        void SwitchCameraButton_Clicked(object sender, EventArgs e)
        {
            if (barcodeView != null)
            {
                barcodeView.CameraLocation = barcodeView.CameraLocation == CameraLocation.Rear ? CameraLocation.Front : CameraLocation.Rear;
            }
        }

        void TorchButton_Clicked(object sender, EventArgs e)
        {
            if (barcodeView != null)
            {
                barcodeView.IsTorchOn = !barcodeView.IsTorchOn;
            }
        }

        void ShowDebugMessage(string message)
        {
            DebugLabel.Text = message;
            DebugLabel.IsVisible = true;
        }
        void HideDebugMessage()
        {
            DebugLabel.Text = string.Empty;
            DebugLabel.IsVisible = false;
        }
        void ShowErrorMessage(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }
        void HideErrorMessage()
        {
            ErrorLabel.Text = string.Empty;
            ErrorLabel.IsVisible = false;
        }
    }
}
