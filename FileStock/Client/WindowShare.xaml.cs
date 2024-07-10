using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Логика взаимодействия для WindowShare.xaml
    /// </summary>
    public partial class WindowShare : Window
    {
        private ObservableCollection<Tuple<int, string>> LoU = new ObservableCollection<Tuple<int, string>>();
        private int fileId;
        public WindowShare(int Id)
        {
            InitializeComponent();
            this.fileId = Id;
            LoU = new ObservableCollection<Tuple<int, string>>(App.fls.GetUsersList());
            ListOfUsers.ItemsSource = LoU;
        }

        private void Button_Share_Click(object sender, RoutedEventArgs e)
        {
            int chosen = ListOfUsers.SelectedIndex;
            if (chosen == -1)
            {
                MessageBox.Show("Не выбран ни один элемент");
                return;
            }
            if (App.fls.Share(fileId, LoU[chosen].Item1))
            {
                MessageBox.Show($"Теперь файл доступен пользователю {LoU[chosen].Item2}");
                Close();
            }
            else
            {
                MessageBox.Show($"Произошли ошибки");
            }
        }
    }
}
