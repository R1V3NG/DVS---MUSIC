﻿using DVS;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using TagLib;
using System.Windows.Forms;
using System.Xml;
using Microsoft.Data.Sqlite;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Collections.Generic;
namespace MediaPlayerApp
{
    public partial class MainWindow : Window
    {
        private int currentTrackIndex = -1; // индекс песни в очереди
        private bool isDragging = false; // если нет перемещения ползунка
        private bool isPaused = true; // если нет паузы
        private readonly DispatcherTimer timer = new DispatcherTimer();
        public static string activeUser;
        List<string> directories = new List<string>();

        public ObservableCollection<MusicFile> MusicQueue { get; set; } // список очереди
        public MainWindow()
        {
            InitializeComponent();
            
            bPause.Focus();
        }
        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            // Открываем диалог для выбора папки
            //MusicQueue.Clear(); // удаляем все предыдущие элементы -> если отключить, то мы будем добавлять музыку в очередь, причём она может дублироваться
            var folderDialog = new FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenFolders(folderDialog.SelectedPath);
            }
        }

        void OpenFolders(string path)
        {
            timer.IsEnabled = true;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;

            string[] audioFiles = Directory.GetFiles(path, "*.*")
                .Where(s => s.EndsWith(".mp3")).ToArray();
            //MusicArea.Children.Clear();
            foreach (var file in audioFiles)
            {
                currentTrackIndex++;
                GetAndSetInfoMusic(file);
                MusicFile music = new MusicFile
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(file), // Получаем название файла без расширения
                    FilePath = file, // Полный путь к файлу
                    FileDirectory = System.IO.Path.GetDirectoryName(file)
                };
                MusicQueue.Add(music);
                System.Windows.Controls.Button button = new System.Windows.Controls.Button
                {
                    Content = System.IO.Path.GetFileNameWithoutExtension(file),
                    Width = 1450,
                    Height = 50,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                    Margin = new Thickness(50, 5, 50, 10),
                    Foreground = new SolidColorBrush(Colors.White),
                    FontFamily = new FontFamily("Nunito"),
                    FontSize = 18,
                    Style = (Style)FindResource("RoundButtonStyle"),
                    Tag = currentTrackIndex
                };
                button.Click += MusicMouse;
                MusicArea.Children.Add(button);
                DbConnect(music.FileDirectory, music.Title, activeUser);
            }
            // Начинаем воспроизведение первой песни, если есть файлы
            if (MusicQueue.Count > 0)
            {
                currentTrackIndex = 0; // Устанавливаем индекс на первый трек
                GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath);
            }
            if (!isPaused)
            {
                PlayImage.Source = new BitmapImage(new Uri("/pause.png", UriKind.Relative));
                mediaElement.Play();
            }
        }

        // Нажатие на кнопку песни
        private void MusicMouse(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button clickedButton = sender as System.Windows.Controls.Button;
            int trackIndex = (int)clickedButton.Tag;
            currentTrackIndex = trackIndex;
            if (clickedButton != null)
            { 
                GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath); // Воспроизводим музыку
            }
        }
        private void Pause()
        {
            if (mediaElement.HasAudio)
                isPaused = !isPaused;
            mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
            if (isPaused && mediaElement.HasAudio)
            {
                mediaElement.Pause();
                PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
            }
            if (!isPaused && mediaElement.HasAudio)
            {
                mediaElement.Play();
                PlayImage.Source = new BitmapImage(new Uri("/pause.png", UriKind.Relative));
            }
        }
        // Метод который выводит информацию и задаёт путь следующей музыки в очереди
        private void GetAndSetInfoMusic(string file)
        {
            var audioFile = TagLib.File.Create(file);
            trackTitle.Text = audioFile.Tag.Title ?? System.IO.Path.GetFileNameWithoutExtension(file);
            trackMusician.Text = string.Join(", ", audioFile.Tag.Performers);
            mediaElement.Source = new Uri(file);
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextTrack();
        }
        private void NextTrack()
        {
            if (MusicQueue.Count > 0)
            {
                currentTrackIndex++;
                // Проверяем, не вышли ли мы за пределы списка
                if (currentTrackIndex >= MusicQueue.Count && MusicQueue.Count > 0)
                {
                    currentTrackIndex = 0;
                    isPaused = true; // т.к очередь закончилась, ставим на паузу
                    PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
                    mediaElement.Stop();
                }
                if (mediaElement.HasAudio)
                    GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath);
            }

        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackTrack();
        }
        private void BackTrack()
        {
            if (currentTrackIndex > 0)
            {
                currentTrackIndex--;
                GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath);
            }
        }
        // Можно будет сделать и с файлом отдельно
        private void OpenFolder1_Click(object sender, RoutedEventArgs e)
        {
            // При открытии файла, он сразу воспроизводится
            isPaused = false;
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Media Files|*.mp3"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                mediaElement.Source = new System.Uri(openFileDialog.FileName);
                mediaElement.LoadedBehavior = MediaState.Manual; // Устанавливаем поведение загрузки
                if (!timer.IsEnabled)
                {
                    timer.IsEnabled = true;
                    timer.Interval = TimeSpan.FromMilliseconds(1);
                }
                timer.Tick += Timer_Tick;
                

                var audioFile = TagLib.File.Create(openFileDialog.FileName);
                trackTitle.Text = audioFile.Tag.Title ?? System.IO.Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                trackMusician.Text = string.Join(", ", audioFile.Tag.Performers);
                /*if (trackTitle.Text == "")
                    trackTitle.Text = "Нет названия";
                if (trackMusician.Text == "")
                    trackMusician.Text = "Нет исполнителя";*/

                if (!isPaused)
                {
                    PlayImage.Source = new BitmapImage(new Uri("/pause.png", UriKind.Relative));
                    mediaElement.Play();
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                sMusic.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                //Оставшиееся время до конца трека
                TimeSpan remainingTime = mediaElement.NaturalDuration.TimeSpan - mediaElement.Position;
                // Обновляем текстовое содержимое с оставшимся временем
                lEnd.Content = remainingTime.ToString(@"hh\:mm\:ss");
            }
            if (!isDragging)
            {
                sMusic.Value = mediaElement.Position.TotalSeconds;
            }

            if (mediaElement.HasAudio)
            {
                lStart.Content = mediaElement.Position.ToString(@"hh\:mm\:ss");
            }
            else
            {
                lStart.Content = "00:00:00";
                lEnd.Content = "00:00:00";
            }
        }
        //Перенос ползунка по точке
        private void sMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if (!isDragging && mediaElement.NaturalDuration.HasTimeSpan && 
               (((mediaElement.Position - TimeSpan.FromSeconds(args.NewValue)) > TimeSpan.FromSeconds(0.5)) || (TimeSpan.FromSeconds(args.NewValue) - mediaElement.Position > TimeSpan.FromSeconds(0.5))))
            {
                mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
            }
        }  
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            Pause();
        }
        // ползунок перемещается
        private void sMusic_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }
        // ползунок не перемещается
        private void sMusic_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
            mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
        }
        // конец песни
        private void MediaElement_MediaEnded(object sender, EventArgs e)
        {
            sMusic.Value = 0;
            NextTrack();

        }
        // условия для play с помощью стрелок
        private void bPause_PreviewKeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (!isPaused && (e.Key == Key.Left || e.Key == Key.Right))
                mediaElement.Play();
        }
        // пермещение ползунка по стрелкам
        private void bPause_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                mediaElement.Pause();
                mediaElement.Position -= TimeSpan.FromSeconds(sMusic.Maximum/ 100d);
            }
            if (e.Key == Key.Right)
            {
                mediaElement.Pause();
                mediaElement.Position += TimeSpan.FromSeconds(sMusic.Maximum / 100d);
            }
        }
        private void Up_Click(object sender, RoutedEventArgs e)
        {
            scroll.LineUp();
        }

        private void Down_Click(object sender, RoutedEventArgs e)
        {
            scroll.LineDown();
        }
        // передвижение мышки по точке и захват мыши
        private void sMusic_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                mediaElement.Pause();
                if (isPaused)
                {
                    PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
                }
                isDragging = true;
                sMusic.CaptureMouse(); // захват мыши( исправил, тем самым баг)
                Point position = e.GetPosition(sMusic); // тут есть баг, когда грузишь музыку, у нас в этот момент считывается мышка и ползунок перемещается
                double d = 1.0d / sMusic.ActualWidth * position.X;
                sMusic.Value = sMusic.Maximum * d;
            }
        }
        // Конец перемещения мыши
        private void sMusic_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragging)
            {
                sMusic.ReleaseMouseCapture();// после перетаскивания сразу отпускаю мышь
                if (!isPaused) 
                { 
                    mediaElement.Play();
                }
                mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
                isDragging = false;
            }
        }
        // пока не знаем, что будет
        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.isCorrect = false;
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this; // Устанавливаем основное окно как владельца
            loginWindow.ShowDialog();
            if(LoginWindow.isCorrect)
            {
                Close();
            }
        }

        void DbConnect(string path, string name, string user)
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();

                command.Connection = connection;
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@path", path);
                command.Parameters.AddWithValue("@user", user);


                //Добавление директории
                command.CommandText = "INSERT INTO directories (directory) SELECT @path WHERE NOT EXISTS (SELECT directory FROM directories WHERE directory = @path)";
                command.ExecuteNonQuery();

                //Добавление песни
                command.CommandText = "INSERT INTO music_names (name) SELECT @name WHERE NOT EXISTS (SELECT name FROM music_names WHERE name = @name)";
                command.ExecuteNonQuery();

                //Добавление записей в костыльную таблицу
                command.CommandText = "INSERT INTO music (name_id, directory_id) SELECT (SELECT id FROM music_names WHERE name = @name), (SELECT id FROM directories WHERE directory = @path) WHERE NOT EXISTS (SELECT music_names.id, directories.id FROM music JOIN music_names ON music.name_id = music_names.id JOIN directories ON music.directory_id = directories.id WHERE music_names.name = @name AND directories.directory = @path)";
                command.ExecuteNonQuery();

                command.CommandText = "INSERT INTO `directories-users` (directory_id, user_id) SELECT (SELECT id FROM directories WHERE directory = @path), (SELECT id FROM users WHERE login = @user) WHERE NOT EXISTS (SELECT directories.id, users.id FROM `directories-users` JOIN directories ON `directories-users`.directory_id = directories.id JOIN users ON `directories-users`.user_id = users.id WHERE directories.directory = @path AND users.login = @user)";
                command.ExecuteNonQuery();
            }
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            DbInit();

            if (directories.Count > 0 && directories != null)
            {
                MusicQueue = new ObservableCollection<MusicFile>();

                foreach (var directory in directories)
                {
                    OpenFolders(directory);
                }
            }
        }

        void DbInit()
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";
            string sqlExpression = $"SELECT directory FROM directories JOIN `directories-users` ON directories.id = `directories-users`.directory_id JOIN users ON `directories-users`.user_id = users.id WHERE users.login =\"{activeUser}\"";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand(sqlExpression, connection);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            directories.Add(reader.GetString(0));
                        }
                    }
                }
            }
        }
    }
}
