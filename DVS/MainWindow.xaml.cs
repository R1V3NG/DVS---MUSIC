using Microsoft.Win32;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
namespace MediaPlayerApp
{
    public partial class MainWindow : Window
    {
        private bool isDragging = false;
        bool isPaused = false;
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            // При открытии файла, он сразу воспроизводится
            isPaused = false;
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaElement.Source = new System.Uri(openFileDialog.FileName);
                mediaElement.LoadedBehavior = MediaState.Manual; // Устанавливаем поведение загрузки
                timer.IsEnabled = true;
<<<<<<< HEAD
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Interval = new TimeSpan(1000);
                if(!isPaused)
                {
                    mediaElement.Play();
=======
                timer.Interval = TimeSpan.FromSeconds(0.005);
                timer.Tick += Timer_Tick;
                if (!isPaused)
                {
                    mediaElement.Play();
                    bPause.Content = "Pause";
>>>>>>> Damir
                }
            }
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                sMusic.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                if (!isDragging)
                {
                    sMusic.Value = mediaElement.Position.TotalSeconds;
                }
<<<<<<< HEAD
            }

            if (mediaElement != null)
            {
                int s = (int)mediaElement.Position.TotalSeconds;
                int h = s / 3600;
                int m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);
                lStart.Content = String.Format("{0:D}:{1:D2}:{2:D2}", h, m, s);

                s = (int)sMusic.Maximum;
                h = s / 3600;
                m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);
                lEnd.Content = String.Format("{0:D}:{1:D2}:{2:D2}", h, m, s);
            }
            else
            {
                lStart.Content = "0:00:00";
                lEnd.Content = "0:00:00";
=======
                //Оставшиееся время до конца трека
                TimeSpan remainingTime = mediaElement.NaturalDuration.TimeSpan - mediaElement.Position;
                // Обновляем текстовое содержимое с оставшимся временем
                lEnd.Content = remainingTime.ToString(@"hh\:mm\:ss");
>>>>>>> Damir
            }

            if (mediaElement != null)
            {
                int s = (int)mediaElement.Position.TotalSeconds;
                int h = s / 3600;
                int m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);
                lStart.Content = mediaElement.Position.ToString(@"hh\:mm\:ss");
                /*s = (int)sMusic.Maximum;
                h = s / 3600;
                m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);*/
            }
            else
            {
                lStart.Content = "0:00:00";
                lEnd.Content = "0:00:00";
            }
<<<<<<< HEAD
        }*/

        private void sMusic_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void sMusic_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
=======
>>>>>>> Damir
            
        }

        //Перенос ползунка по точке
        private void sMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
<<<<<<< HEAD
            
        }

=======
            if (!isDragging && mediaElement.NaturalDuration.HasTimeSpan)
            {
                mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
            }
        }  
>>>>>>> Damir
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;
            if (isPaused)
            {
                mediaElement.Pause();
                bPause.Content = "Play";
            }
            else
            {
                mediaElement.Play();
                bPause.Content = "Pause";
            }
        }
<<<<<<< HEAD

=======
        // полузнок перемещается
>>>>>>> Damir
        private void sMusic_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            isDragging = true;
        }
<<<<<<< HEAD

=======
        // ползунок не перемещается
>>>>>>> Damir
        private void sMusic_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            isDragging = false;
            mediaElement.Position = TimeSpan.FromSeconds(sMusic.Value);
        }
<<<<<<< HEAD
=======
        private void MediaElement_MediaEnded(object sender, EventArgs e)
        {
            mediaElement.Stop();
            bPause.Content = "Play";
            isPaused = true;
            sMusic.Value = 0;

        }
>>>>>>> Damir
    }
}
