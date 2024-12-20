using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MediaPlayerApp
{
    public partial class RegistrationWindow : Window
    {
        private bool isPasswordVisible = false;

        public RegistrationWindow()
        {
            InitializeComponent();
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {

        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Синхронизация текста в PasswordBox с содержимым TextBox
            PasswordBox.Password = VisiblePasswordTextBox.Text;
        }
        private void TogglePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible; // Переключаем состояние видимости пароля
            if (isPasswordVisible)
            {
                // Если виден, скрываем PasswordBox и показываем VisiblePasswordTextBox
                VisiblePasswordTextBox.Text = PasswordBox.Password; // Копируем пароль из PasswordBox
                VisiblePasswordTextBox.Visibility = Visibility.Visible;
                PasswordBox.Visibility = Visibility.Hidden;
                VisiblePasswordTextBox.Focus();
                VisiblePasswordTextBox.SelectionStart = VisiblePasswordTextBox.Text.Length;
                ImgShowHide.Source = new BitmapImage(new Uri("/show.png", UriKind.Relative)); // pause когда виден
            }
            else
            {
                // Если скрыт, показываем PasswordBox и скрываем VisiblePasswordTextBox
                PasswordBox.Password = VisiblePasswordTextBox.Text; // Копируем пароль из TextBox
                PasswordBox.Visibility = Visibility.Visible;
                VisiblePasswordTextBox.Visibility = Visibility.Hidden;
                ImgShowHide.Source = new BitmapImage(new Uri("/hidden.png", UriKind.Relative)); // play когда не виден
            }
        }
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordBox.Password;

            // Здесь вы можете добавить логику для обработки регистрации
            MessageBox.Show($"Логин: {login}\nПароль: {password}\nЗапомнить меня: {RememberMeCheckBox.IsChecked}");
        }

    }
}