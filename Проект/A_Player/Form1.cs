using System;
using System.Windows.Forms;
using WMPLib;

namespace A_Player
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        WindowsMediaPlayer wmp = new WindowsMediaPlayer();
        bool pausePlay;

        private void button1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            if ((ofd.ShowDialog() == DialogResult.OK) && (ofd.FileName != null))
            {
                wmp.URL = ofd.FileName;
                wmp.controls.play();
                trackBar1.Enabled = true;
                timer1.Enabled = true;
                timer1.Interval = 1000;
                label1.Text = "Playing";
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            pausePlay = !pausePlay;
            if (pausePlay)
            {
                wmp.controls.pause();
                label1.Text = "Paused";
            }
            if (!pausePlay)
            {
                wmp.controls.play();
                label1.Text = "Playing";
            }
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            wmp.controls.currentPosition = trackBar1.Value;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            trackBar1.Maximum = Convert.ToInt32(wmp.currentMedia.duration);
            trackBar1.Value = Convert.ToInt32(wmp.controls.currentPosition);

            if (wmp != null)
            {
                int s = (int)wmp.currentMedia.duration;
                int h = s / 3600;
                int m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);
                label3.Text = String.Format("{0:D}:{1:D2}:{2:D2}", h, m, s);

                s = (int)wmp.controls.currentPosition;
                h = s / 3600;
                m = (s - (h * 3600)) / 60;
                s = s - (h * 3600 + m * 60);
                label2.Text = String.Format("{0:D}:{1:D2}:{2:D2}", h, m, s);
            }
            else
            {
                label3.Text = "0:00:00";
                label2.Text = "0:00:00";
            }
        }
    }
}
