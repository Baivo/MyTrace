using Microsoft.Maui.Controls.Xaml;
using MyTraceLib.Services;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]

namespace MyTrace
{
    
    using System.Linq;
    using Microsoft.Maui.Controls;

    public partial class ProductPage : ContentPage
    {
        string BarcodeValue { get; set; } = string.Empty;

        public ProductPage(string barcode)
        {
            BarcodeValue = barcode;
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
            var product = await WoolworthsSqlService.GetProductByBarcodeAsync(BarcodeValue);

            if (product != null)
            {
                LoadingIndicator.IsVisible = false;
                ContentScrollView.IsVisible = true;

                ProductImage.IsVisible = true;
                ProductImage.Source = product.LargeImageFile;

                ProductName.IsVisible = true;
                ProductName.Text = product.Name;

                ProductPrice.IsVisible = true;
                ProductPrice.Text = $"${product.Price:0.00}";

                ProductDescription.IsVisible = true;
                ProductDescription.Text = product.Description;

                BrandLabel.IsVisible = true;
                BrandLabel.Text = $"Brand: {product.Brand}";

                BarcodeLabel.IsVisible = true;
                BarcodeLabel.Text = $"Barcode: {product.Barcode}";

                if (!string.IsNullOrEmpty(product.Ingredients))
                {
                    IngredientsLabel.IsVisible = true;
                    IngredientsLabel.Text = $"Ingredients: {product.Ingredients}";
                }

                if (product.NutritionalInformation != null)
                {
                    NutritionInfoContainer.IsVisible = true;

                    //NutritionInfoCollectionView.ItemsSource = product.NutritionalInformation.Select(n => new NutritionalInfoModel
                    //{
                    //    Component =
                    //    PerServing = n.Values.QuantityPerServing,
                    //    Per100g = n.Values.QuantityPer100g100mL
                    //}).ToList();
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
