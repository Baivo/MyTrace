using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.Logging;

namespace MyTrace
{
    public partial class BarcodeScanner : ContentPage
    {
        private bool _isScanning = true;
        private readonly ILogger<BarcodeScanner> _logger;
        public BarcodeScanner(ILogger<BarcodeScanner> logger)
        {
            _logger = logger;
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
            _isScanning = true;
            barcodeView.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                HideDebugMessage();
                HideErrorMessage();
                barcodeView.IsDetecting = false;
            });
        }

        protected async void BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
        {
            if (!_isScanning)
                return;

            try
            {
                _logger.LogDebug("Barcode detected");
                _isScanning = false;

                var first = e.Results?.FirstOrDefault();
                if (first is not null)
                {
                    string resultCode = first.Value;
                    _logger.LogDebug($"First barcode detected: {resultCode}");

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (ResultLabel != null)
                        {
                            ResultLabel.Text = $"Barcode: {resultCode}";
                        }
                    });

                    if (Navigation != null)
                    {
                        _logger.LogDebug("Navigating to ProductPage...");
                        //await Task.Delay(2000);
                        await MainThread.InvokeOnMainThreadAsync(async () =>
                        {
                            await Navigation.PushAsync(new ProductPage(resultCode));
                        });
                    }
                    else
                    {
                        _logger.LogError("Navigation is null.");
                        ShowErrorMessage("Navigation is null.");
                        _isScanning = true;
                    }
                }
                else
                {
                    _logger.LogWarning("No barcode detected.");
                    ShowDebugMessage("No barcode detected.");
                    _isScanning = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in BarcodesDetected");
                ShowErrorMessage($"Exception in BarcodesDetected: {ex}");
                _isScanning = true;
            }
        }
    

        void SwitchCameraButton_Clicked(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (barcodeView != null)
                {
                    barcodeView.CameraLocation = barcodeView.CameraLocation == CameraLocation.Rear ? CameraLocation.Front : CameraLocation.Rear;
                }
            });
        }

        void TorchButton_Clicked(object sender, EventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (barcodeView != null)
                {
                    barcodeView.IsTorchOn = !barcodeView.IsTorchOn;
                }
            });
        }

        void ShowDebugMessage(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DebugLabel.Text = message;
                DebugLabel.IsVisible = true;
            });
        }
        void HideDebugMessage()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                DebugLabel.Text = string.Empty;
                DebugLabel.IsVisible = false;
            });
        }
        void ShowErrorMessage(string message)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ErrorLabel.Text = message;
                ErrorLabel.IsVisible = true;
            });
        }
        void HideErrorMessage()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ErrorLabel.Text = string.Empty;
                ErrorLabel.IsVisible = false;
            });
        }
    }
}
