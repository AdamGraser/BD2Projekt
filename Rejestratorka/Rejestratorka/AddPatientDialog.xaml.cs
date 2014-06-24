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
        const int ADDR_NUM_MAX_LEN = 4;



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
        /// Właściwość zwracająca płeć pacjenta.
        /// </summary>
        public bool PatientGender
        {
            get
            {
                if (Man.IsChecked == true)
                    return false;
                else
                    return true;
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
            if (flatNumberTextBox.Text.Length > ADDR_NUM_MAX_LEN)
            {
                MessageBox.Show(this, "Wprowadzony numer mieszkania wykracza poza dopuszczalny zakres.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (houseNumberTextBox.Text.Length > ADDR_NUM_MAX_LEN)
            {
                MessageBox.Show(this, "Wprowadzony numer domu wykracza poza dopuszczalny zakres.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (isPeselValid())
            {
                this.DialogResult = true;
                Close();
            }
            else
                MessageBox.Show(this, "Wprowadzony PESEL jest nieprawidłowy!", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                okButton.IsEnabled = true;
            }
            else
            {
                if (okButton != null) //bez tego przy pierwszym uruchomieniu wyrzuca wyjątek.
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
        /// Zgodnie z logiką formularza, funkcja ta jest wywoływana tylko wtedy, gdy formularz jest w całości wypłeniony. Dlatego długość wartości pola z PESEL-em nie jest sprawdzana.
        /// </summary>
        /// <returns>true - gdy PESEL jest prawidłowy, w przeciwnym wypadku false.</returns> 
        private bool isPeselValid()
        {
            bool retval = true;
            int temp;
            
            dateOfBirth = dateOfBirthTextBox.SelectedDate;

            if (dateOfBirth != null)
            {
                temp = dateOfBirth.Value.Year % 100;
                string peselStart = "";

                if (temp < 10)
                    peselStart = "0";

                peselStart += temp.ToString();

                if (dateOfBirth.Value.Year < 1900)
                    peselStart += (dateOfBirth.Value.Month + 80).ToString();
                else if (dateOfBirth.Value.Year > 1999 && dateOfBirth.Value.Year < 2100)
                    peselStart += (dateOfBirth.Value.Month + 20).ToString();
                else if (dateOfBirth.Value.Year > 2099 && dateOfBirth.Value.Year < 2200)
                    peselStart += (dateOfBirth.Value.Month + 40).ToString();
                else if (dateOfBirth.Value.Year > 2199 && dateOfBirth.Value.Year < 2300)
                    peselStart += (dateOfBirth.Value.Month + 60).ToString();
                else
                {
                    if (dateOfBirth.Value.Month < 10)
                        peselStart += "0";

                    peselStart += dateOfBirth.Value.Month.ToString();
                }

                if(dateOfBirth.Value.Day < 10)
                    peselStart += "0";

                peselStart += dateOfBirth.Value.Day.ToString();

                if (!peselTextBox.Text.StartsWith(peselStart))
                    retval = false;
            }



            if (Woman.IsChecked == true && byte.Parse(peselTextBox.Text[9].ToString()) % 2 > 0)
            {
                retval = false;
            }
            else if (Man.IsChecked == true && byte.Parse(peselTextBox.Text[9].ToString()) % 2 == 0)
                retval = false;

            temp = int.Parse(peselTextBox.Text[0].ToString());
            temp += int.Parse(peselTextBox.Text[1].ToString()) * 3;
            temp += int.Parse(peselTextBox.Text[2].ToString()) * 7;
            temp += int.Parse(peselTextBox.Text[3].ToString()) * 9;
            temp += int.Parse(peselTextBox.Text[4].ToString());
            temp += int.Parse(peselTextBox.Text[5].ToString()) * 3;
            temp += int.Parse(peselTextBox.Text[6].ToString()) * 7;
            temp += int.Parse(peselTextBox.Text[7].ToString()) * 9;
            temp += int.Parse(peselTextBox.Text[8].ToString());
            temp += int.Parse(peselTextBox.Text[9].ToString()) * 3;
            temp += int.Parse(peselTextBox.Text[10].ToString());

            if (temp % 10 != 0)
                retval = false;

            return retval;
        }
    }
}
