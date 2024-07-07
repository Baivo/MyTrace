using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTrace.Services
{
    public static class TextHelperService
    {
        public static string FindCountryInString(string input)
        {
            foreach (var country in CountryList.Countries)
            {
                if (input.IndexOf(country, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return country;
                }
            }
            return null;
        }
        public static  List<NutritionalInfoModel> ParseNutritionalInformation(string nutritionalInformationJson)
        {
            var nutritionList = new List<NutritionalInfoModel>();

            var jsonObject = JObject.Parse(nutritionalInformationJson);
            var attributes = jsonObject["Attributes"] as JArray;

            foreach (var attribute in attributes)
            {
                var component = attribute["Description"].ToString();
                var value = attribute["Value"].ToString();

                var perServing = string.Empty;
                var per100g = string.Empty;

                if (component.Contains("Serving Size"))
                    continue;
                if (component.Contains("Servings Per Pack"))
                    continue;
                if (component.Contains("ValueWord"))
                    continue;
                if (component.Contains("SuffixUnits"))
                    continue;

                if (component.Contains("Per Serve"))
                {
                    component = component.Replace("Quantity Per Serve", "").Trim();
                    component = component.Replace("Total", "").Trim();
                    component = component.Replace("NIP", "").Trim();
                    component = component.Replace("-", "").Trim();
                    perServing = value;
                }
                else if (component.Contains("Per 100g"))
                {
                    component = component.Replace("Quantity Per 100g", "").Trim();
                    component = component.Replace("Total", "").Trim();
                    component = component.Replace("NIP", "").Trim();
                    component = component.Replace("-", "").Trim();
                    per100g = value;
                }

                var existingComponent = nutritionList.FirstOrDefault(n => n.Component == component);
                if (existingComponent != null)
                {
                    if (!string.IsNullOrEmpty(perServing))
                    {
                        existingComponent.PerServing = perServing;
                    }
                    if (!string.IsNullOrEmpty(per100g))
                    {
                        existingComponent.Per100g = per100g;
                    }
                }
                else
                {
                    nutritionList.Add(new NutritionalInfoModel
                    {
                        Component = component,
                        PerServing = perServing,
                        Per100g = per100g
                    });
                }
            }

            return nutritionList;
        }
    }
    public class NutritionalInfoModel
    {
        public string Component { get; set; } = "";
        public string PerServing { get; set; } = "";
        public string Per100g { get; set; } = "";
    }
    public static class CountryList
    {
        public static readonly List<string> Countries = new List<string>
    {
        "Afghanistan", "Albania", "Algeria", "Andorra", "Angola", "Antigua and Barbuda",
        "Argentina", "Armenia", "Australia", "Austria", "Azerbaijan", "Bahamas", "Bahrain",
        "Bangladesh", "Barbados", "Belarus", "Belgium", "Belize", "Benin", "Bhutan", "Bolivia",
        "Bosnia and Herzegovina", "Botswana", "Brazil", "Brunei", "Bulgaria", "Burkina Faso",
        "Burundi", "Cabo Verde", "Cambodia", "Cameroon", "Canada", "Central African Republic",
        "Chad", "Chile", "China", "Colombia", "Comoros", "Congo, Democratic Republic of the",
        "Congo, Republic of the", "Costa Rica", "Croatia", "Cuba", "Cyprus", "Czech Republic",
        "Denmark", "Djibouti", "Dominica", "Dominican Republic", "Ecuador", "Egypt", "El Salvador",
        "Equatorial Guinea", "Eritrea", "Estonia", "Eswatini", "Ethiopia", "Fiji", "Finland", "France",
        "Gabon", "Gambia", "Georgia", "Germany", "Ghana", "Greece", "Grenada", "Guatemala", "Guinea",
        "Guinea-Bissau", "Guyana", "Haiti", "Honduras", "Hungary", "Iceland", "India", "Indonesia",
        "Iran", "Iraq", "Ireland", "Israel", "Italy", "Jamaica", "Japan", "Jordan", "Kazakhstan",
        "Kenya", "Kiribati", "Korea, North", "Korea, South", "Kosovo", "Kuwait", "Kyrgyzstan",
        "Laos", "Latvia", "Lebanon", "Lesotho", "Liberia", "Libya", "Liechtenstein", "Lithuania",
        "Luxembourg", "Madagascar", "Malawi", "Malaysia", "Maldives", "Mali", "Malta", "Marshall Islands",
        "Mauritania", "Mauritius", "Mexico", "Micronesia", "Moldova", "Monaco", "Mongolia", "Montenegro",
        "Morocco", "Mozambique", "Myanmar", "Namibia", "Nauru", "Nepal", "Netherlands", "New Zealand",
        "Nicaragua", "Niger", "Nigeria", "North Macedonia", "Norway", "Oman", "Pakistan", "Palau",
        "Panama", "Papua New Guinea", "Paraguay", "Peru", "Philippines", "Poland", "Portugal", "Qatar",
        "Romania", "Russia", "Rwanda", "Saint Kitts and Nevis", "Saint Lucia", "Saint Vincent and the Grenadines",
        "Samoa", "San Marino", "Sao Tome and Principe", "Saudi Arabia", "Senegal", "Serbia", "Seychelles",
        "Sierra Leone", "Singapore", "Slovakia", "Slovenia", "Solomon Islands", "Somalia", "South Africa",
        "South Sudan", "Spain", "Sri Lanka", "Sudan", "Suriname", "Sweden", "Switzerland", "Syria",
        "Taiwan", "Tajikistan", "Tanzania", "Thailand", "Timor-Leste", "Togo", "Tonga", "Trinidad and Tobago",
        "Tunisia", "Turkey", "Turkmenistan", "Tuvalu", "Uganda", "Ukraine", "United Arab Emirates", "United Kingdom",
        "United States", "Uruguay", "Uzbekistan", "Vanuatu", "Vatican City", "Venezuela", "Vietnam",
        "Yemen", "Zambia", "Zimbabwe"
    };
    }

}
