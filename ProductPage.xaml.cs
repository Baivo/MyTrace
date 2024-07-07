using Microsoft.Maui.Controls;
using MyTraceLib.Services;
using MyTraceLib.Tables;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using MyTrace.Services;
using Microsoft.Extensions.Logging;

namespace MyTrace
{
    public partial class ProductPage : ContentPage
    {
        public ProductPageViewModel ViewModel { get; private set; }

        string BarcodeValue { get; set; } = string.Empty;

        public ProductPage(string barcode)
        {
            BarcodeValue = barcode;
            ViewModel = new ProductPageViewModel();
            BindingContext = ViewModel; // Set the BindingContext to the ViewModel
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await GetProductAsync();
            _ = LoadSimilarProductsAsync(BarcodeValue);
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopToRootAsync();
            return true; // Prevent default back button behavior
        }

        private async Task GetProductAsync()
        {
            var product = await FunctionsService.RequestProductByBarcodeAsync(BarcodeValue);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (product != null)
                {
                    LoadingIndicator.IsVisible = false;
                    ContentScrollView.IsVisible = true;

                    ProductImage.IsVisible = true;
                    ProductImage.Source = product.LargeImageFile;

                    ProductName.IsVisible = true;
                    ProductName.Text = product.Name;

                    ProductDescription.IsVisible = true;
                    ProductDescription.Text = product.Description.Replace("<br>", "\n").Trim();

                    BrandLabelTitle.IsVisible = true;
                    BrandLabelTitle.Text = "Brand";
                    BrandLabel.IsVisible = true;
                    BrandLabel.Text = $"{product.Brand}";

                    if (!string.IsNullOrEmpty(product.Ingredients))
                    {
                        IngredientsLabelTitle.IsVisible = true;
                        IngredientsLabelTitle.Text = "Ingredients";

                        IngredientsLabelStack.IsVisible = true;
                        IngredientsLabelStack.Children.Clear();
                        var ingredients = product.Ingredients.Split(',');

                        foreach (var ingredient in ingredients)
                        {
                            var ingredientLabel = new Label
                            {
                                Text = ingredient.Trim(),
                                Style = (Style)Application.Current.Resources["Body"],
                                GestureRecognizers = { new TapGestureRecognizer { Command = new Command<string>(async (ing) => await OnIngredientTapped(ing)), CommandParameter = ingredient.Trim() } }
                            };
                            IngredientsLabelStack.Children.Add(ingredientLabel);
                        }
                    }

                    if (!string.IsNullOrEmpty(product.NutritionalInformation))
                    {
                        NutritionInfoContainer.IsVisible = true;

                        var nutritionInfo = TextHelperService.ParseNutritionalInformation(product.NutritionalInformation);
                        NutritionInfoCollectionView.ItemsSource = nutritionInfo;
                    }
                    CheckSetCountryOfOrigin(product);
                }
                else
                {
                    LoadingIndicator.IsVisible = false;
                    ErrorLabel.IsVisible = true;
                }
            });
        }

        private async Task OnIngredientTapped(string ingredient)
        {
            await Navigation.PushAsync(new IngredientBreakdownPage(ingredient));
        }

        private void CheckSetCountryOfOrigin(WoolworthsProduct product)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                string country = string.Empty;

                if (!string.IsNullOrEmpty(product.CountryOfOrigin))
                    country = TextHelperService.FindCountryInString(product.CountryOfOrigin);
                else if (!string.IsNullOrEmpty(product.CountryOfOriginAltText))
                    country = TextHelperService.FindCountryInString(product.CountryOfOriginAltText);

                if (country != null && country.Length > 0)
                {
                    CountryOfOriginContainer.IsVisible = true;
                    CountryOfOriginTitle.IsVisible = true;
                    CountryOfOrigin.IsVisible = true;
                    CountryOfOrigin.Text = country;
                    if (country == "Australia")
                    {
                        CountryOfOrigin.TextColor = Colors.Yellow;
                        if (product.CountryOfOriginIngredientPercentage > 0.66)
                            CountryOfOrigin.TextColor = Colors.LightGreen;
                    }
                    else
                    {
                        CountryOfOrigin.TextColor = Colors.Red;
                    }
                    if (!string.IsNullOrEmpty(product.CountryOfOriginAltText))
                    {
                        CountryOfOriginAltText.IsVisible = true;
                        CountryOfOriginAltText.Text = product.CountryOfOriginAltText;
                    }
                }
            });
        }

        private async Task LoadSimilarProductsAsync(string barcode)
        {
            var similarProducts = await FunctionsService.GetSimilarProductsAsync(barcode);

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                SimilarProductsLoadingIndicator.IsVisible = false; // Hide the loading indicator

                if (similarProducts != null && similarProducts.Any())
                {
                    var logger = App.ServiceProvider.GetRequiredService<ILogger<SimilarProductViewModel>>();
                    var similarProductViewModels = similarProducts.Select(product => new SimilarProductViewModel(product, logger)).ToList();

                    SimilarProductsLabel.IsVisible = true;
                    SimilarProductsCollectionView.IsVisible = true;
                    SimilarProductsCollectionView.ItemsSource = similarProductViewModels;
                }
            });
        }


    }

    public class ProductPageViewModel : INotifyPropertyChanged
    {
        public ICommand ItemTappedCommand { get; private set; }

        public ProductPageViewModel()
        {
            ItemTappedCommand = new Command<WoolworthsProduct>(OnItemTapped);
        }

        private async void OnItemTapped(WoolworthsProduct product)
        {
            if (product != null)
            {
                await App.Current.MainPage.Navigation.PushAsync(new ProductPage(product.Barcode));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SimilarProductViewModel : INotifyPropertyChanged
    {
        private Color _countryOfOriginTextColor;
        public WoolworthsProduct Product { get; set; }
        public string CountryOfOriginText { get; set; }
        public Color CountryOfOriginTextColor
        {
            get => _countryOfOriginTextColor;
            set
            {
                if (_countryOfOriginTextColor != value)
                {
                    _countryOfOriginTextColor = value;
                    OnPropertyChanged();
                }
            }
        }

        private readonly ILogger<SimilarProductViewModel> _logger;

        public SimilarProductViewModel(WoolworthsProduct product, ILogger<SimilarProductViewModel> logger)
        {
            Product = product;
            _logger = logger;
            SetCountryOfOrigin(product.CountryOfOrigin ?? product.CountryOfOriginAltText, product.CountryOfOriginIngredientPercentage);
        }

        private void SetCountryOfOrigin(string countryOfOrigin, double countryOfOriginIngredientPercentage = 0)
        {
            try
            {
                var country = TextHelperService.FindCountryInString(countryOfOrigin);
                CountryOfOriginText = country;

                if (country == "Australia")
                {
                    CountryOfOriginTextColor = Colors.Yellow;
                    if (countryOfOriginIngredientPercentage > 0.66)
                    {
                        CountryOfOriginTextColor = Colors.LightGreen;
                    }
                }
                else
                {
                    CountryOfOriginTextColor = Colors.Red;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error setting CountryOfOriginTextColor: {ex}");
                CountryOfOriginTextColor = Colors.Black; // Default fallback color
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
