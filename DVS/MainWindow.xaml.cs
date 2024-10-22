﻿using Microsoft.Win32;
using System;
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
        bool isPaused;
        DispatcherTimer timer = new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Media Files|*.mp3";
            if (openFileDialog.ShowDialog() == true)
            {
                mediaElement.Source = new System.Uri(openFileDialog.FileName);
                mediaElement.LoadedBehavior = MediaState.Manual; // Устанавливаем поведение загрузки
                timer.IsEnabled = true;
                timer.Tick += new EventHandler(Timer_Tick);
                timer.Interval = new TimeSpan(1000);
            }

        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaElement.NaturalDuration.HasTimeSpan)
            {
                sMusic.Maximum = mediaElement.NaturalDuration.TimeSpan.TotalSeconds;
                sMusic.Value = mediaElement.Position.TotalSeconds;
            }
        }
        
        /*private void MediaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (isDragging)
            {
                // Здесь можно обновить воспроизведение медиа в зависимости от значения слайдера
                // Например, перемотка медиа на новое время
            }
        }*/

        private void sMusic_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
             // Начало перетаскивания
        }

        private void sMusic_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            
        }
        /*private void sMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            ((Slider)sender).SelectionEnd = e.NewValue;
        }*/
        private void sMusic_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> args)
        {
   
        }

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
    }
}
