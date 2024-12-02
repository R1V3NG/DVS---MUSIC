
using DVS;
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
using TagLib.Mpeg;
using System.Runtime.InteropServices.ComTypes;
using MenuItem = System.Windows.Controls.MenuItem;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using DataFormats = System.Windows.DataFormats;
namespace MediaPlayerApp
{
    public partial class MainWindow : Window
    {
        private int currentTrackIndex = -1; // индекс песни в очереди
        private int playlistTrackIndex = -1; // индекс песни в очереди
        private int MaxTrackIndex = 0;
        private int MaxPlaylistTrackIndex = 0; // Флаг для отслеживания состояния кнопки мыши
        private bool isDragging = false; // если нет перемещения ползунка
        private bool VolumeisDragging = false;
        private bool isPaused = true; // если нет паузы
        private bool isRepeat = false; // если нет повтора
        public static bool isWindowOpened = false;
        private bool isDraggingButton = false;
        public static string activeUser;
        string author;
        List<string> directories = new List<string>();
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private Button draggedButton = null;
        private Point mouseDownPosition;

        public ObservableCollection<MusicFile> MusicQueue { get; set; }// список очереди
        public ObservableCollection<Songs> PlaylistQueue { get; set; } // Очередь для плейлистов
        public ObservableCollection<Playlist> playlists { get; set; } // Плейлисты
        public ObservableCollection<Songs> songs { get; set; } // Песни
        public MainWindow()
        {
            InitializeComponent();
            bPause.Focus();
            VolumeLevel.Content = Convert.ToString(VolumeSlider.Value * 100);
            VolumeSlider.Value = VolumeSettings.LoadVolume(); // При инициализации вызываю метод класса, в котором вовзращаю сохраннённую громкость
            this.Closing += MainWindow_Closing;
            UpdateVolumeImage(); //Обновляем изображение громкости при инициализации
            CreatePlaylistButtons();
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
            MaxTrackIndex = MusicQueue.Count() - 1;
            timer.IsEnabled = true;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;

            string[] audioFiles = Directory.GetFiles(path, "*.*")
                .Where(s => s.EndsWith(".mp3")).ToArray();
            foreach (var file in audioFiles)
            {
                MaxTrackIndex++;
                GetAndSetInfoMusic(file);
                MusicFile music = new MusicFile
                {
                    Title = System.IO.Path.GetFileNameWithoutExtension(file), // Получаем название файла без расширения
                    FilePath = file, // Полный путь к файлу
                    FileDirectory = System.IO.Path.GetDirectoryName(file)
                };
                MusicQueue.Add(music);
                Button button = new Button
                {
                    Content = System.IO.Path.GetFileNameWithoutExtension(file),
                    Width = 1450,
                    Height = 45,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                    VerticalAlignment = System.Windows.VerticalAlignment.Top,
                    HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                    Margin = new Thickness(50, 5, 50, 10),
                    Foreground = new SolidColorBrush(Colors.White),
                    FontFamily = new FontFamily("Nunito"),
                    FontSize = 16,
                    Style = (Style)FindResource("SongButtonStyle"),
                    Tag = MaxTrackIndex
                };
                button.Click += MusicMouse;
                button.MouseRightButtonDown += Button_MouseRightButtonDown;
                MusicArea.Children.Add(button);
                DbConnect(music.FileDirectory, music.Title, activeUser, author);
            }
            // Начинаем воспроизведение первой песни, если есть файлы
            if (MusicQueue.Count > currentTrackIndex - 1)
            {
                currentTrackIndex = 0; // Устанавливаем индекс на первый трек
                GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath);
            }
        }
        // Нажатие на кнопку песни
        private void MusicMouse(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int trackIndex = (int)clickedButton.Tag;
            currentTrackIndex = trackIndex;
            if (clickedButton != null)
            {
                isPaused = false;
                mediaElement.Play();
                PlayImage.Source = new BitmapImage(new Uri("/pause.png", UriKind.Relative));
                GetAndSetInfoMusic(MusicQueue[trackIndex].FilePath); // Воспроизводим музыку
            }
        }
        private void Button_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Button clickedButton = sender as Button; // Получаем ссылку на нажатую кнопку
            if (clickedButton != null)
            {
                string buttonText = clickedButton.Content.ToString(); // Получаем текст кнопки
                CreateContextMenu(buttonText, (int)clickedButton.Tag); // Передаем текст в метод создания контекстного меню
            }
        }
        private void RepeatButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // Переключаем состояние повтора
            if (button.Tag == null || (bool)button.Tag == false)
            {
                // Включаем повтор
                button.Tag = true;
                isRepeat = true;
                button.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#454545")); // Подсветка включена
            }
            else
            {
                // Выключаем повтор
                button.Tag = false;
                isRepeat = false;
                button.Background = Brushes.Transparent; // Сбрасываем подсветку
            }
        }
        private void CreateContextMenu(string songName, int playlistTrackIndex)
        {
            ContextMenu contextMenu = new ContextMenu();
            {
                contextMenu.Style = (Style)FindResource("ContextMenuStyle");
            }
            // Добавляем пункт "Создать новый плейлист"
            MenuItem createPlaylistItem = new MenuItem { Header = "Создать новый плейлист" };
            createPlaylistItem.Click += (s, e) => OpenPlaylistPanel(); // Обработчик для создания нового плейлиста
            contextMenu.Items.Add(createPlaylistItem);
            // Добавляем пункт "Добавить в"
            MenuItem addToItem = new MenuItem { Header = "Добавить в" };
            foreach (var playlist in playlists)
            {
                MenuItem playlistItem = new MenuItem { Header = playlist.name };

                // Обработчик клика для добавления песни в плейлист
                playlistItem.Click += (s, e) =>
                {
                    // Выполняем добавление песни в плейлист только при выборе пункта меню
                    AddSongIntoPlaylist(songName, playlist.name, songName, Convert.ToString(playlistTrackIndex));
                };

                addToItem.Items.Add(playlistItem);
            }

            contextMenu.Items.Add(addToItem);

            // Открываем контекстное меню
            contextMenu.IsOpen = true;
        }
        // создаю кнопки плейлистов
        private void CreatePlaylistButtons()
        {
            PlaylistsArea.Children.Clear();
            // Создаем кнопки для каждого плейлиста
            foreach (var playlist in playlists)
            {
                // Создание кнопки
                Button playlistButton = new Button
                {
                    Width = 150,
                    Height = 200,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontFamily = new FontFamily("Nunito"),
                    FontSize = 16,
                    Style = (Style)FindResource("PlaylistsButtonStyle"),
                    Margin = new Thickness(20)
                };
                // Создание StackPanel для размещения изображения и текста
                StackPanel stackPanel = new StackPanel
                {
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };

                // Создание Image
                Image image = new Image
                {
                    Source = new BitmapImage(new Uri("/playlistSquare.png", UriKind.Relative)), // Убедитесь, что путь к изображению правильный
                    Stretch = Stretch.Uniform, // Настройка растяжения изображения
                    Width = 140, // Ширина изображения
                    Height = 140 // Высота изображения
                };

                // Создание TextBlock для текста
                TextBlock textBlock = new TextBlock
                {
                    Text = playlist.name,
                    Foreground = new SolidColorBrush(Colors.White),
                    FontFamily = new FontFamily("Nunito"),
                    FontSize = 16,
                    TextWrapping = TextWrapping.Wrap,
                    TextTrimming = TextTrimming.CharacterEllipsis, // Обрезка текста с троеточием
                    MaxWidth = 140, // Установка максимальной ширины для текста
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top, // Выравнивание текста вверху
                    Margin = new Thickness(0, 10, 0, 0) // Отступ сверху
                };
                // Добавление элементов в StackPanel
                stackPanel.Children.Add(image); // Добавляем изображение
                stackPanel.Children.Add(textBlock); // Добавляем текст

                // Установка содержимого кнопки
                playlistButton.Content = stackPanel;
                // Обработчик клика для кнопки плейлиста
                playlistButton.Click += (s, e) =>
                {
                    // Здесь можно добавить логику для обработки клика по кнопке
                    OpenPlaylist(playlist.name);
                };
                // Добавляем кнопку в контейнер
                PlaylistsArea.Children.Add(playlistButton);
            }
        }
        private void OpenPlaylist(string name)
        {
            PlaylistArea.Visibility = Visibility.Visible; // Открываем зону выбранного плейлиста
            SongIntoPlaylist.Visibility = Visibility.Visible; // Окрываем зону песен из плейлиста
            AddPlaylist.Visibility = Visibility.Collapsed; // Убираем кнопку добавления плейлиста
            PlaylistsArea.Visibility = Visibility.Collapsed; // Убираем зону всех доступных плейлистов
            SongIntoPlaylist.Children.Clear(); // Очищаем панель, чтобы не дублировалось
            songs.Clear(); // очищаем, чтобы не дублировалось
            PlaylistQueue.Clear(); // Чтобы у каждого плейлиста была своя очередь(чтобы не было слияние плейлистов)
            ReadSongsInPlaylist(name);
             
            MaxPlaylistTrackIndex = PlaylistQueue.Count() - 1;
            foreach (var song in songs)
            {
                MaxPlaylistTrackIndex++;
                Songs newSong = new Songs(name, song.name, song.directory, Convert.ToString(MaxPlaylistTrackIndex + 1)); // Создаем новый объект Songs
                PlaylistQueue.Add(newSong); // Добавляем в очередь плейлистов
                CreateSongButton(song.name, MaxPlaylistTrackIndex);
            }
            /*if (PlaylistQueue.Count > 0 && isPaused)
            {
                playlistTrackIndex = 0; // Устанавливаем индекс на первый трек
                GetAndSetInfoMusic(PlaylistQueue[playlistTrackIndex].directory + "\\" + PlaylistQueue[playlistTrackIndex].name + ".mp3"); // Воспроизводим музыку
            }*/
        }
        private Button CreateSongButton(string content, int index)
        {
            Button button = new Button
            {
                Content = content,
                Width = 1450,
                Height = 45,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = System.Windows.HorizontalAlignment.Left,
                Margin = new Thickness(50, 5, 50, 10),
                Foreground = new SolidColorBrush(Colors.White),
                FontFamily = new FontFamily("Nunito"),
                FontSize = 16,
                Style = (Style)FindResource("SongButtonStyle"),
                Tag = index // Устанавливаем индекс трека
            };
            button.Click += PlaylistMusicMouse; // Обработчик клика по кнопке
            button.PreviewMouseLeftButtonDown += Button_PreviewMouseLeftButtonDown;
            button.PreviewMouseMove += Button_PreviewMouseMove;
            button.PreviewMouseLeftButtonUp += Button_PreviewMouseLeftButtonUp;
            SongIntoPlaylist.Children.Add(button);

            return button;
        }
        private void Button_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            draggedButton = sender as Button;
            mouseDownPosition = e.GetPosition(SongIntoPlaylist);
            isDraggingButton = false; // Сбрасываем флаг перед началом нового перетаскивания
        }

        private void Button_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && draggedButton != null)
            {
                Point currentPosition = e.GetPosition(SongIntoPlaylist);
                Vector diff = mouseDownPosition - currentPosition;

                // Проверяем, превышает ли перемещение порог
                if (!isDraggingButton && (Math.Abs(diff.X) > 5 || Math.Abs(diff.Y) > 5))
                {
                    isDraggingButton = true;
                    DragDrop.DoDragDrop(draggedButton, draggedButton.Content, DragDropEffects.Move);
                }
            }
        }

        private void Button_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDraggingButton = false;
            draggedButton = null; // Сбрасываем ссылку на текущую перетаскиваемую кнопку
        }
        private void SongIntoPlaylist_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.StringFormat))
            {
                string songName = e.Data.GetData(DataFormats.StringFormat) as string;
                Button targetButton = GetButtonAtMousePosition(e.GetPosition(SongIntoPlaylist));

                if (targetButton != null && targetButton.Content.ToString() != songName)
                {
                    int sourceIndex = GetButtonIndexByContent(songName);
                    int targetIndex = SongIntoPlaylist.Children.IndexOf(targetButton);

                    if (sourceIndex >= 0 && targetIndex >= 0)
                    {
                        // Получаем кнопку, которую нужно переместить
                        Button buttonToMove = SongIntoPlaylist.Children[sourceIndex] as Button;

                        // Удаляем кнопку из исходного места
                        SongIntoPlaylist.Children.RemoveAt(sourceIndex);

                        // Вставляем кнопку в новое место
                        SongIntoPlaylist.Children.Insert(targetIndex, buttonToMove);
                        // Обновляем индексы для всех кнопок
                        UpdateButtonIndex();
                        ReadIndexes();
                        if (songName.ToLower().Contains(trackTitle.Text.ToLower()))// сделал contains потому что trackTitle.Text выдаёт названия из taglib
                        {
                            playlistTrackIndex = targetIndex;
                        }
                        else
                        {
                            playlistTrackIndex = GetSongIndex(trackTitle.Text); // Ставлю индекс на песню в очереди 
                        }
                    }
                }
            }
        }
        private void UpdateButtonIndex()
        {
            for (int i = 0; i < SongIntoPlaylist.Children.Count; i++)
            {
                Button button = SongIntoPlaylist.Children[i] as Button;
                if (button != null)
                {
                    button.Tag = i; // Обновляем индекс в Tag
                }
            }
            
        }
        private void ReadIndexes()
        {
            foreach (Button button in SongIntoPlaylist.Children)
            {
                foreach (var song in songs)
                {
                    if ((string)button.Content == song.name)
                    {
                        song.PlaylistTrackIndex = Convert.ToString(Convert.ToInt32(button.Tag) + 1);// новый индекс берём через tag кнопки
                    }
                }
            }
            ObservableCollection<Songs> sortSongs = new ObservableCollection<Songs>();
            for (int i = 1; i <= songs.Count; i++)
            {
                foreach (var song in songs)
                {
                    if (song.PlaylistTrackIndex == Convert.ToString(i))
                    {
                        sortSongs.Add(song);
                    }
                }
                if (sortSongs.Count == songs.Count)
                {
                    PlaylistQueue = sortSongs;
                    break;
                }
            }

            UpdatePlaylistDB();
        }

        private Button GetButtonAtMousePosition(Point position)
        {
            foreach (UIElement child in SongIntoPlaylist.Children)
            {
                if (child is Button button)
                {
                    Point buttonPosition = button.TranslatePoint(new Point(0, 0), SongIntoPlaylist);
                    if (position.X >= buttonPosition.X && position.X <= buttonPosition.X + button.ActualWidth &&
                        position.Y >= buttonPosition.Y && position.Y <= buttonPosition.Y + button.ActualHeight)
                    {
                        return button;
                    }
                }
            }
            return null;
        }

        private void SongIntoPlaylist_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }
        private int GetButtonIndexByContent(string content)
        {
            for (int i = 0; i < SongIntoPlaylist.Children.Count; i++)
            {
                if (SongIntoPlaylist.Children[i] is Button button && button.Content.ToString() == content)
                {
                    return i;
                }
            }
            return -1;
        }
        private int GetSongIndex(string content)
        {
            for (int i = 0; i < PlaylistQueue.Count; i++)
            {
                if (PlaylistQueue[i].name.ToLower().Contains(content.ToLower()))
                {
                    return i;
                }
            }
            return -1;
        }
        // Нажатие на кнопку песни из плейлиста
        private void PlaylistMusicMouse(object sender, RoutedEventArgs e)
        {
            Button clickedButton = sender as Button;
            int trackIndex = (int)clickedButton.Tag; // Получаем индекс трека из Tag кнопки
            playlistTrackIndex = trackIndex; // Обновляем текущий индекс

            if (clickedButton != null)
            {
                isPaused = false;
                mediaElement.Play();
                PlayImage.Source = new BitmapImage(new Uri("/pause.png", UriKind.Relative));
                GetAndSetInfoMusic(PlaylistQueue[trackIndex].directory + "\\" + PlaylistQueue[playlistTrackIndex].name + ".mp3");  // Воспроизводим музыку
            }
        }
        private void Pause()
        {
            if (mediaElement.Source != null)
                isPaused = !isPaused;
            mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
            if (isPaused && mediaElement.Source != null)
            {
                mediaElement.Pause();
                PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
            }
            if (!isPaused && mediaElement.Source != null)
            {
                mediaElement.Play();
                PlayImage.Source = new BitmapImage(new Uri("/pause.png", UriKind.Relative));
            }
        }
        private void Pause_Click(object sender, RoutedEventArgs e)
        {
            Pause();
        }
        // Метод который выводит информацию и задаёт путь следующей музыки в очереди
        private string GetAndSetInfoMusic(string file)
        {
            var audioFile = TagLib.File.Create(file);
            trackTitle.Text = audioFile.Tag.Title ?? System.IO.Path.GetFileNameWithoutExtension(file);
            trackMusician.Text = string.Join(", ", audioFile.Tag.Performers);
            author = trackMusician.Text;
            if (author == "")
            {
                author = "Неизвестный исполнитель";
            }
            mediaElement.Source = new Uri(file);
            lEnd.Content = audioFile.Properties.Duration.ToString(@"hh\:mm\:ss");
            LoadAlbumArt(file);
            return trackTitle.Text;
        }
        private void LoadAlbumArt(string filePath)
        {
            AlbumImage.Source = new BitmapImage(new Uri("/NoAlbums.png", UriKind.Relative));
            var file = TagLib.File.Create(filePath);
            if (file.Tag.Pictures.Length > 0)
            {
                var picture = file.Tag.Pictures[0]; // Извлекаем первое изображение
                using (var stream = new MemoryStream(picture.Data.Data))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = stream;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze(); // Замораживаем объект для потокобезопасности

                    // Устанавливаем изображение в элемент Image
                    AlbumImage.Source = bitmap;
                }
            }
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            NextTrack();
        }
        private async void TrackWithDelay()
        {
            if (SongIntoPlaylist.Visibility != Visibility.Visible)
            {
                if (MusicQueue.Count != 0)
                {
                    currentTrackIndex++;
                    // Проверяем, не вышли ли мы за пределы списка
                    if (currentTrackIndex >= MusicQueue.Count && MusicQueue.Count != 0)
                    {
                        currentTrackIndex = 0;
                        isPaused = true; // т.к очередь закончилась, ставим на паузу
                        PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
                        mediaElement.Stop();
                    }
                    if (mediaElement.Source != null)
                        GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath);
                    mediaElement.Stop();
                    await Task.Delay(700);
                    // Начинаем воспроизведение следующего трека
                    mediaElement.Play();
                }
            }
            else
            {
                if (PlaylistQueue.Count != 0)
                {
                    playlistTrackIndex++;
                    // Проверяем, не вышли ли мы за пределы списка
                    if (playlistTrackIndex >= PlaylistQueue.Count && PlaylistQueue.Count != 0)
                    {
                        playlistTrackIndex = 0;
                        isPaused = true; // т.к очередь закончилась, ставим на паузу
                        PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
                        mediaElement.Stop();
                    }
                    if (mediaElement.Source != null)
                        GetAndSetInfoMusic(PlaylistQueue[playlistTrackIndex].directory + "\\" + PlaylistQueue[playlistTrackIndex].name + ".mp3");
                    mediaElement.Stop();
                    await Task.Delay(700);
                    // Начинаем воспроизведение следующего трека
                    mediaElement.Play();
                }
            }
        }
        private void NextTrack()
        {
            if (SongIntoPlaylist.Visibility != Visibility.Visible)
            {
                if (MusicQueue.Count != 0)
                {
                    currentTrackIndex++;
                    // Проверяем, не вышли ли мы за пределы списка
                    if (currentTrackIndex >= MusicQueue.Count && MusicQueue.Count != 0)
                    {
                        currentTrackIndex = 0;
                        isPaused = true; // т.к очередь закончилась, ставим на паузу
                        PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
                        mediaElement.Stop();
                    }
                    if (mediaElement.Source != null)
                        GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath);
                }
            }
            else
            {
                if (PlaylistQueue.Count != 0)
                {
                    playlistTrackIndex++;
                    // Проверяем, не вышли ли мы за пределы списка
                    if (playlistTrackIndex >= PlaylistQueue.Count && PlaylistQueue.Count != 0)
                    {
                        playlistTrackIndex = 0;
                        isPaused = true; // т.к очередь закончилась, ставим на паузу
                        PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
                        mediaElement.Stop();
                    }
                    if (mediaElement.Source != null)
                        GetAndSetInfoMusic(PlaylistQueue[playlistTrackIndex].directory + "\\" + PlaylistQueue[playlistTrackIndex].name + ".mp3");
                }
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            BackTrack();
        }
        private void BackTrack()
        {
            if (SongIntoPlaylist.Visibility != Visibility.Visible)
            {
                if (currentTrackIndex > 0)
                {
                    currentTrackIndex--;
                    GetAndSetInfoMusic(MusicQueue[currentTrackIndex].FilePath);
                }
            }
            else
            {
                if (playlistTrackIndex > 0)
                {
                    playlistTrackIndex--;
                    GetAndSetInfoMusic(PlaylistQueue[playlistTrackIndex].directory + "\\" + PlaylistQueue[playlistTrackIndex].name + ".mp3");
                }
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
            }
        }
        // конец песни
        private void MediaElement_MediaEnded(object sender, EventArgs e)
        {
            sMusic.Value = 0;

            if (currentTrackIndex == MaxTrackIndex || playlistTrackIndex == MaxPlaylistTrackIndex)
            {
                if (!isRepeat)
                    NextTrack();
            }
            else
            { 
                if (!isRepeat)
                    TrackWithDelay(); 
            }
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
                mediaElement.Position -= TimeSpan.FromSeconds(sMusic.Maximum / 100d);
            }
            if (e.Key == Key.Right)
            {
                mediaElement.Pause();
                mediaElement.Position += TimeSpan.FromSeconds(sMusic.Maximum / 100d);
            }
        }
        private void sMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if (!isDragging && mediaElement.NaturalDuration.HasTimeSpan &&
               (((mediaElement.Position - TimeSpan.FromSeconds(args.NewValue)) > TimeSpan.FromSeconds(0.5)) || (TimeSpan.FromSeconds(args.NewValue) - mediaElement.Position > TimeSpan.FromSeconds(0.5))))
            {
                mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
            }
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
                sMusic.CaptureMouse(); // захват мыши
                Point position = e.GetPosition(sMusic); 
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
        private void VolumeButton_Click(object sender, RoutedEventArgs e)
        {
            if (VolumeArea.Visibility == Visibility.Visible)
            {
                VolumeArea.Visibility = Visibility.Hidden;
            }
            else
            {
                VolumeArea.Visibility = Visibility.Visible;
            }
        }
        private void UpdateVolumeImage()
        {
            if (Convert.ToInt16(VolumeLevel.Content) == 0)
            {
                ImageVolumeLevel.Source = new BitmapImage(new Uri("/mute.png", UriKind.Relative));
                ImageVolume.Source = new BitmapImage(new Uri("/mute.png", UriKind.Relative));
            }
            if (Convert.ToInt16(VolumeLevel.Content) > 0 && Convert.ToInt16(VolumeLevel.Content) <= 32)
            {
                ImageVolumeLevel.Source = new BitmapImage(new Uri("/LowVolume.png", UriKind.Relative));
                ImageVolume.Source = new BitmapImage(new Uri("/LowVolume.png", UriKind.Relative));
            }
            if (Convert.ToInt16(VolumeLevel.Content) > 32 && Convert.ToInt16(VolumeLevel.Content) <= 65)
            {
                ImageVolumeLevel.Source = new BitmapImage(new Uri("/AverageVolume.png", UriKind.Relative));
                ImageVolume.Source = new BitmapImage(new Uri("/AverageVolume.png", UriKind.Relative));
            }
            if (Convert.ToInt16(VolumeLevel.Content) > 65)
            {
                ImageVolumeLevel.Source = new BitmapImage(new Uri("/HighVolume.png", UriKind.Relative));
                ImageVolume.Source = new BitmapImage(new Uri("/HighVolume.png", UriKind.Relative));
            }
        }
        private void VolumeSlider_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                UpdateVolumeImage();
                VolumeisDragging = true;
                VolumeSlider.CaptureMouse(); // захват мыши
                Point position = e.GetPosition(VolumeSlider);
                double d = 1.0d / VolumeSlider.ActualWidth * position.X;
                VolumeSlider.Value = VolumeSlider.Maximum * d;
            }
        }
        private void VolumeSlider_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (VolumeisDragging)
            {
                VolumeSlider.ReleaseMouseCapture();// после перетаскивания сразу отпускаю мышь
                VolumeisDragging = false;
            }
            UpdateVolumeImage();
        }
        // передаём значение в label
        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int volumePercentage = (int)(e.NewValue * 100);
            mediaElement.Volume = e.NewValue;
            if (VolumeLevel != null)
                VolumeLevel.Content = Convert.ToString(volumePercentage);
        }
        private void RegistrationButton_Click(object sender, RoutedEventArgs e)
        {
            LoginWindow.isCorrect = false;
            LoginWindow loginWindow = new LoginWindow();
            loginWindow.Owner = this; // Устанавливаем основное окно как владельца
            loginWindow.ShowDialog();
            if (LoginWindow.isCorrect)
            {
                Close();
            }
        }
        //Меняю выделение и видимость панельки
        private void Library_Click(object sender, RoutedEventArgs e)
        {
            AddPlaylist.Visibility = Visibility.Visible;
            SetButtonSelected(Library);
            if (PlaylistArea.Visibility == Visibility.Visible)
            {
                SongIntoPlaylist.Visibility = Visibility.Collapsed;
                PlaylistsArea.Visibility = Visibility.Collapsed;
                PlaylistArea.Visibility = Visibility.Collapsed;
                MusicPlaylist.Visibility = Visibility.Collapsed;
                MusicArea.Visibility = Visibility.Visible;
                //LabelInfo.Visibility = Visibility.Visible;
                OpenFile.Visibility = Visibility.Visible;
            }

        }
        // Переключаем на плейлисты, остальные элементы скрываем или показываем
        private void Playlists_Click(object sender, RoutedEventArgs e)
        {
            AddPlaylist.Visibility = Visibility.Visible;
            SetButtonSelected(Playlists);
            if (PlaylistArea.Visibility == Visibility.Collapsed || SongIntoPlaylist.Visibility == Visibility.Visible)
            {
                PlaylistsArea.Visibility = Visibility.Visible;
                SongIntoPlaylist.Visibility = Visibility.Collapsed;
                PlaylistArea.Visibility = Visibility.Visible; // Видимость верхнего Grida
                MusicPlaylist.Visibility = Visibility.Visible; // Видимость панели с плейлистами
                MusicArea.Visibility = Visibility.Collapsed; // Другие панельки мы скрываем
                //LabelInfo.Visibility = Visibility.Collapsed; // панелька с текстом (в разработке)
                OpenFile.Visibility = Visibility.Collapsed;
            }
        }
        //Установка выделения
        private void SetButtonSelected(Button selectedButton)
        {
            // Сбросить выделение для обеих кнопок
            ResetButtonSelection();

            // Установить выделение для выбранной кнопки
            var leftStrip = (Rectangle)selectedButton.Template.FindName("LeftStrip", selectedButton);
            if (leftStrip != null)
            {
                leftStrip.Visibility = Visibility.Visible; // Показать LeftStrip
            }
        }
        // Сбросить выделение для обеих кнопок
        private void ResetButtonSelection()
        {
            // Сбросить выделение для кнопки "Библиотека"
            var libraryLeftStrip = (Rectangle)Library.Template.FindName("LeftStrip", Library);
            if (libraryLeftStrip != null)
            {
                libraryLeftStrip.Visibility = Visibility.Collapsed; // Скрыть LeftStrip
            }

            // Сбросить выделение для другой кнопки
            var PlaylistLeftStrip = (Rectangle)Playlists.Template.FindName("LeftStrip", Playlists);
            if (PlaylistLeftStrip != null)
            {
                PlaylistLeftStrip.Visibility = Visibility.Collapsed; // Скрыть LeftStrip
            }
        }
        // Создаём плейлист в базе данных и убираем панельку с вводом данных
        private void CreatePlaylistButton_Click(object sender, RoutedEventArgs e)
        {
            playlists.Add(new Playlist(tPlaylistName.Text, activeUser));
            AddNewPlaylist(tPlaylistName.Text);
            CreatePlaylistButtons();
            PlaylistName.Visibility = Visibility.Collapsed;
            tPlaylistName.Text = "Безымянный плейлист";
        }
        // Кнопка показывает и убирает панель с вводом данных
        private void AddPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (PlaylistName.Visibility == Visibility.Visible)
            {
                PlaylistName.Visibility = Visibility.Collapsed;
                tPlaylistName.Text = "Безымянный плейлист";
            }
            else { PlaylistName.Visibility = Visibility.Visible; }
        }
        //Очищаем textBox и убираем кнопку крестик
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            DownStrip.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
            tPlaylistName.Clear();
            ClearButton.Visibility = Visibility.Collapsed;
        }
        //Меняем действие с кнопкой при изменении данных
        private void tPlaylistName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ClearButton != null)
            {
                ClearButton.Visibility = string.IsNullOrWhiteSpace(tPlaylistName.Text) ? Visibility.Collapsed : Visibility.Visible;
                if (ClearButton.Visibility == Visibility.Visible)
                {
                    CreatePlaylistButton.IsEnabled = true;
                    CreatePlaylistButton.Opacity = 1;
                }
                else
                {
                    CreatePlaylistButton.IsEnabled = false;
                    CreatePlaylistButton.Opacity = 0.5;
                }
            }
        }
        // функция для изменения border DownStrip
        private void tPlaylistName_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Border downStrip = (Border)VisualTreeHelper.GetParent(NameArea);
            while (downStrip != null && !(downStrip is Border))
            {
                downStrip = (Border)VisualTreeHelper.GetParent(downStrip);
            }
            if (downStrip != null)
            {
                downStrip.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D55CFF"));
                DownStrip.Background = Brushes.Transparent;
            }
        }
        ///
        /// Повторение кнопок
        ///
        private void CreatePlaylistButton_Click_1(object sender, RoutedEventArgs e)
        {
            playlists.Add(new Playlist(tPlaylistName1.Text, activeUser));
            AddNewPlaylist(tPlaylistName1.Text);
            CreatePlaylistButtons();
            tPlaylistName1.Text = "Безымянный плейлист";
            BlackOverlay.Visibility = Visibility.Collapsed; // Скрываем затемнение
            AddPlaylistPanel.Visibility = Visibility.Collapsed;
        }
        //Очищаем textBox и убираем кнопку крестик
        private void ClearButton_Click_1(object sender, RoutedEventArgs e)
        {
            DownStrip1.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#C0C0C0"));
            tPlaylistName1.Clear();
            ClearButton1.Visibility = Visibility.Collapsed;
        }
        //Меняем действие с кнопкой при изменении данных
        private void tPlaylistName_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            if (ClearButton1 != null)
            {
                ClearButton1.Visibility = string.IsNullOrWhiteSpace(tPlaylistName1.Text) ? Visibility.Collapsed : Visibility.Visible;
                if (ClearButton1.Visibility == Visibility.Visible)
                {
                    CreatePlaylistButton1.IsEnabled = true;
                    CreatePlaylistButton1.Opacity = 1;
                }
                else
                {
                    CreatePlaylistButton1.IsEnabled = false;
                    CreatePlaylistButton1.Opacity = 0.5;
                }
            }
        }
        // функция для изменения border DownStrip
        private void tPlaylistName_PreviewMouseDown_1(object sender, MouseButtonEventArgs e)
        {
            Border downStrip = (Border)VisualTreeHelper.GetParent(NameArea1);
            while (downStrip != null && !(downStrip is Border))
            {
                downStrip = (Border)VisualTreeHelper.GetParent(downStrip);
            }
            if (downStrip != null)
            {
                downStrip.BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D55CFF"));
                DownStrip1.Background = Brushes.Transparent;
            }
        }

        private void OpenPlaylistPanel()
        {
            BlackOverlay.Visibility = Visibility.Visible; // Показываем затемнение
            AddPlaylistPanel.Visibility = Visibility.Visible; // Показываем панель
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            BlackOverlay.Visibility = Visibility.Collapsed; // Скрываем затемнение
            AddPlaylistPanel.Visibility = Visibility.Collapsed; // Скрываем панель
            tPlaylistName1.Text = "Безымянный плейлист";
        }
        private void DbConnect(string path, string name, string user, string author)
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
                command.Parameters.AddWithValue("@author", author);

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

                command.CommandText = "INSERT INTO singers (name) SELECT @author WHERE NOT EXISTS (SELECT name FROM singers WHERE name = @author)";
                command.ExecuteNonQuery();
            }
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetButtonSelected(Library);
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            isWindowOpened = true;
            playlists = new ObservableCollection<Playlist>();
            MusicQueue = new ObservableCollection<MusicFile>();
            PlaylistQueue = new ObservableCollection<Songs>();
            songs = new ObservableCollection<Songs>();
            DbInit();
            ReadPlaylists();
            if (directories.Count > 0 && directories != null)
            {
                foreach (var directory in directories)
                {
                    OpenFolders(directory);
                }
            }
        }
        // При закрытии формы запоминаю значение громкости
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            VolumeSettings.SaveVolume(VolumeSlider.Value); // Сохранение громкости
        }
        private void DbInit()
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
        // добавление плейлиста в базу данных (названия не повторяются)
        private void AddNewPlaylist(string name)
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();

                command.Connection = connection;
                command.Parameters.AddWithValue("@name", name);
                command.Parameters.AddWithValue("@user", activeUser);

                command.CommandText = "INSERT INTO playlists (name, user_id) SELECT @name, (SELECT id FROM users WHERE login=@user) WHERE NOT EXISTS (SELECT name FROM playlists WHERE name=@name)";
                command.ExecuteNonQuery();
            }
        }
        private void ReadPlaylists()
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "SELECT name FROM playlists WHERE user_id=(SELECT id FROM users WHERE login=@user)";
                command.Parameters.AddWithValue("@user", activeUser);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            playlists.Add(new Playlist(reader.GetString(0), activeUser));
                        }
                    }
                }
            }
        }
        private void AddSongIntoPlaylist(string songName, string playlistName, string directory, string playlistTrackIndex)
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();

                command.Connection = connection;
                command.Parameters.AddWithValue("@songName", songName);
                command.Parameters.AddWithValue("@playlistName", playlistName);
                command.Parameters.AddWithValue("@number_in_playlist", playlistTrackIndex);
                command.Parameters.AddWithValue("@user", activeUser);

                command.CommandText = "INSERT INTO `music-playlists` (number_in_playlist, music_id, playlist_id) SELECT (SELECT COUNT(*) + 1 FROM `music-playlists` JOIN playlists ON `music-playlists`.playlist_id=playlists.id WHERE playlists.name=@playlistName AND user_id=(SELECT id FROM users WHERE login=@user)), (SELECT music.id FROM music JOIN music_names ON music.name_id = music_names.id WHERE name=@songName), (SELECT id FROM playlists WHERE name=@playlistName)";
                command.ExecuteNonQuery();
            }
            songs.Clear();
            ReadSongsInPlaylist(playlistName);
        }
        private void ReadSongsInPlaylist(string playlistName)
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";

            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "SELECT playlists.name, music_names.name, directories.directory, 'music-playlists'.number_in_playlist FROM playlists JOIN `music-playlists` ON playlists.id = `music-playlists`.playlist_id JOIN music ON `music-playlists`.music_id = music.id JOIN music_names ON music.name_id = music_names.id JOIN directories ON music.directory_id = directories.id WHERE playlists.name = @playlistName AND playlists.user_id = (SELECT id FROM users WHERE login=@user) ORDER BY `music-playlists`.number_in_playlist";
                command.Parameters.AddWithValue("@user", activeUser);
                command.Parameters.AddWithValue("@playlistName", playlistName);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            songs.Add(new Songs(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                        }
                    }
                }
            }
        }
        private void UpdatePlaylistDB()
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                foreach (var song in songs)
                {
                    SqliteCommand command = new SqliteCommand();
                    command.Connection = connection;

                    command.CommandText = "UPDATE `music-playlists` SET number_in_playlist=@num WHERE music_id = (SELECT music.id FROM music JOIN music_names ON music.name_id = music_names.id WHERE music_names.name=@name) AND playlist_id = (SELECT id FROM playlists WHERE name=@playlist AND user_id = (SELECT id FROM users WHERE login=@user))";
                    command.Parameters.AddWithValue("@num", song.PlaylistTrackIndex);
                    command.Parameters.AddWithValue("@name", song.name);
                    command.Parameters.AddWithValue("@playlist", song.playlist);
                    command.Parameters.AddWithValue("@user", activeUser);

                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
