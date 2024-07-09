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
using System.Net.Http;
using System.Net.Http.Json;
using System.Configuration;
using System.Windows.Media.Animation;
using System.Security.Cryptography;
using static System.Net.WebRequestMethods;
using Newtonsoft.Json;
using NLog;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            if (App.http.BaseAddress == null)
            {
                bool IsHttps = Properties.Settings.Default.IsHttpsPrefer;
                if (IsHttps)
                {
                    App.http.BaseAddress = Properties.Settings.Default.HttpsAddr != null ? new Uri(Properties.Settings.Default.HttpsAddr) : new Uri(Properties.Settings.Default.HttpAddr);
                }
                else
                {
                    App.http.BaseAddress = Properties.Settings.Default.HttpAddr != null ? new Uri(Properties.Settings.Default.HttpAddr) : new Uri(Properties.Settings.Default.HttpsAddr);
                }
                if (App.http.BaseAddress == null)
                {
                    throw new Exception("Не задан адрес сервера");
                }
            }
        }

        private async void ButtonSubmit_Click(object sender, RoutedEventArgs e) // TODO логику handshake + авторизации + регистрации вынести в отдельный класс и дергать его
        {
            if (await App.auth.Authorize(Login.Text, Password.Password))
            {
                FileStockWindow fileStockWindow = new FileStockWindow();
                fileStockWindow.Show();
                Close();

            }
            else
            {
                ThisBox.Content = "Пользователь не найден, возможно неверный логин и/или пароль";
            }
        }

        private async void ButtonRegister_Click(object sender, RoutedEventArgs e)// TODO логику handshake + авторизации + регистрации вынести в отдельный класс и дергать его
        {
            string result = await App.auth.Register(Login.Text, Password.Password);
            if (result == "true")
            {
                FileStockWindow fileStockWindow = new FileStockWindow();
                fileStockWindow.Show();
                Close();

            }
            else
            {
                ThisBox.Content = result;
            }
        }
    }
}
