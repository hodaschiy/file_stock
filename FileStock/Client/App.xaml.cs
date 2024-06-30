using System;
using System.Net.Http;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Client.Controls;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static HttpClient http = new HttpClient();
        public static User usr = new User();
        public static AuthControl auth = new AuthControl(); 
        public static FileControl fls = new FileControl();
    }
}
