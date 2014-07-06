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

namespace Administrator
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private string name;   //zawiera imię i nazwisko zalogowanego użytkownika
        private bool hardExit; //determinuje sposób zamknięcia okna (true = naciśnięto krzyżyk, false = naciśnięto "Zaloguj")


        /// <summary>
        /// Konstruktor.
        /// </summary>
        public LoginWindow()
        {
            hardExit = true;

            InitializeComponent();
            loginTextBox.Focus();
        }



        /// <summary>
        /// Właściwość zwracająca imię i nazwisko zalogowanego użytkownika.
        /// </summary>
        public string UserName
        {
            get
            {
                return name;
            }
        }



        /// <summary>
        /// Właściwość determinująca sposób zamknięcia okna.
        /// </summary>
        public bool WindowClosed
        {
            get
            {
                return hardExit;
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

            bool? userFound = db.FindUser(loginTextBox.Text, sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(passwordBox.Password)));

            hardExit = false;

            //Sprawdzanie czy w bazie istnieje podany użytkownik
            if (userFound == true)
            {
                //zapisanie imienia i nazwiska użytkownika, do użycia w oknie głównym aplikacji
                name = db.UserName;
                DialogResult = true;
            }
            else
            {
                DialogResult = false;

                if (userFound == null)
                    System.Windows.MessageBox.Show("Wystąpił błąd podczas sprawdzania poświadczeń w bazie danych.", "Błąd sprawdzania poświadczeń", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
