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
using DBClient;

namespace Administrator
{
    /// <summary>
    /// Interaction logic for AddLekarzDialog.xaml
    /// </summary>
    public partial class AddLekarzDialog : Window
    {
        private List<Sl_specData> speclist;

        public AddLekarzDialog(ref List<Sl_specData> list)
        {
            InitializeComponent();
            speclist = list;
            if (speclist != null && speclist.Count > 0) specjalizacjaBox.ItemsSource = speclist;
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

        public short kod_spec
        {
            get
            {
                return speclist[specjalizacjaBox.SelectedIndex].kod_spec;
            }
        }


        /// <summary>
        /// Metoda sprawdzająca czy formularz zawiera wszystkie niezbędne dane.
        /// </summary>
        /// <returns>true jeżeli formularz jest wypełniony prawidłowo, false w przeciwnym przypadku.</returns>
        private bool IsFormCompleted()
        {
            if (imieTextBox.Text != "" && nazwiskoTextBox.Text != ""
                && loginTextBox.Text != "" && pwdTextBox.Password != "" && DeactivateBox.Text != "" && specjalizacjaBox.SelectedIndex != -1)
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
