using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WMPLib;


namespace DVS_Player
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        bool dragStarted = false;        

        WindowsMediaPlayer wmp = new WindowsMediaPlayer();
        bool isPaused;

        private void bOpen_Click(object sender, RoutedEventArgs e)
        { 
            var ofd = new OpenFileDialog();
            if ((ofd.ShowDialog() == true) && (ofd.FileName != null))
            {
                wmp.URL = ofd.FileName;
                sMusic.IsEnabled = true;
                timer.IsEnabled = true;
                timer.Tick += new EventHandler(timerTick);
                timer.Interval = new TimeSpan(1000);
            }
            sMusic.IsMoveToPointEnabled = true;
        }


        private void timerTick(object sender, EventArgs e)
        {
            if (!dragStarted)
            {
                sMusic.Value = Convert.ToInt32(wmp.controls.currentPosition);
            }
            else
            {
                //wmp.controls.currentPosition = sMusic.Value;
            }

            sMusic.Maximum = Convert.ToInt32(wmp.currentMedia.duration);

            if (wmp != null)
            {
                int s = (int)wmp.currentMedia.duration;
                int h = s / 3600;
                int m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);
                lEnd.Content = String.Format("{0:D}:{1:D2}:{2:D2}", h, m, s);

                s = (int)wmp.controls.currentPosition;
                h = s / 3600;
                m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);
                lStart.Content = String.Format("{0:D}:{1:D2}:{2:D2}", h, m, s);
            }
            else
            {
                lStart.Content = "0:00:00";
                lEnd.Content = "0:00:00";
            }


        }

        private void sMusic_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this.dragStarted = true;
        }

        private void sMusic_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            this.dragStarted = false;
            wmp.controls.currentPosition = sMusic.Value;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            isPaused = !isPaused;

            if (isPaused)
            {
                wmp.controls.pause();
                bPause.Content = "Play";
            }
            else
            {
                wmp.controls.play();
                bPause.Content = "Pause";
            }
        }
    }
}
