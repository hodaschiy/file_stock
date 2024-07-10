using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для FileStockWindow.xaml
    /// </summary>
    public partial class FileStockWindow : Window
    {
        private ObservableCollection<FileModel> LoF = new ObservableCollection<FileModel>();
        
        public FileStockWindow()
        {
            InitializeComponent();
            HeaderLabel.Content = $"Файлы пользователя {App.usr.Name}";
            LoF = new ObservableCollection<FileModel>(App.fls.GetUserFiles());
            ListOfFiles.ItemsSource = LoF;
        }

        private async void Button_Download_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            FileModel savingFile = LoF.Where(fl => fl.Id == Convert.ToInt32(btn.DataContext)).FirstOrDefault();

            if (await App.fls.Download(savingFile))
            {
                MessageBox.Show("Файл успешно сохранен!");
            }
            else
            {
                MessageBox.Show("Возникли ошибки!");
            };
        }

        private async void Button_AddFile_Click(object sender, RoutedEventArgs e)
        {
            FileModel newfl = await App.fls.Add();
            if (newfl == null || newfl.IsEmpty())
            {
                MessageBox.Show("Файл не добавлен");
                return;
            }
            LoF.Add(newfl);
        }

        private async void Button_Delete_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            int Id = Convert.ToInt32(btn.DataContext);
            var res = await App.fls.Remove(Id);

            if (res)
            {
                LoF.Remove(LoF.Where(fl => fl.Id == Id).FirstOrDefault());
            }
        }

        private void Button_Exit_Click(object sender, RoutedEventArgs e)
        {
            App.auth.Exit();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            Close();
        }

        private void Button_Share_Click(object sender, RoutedEventArgs e)
        {
            var btn = (Button)sender;
            int Id = Convert.ToInt32(btn.DataContext);
            WindowShare windowShare = new WindowShare(Id);
            windowShare.ShowDialog();
        }
    }
}
