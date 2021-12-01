using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ChatApp2
{
    /// <summary>
    /// Interaction logic for FileWindow.xaml
    /// </summary>
    public partial class FileWindow : Window
    {
        readonly string currentDirectoryStream = Directory.GetCurrentDirectory() + "\\s";
        class MyFile
        {
            public string UserName { get; set; }
            public string FileStream { get; set; }
            public string FileName { get; set; }
        }
        /// <summary>
        /// Actions for inserting files in the File Window.
        /// </summary>
        public FileWindow()
        {
            InitializeComponent();
            if (File.Exists(currentDirectoryStream) == false)
                File.Create(currentDirectoryStream).Close();
            var fileList = from row in File.ReadLines(currentDirectoryStream)
                           let arr = row.Split('\t')
                           select new MyFile { UserName = arr.ElementAtOrDefault(0), FileStream = arr.ElementAtOrDefault(1), FileName = arr.ElementAtOrDefault(2) };
            if (fileList.Count() > 0)
            {
                FileList.ItemsSource = fileList.ToList();
            }
            else
            {
                MessageBox.Show("Вложений в диалоге нет", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Actions for saving the selected file.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as Button).DataContext as MyFile;

            var stream = Convert.FromBase64String(context.FileStream);
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                FileName = context.FileName
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllBytes(saveFileDialog.FileName, stream);
            }
        }
    }
}
