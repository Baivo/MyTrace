using Microsoft.Maui.Controls.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace MyTrace
{
    using MyTrace.Data.Woolworths;
    using MyTrace.Helpers;
    using System.Linq;
    using Microsoft.Maui.Controls;

    public partial class ProductPage : ContentPage
    {
        string BarcodeValue { get; set; } = string.Empty;
        WooliesAPIHelper WooliesAPIHelper { get; set; }

        public ProductPage(string barcode)
        {
            BarcodeValue = barcode;
            WooliesAPIHelper = new WooliesAPIHelper();
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            GetProductAsync(); // Ensure this is called here
        }

        protected override bool OnBackButtonPressed()
        {
            Navigation.PopToRootAsync();
            return true; // Prevent default back button behavior
        }

        private async void GetProductAsync()
        {
            var product = await WooliesAPIHelper.RequestProductFromBarcodeAsync(BarcodeValue);

            if (product != null)
            {
                LoadingIndicator.IsVisible = false;
                ContentScrollView.IsVisible = true;

                ProductImage.IsVisible = true;
                ProductImage.Source = product.Product.LargeImageFile;

                ProductName.IsVisible = true;
                ProductName.Text = product.Product.Name;

                ProductPrice.IsVisible = true;
                ProductPrice.Text = $"${product.Product.Price:0.00}";

                ProductDescription.IsVisible = true;
                ProductDescription.Text = product.Product.Description;

                BrandLabel.IsVisible = true;
                BrandLabel.Text = $"Brand: {product.Product.Brand}";

                BarcodeLabel.IsVisible = true;
                BarcodeLabel.Text = $"Barcode: {product.Product.Barcode}";

                if (!string.IsNullOrEmpty(product.Product.AdditionalAttributes?.Ingredients))
                {
                    IngredientsLabel.IsVisible = true;
                    IngredientsLabel.Text = $"Ingredients: {product.Product.AdditionalAttributes.Ingredients}";
                }

                if (product.NutritionalInformation != null && product.NutritionalInformation.Count > 0)
                {
                    NutritionInfoContainer.IsVisible = true;
                    NutritionInfoCollectionView.ItemsSource = product.NutritionalInformation.Select(n => new NutritionalInfoModel
                    {
                        Component = n.Name,
                        PerServing = n.Values.QuantityPerServing,
                        Per100g = n.Values.QuantityPer100g100mL
                    }).ToList();
                }
            }
            else
            {
                LoadingIndicator.IsVisible = false;
                ErrorLabel.IsVisible = true;
            }
        }
    }

    public class NutritionalInfoModel
    {
        public string Component { get; set; } = "";
        public string PerServing { get; set; } = "";
        public string Per100g { get; set; } = "";
    }
}
