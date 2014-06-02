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

namespace Rejestratorka
{
    /// <summary>
    /// Interaction logic for AddPatienDialog.xaml
    /// </summary>
    public partial class AddPatientDialog : Window
    {
        DateTime? dateOfBirth;

        public AddPatientDialog()
        {
            InitializeComponent();
        }

        public string PatientName
        {
            get
            {
                return nameTextBox.Text;
            }
        }

        public string PatientSurname
        {
            get
            {
                return surnameTextBox.Text;
            }
        }

        public string PatientPesel
        {
            get
            {
                return peselTextBox.Text;
            }
        }

        public DateTime PatientDateOfBirth
        {
            get
            {
                return (DateTime)dateOfBirth;
            }
        }

        public string PatientHouseNumber
        {
            get
            {
                return houseNumberTextBox.Text;
            }
        }

        public string PatientFlatNumber
        {
            get
            {
                return flatNumberTextBox.Text;
            }
        }

        public string PatientStreet
        {
            get
            {
                return streetTextBox.Text;
            }
        }

        public string PatientPostCode
        {
            get
            {
                return postCodeTextBox.Text;
            }
        }

        public string PatientCity
        {
            get
            {
                return cityTextBox.Text;
            }
        }

        private void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            Close();
        }

        private void okButton_Click(object sender, RoutedEventArgs e)
        {
            if (isPeselValid())
            {
                this.DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show(this, "Wprowadzony PESEL jest nieprawidłowy");
            }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
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

        private void dateOfBirthTextBox_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsFormCompleted())
            {
                MessageBox.Show(peselTextBox.Text);
                okButton.IsEnabled = true;
            }
            else
            {
                okButton.IsEnabled = false;
            }
        }

        private bool IsFormCompleted()
        {
            if (nameTextBox.Text != "" && surnameTextBox.Text != "" 
                && peselTextBox.IsMaskFull == true && dateOfBirthTextBox.Text != ""
                && houseNumberTextBox.Text != "" && streetTextBox.Text != ""
                && postCodeTextBox.IsMaskFull == true && cityTextBox.Text != "")
                return true;
            return false;
        }

        private void addPatientDialog_Loaded(object sender, RoutedEventArgs e)
        {
            nameTextBox.Focus();
        }

        /// <summary>
        /// Sprawdza czy PESEL zgadza się z podaną datą urodzenia.
        /// </summary>
        /// <returns>true - gdy PESEL jest prawidłowy, w przeciwnym wypadku false.</returns> 
        private bool isPeselValid()
        {
            dateOfBirth = dateOfBirthTextBox.SelectedDate;
            if (dateOfBirth != null)
            {
                string year = dateOfBirth.Value.Year.ToString();
                year = year.Substring(year.Length - 2);
                if (year != peselTextBox.Text.Substring(0, 2))
                {
                    return false;
                }

                int monthNumer = dateOfBirth.Value.Month;
                if (dateOfBirth.Value.Year >= 2000 && dateOfBirth.Value.Year <= 2099)
                {
                    monthNumer += 20;
                }
                string month = monthNumer.ToString();
                if (month.Length < 2)
                {
                    month = "0" + month;
                }
                if (month != peselTextBox.Text.Substring(2, 2))
                {
                    return false;
                }

                string day = dateOfBirth.Value.Day.ToString();
                if (day.Length < 2)
                {
                    day = "0" + day;
                }
                if (day != peselTextBox.Text.Substring(4, 2))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
