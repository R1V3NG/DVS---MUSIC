using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MediaPlayerApp;
using Microsoft.Data.Sqlite;
using System.Text.RegularExpressions;
using System.Security.Cryptography;

namespace DVS
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
        }

        bool isRememberPressed;
        bool isWasRemember;
        public static bool isCorrect = false;

        public static string path = @"audioplayer.db";

        public static List<Users> users = new List<Users>();

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Err.Visibility = Visibility.Hidden;
            users.Clear();

            if(CheckTextBox())
            {
                DbConnect();
                CheckData();
            }
            else
            {
                MessageBox.Show("Некорректный ввод данных!", "Ошибка");
            }
        }

        bool CheckTextBox()
        {
            Regex regex = new Regex(@"\s");
            if(regex.IsMatch(tLogin.Text) || regex.IsMatch(tPassword.Password) || tLogin.Text == "" || tPassword.Password == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public static void DbConnect()
        {
            string sqlExpression = "SELECT login, password, isRemember FROM users";
            string connectionString = $"Data Source={path};Mode=ReadWrite";

            using (var conn = new SqliteConnection(connectionString))
            {
                conn.Open();

                SqliteCommand command = new SqliteCommand(sqlExpression, conn);

                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            users.Add(new Users() { login = Convert.ToString(reader.GetValue(0)), password = Convert.ToString(reader.GetValue(1)), isRemember = Convert.ToBoolean(reader.GetValue(2))});
                        }
                    }
                }
            }

        }

        void DbRemember(string user)
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
                command.CommandText = "UPDATE users SET isRemember = 0";
                command.Parameters.AddWithValue("@login", user);

                command.ExecuteNonQuery();

                if (isRememberPressed)
                {
                    command.CommandText = "UPDATE users SET isRemember = 1 WHERE login = @login";
                    command.ExecuteNonQuery();
                }
            }
        }

        void CheckData()
        {
            foreach (var user in users)
            {
                

                if (user.login == tLogin.Text && user.password == RegisterWindow.HashPassword(tPassword.Password))
                {
                    MainWindow.activeUser = user.login;
                    isCorrect = true;
                    DbRemember(user.login);
                }
                if (user.isRemember && !isCorrect)
                {
                    MainWindow.activeUser = user.login;
                    isCorrect = true;
                }
            }

            if (isCorrect)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();


                Close();
            }
            else if (tLogin.Text != "" && tPassword.Password != "")
            {
                Err.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
        }

        private void RememberMeCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            isRememberPressed = true;
        }

        private void RememberMeCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            isRememberPressed = false;
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            Err.Visibility = Visibility.Hidden;
            if (!MainWindow.isWindowOpened)
            {
                DbConnect();
                CheckData();
            }
        }
    }
}
