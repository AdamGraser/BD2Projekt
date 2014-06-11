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
using System.Security.Cryptography;

namespace Laborant
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private byte[] _hash;
        private string _login = "";



        public LoginWindow()
        {
            InitializeComponent();
            loginTextBox.Focus();
        }



        public string Login
        {
            get
            {
                return _login;
            }
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Zaloguj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LogInButton_Click(object sender, RoutedEventArgs e)
        {
            DBClient.DBClient db = new DBClient.DBClient();  // klient bazy danych
            HashAlgorithm sha = HashAlgorithm.Create("SHA512");
            byte[] passwordBytes = System.Text.Encoding.ASCII.GetBytes(passwordBox.Password);
            _hash = sha.ComputeHash(passwordBytes);
            _login = loginTextBox.Text;

            bool? userFound = db.FindUser(_login, _hash);

            //Sprawdzanie czy w bazie istnieje podany użytkownik
            if (userFound == true)
            {
                this.DialogResult = true;
            }
            else
            {
                this.DialogResult = false;

                if (userFound == null)
                    System.Windows.MessageBox.Show("Wystąpił błąd podczas sprawdzania poświadczeń w bazie danych.", "Błąd sprawdzania poświadczeń", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            //wyczyszczenie hashu (dla bezpieczeństwa)
            _hash = null;
        }



        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (loginTextBox.Text != "" && passwordBox.Password != "")
            {
                logInButton.IsEnabled = true;
            }
            else
            {
                logInButton.IsEnabled = false;
            }
        }



        private void PasswordBox_TextChanged(object sender, RoutedEventArgs e)
        {
            if (loginTextBox.Text != "" && passwordBox.Password != "")
            {
                logInButton.IsEnabled = true;
            }
            else
            {
                logInButton.IsEnabled = false;
            }
        }
    }
}
