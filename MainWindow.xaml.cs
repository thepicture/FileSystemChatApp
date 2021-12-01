using ChatApp2.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        private readonly Timer timer = new Timer
        {
            Interval = 2000,
        };
        private readonly string openedFileName = "";
        private readonly string currentDirectory = Directory.GetCurrentDirectory()
            + "\\t";
        private readonly string currentDirectoryOnline = Directory.GetCurrentDirectory()
            + "\\o";
        public readonly string currentDirectoryStream = Directory.GetCurrentDirectory()
            + "\\s";

        private string sentFile; // Stream of the sent file.
        private string fileName; // Filename of the sent file.

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
                if (UserWantsToOpenAttachedFile())
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
                _ = myCodeWindow.ShowDialog();
                if (myCodeWindow.DialogResult == true)
                {
                    TBoxMyCode.Text = myCodeWindow.TBoxName.Text;
                    BtnFile_Click(this, null);
                }
                else
                {
                    _ = MessageBox.Show("Операция отменена.",
                        "Информация",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }

            if (File.Exists(currentDirectory) == false)
            {
                File.Create(currentDirectory).Close();
            }

            if (File.Exists(currentDirectoryOnline) == false)
            {
                File.Create(currentDirectoryOnline).Close();
            }

            _ = TBoxMessage.Focus();
            timer.Elapsed += Timer_Elapsed;
        }

        public MainWindow()
        {
        }

        private static bool UserWantsToOpenAttachedFile()
        {
            return MessageBox.Show("Открыть с помощью чата можно один файл за раз. " +
                                "Открыть первый выделенный файл?",
                                "Информация",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Information) == MessageBoxResult.Yes;
        }

        /// <summary>
        /// Actions when the timer has elapsed.
        /// </summary>
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Dispatcher.Invoke(() => BtnUpdate_Click(timer, null));
            Dispatcher.Invoke(() =>
            {
                File.AppendAllText(currentDirectoryOnline,
                    $"{GetMachineOrUserName()}\t{DateTime.Now}\n");
            });
        }

        private string GetMachineOrUserName()
        {
            return string.IsNullOrWhiteSpace(TBoxMyCode.Text)
                ? Environment.MachineName.ToString()
                : TBoxMyCode.Text;
        }

        /// <summary>
        /// Actions for validating and sending a message.
        /// </summary>
        private void BtnSend_Click(object sender = null, RoutedEventArgs e = null)
        {
            if (TBoxMyCode.Text.Trim().ToLower() == "я")
            {
                _ = MessageBox.Show($"Нельзя использовать имя 'Я'. " +
                    $"Сообщение не было доставлено. " +
                    $"Пожалуйста, смените имя.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            if (string.IsNullOrWhiteSpace(TBoxMessage.Text) && sentFile == null)
            {
                _ = MessageBox.Show("Нельзя отправить пустое сообщение. " +
                    "Пожалуйста, введите непустое сообщение.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }
            string myName = string.IsNullOrWhiteSpace(TBoxMyCode.Text)
                ? Environment.MachineName.ToString()
                : TBoxMyCode.Text;
            if (sentFile == null)
            {

                File.AppendAllText(currentDirectory, $"{myName}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[0]}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[1]}\t" +
                    $"{TBoxMessage.Text.Replace(Environment.NewLine, "&#`")}\n");
                TBoxMessage.Text = null;
                BtnUpdate_Click(this, e);
                MessageView.ScrollIntoView
                    (
                        MessageView.Items[MessageView.Items.Count - 1]
                    );
            }
            else
            {
                File.AppendAllText(currentDirectory, $"{myName}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[0]}\t" +
                    $"{DateTime.Now.ToString().Split(' ')[1]}\t" +
                    $"{TBoxMessage.Text}\t" +
                    $"в\t" +
                    $"{fileName}\n");
                File.AppendAllText(currentDirectoryStream,
                    $"{myName}\t{sentFile}\t{fileName}\n");
                TBoxMessage.Text = null;
                BtnUpdate_Click(this, e);
                sentFile = null;
                BtnFile.Content = "Прикрепить файл";
                BtnFile.IsEnabled = true;
                MessageView.ScrollIntoView
                    (
                        MessageView.Items[MessageView.Items.Count - 1]
                    );
            }
            RunTimeOut(2);
        }

        /// <summary>
        /// Actions for preventing spam. 
        /// Accepts seconds as the timeout parameter.
        /// </summary>
        /// <param name="seconds">Seconds to the next timeout.</param>
        private void RunTimeOut(int seconds)
        {
            BtnSend.IsEnabled = false;
            Timer timeOutTimer = new Timer
            {
                Interval = seconds * 1000,
                AutoReset = false,
            };
            timeOutTimer.Elapsed += TimeOutTimer_Elapsed;
            timeOutTimer.Start();
        }

        private void TimeOutTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _ = Dispatcher.Invoke(() => BtnSend.IsEnabled = true);
        }

        /// <summary>
        /// Actions for loading messages from the current directory.
        /// </summary>
        private void BtnUpdate_Click(object sender, RoutedEventArgs e)
        {

            CheckOnline();
            int messagesCount = File.ReadLines(currentDirectory).Count();
            if (NoNewMessages(sender, messagesCount))
            {
                return;
            }
            IEnumerable<Tuple<string, string, string, string, bool, string>> tupleList =
                from row in File.ReadLines(currentDirectory)
                let arr = row.Split('\t')
                select new Tuple<string, string, string, string, bool, string>
                (
                    arr[0],
                    arr[1],
                    arr[2],
                    arr[3].Replace("&#`", Environment.NewLine),
                    arr.ElementAtOrDefault(4) != null,
                    arr.ElementAtOrDefault(5)
                );
            int filesCount = tupleList.Count(p => p.Item5);
            List<MessageEntity> messageEntity = new List<MessageEntity>();

            foreach (Tuple<string, string, string, string, bool, string> tuple
                in tupleList)
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
                MessageView.ItemsSource = Enumerable
                    .Reverse(messageEntity)
                    .Take(10)
                    .Reverse()
                    .ToList();
            }
            else
            {
                try
                {
                    MessageView.ItemsSource = Enumerable
                        .Reverse(messageEntity)
                        .Take(int.Parse(MessagesCountBox.Text))
                        .Reverse()
                        .ToList();
                }
                catch (Exception)
                {
                    _ = MessageBox.Show("С количеством последних сообщений " +
                        "что-то пошло не так. " +
                        "Перезапустите чат.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            if (CheckBoxUpdate.IsChecked == true)
            {
                timer.Start();
            }

            if (CheckBoxUpdate.IsEnabled == false)
            {
                CheckBoxUpdate.IsEnabled = true;
            }

            if (MessagesCountBox.IsEnabled == false)
            {
                MessagesCountBox.IsEnabled = true;
            }

            if (BtnScroll.IsEnabled == false)
            {
                BtnScroll.IsEnabled = true;
            }

            TBlockFilesCount.Text = $"вложений: {filesCount} " +
                $"\nсообщений: {messagesCount}";
        }

        private bool NoNewMessages(object sender, int messagesCount)
        {
            return TBlockFilesCount.Text.Contains("...") == false
                   && Convert.ToInt32(TBlockFilesCount.Text.Split(' ')[3])
                   == messagesCount
                   && sender != TBoxMyCode
                   && sender != this;
        }

        /// <summary>
        /// Actions for getting online users count 
        /// with less than 4 seconds 
        /// last activity.
        /// </summary>
        private void CheckOnline()
        {
            if (ChatWindow.IsActive == false)
            {
                if (NeedsToUpdateOnline())
                {
                    dispatcherTimer.Start();
                }
            }
            else
            {
                dispatcherTimer.Stop();
                taskBarItem.ProgressState = System.Windows.Shell
                    .TaskbarItemProgressState.None;
            }

            IEnumerable<IGrouping<(string Name, string time), string[]>> onlineUsers =
                from row in File.ReadLines(currentDirectoryOnline)
                let word = row.Split('\t')
                where MessageIsRecentlySent(word)
                group word by (Name: word[0], time: word[1]);
            TBoxOnline.Text = "Онлайн: " +
                $"{onlineUsers.GroupBy(o => o.Key.Name).Count()}\n";
            foreach (IGrouping<(string Name, string time), string[]> o in onlineUsers)
            {
                if (TBoxOnline.Text.Contains(o.Key.Name) == false)
                {
                    TBoxOnline.Text += $"{o.Key.Name}\n";
                }
            }
        }

        private static bool MessageIsRecentlySent(string[] word)
        {
            return DateTime.Now
                   .ToUniversalTime() < DateTime.Parse(word[1])
                   .ToUniversalTime().AddSeconds(4);
        }

        private bool NeedsToUpdateOnline()
        {
            return Convert.ToInt32(TBlockFilesCount.Text.Split(' ')[3])
                != File.ReadLines(currentDirectory).Count()
                && (TBoxOnline.Text.Contains("Лоадинг") == false);
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
        private void TBoxMessage_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && BtnSend.IsEnabled)
            {
                BtnSend_Click();
            }
        }

        /// <summary>
        /// Sends the file with a message.
        /// </summary>
        private void BtnFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MainWindow)
            {
                string filePath = openedFileName;
                byte[] bytes = File.ReadAllBytes(filePath);
                sentFile = Convert.ToBase64String(bytes);
                fileName = Path.GetFileName(filePath);
                BtnFile.IsEnabled = false;
                BtnFile.Content = fileName;
                ShowFileIsAttachedMessage();
                return;
            }
            if (!string.IsNullOrWhiteSpace(TBoxMyCode.Text))
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    string stream = openFileDialog.FileName;
                    byte[] bytes = File.ReadAllBytes(stream);
                    sentFile = Convert.ToBase64String(bytes);
                    fileName = openFileDialog.SafeFileName;
                    BtnFile.IsEnabled = false;
                    BtnFile.Content = fileName;
                    ShowFileIsAttachedMessage();
                }
            }
            else
            {
                _ = MessageBox.Show("Нельзя отправлять файлы анонимно. " +
                    "Укажите имя.", "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ShowFileIsAttachedMessage()
        {
            _ = MessageBox.Show("Файл прикреплён " +
                "и будет отправлен с сообщением",
                "Операция успешна",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Actions for stopping the timer when the form is closing.
        /// </summary>
        private void Window_Closing(object sender, CancelEventArgs e)
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
            _ = fileWindow.ShowDialog();
        }

        /// <summary>
        /// Actions for cleaning the dialog window.
        /// </summary>
        private void BtnClean_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(currentDirectory, "");
            File.WriteAllText(currentDirectoryStream, "");
            _ = MessageBox.Show("Диалог и вложения успешно очищены!",
                "Операция успешна",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        /// <summary>
        /// Actions for updating a dialog 
        /// when its name was changed 
        /// but the count of dialog lines was not.
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
            {
                _ = MessageBox.Show("Онлайн успешно очищен!",
                    "Операция успешна",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }

        private const string emojiesTemplate = "😀 " +
            "😃 " +
            "😄 " +
            "😁 " +
            "😆 " +
            "😅 " +
            "😂 " +
            "😊 " +
            "😇 " +
            "😉 " +
            "😌 " +
            "😍 " +
            "😘 " +
            "😗 " +
            "😙 " +
            "😚 " +
            "😋 " +
            "😛 " +
            "😝 " +
            "😜 " +
            "😎 " +
            "😏 " +
            "😒 " +
            "😞 " +
            "😔 " +
            "😟 " +
            "😕 " +
            "😣 " +
            "😖 " +
            "😫 " +
            "😩 " +
            "😢 " +
            "😭 " +
            "😤 " +
            "😠 " +
            "😡 " +
            "😳 " +
            "😱 " +
            "😨 " +
            "😰 " +
            "😥 " +
            "😓 " +
            "😶 " +
            "😐 " +
            "😑 " +
            "😬 " +
            "😯 " +
            "😦 " +
            "😧 " +
            "😮 " +
            "😲 " +
            "😴 " +
            "😪 " +
            "😵 " +
            "😷 " +
            "😈 " +
            "👿 " +
            "👹 " +
            "👺 " +
            "💩 " +
            "👻 " +
            "💀 " +
            "☠️ " +
            "👽 " +
            "👾 " +
            "🎃 " +
            "😺 " +
            "😸 " +
            "😹 " +
            "😻 " +
            "😼 " +
            "😽 " +
            "🙀 " +
            "😿 " +
            "😾";
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
            EmojiList.ItemsSource = from emoji in emojiesTemplate.Split(' ')
                                    select new Emoji { EmojiSelected = emoji };
        }

        /// <summary>
        /// Actions for adding the emoji to the textbox.
        /// </summary>
        private void EmojiSquare_Click(object sender, RoutedEventArgs e)
        {
            Emoji emoji = (sender as Button).DataContext as Emoji;
            TBoxMessage.Text += emoji.EmojiSelected;
        }

        private void Slider_ValueChanged(object sender,
                                         RoutedPropertyChangedEventArgs<double> e)
        {
            ChatWindow.FontSize = FontSlider.Value;
        }

        private void MessagesCountBox_PreviewTextInput(object sender,
                                                       TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^1-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void MessagesCountBox_PreviewExecuted(object sender,
                                                      ExecutedRoutedEventArgs e)
        {
            if (CommandIsCopyOrCutOrPaste(e))
            {
                e.Handled = true;
            }
        }

        private static bool CommandIsCopyOrCutOrPaste(ExecutedRoutedEventArgs e)
        {
            return e.Command == ApplicationCommands.Copy ||
                     e.Command == ApplicationCommands.Cut ||
                     e.Command == ApplicationCommands.Paste;
        }

        private void MessagesCountBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BtnUpdate_Click(this, null);
        }

        public readonly DispatcherTimer dispatcherTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };

        /// <summary>
        /// Actions for blinking app icon in the user's toolbar.
        /// </summary>
        private void DispatcherTimer_Tick(object sender, EventArgs e)
        {
            taskBarItem.ProgressState = IsNewMessagesState()
                ? System.Windows.Shell.TaskbarItemProgressState.None
                : System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
        }

        private bool IsNewMessagesState()
        {
            return taskBarItem.ProgressState ==
                System.Windows.Shell.TaskbarItemProgressState.Indeterminate;
        }

        private void BtnScroll_Click(object sender, RoutedEventArgs e)
        {
            if (MessageView.Items.Count > 0)
            {
                MessageView.ScrollIntoView
                    (
                        MessageView.Items[MessageView.Items.Count - 1]
                    );
            }
        }

        private void CheckBoxImage_Checked(object sender, RoutedEventArgs e)
        {
            if (ChatWindow.IsInitialized)
            {
                BtnUpdate_Click(this, null);
            }
        }

        private void CheckBoxImage_Unchecked(object sender, RoutedEventArgs e)
        {
            BtnUpdate_Click(this, null);
        }

        private void BtnCopyMessage_Click(object sender, RoutedEventArgs e)
        {
            string message = ((sender as Button).DataContext as MessageEntity).Message;
            Clipboard.SetText(message);
            _ = MessageBox.Show("Текст скопирован");
        }

        private void NoteUserButton_Click(object sender, RoutedEventArgs e)
        {
            TBoxMessage.Text += $"{GetNotedUserName(sender)},";
        }

        private static string GetNotedUserName(object sender)
        {
            return ((sender as Button).DataContext as MessageEntity).UserName;
        }
    }
}
