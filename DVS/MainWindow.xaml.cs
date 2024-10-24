using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
namespace MediaPlayerApp
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false; // если нет перемещения ползунка
        private bool isPaused = false; // если нет паузы
        private bool isOpen; // если файл открыт
        private readonly DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }

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
                isOpen = true;
                mediaElement.Source = new System.Uri(openFileDialog.FileName);
                mediaElement.LoadedBehavior = MediaState.Manual; // Устанавливаем поведение загрузки
                timer.IsEnabled = true;
                timer.Interval = TimeSpan.FromMilliseconds(0.000001);
                timer.Tick += Timer_Tick;
                
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

            if (mediaElement != null)
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
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused && isOpen)
            {
                mediaElement.Pause();
                PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
            }
            if (!isPaused && isOpen)
            {
                mediaElement.Play();
                PlayImage.Source = new BitmapImage(new Uri("/pause.png", UriKind.Relative));
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
        private void MediaElement_MediaEnded(object sender, EventArgs e)
        {
            PlayImage.Source = new BitmapImage(new Uri("/play.png", UriKind.Relative));
            mediaElement.Stop();
            isPaused = true;
            sMusic.Value = 0;

        }
    }
}
