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
using System.Windows.Shapes;

namespace ChatApp2
{
    /// <summary>
    /// Interaction logic for MyCodeWindow.xaml
    /// </summary>
    public partial class MyCodeWindow : Window
    {
        public MyCodeWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBoxName.Text) == false)
                this.DialogResult = true;
            else
                MessageBox.Show("Необходимо ввести имя", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
