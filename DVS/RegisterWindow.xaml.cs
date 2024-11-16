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
using System.Windows.Forms;
using System.Security.Cryptography;

using MessageBox = System.Windows.Forms.MessageBox;

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
                        Close();
                    }
                    else
                    {
                        MessageBox.Show("Пользователь с таким именем уже существует", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                else
                {
                    MessageBox.Show("Пароли не совпадают", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        bool CheckTextBox()
        {
            Regex logRegex = new Regex("[^a-zA-Z0-9]");
            Regex passRegex = new Regex(@"\s");
            Regex sizePassRegex = new Regex(@"\S\S\S\S\S\S");
            Regex sizeLogRegex = new Regex(@"\S\S\S");

            if(logRegex.IsMatch(tLogin.Text))
            {
                MessageBox.Show("Логин может содержать только латинские буквы и цифры!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (passRegex.IsMatch(tPassword.Password))
            {
                MessageBox.Show("Пароль не может содержать пробелы!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (tLogin.Text == "" || tPassword.Password == "" || tCheckPassword.Password == "")
            {
                MessageBox.Show("Все поля обязательны к заполнению!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!sizeLogRegex.IsMatch(tLogin.Text))
            {
                MessageBox.Show("Логин должен содержать минимум 3 символа!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (!sizePassRegex.IsMatch(tPassword.Password))
            {
                MessageBox.Show("Пароль должен содержать минимум 6 символов!", "Ошибка!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        bool CheckPassword()
        {
            if(tPassword.Password == tCheckPassword.Password)
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
                command.Parameters.AddWithValue ("@password", HashPassword(tPassword.Password));

                command.ExecuteNonQuery();
            }
        }

        public static string HashPassword(string password)
        {
            string hash;
            using(SHA1 sha1Hash = SHA1.Create())
            {
                byte[] sourceBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha1Hash.ComputeHash(sourceBytes);
                hash = BitConverter.ToString(hashBytes).Replace("-", String.Empty);
            }
            return hash;
        }
    }
}
