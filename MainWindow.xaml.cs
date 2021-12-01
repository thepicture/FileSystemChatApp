using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChatApp2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly Timer timer = new Timer
        {
            Interval = 2000,
        };
        string openedFileName = "";
        readonly string currentDirectory = Directory.GetCurrentDirectory() + "\\t";
        readonly string currentDirectoryOnline = Directory.GetCurrentDirectory() + "\\o";
        public readonly string currentDirectoryStream = Directory.GetCurrentDirectory() + "\\s";

        string sentFile; // Stream of the sent file.
        string fileName; // Filename of the sent file.

        public MainWindow()
        {
        }

        /// <summary>
        /// Actions for initializating a program.
        /// </summary>
        public MainWindow(string[] e)
        {
            InitializeComponent();

            dispatcherTimer.Tick += DispatcherTimer_Tick;
            AppData.MainWindow = this;
            if (e.Count() > 1)
            {
                if (MessageBox.Show("Открыть с помощью чата можно один файл за раз. Желаете ли вы прикрепить первый выделенный файл?", "Информация", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    openedFileName = e.First();
                }
            }
            else if (e.Count() > 0)
            {
                openedFileName = e.First();
            }

            if (!string.IsNullOrEmpty(openedFileName))
            {
                MyCodeWindow myCodeWindow = new MyCodeWindow();
                myCodeWindow.ShowDialog();
                if (myCodeWindow.DialogResult == true)
                {
                    TBoxMyCode.Text = myCodeWindow.TBoxName.Text;
                    BtnFile_Click(this, null);
                }
                else
                {
                    MessageBox.Show("Операция отменена.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            if (File.Exists(currentDirectory) == false)
                File.Create(currentDirectory).Close();
            if (File.Exists(currentDirectoryOnline) == false)
                File.Create(currentDirectoryOnline).Close();


            TBoxMessage.Focus();
            timer.Elapsed += Timer_Elapsed;
        }

        /// <summary>
        /// Actions when Timer has elapsed.
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => BtnUpdate_Click(timer, null));
            Dispatcher.Invoke(() =>
            {
                File.AppendAllText(currentDirectoryOnline, $"{(string.IsNullOrWhiteSpace(TBoxMyCode.Text) ? Environment.MachineName.ToString() : TBoxMyCode.Text)}\t" +
                $"{DateTime.Now}\n");
            });
        }

        /// <summary>
        /// Actions for validating and sending the message.
        /// </summary>
        private void BtnSend_Click(object sender, RoutedEventArgs e)
        {
            if (TBoxMyCode.Text.Trim().ToLower() == "я")
            {
                MessageBox.Show($"Нельзя использовать имя 'Я'. Сообщение не было доставлено. Пожалуйста, смените имя.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(TBoxMessage.Text) && sentFile == null)
            {
                MessageBox.Show("Нельзя отправить пустое сообщение. Пожалуйста, введите сообщение.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            var myName = string.IsNullOrWhiteSpace(TBoxMyCode.Text) ? Environment.MachineName.ToString() : TBoxMyCode.Text;
            if (sentFile == null)
            {

                File.AppendAllText(currentDirectory, $"{myName}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[0]}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[1]}\t" +
                    $"{TBoxMessage.Text}\n");
                TBoxMessage.Text = null;
                BtnUpdate_Click(this, e);
                MessageView.ScrollIntoView(MessageView.Items[MessageView.Items.Count - 1]);
            }
            else
            {
                File.AppendAllText(currentDirectory, $"{myName}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[0]}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[1]}\t" +
                    $"{TBoxMessage.Text}\t" +
                    $"в\t" +
                    $"{fileName}\n");
                File.AppendAllText(currentDirectoryStream, $"{myName}\t{sentFile}\t{fileName}\n");
                TBoxMessage.Text = null;
                BtnUpdate_Click(this, e);
                sentFile = null;
                BtnFile.Content = "Прикрепить файл";
                BtnFile.IsEnabled = true;
                MessageView.ScrollIntoView(MessageView.Items[MessageView.Items.Count - 1]);
            }
            RunTimeOut(2);
        }

        /// <summary>
        /// Actions for antispam. Accepts seconds as parameter.
        /// </summary>
        /// <param name="v">Seconds</param>
        private void RunTimeOut(int v)
        {
            BtnSend.IsEnabled = false;
            var timeOutTimer = new Timer
            {
                Interval = v * 1000,
                AutoReset = false
            };
            timeOutTimer.Elapsed += TimeOutTimer_Elapsed;
            timeOutTimer.Start();
        }

        private void TimeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => BtnSend.IsEnabled = true);
        }

        /// <summary>
        /// Actions for loading messages from a directory.
        /// </summary>
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {

            CheckOnline();
            int messagesCount = File.ReadLines(currentDirectory).Count();
            if (TBlockFilesCount.Text.Contains("...") == false
                && Convert.ToInt32(TBlockFilesCount.Text.Split(' ')[3]) == messagesCount
                && sender != TBoxMyCode
                && sender != this)
            {
                return;
            }
            var tupleList = from row in File.ReadLines(currentDirectory)
                            let arr = row.Split('\t')
                            select new Tuple<string, string, string, string, bool, string>(arr[0], arr[1], arr[2], arr[3], arr.ElementAtOrDefault(4) != null, arr.ElementAtOrDefault(5));
            int filesCount = tupleList.Where(p => p.Item5 == true).Count();
            var messageEntity = new List<MessageEntity>();

            foreach (var tuple in tupleList)
            {
                messageEntity.Add(new MessageEntity
                {
                    UserName = tuple.Item1,
                    Date = tuple.Item2,
                    Time = tuple.Item3,
                    Message = tuple.Item4,
                    IsFileAttached = tuple.Item5,
                    FileName = tuple.Item6
                });
            }
            if (string.IsNullOrWhiteSpace(MessagesCountBox.Text))
            {
                MessageView.ItemsSource = Enumerable.Reverse(messageEntity).Take(10).Reverse().ToList();
            }
            else
            {

                try
                {
                    MessageView.ItemsSource = Enumerable.Reverse(messageEntity).Take(int.Parse(MessagesCountBox.Text)).Reverse().ToList();
                }
                catch (Exception)
                {
                    MessageBox.Show("С количеством последних сообщений что-то пошло не так. Перезапустите, пожалуйста, чат.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (CheckBoxUpdate.IsChecked == true)
                timer.Start();
            if (CheckBoxUpdate.IsEnabled == false)
                CheckBoxUpdate.IsEnabled = true;
            if (MessagesCountBox.IsEnabled == false)
                MessagesCountBox.IsEnabled = true;
            if (BtnScroll.IsEnabled == false)
                BtnScroll.IsEnabled = true;

            TBlockFilesCount.Text = $"вложений: {filesCount} \nсообщений: {messagesCount}";
        }

        /// <summary>
        /// Actions for getting online users count with less than 4 seconds last activity.
        /// </summary>
        private void CheckOnline()
        {
            if (ChatWindow.IsActive == false)
            {
                if (Convert.ToInt32(TBlockFilesCount.Text.Split(' ')[3]) != File.ReadLines(currentDirectory).Count() && (TBoxOnline.Text.Contains("Лоадинг") == false))
                {
                    dispatcherTimer.Start();
                }
            }
            else
            {
                dispatcherTimer.Stop();
                taskBarItem.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            }

            var onlineUsers = from row in File.ReadLines(currentDirectoryOnline)
                              let word = row.Split('\t')
                              where DateTime.Now.ToUniversalTime() < DateTime.Parse(word[1]).ToUniversalTime().AddSeconds(4)
                              group word by new { Name = word[0], time = word[1] };
            TBoxOnline.Text = $"Онлайн: {onlineUsers.GroupBy(o => o.Key.Name).Count()}\n";
            foreach (var o in onlineUsers)
            {
                if (TBoxOnline.Text.Contains(o.Key.Name) == false)
                    TBoxOnline.Text += $"{o.Key.Name}\n";
            }
        }

        /// <summary>
        /// Starts a timer.
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
        /// Sends a message after clicking Enter key.
        /// </summary>
        private void TBoxMessage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter && BtnSend.IsEnabled == true)
            {
                BtnSend_Click(null, null);
            }
        }

        /// <summary>
        /// Sends the file with a message.
        /// </summary>
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MainWindow)
            {
                var stream = openedFileName;
                byte[] bytes = File.ReadAllBytes(stream);
                sentFile = Convert.ToBase64String(bytes);
                fileName = Path.GetFileName(stream);
                BtnFile.IsEnabled = false;
                BtnFile.Content = fileName;
                MessageBox.Show("Файл прикреплён и будет отправлен с сообщением", "Операция успешна",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
            if (!string.IsNullOrWhiteSpace(TBoxMyCode.Text))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    var stream = openFileDialog.FileName;
                    byte[] bytes = File.ReadAllBytes(stream);
                    sentFile = Convert.ToBase64String(bytes);
                    fileName = openFileDialog.SafeFileName;
                    BtnFile.IsEnabled = false;
                    BtnFile.Content = fileName;
                    MessageBox.Show("Файл прикреплён и будет отправлен с сообщением", "Операция успешна",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            else
            {
                MessageBox.Show("Нельзя отправлять файлы анонимно. Пожалуйста, укажите имя.", "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Actions for stopping the timer when the form is closing.
        /// </summary>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            timer.Stop();
            App.Current.Shutdown();
        }

        /// <summary>
        /// Actions for saving all files contained in dialogue consistently.
        /// </summary>
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            FileWindow fileWindow = new FileWindow();
            fileWindow.ShowDialog();
        }

        /// <summary>
        /// Actions for cleaning the dialog window.
        /// </summary>
        private void BtnClean_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(currentDirectory, "");
            File.WriteAllText(currentDirectoryStream, "");
            MessageBox.Show("Диалог и вложения успешно очищены!", "Операция успешна", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Actions for updating dialog when name is changed but dialog lines count does not.
        /// </summary>
        private void TBoxMyCode_LostFocus(object sender, RoutedEventArgs e)
        {
            BtnUpdate_Click(TBoxMyCode, null);
            BtnClearOnline_Click(null, null);
        }

        /// <summary>
        /// Actions for clearing online list.
        /// </summary>
        private void BtnClearOnline_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(currentDirectoryOnline, "");
            if (e != null)
                MessageBox.Show("Онлайн успешно очищен!", "Операция успешна", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        readonly string emojies = "😀 😃 😄 😁 😆 😅 😂 😊 😇 😉 😌 😍 😘 😗 😙 😚 😋 😛 😝 😜 😎 😏 😒 😞 😔 😟 😕 😣 😖 😫 😩 😢 😭 😤 😠 😡 😳 😱 😨 😰 😥 😓 😶 😐 😑 😬 😯 😦 😧 😮 😲 😴 😪 😵 😷 😈 👿 👹 👺 💩 👻 💀 ☠️ 👽 👾 🎃 😺 😸 😹 😻 😼 😽 🙀 😿 😾";
        /// <summary>
        /// Actions for showing the emoji panel.
        /// </summary>
        private void BtnEmoji_Click(object sender, RoutedEventArgs e)
        {
            if (EmojiList.Visibility == Visibility.Visible)
            {
                EmojiList.Visibility = Visibility.Collapsed;
                return;
            }
            EmojiList.Visibility = Visibility.Visible;
            EmojiList.ItemsSource = from emoji in emojies.Split(' ')
                                    select new EmojiClass { EmojiSelected = emoji };
        }

        class EmojiClass
        {
            public string EmojiSelected { get; set; }
        }

        /// <summary>
        /// Actions for adding the emoji to the textbox.
        /// </summary>
        private void EmojiSquare_Click(object sender, RoutedEventArgs e)
        {
            var context = (sender as Button).DataContext;
            TBoxMessage.Text += (context as EmojiClass).EmojiSelected;
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ChatWindow.FontSize = FontSlider.Value;
        }

        private void Border_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TBoxMessage.Text += $"{((sender as Border).DataContext as MessageEntity).UserName},";
        }

        private void MessagesCountBox_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^1-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void MessagesCountBox_PreviewExecuted(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
         e.Command == ApplicationCommands.Cut ||
         e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        private void MessagesCountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnUpdate_Click(this, null);
        }

        readonly DispatcherTimer dispatcherTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
       
        /// <summary>
        /// Actions for blinking app icon in the user's toolbar.
        /// </summary>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            if (taskBarItem.ProgressState == System.Windows.Shell.TaskbarItemProgressState.Indeterminate)
                taskBarItem.ProgressState = System.Windows.Shell.TaskbarItemProgressState.None;
            else
                taskBarItem.ProgressState = System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
        }

        private void BtnScroll_Click(object sender, RoutedEventArgs e)
        {
            if(MessageView.Items.Count > 0)
            MessageView.ScrollIntoView(MessageView.Items[MessageView.Items.Count - 1]);
        }

        private void CheckBoxImage_Checked(object sender, RoutedEventArgs e)
        {
            if(ChatWindow.IsInitialized == true)
            BtnUpdate_Click(this, null);
        }

        private void CheckBoxImage_Unchecked(object sender, RoutedEventArgs e)
        {
            BtnUpdate_Click(this, null);
        }
    }
}
