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
using System.Data;


namespace Rejestratorka
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBClient.DBClient db;  // klient bazy danych
        private LoginWindow loginWindow; //okno logowania
        private DataTable registeredVisitsTable; //zawiera dane wyświetlane w tabeli z zarejestrowanymi wizytami.

        /// <summary>
        /// Domyślny konstruktor. Inicjalizuje elementy interfejsu, klienta bazy danych oraz pola pomocnicze. Wypełnia odpowiednie elementy danymi.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            loginWindow = new LoginWindow();
            while (true)
            {
                if (LogIn() == true)
                {

                    this.Title += " - " + loginWindow.Login;
                    db = new DBClient.DBClient();
                    GetDataFromDB();
                    break;
                }                
            }
        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Dodaj pacjenta".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Metoda odświeżająca zawartość combobox'a zawierającego listę pacjentów.
        /// </summary>
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

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Zarejestruj wyzytę".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RegisterVisit_Click(object sender, RoutedEventArgs e)
        {
            DateTime? timeOfVisit = visitTime.Value;
            if (VisitDate.SelectedDate == null)
            {
                System.Windows.MessageBox.Show("Nie podano daty odbycia się wizyty!", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else if (timeOfVisit == null)
            {
                System.Windows.MessageBox.Show("Nie podano godziny odbyczia się wizyty!", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                DateTime dateOfVisit = (DateTime)VisitDate.SelectedDate;
                DateTime dateToSaveInDB = new DateTime(dateOfVisit.Year, dateOfVisit.Month, dateOfVisit.Day, timeOfVisit.Value.Hour, timeOfVisit.Value.Minute, timeOfVisit.Value.Second);
                if (db.AddVisit(dateToSaveInDB, (byte)(DoctorsList.SelectedIndex + 1), PatientsList.SelectedIndex + 1))
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

        /// <summary>
        /// Metoda obsługująca zwinięcie expandera zawierającego szczegółowe dane pacjenta.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PatientDetails_Collapsed(object sender, RoutedEventArgs e)
        {
            PatientName.Text = "";
            PatientPesel.Text = "";
            PatientBirthDate.Text = "";
            PatientAddress.Text = "";
        }

        /// <summary>
        /// Metoda obsługująca rozwinięcie expandera zawierającego szczegółowe dane pacjenta.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Metoda obsługująca zaznaczenie elementu na liście pacjentów.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
       
        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Szukaj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findVisitButton_Click(object sender, RoutedEventArgs e)
        {
            clearFilterButton.IsEnabled = true; 
     
            //wyczyszczenie dotychczasowej zawartości tabeli:
            visitsDataGrid.Items.Clear();            

            //tworzenia filtra:
            string filterString = "";

            if (patientNameTextBox.Text != "")
            {
                filterString += string.Format("Imię = '{0}'", patientNameTextBox.Text);
            }
            if (patientSurnameTextBox.Text != "")
            {
                if (patientNameTextBox.Text != "")
                {
                    filterString += " && ";
                }
                filterString += string.Format("Nazwisko = '{0}'", patientSurnameTextBox.Text);
            }
            if (DoctorsList2.SelectedIndex != -1)
            {
                if (patientNameTextBox.Text != "" || patientSurnameTextBox.Text != "")
                {
                    filterString += " && ";
                }
                filterString += string.Format("Lekarz = '{0}'", DoctorsList2.SelectedItem.ToString());
            }

            DataRow[] selectedRows = registeredVisitsTable.Select(filterString);
            if (selectedRows.Count() > 0)
            {
                DataTable filteredTable = new DataTable();
                DataColumn patientNameColumn = new DataColumn("Imię", typeof(string));
                DataColumn patientSurnameColumn = new DataColumn("Nazwisko", typeof(string));
                DataColumn patientDateOfBirthColum = new DataColumn("Data urodzenia", typeof(string));
                DataColumn patientPeselColumn = new DataColumn("PESEL", typeof(string));
                DataColumn dateOfVisitColumn = new DataColumn("Data wizyty", typeof(string));
                DataColumn doctorColumn = new DataColumn("Lekarz", typeof(string));
                filteredTable.Columns.AddRange(new DataColumn[] {patientNameColumn, patientSurnameColumn, patientDateOfBirthColum,
                    patientPeselColumn, dateOfVisitColumn, doctorColumn});
                
                foreach (DataRow row in selectedRows)
	            {
	            	 filteredTable.Rows.Add(row);
	            }
                visitsDataGrid.ItemsSource = filteredTable.DefaultView;
            }
        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Anuluj wizytę".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelVisitButton_Click(object sender, RoutedEventArgs e)
        {
            System.Data.DataRowView selectedRow = (System.Data.DataRowView)visitsDataGrid.SelectedItem;
            object[] rowValues = selectedRow.Row.ItemArray;
            string patientName = (string)rowValues[0];
            string patientSurname = (string)rowValues[1];
            //string patientDateOfBirth = (string)rowValues[2];
            string patientPesel = (string)rowValues[3];
            string dateOfVisit = (string)rowValues[4];
            string doctor = (string)rowValues[5];

            if (db.DeleteVisit(patientName, patientSurname, patientPesel, dateOfVisit, (byte)DoctorsList2.SelectedIndex))
            {
                System.Windows.MessageBox.Show("Wizyta została anulowana");
            }
            else
            {
                System.Windows.MessageBox.Show("Nie udało się anulować wizyty", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
             
        }

        /// <summary>
        /// Metoda obsługująca zmianę zaznaczenia wiersza w tabeli zawierającej zarejestrowane wizyty.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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

        /// <summary>
        /// Metoda obsługująca wyświetalanie okna dialogowego odpowiedzialnego za logowanie do systemu.
        /// </summary>
        /// <returns></returns>
        private bool LogIn()
        {
            bool? result = loginWindow.ShowDialog();
            if (result == true)
                return true;
            else if (result == false) //zamknięcie okna logowania
            {
                Environment.Exit(0);
            }
            return false;
        }

        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Wyloguj się" na pasku menu.
        /// Powoduje ukrycie okna głównego i wyświetlenie okna logowania.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            db = null;
            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):
            PatientsList.Items.Clear();
            DoctorsList.Items.Clear();
            DoctorsList2.Items.Clear();
            patientNameTextBox.Text = "";
            patientSurnameTextBox.Text = "";
            visitsDataGrid.Items.Clear();
            PatientName.Text = "";
            PatientPesel.Text = "";
            PatientBirthDate.Text = "";
            PatientAddress.Text = "";
            registeredVisitsTable.Dispose();
            VisitDate.SelectedDate = null;
            VisitDate.DisplayDate = DateTime.Today;
            visitTime.Value = null;

            while (true)
            {
                if (LogIn() == true)
                {                    
                    db = new DBClient.DBClient();
                    GetDataFromDB();
                    this.Visibility = System.Windows.Visibility.Visible;
                    break;
                }                
            }
        }

        /// <summary>
        /// Metoda wywoływana po kliknięciu przycisku "O programie".
        /// Wyświetala okno dialogowe prezentujące informacje o autorach programu.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

        /// <summary>
        /// Metoda obsługująca klikniecie przycisku "Odśwież dane".
        /// Klikniecie przycisku powoduje pobranie aktualnych danych z bazy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetDataFromDB();
        }

        /// <summary>
        /// Metoda pobierająca dane z bazy i inicjalizująca kontrolki odpowiedzialne za ich prezentację.
        /// </summary>
        private void GetDataFromDB()
        {
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
                    DoctorsList2.Items.Add(new ComboBoxItem().Content = d);
                }
            }
            else
                System.Windows.MessageBox.Show("Brak lekarzy w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            // <-- Tworzenie listy lekarzy.  

            //wypełnianie tabeli z wizytami:
            registeredVisitsTable = new DataTable();
            List<VisitData> visits = db.GetVisits();

            //kolumny tabeli:
            DataColumn patientNameColumn = new DataColumn("Imię", typeof(string));
            DataColumn patientSurnameColumn = new DataColumn("Nazwisko", typeof(string));
            DataColumn patientDateOfBirthColum = new DataColumn("Data urodzenia", typeof(string));
            DataColumn patientPeselColumn = new DataColumn("PESEL", typeof(string));
            DataColumn dateOfVisitColumn = new DataColumn("Data wizyty", typeof(string));
            DataColumn doctorColumn = new DataColumn("Lekarz", typeof(string));
            registeredVisitsTable.Columns.AddRange(new DataColumn[] {patientNameColumn, patientSurnameColumn, patientDateOfBirthColum,
                    patientPeselColumn, dateOfVisitColumn, doctorColumn});

            //wiersze:
            foreach (VisitData visit in visits)
            {
                DataRow newRow = registeredVisitsTable.NewRow();
                newRow["Imię"] = visit.PatientName;
                newRow["Nazwisko"] = visit.PatientSurname;
                newRow["Data urodzenia"] = visit.Date;
                newRow["PESEL"] = visit.PatientPesel;
                newRow["Data wizyty"] = visit.Date;
                newRow["Lekarz"] = visit.Doctor;
                registeredVisitsTable.Rows.Add(newRow);
            }

            if (registeredVisitsTable.DefaultView.Count > 0)
            {
                visitsDataGrid.ItemsSource = registeredVisitsTable.DefaultView;
            }
        }

        /// <summary>
        /// Metoda wywoływana po kliknięciu przycisku "Wyczyść filtr".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            //wyczyszczenie pól filtra:
            patientNameTextBox.Text = "";
            patientSurnameTextBox.Text = "";
            DoctorsList2.SelectedIndex = -1; 
           
            //przywrócenie zawartości tabeli:
            if (registeredVisitsTable.DefaultView.Count > 0)
            {
                visitsDataGrid.ItemsSource = registeredVisitsTable.DefaultView;
            }
            clearFilterButton.IsEnabled = false;
        }

        private void DoctorsList2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            clearFilterButton.IsEnabled = true;
        }
    }
}
