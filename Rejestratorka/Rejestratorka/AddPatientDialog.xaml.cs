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
    /// Logika interakcji dla klasy AddPatienDialog.xaml
    /// </summary>
    public partial class AddPatientDialog : Window
    {
        DateTime? dateOfBirth;

        /// <summary>
        /// Konstruktor
        /// </summary>
        public AddPatientDialog()
        {
            InitializeComponent();
            nameTextBox.Focus();
        }

        /// <summary>
        /// Właściwość zwracająca imię pacjenta.
        /// </summary>
        public string PatientName
        {
            get
            {
                return nameTextBox.Text;
            }
        }

        /// <summary>
        /// Właściwość zwracająca nazwisko pacjenta.
        /// </summary>
        public string PatientSurname
        {
            get
            {
                return surnameTextBox.Text;
            }
        }

        /// <summary>
        /// Właściwość zwracająca PESEL pacjenta.
        /// </summary>
        public string PatientPesel
        {
            get
            {
                return peselTextBox.Text;
            }
        }

        /// <summary>
        /// Właściwość zwracająca datę urodzenia pacjenta.
        /// </summary>
        public DateTime PatientDateOfBirth
        {
            get
            {
                return (DateTime)dateOfBirth;
            }
        }

        /// <summary>
        /// Właściwość zwracająca numer domu pacjenta.
        /// </summary>
        public string PatientHouseNumber
        {
            get
            {
                return houseNumberTextBox.Text;
            }
        }

        /// <summary>
        /// Właściwość zwracająca numer mieszkania pacjenta.
        /// </summary>
        public string PatientFlatNumber
        {
            get
            {
                return flatNumberTextBox.Text;
            }
        }

        /// <summary>
        /// Właściwość zwracająca nazwę ulicy pacjenta.
        /// </summary>
        public string PatientStreet
        {
            get
            {
                return streetTextBox.Text;
            }
        }

        /// <summary>
        /// Właściwość zwracająca kod pocztowy miasta pacjenta.
        /// </summary>
        public string PatientPostCode
        {
            get
            {
                return postCodeTextBox.Text;
            }
        }

        /// <summary>
        /// Właściwość zwracająca miasto pacjenta.
        /// </summary>
        public string PatientCity
        {
            get
            {
                return cityTextBox.Text;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Metoda sprawdzająca czy formularz dodawania nowego pacjenta zawiera wszystkie niezbędne dane.
        /// </summary>
        /// <returns>true jeżeli formularz jest wypełniony prawidłowo, false w przeciwnym przypadku.</returns>
        private bool IsFormCompleted()
        {
            if (nameTextBox.Text != "" && surnameTextBox.Text != "" 
                && peselTextBox.IsMaskFull == true && dateOfBirthTextBox.Text != ""
                && houseNumberTextBox.Text != "" && streetTextBox.Text != ""
                && postCodeTextBox.IsMaskFull == true && cityTextBox.Text != "")
                return true;
            return false;
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

                //Sprawdzenie sumy kontrolnej
                string temp;
                int control = 0;

                temp = peselTextBox.Text.Substring(0, 1);
                control += Convert.ToInt32(temp) * 1;

                temp = peselTextBox.Text.Substring(1, 1);
                control += Convert.ToInt32(temp) * 3;

                temp = peselTextBox.Text.Substring(2, 1);
                control += Convert.ToInt32(temp) * 7;

                temp = peselTextBox.Text.Substring(3, 1);
                control += Convert.ToInt32(temp) * 9;

                temp = peselTextBox.Text.Substring(4, 1);
                control += Convert.ToInt32(temp) * 1;

                temp = peselTextBox.Text.Substring(5, 1);
                control += Convert.ToInt32(temp) * 3;

                temp = peselTextBox.Text.Substring(6, 1);
                control += Convert.ToInt32(temp) * 7;

                temp = peselTextBox.Text.Substring(7, 1);
                control += Convert.ToInt32(temp) * 9;

                temp = peselTextBox.Text.Substring(8, 1);
                control += Convert.ToInt32(temp) * 1;

                temp = peselTextBox.Text.Substring(9, 1);
                control += Convert.ToInt32(temp) * 3;

                control = control % 10;
                temp = control.ToString();
                if (temp != peselTextBox.Text.Substring(10, 1))
                {
                    return false;
                }

            }
            return true;
        }
    }
}
