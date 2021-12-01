﻿using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;

namespace ChatApp2
{
    public class MessageEntity
    {
        public readonly string[] imageFormats = new string[] { "png", "jpg", "gif", "bmp", "jpeg", "webp" };
        public enum Day : int
        {
            Пнд = 1,
            Втр = 2,
            Срд = 3,
            Чтв = 4,
            Птн = 5,
            Суб = 6,
            Вск = 7
        }
        public readonly string currentDirectoryStream = Directory.GetCurrentDirectory() + "\\s";
        public string UserName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string Message { get; set; }
        public bool IsFileAttached { get; set; }
        public string FileName { get; set; }

        public string GetDay
        {
            get
            {
                var intDate = (DateTime.Now.DayOfWeek == 0) ? (Day)7 : (Day)DateTime.Now.DayOfWeek; ;
                return intDate.ToString();
            }
        }

        public string GetDate
        {
            get
            {
                return Date.Replace('.', '/');
            }
        }
        public string GetMessage
        {
            get
            {
                return Message.Length > 0 ? Message : $"[{FileName}]";
            }
        }
        public BitmapImage GetImage
        {
            get
            {
                if (AppData.MainWindow.CheckBoxImage.IsChecked == false)
                    return null;
                if ((FileName != null) && imageFormats.Any(FileName.Contains))
                {
                    var lines = File.ReadLines(currentDirectoryStream);
                    foreach (var line in lines)
                    {
                        {
                            if (line.Contains(FileName))
                            {
                                var encodedStream = line.Split('\t')[1];
                                var data = Convert.FromBase64String(encodedStream);
                                var stream = new MemoryStream(data);
                                using (MemoryStream memoryStream = new MemoryStream())
                                {
                                    var imageSource = new BitmapImage();
                                    imageSource.BeginInit();
                                    imageSource.StreamSource = stream;
                                    imageSource.EndInit();

                                    // Assign the Source property of your image
                                    return imageSource;
                                }
                            }
                        }
                    }
                }
                return null;
            }
        }
    }
}