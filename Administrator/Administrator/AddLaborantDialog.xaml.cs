﻿using System;
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

namespace Administrator
{
    /// <summary>
    /// Interaction logic for AddLaborantDialog.xaml
    /// </summary>
    public partial class AddLaborantDialog : Window
    {
        public AddLaborantDialog()
        {
            InitializeComponent();
            loginTextBox.Focus();
        }

        public string login
        {
            get
            {
                return loginTextBox.Text;
            }
        }

        public string haslo
        {
            get
            {
                return pwdTextBox.Password;
            }
        }

        public DateTime? wygasa
        {
            get
            {
                return DeactivateBox.SelectedDate;
            }
        }

        public string imie
        {
            get
            {
                return imieTextBox.Text;
            }
        }

        public string nazwisko
        {
            get
            {
                return nazwiskoTextBox.Text;
            }
        }

        public bool? kier
        {
            get
            {
                return kierCheckBox.IsChecked;
            }
        }

        /// <summary>
        /// Metoda sprawdzająca czy formularz zawiera wszystkie niezbędne dane.
        /// </summary>
        /// <returns>true jeżeli formularz jest wypełniony prawidłowo, false w przeciwnym przypadku.</returns>
        private bool IsFormCompleted()
        {
            if (imieTextBox.Text != "" && nazwiskoTextBox.Text != ""
                && loginTextBox.Text != "" && pwdTextBox.Password != "" && DeactivateBox.Text != "")
                return true;
            return false;
        }

        private void BoxChanged(object sender, EventArgs e)
        {
            if (IsFormCompleted())
            {
                okButton.IsEnabled = true;
            }
            else
            {
                if (okButton != null) //bez tego przy pierwszym uruchomieniu wyrzuca wyjątek.
                    okButton.IsEnabled = false;
            }

        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Anuluj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "OK".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            Close();
        }
    }
}
