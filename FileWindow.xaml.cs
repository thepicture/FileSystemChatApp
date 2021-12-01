using ChatApp2.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
        private readonly string currentDirectoryStream =
            Directory.GetCurrentDirectory() + "\\s";

        /// <summary>
        /// Actions for inserting files in the File Window.
        /// </summary>
        public FileWindow()
        {
            InitializeComponent();
            if (File.Exists(currentDirectoryStream) == false)
            {
                File.Create(currentDirectoryStream).Close();
            }

            IEnumerable<ChatFile> fileList =
                from row in File.ReadLines(currentDirectoryStream)
                let arr = row.Split('\t')
                select new ChatFile
                {
                    UserName = arr.ElementAtOrDefault(0),
                    FileStream = arr.ElementAtOrDefault(1),
                    FileName = arr.ElementAtOrDefault(2)
                };
            if (fileList.Count() > 0)
            {
                FileList.ItemsSource = fileList.ToList();
            }
            else
            {
                _ = MessageBox.Show("Вложений в диалоге нет",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                Visibility = Visibility.Hidden;
            }
        }

        /// <summary>
        /// Actions for saving the selected file.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            ChatFile context = (sender as Button).DataContext as ChatFile;

            byte[] stream = Convert.FromBase64String(context.FileStream);
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
