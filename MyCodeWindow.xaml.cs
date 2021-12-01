using System.Windows;

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

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TBoxName.Text) == false)
            {
                DialogResult = true;
            }
            else
            {
                _ = MessageBox.Show("Необходимо ввести имя",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
