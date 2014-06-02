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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using DBClient;


namespace Rejestratorka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBClient.DBClient db;  // klient bazy danych
        public MainWindow()
        {
            InitializeComponent();
            LoginWindow loginWindow = new LoginWindow();
            if (LogIn() == true)
            {
                this.Title += " - " + loginWindow.Login;                
                db = new DBClient.DBClient();
                // --> Tworzenie listy pacjentów.
                List<string> patients = db.GetPatients();

                if (patients != null && patients.Count > 0)
                {
                    foreach (string p in patients)
                    {
                        PatientsList.Items.Add(new ComboBoxItem().Content = p);
                    }
                }
                else
                    System.Windows.MessageBox.Show("Brak pacjentów w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                // <-- Tworzenie listy pacjentów.

                // --> Tworzenie listy lekarzy.
                List<string> doctors = db.GetDoctors();

                if (doctors != null && doctors.Count > 0)
                {
                    foreach (string d in doctors)
                    {
                        DoctorsList.Items.Add(new ComboBoxItem().Content = d);
                    }
                }
                else
                    System.Windows.MessageBox.Show("Brak lekarzy w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                // <-- Tworzenie listy lekarzy.                  
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            bool? dialogResult;
            AddPatientDialog addPatientDialog = new AddPatientDialog();
            dialogResult=addPatientDialog.ShowDialog();
            if (dialogResult == true)
            {
                if (db.AddPatient(addPatientDialog.PatientName, addPatientDialog.PatientSurname,
                    addPatientDialog.PatientDateOfBirth, addPatientDialog.PatientPesel,
                    addPatientDialog.PatientHouseNumber, addPatientDialog.PatientFlatNumber,
                    addPatientDialog.PatientStreet, addPatientDialog.PatientPostCode, addPatientDialog.PatientCity))
                {
                    System.Windows.MessageBox.Show("Pacjent został dodany");
                }
                else
                {
                    System.Windows.MessageBox.Show("Nie udało się dodać pacjenta", "Błąd dodawania pacjenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            RefreshPatientsList();
        }

        private void RefreshPatientsList()
        {            
            // --> Tworzenie listy pacjentów.
            List<string> patients = db.GetPatients();
            PatientsList.Items.Clear();
            if (patients != null && patients.Count > 0)
            {
                foreach (string p in patients)
                {
                    PatientsList.Items.Add(new ComboBoxItem().Content = p);
                }
            }
            else
                System.Windows.MessageBox.Show("Brak pacjentów w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void RegisterVisit_Click(object sender, RoutedEventArgs e)
        {
            if (VisitDate.SelectedDate == null)
            {
                System.Windows.MessageBox.Show("Nie podano daty odbycia się wizyty!", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (db.AddVisit((DateTime)VisitDate.SelectedDate, (byte)(DoctorsList.SelectedIndex + 1), PatientsList.SelectedIndex + 1))
                {
                    System.Windows.MessageBox.Show("Zarejestrowano nową wizytę.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    PatientsList.SelectedIndex = -1;
                    DoctorsList.SelectedIndex = -1;
                    VisitDate.SelectedDate = null;
                }
                else
                    System.Windows.MessageBox.Show("Wystąpił błąd podczas rejestrowania wizyty i nie została ona zarejestrowana.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void PatientDetails_Collapsed(object sender, RoutedEventArgs e)
        {
            PatientName.Text = "";
            PatientPesel.Text = "";
            PatientBirthDate.Text = "";
            PatientAddress.Text = "";
        }

        private void PatientDetails_Expanded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> patientDetails = db.GetPatientDetails(PatientsList.SelectedIndex + 1);

            if (patientDetails != null)
            {
                if (patientDetails.Count > 0)
                {
                    PatientName.Text = PatientsList.SelectedValue.ToString();
                    PatientPesel.Text = patientDetails["pesel"];
                    PatientBirthDate.Text = patientDetails["dataur"];
                    PatientAddress.Text = patientDetails["adres"];

                    TabRejScrollViewer.ScrollToEnd();
                }
                else
                {
                    System.Windows.MessageBox.Show("Pacjent o podanym numerze nie istnieje.", "Nieprawidłowy nr pacjenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PatientDetails.IsExpanded = false;
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Wystąpił błąd podczas pobierania szczegółowych danych o pacjencie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                PatientDetails.IsExpanded = false;
            }
        }

        private void PatientsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatientsList.SelectedIndex > -1)
                PatientDetails.IsEnabled = true;

            if (PatientDetails.IsExpanded)
            {
                Dictionary<string, string> patientDetails = db.GetPatientDetails(PatientsList.SelectedIndex + 1);

                if (patientDetails != null)
                {
                    if (patientDetails.Count > 0)
                    {
                        PatientName.Text = PatientsList.SelectedValue.ToString();
                        PatientPesel.Text = patientDetails["pesel"];
                        PatientBirthDate.Text = patientDetails["dataur"];
                        PatientAddress.Text = patientDetails["adres"];
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Pacjent o podanym numerze nie istnieje.", "Nieprawidłowy nr pacjenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                        PatientDetails.IsExpanded = false;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Wystąpił błąd podczas pobierania szczegółowych danych o pacjencie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PatientDetails.IsExpanded = false;
                }
            }

        }

        private void registeredVisitsTab_Loaded(object sender, RoutedEventArgs e)
        {
            /*
            List<string> doctors = db.GetDoctors();

            if (doctors != null && doctors.Count > 0)
            {
                foreach (string d in doctors)
                {
                    DoctorsList2.Items.Add(new ComboBoxItem().Content = d);
                }
            }
            else
                System.Windows.MessageBox.Show("Brak lekarzy w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            // <-- Tworzenie listy lekarzy.
             */ 
        }

        private void findVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (patientNameTextBox.Text != "" && patientNameTextBox.Text != "" && DoctorsList2.SelectedItem != null)
            {
                visitsDataGrid.ItemsSource = db.GetVisits(patientNameTextBox.Text, patientSurnameTextBox.Text, DoctorsList2.SelectedItem.ToString()); ;
            }
        }

        private void cancelVisitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Data.DataRowView selectedRow = (System.Data.DataRowView)visitsDataGrid.SelectedItem;
            object[] rowValues = selectedRow.Row.ItemArray;
            string patientName = (string)rowValues[0];
            string patientSurname = (string)rowValues[1];
            string patientDateOfBirth = (string)rowValues[2];
            string patientPesel = (string)rowValues[3];
            string dateOfVisit = (string)rowValues[4];
            string doctor = (string)rowValues[5];

            if (db.DeleteVisit(patientName, patientSurname, patientDateOfBirth, patientPesel, dateOfVisit, doctor))
            {
                System.Windows.MessageBox.Show("Wizyta została anulowana");
            }
            else
            {
                System.Windows.MessageBox.Show("Nie udało się anulować wizyty", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
             
        }

        private void visitsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (visitsDataGrid.SelectedItem != null)
            {
                cancelVisitButton.IsEnabled = true;
            }
            else
            {
                cancelVisitButton.IsEnabled = false;
            }
        }

        private bool LogIn()
        {
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
                return true;
            return false;
        }

        private void logoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            //TODO: wylogowanie z bazy danych            
            if (LogIn() == true)
            {
                //TODO: ponowne zalogowanie
                this.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }
    }
}
