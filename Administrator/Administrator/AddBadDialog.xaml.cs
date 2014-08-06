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

namespace Administrator
{
    /// <summary>
    /// Interaction logic for AddBadDialog.xaml
    /// </summary>
    public partial class AddBadDialog : Window
    {
        public AddBadDialog()
        {
            InitializeComponent();
            nazwaTextBox.Focus();
        }

        public string nazwa
        {
            get
            {
                return nazwaTextBox.Text;
            }
        }

        public string opis
        {
            get
            {
                return opisTextBox.Text;
            }
        }

        public bool? lab
        {
            get
            {
                return labCheckBox.IsChecked;
            }
        }

        

        /// <summary>
        /// Obsługa zdarzenia zmiany tekstu w okienku "Nazwa"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BoxChanged(object sender, EventArgs e)
        {
            if (nazwaTextBox.Text.Length > 0)
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
