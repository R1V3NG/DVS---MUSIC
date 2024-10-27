﻿using DVS;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
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
namespace MediaPlayerApp
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false; // если нет перемещения ползунка
        private bool isPaused = false; // если нет паузы
        private readonly DispatcherTimer timer = new DispatcherTimer();
        public ObservableCollection<MusicFile> MusicQueue { get; set; }
        public MainWindow()
        {
            InitializeComponent();
            MusicQueue = new ObservableCollection<MusicFile>();
            DataContext = this;

            //mediaElement.MediaEnded += MediaElementMediaEnded;
            bPause.Focus();
        }
        /*private void MediaElementMediaEnded(object sender, RoutedEventArgs e)
        {
            PlayNext();
        }
        private void PlayNext()
        {
            if (MusicQueue.Count > 0)
            {
                //MusicQueue.RemoveAt(0); // Удаляем текущий трек
                if (MusicQueue.Count > 0)
                {
                    mediaElement.Source = new Uri(MusicQueue[0].FilePath);
                    mediaElement.Play();
                }
            }
        }*/
        /*private void UpdateQueueDisplay()
        {
            MusicArea.Children.Clear();
            foreach (var music in MusicQueue)
            {
                MusicArea.Children.Add(new System.Windows.Controls.TextBlock
                {
                    Text = music.Title,
                    Margin = new Thickness(5),
                    Foreground = new SolidColorBrush(Colors.White)
            });
            }
        }*/
            private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            // При открытии файла, он сразу воспроизводится
            isPaused = false;
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Media Files|*.mp3"
        };
            if (openFileDialog.ShowDialog() == true)
            {
                mediaElement.Source = new System.Uri(openFileDialog.FileName);
                mediaElement.LoadedBehavior = MediaState.Manual; // Устанавливаем поведение загрузки
                timer.IsEnabled = true;
                timer.Interval = TimeSpan.FromMilliseconds(1);
                timer.Tick += Timer_Tick;

                var audioFile = TagLib.File.Create(openFileDialog.FileName);
                trackTitle.Text = audioFile.Tag.Title;
                trackMusician.Text = string.Join(", ", audioFile.Tag.Performers);
                /*if (trackTitle.Text == "")
                    trackTitle.Text = "Нет названия";
                if (trackMusician.Text == "")
                    trackMusician.Text = "Нет исполнителя";*/

                /*var musicFile = new MusicFile
                {
                    Title = trackTitle.Text,
                    FilePath = openFileDialog.FileName
                };
                MusicQueue.Add(musicFile);
                UpdateQueueDisplay();*/

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

        private void Pause()
        {
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

        //Перенос ползунка по точке
        private void sMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
            if (!isDragging && mediaElement.NaturalDuration.HasTimeSpan && 
               (((mediaElement.Position - TimeSpan.FromSeconds(args.NewValue)) > TimeSpan.FromSeconds(0.5)) || (TimeSpan.FromSeconds(args.NewValue) - mediaElement.Position > TimeSpan.FromSeconds(0.5))))
            {
                mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
            }
        }  
        private void Button_Click(object sender, RoutedEventArgs e)
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
        private void MediaElement_MediaEnded(object sender, EventArgs e)
        {
            PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
            mediaElement.Stop();
            isPaused = true;
            sMusic.Value = 0;

        }
        // условия для play с помощью стрелок
        private void bPause_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (!isPaused && (e.Key == Key.Left || e.Key == Key.Right))
                mediaElement.Play();
        }
        // пермещение ползунка по стрелкам
        private void bPause_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                mediaElement.Pause();
                mediaElement.Position -= TimeSpan.FromSeconds(5);
            }
            if (e.Key == Key.Right)
            {
                mediaElement.Pause();
                mediaElement.Position += TimeSpan.FromSeconds(5);
            }
        }
        // передвижение мышки по точке и захват мыши
        private void sMusic_MouseMove(object sender, MouseEventArgs e)
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
    }
}
