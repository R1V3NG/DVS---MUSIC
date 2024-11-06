using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using System.IO;

namespace DVS
{
    /// <summary>
    /// Логика взаимодействия для RegisterWindow.xaml
    /// </summary>
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(CheckTextBox())
            {
                if (CheckPassword())
                {
                    if(CheckLogin())
                    {
                        DbConnect();
                    }
                    else
                    {
                        MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка!");
                    }
                }

                else
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка!");
                }
            }
            else
            {
                MessageBox.Show("Логин и/или пароль содержат неразрешенные символы!", "Ошибка!");
            }
        }
        bool CheckTextBox()
        {
            Regex logRegex = new Regex("[^a-zA-Z0-9]");
            Regex passRegex = new Regex(@"\s");

            if(logRegex.IsMatch(tLogin.Text) || passRegex.IsMatch(tPassword.Text) || tLogin.Text == "" || tPassword.Text == "" || tCheckPassword.Text == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        bool CheckPassword()
        {
            if(tPassword.Text == tCheckPassword.Text)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        bool CheckLogin()
        {
            LoginWindow.users.Clear();
            LoginWindow.DbConnect();

            bool isCorrect = true;

            foreach(var user in LoginWindow.users)
            {
                if(user.login == tLogin.Text)
                {
                    isCorrect = false;
                }
            }
            return isCorrect;
        }

        void DbConnect()
        {
            string connectionString = $"Data Source={LoginWindow.path};Mode=ReadWrite";
            using(var connection = new SqliteConnection(connectionString))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();

                string commandText = "INSERT INTO users (login, password) VALUES (@login, @password)";
                command.Connection = connection;
                command.CommandText = commandText;
                command.Parameters.AddWithValue("@login", tLogin.Text);
                command.Parameters.AddWithValue ("@password", tPassword.Text);

                command.ExecuteNonQuery();
            }
        }
    }
}
