using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace AdApp_Pushkina_Tciguleeva_422
{
  
    public partial class AdsPage : Page
    {
        private class AdView
        {
            public int ID { get; set; }
            public string Name { get; set; }
            public string City { get; set; }
            public string Type { get; set; }
            public string Description { get; set; }
            public decimal Price { get; set; }

            public int CategoryID { get; set; }
            public string CategoryName { get; set; }

            public int StatusID { get; set; }
            public string StatusName { get; set; }

            public string PhotoRaw { get; set; }
            public string PhotoResolved { get; set; }

            public int UserId { get; set; } 
        }

        private List<AdView> _allAds;
        public bool IsAuthorized { get; set; }

        public AdsPage()
        {
            InitializeComponent();

            var currentUser = App.Current.Properties["CurrentUser"] as Users;
            IsAuthorized = currentUser != null;

            AddButton.Visibility = IsAuthorized ? Visibility.Visible : Visibility.Collapsed;

            DataContext = this;

            Loaded += (s, e) =>
            {
                LoadFilters();
                LoadAds();
            };
        }

        #region Поиск

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text == "Поиск...")
            {
                SearchBox.Text = "";
                SearchBox.Foreground = System.Windows.Media.Brushes.Black;
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                SearchBox.Text = "Поиск...";
                SearchBox.Foreground = System.Windows.Media.Brushes.Gray;
            }
        }

        #endregion

        #region Загрузка объявлений

        private void LoadAds()
        {
            using (var db = new AdsServiceEntities())
            {
                var categories = db.Categories.ToDictionary(c => c.category_id, c => c.category_name);
                var statuses = db.Statuses.ToDictionary(s => s.status_id, s => s.status_name);
                var cities = db.Cities.ToDictionary(c => c.city_id, c => c.city_name);
                var types = db.AdTypes.ToDictionary(t => t.type_id, t => t.type_name);

                var adsFromDb = db.Ads.ToList();

                var basePath = AppDomain.CurrentDomain.BaseDirectory;
                var placeholder = Path.Combine(basePath, "Images", "C:/Users/Пользователь/OneDrive/Рабочий стол/Photo/глушка.png");

                if (!File.Exists(placeholder))
                {
                    MessageBox.Show("Заглушка изображения не найдена: " + placeholder, "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

                _allAds = adsFromDb.Select(a =>
                {
                    string resolved = ResolvePhoto(a.photo_path, basePath, placeholder);

                    return new AdView
                    {
                        ID = a.ad_id,
                        Name = a.title,
                        Description = a.description,
                        City = a.city_id != null && cities.ContainsKey(a.city_id) ? cities[a.city_id] : string.Empty,
                        Type = a.type_id != null && types.ContainsKey(a.type_id) ? types[a.type_id] : string.Empty,
                        Price = a.price,
                        CategoryID = a.category_id,
                        CategoryName = categories.ContainsKey(a.category_id) ? categories[a.category_id] : string.Empty,
                        StatusID = a.status_id,
                        StatusName = statuses.ContainsKey(a.status_id) ? statuses[a.status_id] : string.Empty,
                        PhotoRaw = a.photo_path,
                        PhotoResolved = resolved,
                        UserId = a.user_id
                    };
                }).ToList();

                ApplyFilters();
            }
        }

        private string ResolvePhoto(string stored, string basePath, string placeholder)
        {
            if (string.IsNullOrWhiteSpace(stored))
                return File.Exists(placeholder) ? placeholder : null;

            if (Path.IsPathRooted(stored))
                return File.Exists(stored) ? stored : (File.Exists(placeholder) ? placeholder : null);

            string combined = Path.Combine(basePath, stored);
            return File.Exists(combined) ? combined : (File.Exists(placeholder) ? placeholder : null);
        }

        #endregion

        #region Фильтры

        private void LoadFilters()
        {
            using (var db = new AdsServiceEntities())
            {
                var cities = db.Cities.Select(c => c.city_name).OrderBy(x => x).ToList();
                cities.Insert(0, "Все");
                CityFilter.ItemsSource = cities;
                CityFilter.SelectedIndex = 0;

                var categories = db.Categories.Select(c => c.category_name).OrderBy(x => x).ToList();
                categories.Insert(0, "Все");
                CategoryFilter.ItemsSource = categories;
                CategoryFilter.SelectedIndex = 0;

                var types = db.AdTypes.Select(t => t.type_name).OrderBy(x => x).ToList();
                types.Insert(0, "Все");
                TypeFilter.ItemsSource = types;
                TypeFilter.SelectedIndex = 0;

                var statuses = db.Statuses.Select(s => s.status_name).OrderBy(x => x).ToList();
                statuses.Insert(0, "Все");
                StatusFilter.ItemsSource = statuses;
                StatusFilter.SelectedIndex = 0;
            }
        }

        private void FiltersChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            if (_allAds == null) return;

            IEnumerable<AdView> filtered = _allAds;

            string search = SearchBox.Text?.Trim();
            if (!string.IsNullOrEmpty(search) && search != "Поиск...")
            {
                filtered = filtered.Where(a =>
                    (!string.IsNullOrEmpty(a.Name) && a.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0)
                    || (!string.IsNullOrEmpty(a.Description) && a.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));
            }

            if (CityFilter.SelectedItem is string city && city != "Все")
                filtered = filtered.Where(a => a.City == city);

            if (CategoryFilter.SelectedItem is string cat && cat != "Все")
                filtered = filtered.Where(a => a.CategoryName == cat);

            if (TypeFilter.SelectedItem is string type && type != "Все")
                filtered = filtered.Where(a => a.Type == type);

            if (StatusFilter.SelectedItem is string status && status != "Все")
                filtered = filtered.Where(a => a.StatusName == status);

            if (OwnerFilter.SelectedItem is ComboBoxItem ownerItem)
            {
                string ownerFilter = ownerItem.Content.ToString();
                var currentUser = App.Current.Properties["CurrentUser"] as Users;
                if (ownerFilter == "Мои" && currentUser != null)
                    filtered = filtered.Where(a => a.UserId == currentUser.user_id);
                else if (ownerFilter == "Другие" && currentUser != null)
                    filtered = filtered.Where(a => a.UserId != currentUser.user_id);
            }

            AdsList.ItemsSource = filtered.ToList();
        }

        #endregion

        #region Кнопки управления

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AdEditPage(null));
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || btn.Tag == null) return;
            if (!int.TryParse(btn.Tag.ToString(), out int id)) return;

            NavigationService.Navigate(new AdEditPage(id));
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || btn.Tag == null) return;
            if (!int.TryParse(btn.Tag.ToString(), out int id)) return;

            var result = MessageBox.Show("Удалить объявление?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            using (var db = new AdsServiceEntities())
            {
                var ad = db.Ads.FirstOrDefault(a => a.ad_id == id);
                if (ad != null)
                {
                    db.Ads.Remove(ad);
                    db.SaveChanges();
                }
            }

            LoadAds();
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button btn) || btn.Tag == null) return;
            if (!int.TryParse(btn.Tag.ToString(), out int id)) return;

            var result = MessageBox.Show("Вы хотите 'купить' это объявление?", "Подтверждение",
                                         MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            using (var db = new AdsServiceEntities())
            {
                var ad = db.Ads.FirstOrDefault(a => a.ad_id == id);
                if (ad != null)
                {
                    var soldStatus = db.Statuses.FirstOrDefault(s => s.status_name == "Завершено");
                    if (soldStatus != null)
                        ad.status_id = soldStatus.status_id;

                    db.SaveChanges();
                }
            }

            LoadAds();
        }

        #endregion
    }
}
