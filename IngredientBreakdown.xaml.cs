using Microsoft.Maui.Controls;
using MyTraceLib.Services;
using MyTraceLib.Tables;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MyTrace
{
    public partial class IngredientBreakdownPage : ContentPage
    {
  

        public IngredientBreakdownPage(string ingredient)
        {
            InitializeComponent();
            LoadIngredientBreakdown(ingredient);
        }

        private async Task LoadIngredientBreakdown(string ingredient)
        {

            var breakdown = await AiService.GetIngredientBreakdownAsync(ingredient);

            if (breakdown != null)
            {
                LoadingIndicator.IsVisible = false;
                ContentScrollView.IsVisible = true;
                IngredientNameLabel.Text = breakdown.IngredientName;
                PurposeLabel.Text = breakdown.Purpose;
                SourceLabel.Text = breakdown.Source;
                ToxicityLabel.Text = breakdown.Toxicity;
                CarcinogenicPropertiesLabel.Text = breakdown.CarcinogenicProperties;
                HealthierAlternativesLabel.Text = string.Join(", ", breakdown.HealthierAlternatives);
                CommonUsesLabel.Text = string.Join(", ", breakdown.CommonUses);
                RegulatoryStatusInAustraliaLabel.Text = breakdown.RegulatoryStatusInAustralia;
                EnvironmentalImpactLabel.Text = breakdown.EnvironmentalImpact;
            }
            else
            {
                await DisplayAlert("Error", "Failed to load ingredient breakdown.", "OK");
                await Navigation.PopAsync();
            }

        }
    }

}
