using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace AdApp_Pushkina_Tciguleeva_422
{
    public partial class AdEditPage : Page
    {
        private int? _adId;

        public AdEditPage(int? adId)
        {
            InitializeComponent();
            _adId = adId;

            LoadCategories();
            LoadStatuses();

            PageTitle.Text = _adId == null ? "Добавить объявление" : "Редактировать объявление";

            if (_adId != null)
                LoadAdData();
        }

        private void LoadCategories()
        {
            using (var db = new AdsServiceEntities())
            {
                CategoryBox.ItemsSource = db.Categories.ToList();
                CategoryBox.DisplayMemberPath = "category_name";
                CategoryBox.SelectedValuePath = "category_id";
            }
        }

        private void LoadStatuses()
        {
            using (var db = new AdsServiceEntities())
            {
                StatusBox.ItemsSource = db.Statuses.ToList();
                StatusBox.DisplayMemberPath = "status_name";
                StatusBox.SelectedValuePath = "status_id";
            }
        }

        private void LoadAdData()
        {
            using (var db = new AdsServiceEntities())
            {
                var ad = db.Ads.FirstOrDefault(a => a.ad_id == _adId);

                if (ad == null)
                {
                    MessageBox.Show("Объявление не найдено", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    NavigationService?.GoBack();
                    return;
                }

                NameBox.Text = ad.title;
                DescriptionBox.Text = ad.description;
                PhotoPathBox.Text = ad.photo_path;
                if (!string.IsNullOrEmpty(ad.photo_path) && System.IO.File.Exists(ad.photo_path))
                {
                    PreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(ad.photo_path));
                }

                var city = db.Cities.FirstOrDefault(c => c.city_id == ad.city_id);
                CityBox.Text = city != null ? city.city_name : string.Empty;

                var type = db.AdTypes.FirstOrDefault(t => t.type_id == ad.type_id);
                TypeBox.Text = type != null ? type.type_name : string.Empty;

                PriceBox.Text = ad.price.ToString();

                CategoryBox.SelectedValue = ad.category_id;
                StatusBox.SelectedValue = ad.status_id;
            }
        }

        private string ShowInput(string text, string title)
        {
            Window w = new Window
            {
                Title = title,
                Width = 320,
                Height = 160,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.NoResize
            };

            var panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = text, Margin = new Thickness(0, 0, 0, 8) });
            var box = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            var ok = new Button { Content = "OK", Width = 80, HorizontalAlignment = HorizontalAlignment.Right };
            ok.Click += (s, e) => w.DialogResult = true;

            panel.Children.Add(box);
            panel.Children.Add(ok);
            w.Content = panel;

            return w.ShowDialog() == true ? box.Text : null;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Название обязательно.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(PriceBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Цена должна быть неотрицательным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CategoryBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (StatusBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int statusId = (int)StatusBox.SelectedValue;
            decimal? profit = null;

            using (var db = new AdsServiceEntities())
            {
                var finished = db.Statuses.FirstOrDefault(s => s.status_name == "Завершено");
                if (finished != null && statusId == finished.status_id)
                {
                    var input = ShowInput("Введите сумму продажи:", "Продажа");
                    if (!decimal.TryParse(input, out decimal sum) || sum < 0)
                    {
                        MessageBox.Show("Сумма должна быть неотрицательным числом.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    profit = sum;
                }

                int cityId;
                var cityName = (CityBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(cityName))
                {
                    MessageBox.Show("Укажите город.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var city = db.Cities.FirstOrDefault(c => c.city_name == cityName);
                if (city == null)
                {
                    city = new Cities { city_name = cityName };
                    db.Cities.Add(city);
                    db.SaveChanges(); 
                }
                cityId = city.city_id;

                int typeId;
                var typeName = (TypeBox.Text ?? string.Empty).Trim();
                if (string.IsNullOrEmpty(typeName))
                {
                    MessageBox.Show("Укажите тип объявления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var type = db.AdTypes.FirstOrDefault(t => t.type_name == typeName);
                if (type == null)
                {
                    type = new AdTypes { type_name = typeName };
                    db.AdTypes.Add(type);
                    db.SaveChanges();
                }
                typeId = type.type_id;

                Ads ad;
                if (_adId == null)
                {
                    ad = new Ads
                    {
                        post_date = DateTime.Now,
                        user_id = 1 
                    };
                    db.Ads.Add(ad);
                }
                else
                {
                    ad = db.Ads.FirstOrDefault(a => a.ad_id == _adId);
                    if (ad == null)
                    {
                        MessageBox.Show("Объявление не найдено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                ad.title = NameBox.Text.Trim();
                ad.description = DescriptionBox.Text?.Trim();
                ad.city_id = cityId;
                ad.type_id = typeId;
                ad.price = (int)price;
                ad.category_id = (int)CategoryBox.SelectedValue;
                ad.status_id = statusId;
                ad.profit = profit.HasValue ? (int?)profit.Value : null;
                ad.photo_path = PhotoPathBox.Text;
                db.SaveChanges();
            }

            MessageBox.Show("Изменения сохранены.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            NavigationService?.Navigate(new AdsPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void SelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp";
            dlg.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (dlg.ShowDialog() == true)
            {
                PhotoPathBox.Text = dlg.FileName;
                PreviewImage.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(dlg.FileName));
            }
        }


    }
}
