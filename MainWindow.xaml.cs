using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows;

namespace ChatApp2
{


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Timer timer = new Timer
        {
            Interval = 1000,
        };
        readonly string currentDirectory = Directory.GetCurrentDirectory() + "\\t";
        string sentFile; // Stream of the sent file.
        string fileName; // Filename of the sent file.
        readonly string[] reservedWords = new string[] {"<вложение>"};
        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(currentDirectory) == false)
                File.Create(currentDirectory).Close();

            TBoxMessage.Focus();
            timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// Actions when Timer has elapsed.
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => BtnUpdate_Click(sender, null));
        }

        /// <summary>
        /// Actions for validating and sending the message.
        /// </summary>
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if(TBoxMyCode.Text.Trim() == "Я")
            {
                MessageBox.Show($"Нельзя использовать имя 'Я'. Сообщение не было доставлено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (TBoxMessage.Text.ToLower().Contains(reservedWords[0]))
            {
                MessageBox.Show($"Вы не можете отправлять зарезервированные слова. Сообщение не было доставлено.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (sentFile == null)
            {
                File.AppendAllText(currentDirectory, $"{TBoxMyCode.Text}\t{DateTime.Now.ToString().Split(' ')[0]}\t{DateTime.Now.ToString().Split(' ')[1]}\t{TBoxMessage.Text}\n");
                TBoxMessage.Text = null;
                BtnUpdate_Click(this, e);
            }
            else
            {
                File.AppendAllText(currentDirectory, $"{TBoxMyCode.Text}\t{DateTime.Now.ToString().Split(' ')[0]}\t{DateTime.Now.ToString().Split(' ')[1]}\t{TBoxMessage.Text}\t{sentFile}\t{fileName}\n");
                TBoxMessage.Text = null;
                BtnUpdate_Click(this, e);
                sentFile = null;
                BtnFile.Content = "Сохранить файлы";
                BtnFile.IsEnabled = true;
            }
        }

        /// <summary>
        /// Actions for loading messages from directory.
        /// </summary>
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {
            int filesCount = 0;
            int messagesCount = 0;
            try
            {
                var fileData = File.ReadAllLines(currentDirectory);
                TBoxDialog.Text = null;
                foreach (var line in fileData)
                {
                    messagesCount++;
                    var data = line.Split('\t');
                    if (data[0].Trim() == TBoxMyCode.Text.Trim() && (string.IsNullOrWhiteSpace(TBoxMyCode.Text) == false))
                    {
                        TBoxDialog.Text += "Я ";
                    }
                    else
                    {
                        TBoxDialog.Text += $"{((data[0].Length == 0) ? "Аноним " : data[0].Trim())} ";
                    }
                    if (data.ElementAtOrDefault(4) != null)
                    {
                        filesCount++;
                        TBoxDialog.Text += $"<{data[1].Trim()} {data[2].Trim()}>: {data[3]} <Вложение>\n";
                    }
                    else
                    {
                        TBoxDialog.Text += $"<{data[1].Trim()} {data[2].Trim()}>: {data[3]}\n";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            if (CheckBoxUpdate.IsChecked == true)
                timer.Start();
            if (CheckBoxUpdate.IsEnabled == false)
                CheckBoxUpdate.IsEnabled = true;
            TBlockFilesCount.Text = $"вложений: {filesCount}\nсообщений: {messagesCount}";
        }

        /// <summary>
        /// Starts the timer.
        /// </summary>
        private void CheckBoxUpdate_Checked(object sender, RoutedEventArgs e)
        {
            timer.Start();
        }

        /// <summary>
        /// Stops the timer.
        /// </summary>
        private void CheckBoxUpdate_Unchecked(object sender, RoutedEventArgs e)
        {
            timer.Stop();
        }

        /// <summary>
        /// Sends a message from clicking Enter key.
        /// </summary>
        private void TBoxMessage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                BtnSend_Click(null, null);
            }
        }

        /// <summary>
        /// Sends the file with a message.
        /// </summary>
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TBoxMyCode.Text))
            {

                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    var stream = openFileDialog.FileName;
                    Byte[] bytes = File.ReadAllBytes(stream);
                    sentFile = Convert.ToBase64String(bytes);
                    fileName = openFileDialog.SafeFileName;
                    //File.Copy(stream, Path.Combine(currentDirectoryPath, Path.GetFileName(stream)));
                    BtnFile.IsEnabled = false;
                    BtnFile.Content = fileName;
                    MessageBox.Show("Файл прикреплён и будет отправлен с сообщением");
                }
            }
            else
            {
                MessageBox.Show("Нельзя отправлять файлы анонимно");
            }
        }

        /// <summary>
        /// Actions for stopping the timer when form is closing.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
        }

        /// <summary>
        /// Actions for saving all files contained in dialogue.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var atLeastOneFile = false;

            if (CheckBoxUpdate.IsChecked == true)
                timer.Stop();

            if (MessageBox.Show("Вы точно хотите сохранить все файлы, содержащиеся в диалоге?", "Внимание", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
                return;
            else
            {
                var fileData = File.ReadAllLines(currentDirectory);
                foreach (var line in fileData)
                {
                    var data = line.Split('\t');
                    if (data.ElementAtOrDefault(4) != null)
                    {
                        atLeastOneFile = true;

                        var stream = Convert.FromBase64String(data[4]);
                        SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            FileName = data[5]
                        };
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            File.WriteAllBytes(saveFileDialog.FileName, stream);
                        }
                    }
                }
                if (atLeastOneFile == false)
                    MessageBox.Show("В диалоге нет имеющихся файлов");
                if (CheckBoxUpdate.IsChecked == true)
                    timer.Start();
            }
        }

        /// <summary>
        /// Actions for cleaning a dialogue.
        /// </summary>
        private void BtnClean_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(currentDirectory, "");
            MessageBox.Show("Диалог успешно очищен!");
        }
    }
}
