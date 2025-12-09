using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdApp_Pushkina_Tciguleeva_422
{
    public partial class CompletedAdsPage : Page
    {
        public CompletedAdsPage()
        {
            InitializeComponent();
            LoadCompletedAds();
        }

        private void LoadCompletedAds()
        {
            var db = new AdsServiceEntities();
            var currentUser = App.Current.Properties["CurrentUser"] as Users;

            var completedAds = db.Ads
                .Where(a => a.user_id == currentUser.user_id && a.status_id == 2)
                .Select(a => new
                {
                    a.ad_id,
                    a.title,
                    a.description,
                    a.price,
                })
                .ToList();

            CompletedAdsGrid.ItemsSource = completedAds;

            decimal totalProfit = completedAds.Sum(a => (decimal)a.price);
            TotalProfitText.Text = $"Итоговая прибыль: {totalProfit:C}";
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new AdsPage());
        }
       
    }

}
