using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AdApp_Pushkina_Tciguleeva_422
{
    /// <summary>
    /// Логика взаимодействия для LoginPage.xaml
    /// </summary>

        public partial class LoginPage : Page
        {
            public LoginPage()
            {
                InitializeComponent();
            }

            private void LoginButton_Click(object sender, RoutedEventArgs e)
            {
                string login = LoginTextBox.Text.Trim();
                string password = PasswordBox.Password;

                using (var db = new AdsServiceEntities())
                {
                    var user = db.Users.FirstOrDefault(u => u.login == login && u.password == password);
                    if (user != null)
                    {
                        App.Current.Properties["CurrentUser"] = user;
                    ((MainWindow)Application.Current.MainWindow).OnLoginSuccessful();
                        NavigationService.Navigate(new AdsPage());
                    }
                    else
                    {
                        ErrorText.Text = "Неверный логин или пароль";
                    }
                }
            }
        }
    }