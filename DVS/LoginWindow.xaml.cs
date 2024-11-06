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

        string activeUser;
        bool isCorrect = false;

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
            if(regex.IsMatch(tLogin.Text) || regex.IsMatch(tPassword.Text) || tLogin.Text == "" || tPassword.Text == "")
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
            string sqlExpression = "SELECT login, password FROM users";
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
                            users.Add(new Users() { login = Convert.ToString(reader.GetValue(0)), password = Convert.ToString(reader.GetValue(1)) });
                        }
                    }
                }
            }

        }

        void CheckData()
        {
            foreach (var user in users)
            {
                if (user.login == tLogin.Text && user.password == tPassword.Text)
                {
                    activeUser = user.login;
                    isCorrect = true;
                }
            }

            if (isCorrect)
            {
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();
            }
            else
            {
                Err.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            RegisterWindow registerWindow = new RegisterWindow();
            registerWindow.Show();
        }
    }
}
