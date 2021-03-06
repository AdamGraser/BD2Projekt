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
        DBClient.DBClient db;            // klient bazy danych
        DataTable registeredVisitsTable; // zawiera dane wyświetlane w tabeli z zarejestrowanymi wizytami.
        List<byte> doctorsIdList;
        List<int> patientsIdList;
        List<int> visitsIdList;
        List<DateTime> hoursOfVisits;        // lista godzin we wskazanym przy rejestracji wizyt dniu, na które zarejestrowane są do wybranego lekarza wizyty
        bool findVisitButtonClicked = false;



        /// <summary>
        /// Domyślny konstruktor. Inicjalizuje elementy interfejsu, klienta bazy danych oraz pola pomocnicze. Wypełnia odpowiednie elementy danymi.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();
                    GetDataFromDB();
                    FillPatientsDataGrid();
                    visitTime.TimeInterval = new TimeSpan(0, 30, 0);
                    break;
                }
            }
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Dodaj pacjenta".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addPatientButton_Click(object sender, RoutedEventArgs e)
        {
            AddPatientDialog addPatientDialog = new AddPatientDialog();
            
            bool? dialogResult = addPatientDialog.ShowDialog();

            if (dialogResult == true)
            {
                if (db.AddPatient(addPatientDialog.PatientName, addPatientDialog.PatientSurname,
                    addPatientDialog.PatientDateOfBirth, addPatientDialog.PatientPesel, addPatientDialog.PatientGender,
                    addPatientDialog.PatientHouseNumber, addPatientDialog.PatientFlatNumber,
                    addPatientDialog.PatientStreet, addPatientDialog.PatientPostCode, addPatientDialog.PatientCity))
                {
                    System.Windows.MessageBox.Show("Pacjent został pomyślnie dodany do bazy danych.", "Dodanie nowego pacjenta", MessageBoxButton.OK, MessageBoxImage.Information);
                    RefreshVisitsDataGrid(0);
                }
                else
                {
                    System.Windows.MessageBox.Show("Nie udało się dodać nowego pacjenta do bazy danych!", "Błąd dodawania pacjenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }            
        }


        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Rejestruj wizytę".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void registerVisitButton_Click(object sender, RoutedEventArgs e)
        {
            
            DateTime dateOfVisit = ((DateTime)visitDate.SelectedDate).AddHours(visitTime.Value.Value.Hour).AddMinutes(visitTime.Value.Value.Minute);

            if (db.AddVisit(dateOfVisit, doctorsIdList[doctorsList.SelectedIndex], patientsIdList[patientsDataGrid.SelectedIndex]))
            {
                System.Windows.MessageBox.Show("Zarejestrowano nową wizytę.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                patientsDataGrid.SelectedIndex = -1;                    
                doctorsList.SelectedIndex = -1;
                visitDate.SelectedDate = null;
                visitTime.Value = null;                   
                registerVisitButton.IsEnabled = false;
                }
                else
                    System.Windows.MessageBox.Show("Wystąpił błąd podczas rejestrowania wizyty i nie została ona zarejestrowana.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Szukaj" przy filtrze wyszukującym wizyty (karta "Zarejestrowane wizyty").
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findVisitButton_Click(object sender, RoutedEventArgs e)
        {
            clearFilterButton2.IsEnabled = true;
            RefreshVisitsDataGrid((byte)visitStatusComboBox.SelectedIndex);
            findVisitButtonClicked = true;
     
            //wyczyszczenie dotychczasowej zawartości tabeli:
            if (visitsDataGrid.ItemsSource != null && !registeredVisitsTable.DefaultView.Equals((DataView)visitsDataGrid.ItemsSource))
            {
                DataView src = (DataView)visitsDataGrid.ItemsSource;
                src.Dispose();
                visitsDataGrid.ItemsSource = null;
            }            

            //tworzenia filtra:
            string filterString = "";

            if (patientNameTextBox2.Text.Length != 0)
            {
                filterString += string.Format("Imię = '{0}'", patientNameTextBox2.Text);
            }
            if (patientSurnameTextBox2.Text.Length != 0)
            {
                if (patientNameTextBox2.Text.Length != 0)
                {
                    filterString += " && ";
                }
                filterString += string.Format("Nazwisko = '{0}'", patientSurnameTextBox2.Text);
            }
            if (doctorsList2.SelectedIndex > -1)
            {
                if (patientNameTextBox2.Text.Length != 0 || patientSurnameTextBox2.Text.Length != 0)
                {
                    filterString += " && ";
                }
                filterString += string.Format("Lekarz = '{0}'", doctorsList2.SelectedItem.ToString());
            }
            if (visitDate2.SelectedDate != null)
            {
                if (patientNameTextBox2.Text.Length != 0 || patientSurnameTextBox2.Text.Length != 0)
                {
                    filterString += " && ";
                }
                filterString += string.Format("[Data wizyty] LIKE '{0}%'", visitDate2.SelectedDate.Value.ToShortDateString());
            }

            DataRow[] selectedRows = registeredVisitsTable.Select(filterString);
            DataTable filteredTable = new DataTable();
            DataColumn patientNameColumn = new DataColumn("Imię", typeof(string));
            DataColumn patientSurnameColumn = new DataColumn("Nazwisko", typeof(string));
            DataColumn patientDateOfBirthColum = new DataColumn("Data urodzenia", typeof(string));
            DataColumn patientPeselColumn = new DataColumn("PESEL", typeof(string));
            DataColumn dateOfVisitColumn = new DataColumn("Data wizyty", typeof(string));
            DataColumn doctorColumn = new DataColumn("Lekarz", typeof(string));
            DataColumn statusColumn = new DataColumn("Stan wizyty", typeof(string));
            filteredTable.Columns.AddRange(new DataColumn[] {patientNameColumn, patientSurnameColumn, patientDateOfBirthColum,
                patientPeselColumn, dateOfVisitColumn, doctorColumn, statusColumn});
                
            foreach (DataRow row in selectedRows)
	        {
                filteredTable.ImportRow(row);
	        }

            visitsDataGrid.ItemsSource = filteredTable.DefaultView;
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Anuluj wizytę".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (db.CancelVisit(visitsIdList[visitsDataGrid.SelectedIndex]))
            {
                System.Windows.MessageBox.Show("Wizyta została anulowana.", "Anulowanie wizyty", MessageBoxButton.OK, MessageBoxImage.Information);
                visitsDataGrid.SelectedIndex = -1;
                visitsDataGrid.SelectedItem = null;
                RefreshVisitsDataGrid(0);
            }
            else
            {
                System.Windows.MessageBox.Show("Nie udało się anulować wizyty!", "Błąd - anulowanie wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
             
        }



        /// <summary>
        /// Metoda obsługująca zmianę zaznaczenia wiersza w tabeli zawierającej zarejestrowane wizyty.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (visitsDataGrid.SelectedItem != null && visitStatusComboBox.SelectedIndex == 0)
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
        /// Okienko logowania zwraca false tylko gdy zostanie zamknięte krzyżykiem. Ta metoda wtedy powoduje zamknięcie aplikacji.
        /// </summary>
        /// <returns>Zwraca true jeśli podano poprawne poświadczenia, w przeciwnym razie zwraca false.</returns>
        private bool LogIn()
        {
            LoginWindow loginWindow = new LoginWindow();

            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                Title += " - " + loginWindow.UserName;
                return true;
            }
            else if (loginWindow.WindowClosed) //zamknięcie okna logowania
                Environment.Exit(0);

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
            Title = "Rejestracja";
            Visibility = System.Windows.Visibility.Hidden;
            db.ResetClient();
            db.Dispose();
            db = null;

            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):            
            doctorsList.Items.Clear();
            doctorsList2.Items.Clear();
            patientNameTextBox2.Text = "";
            patientSurnameTextBox2.Text = "";

            //czyszczenie listy zarejestrowanych wizyt
            DataView src;
            if (visitsDataGrid.ItemsSource != null)
            {
                src = (DataView)visitsDataGrid.ItemsSource;
                src.Dispose();
                visitsDataGrid.ItemsSource = null;
            }

            //czyszczenie listy pacjentów
            if (patientsDataGrid.ItemsSource != null)
            {
                src = (DataView)patientsDataGrid.ItemsSource;
                src.Dispose();
                patientsDataGrid.ItemsSource = null;
            }       
            registeredVisitsTable.Dispose();
            visitDate.SelectedDate = null;
            visitDate.DisplayDate = DateTime.Today;
            visitTime.Value = null;

            while (true)
            {
                if (LogIn() == true)
                {                    
                    db = new DBClient.DBClient();
                    GetDataFromDB();
                    Visibility = System.Windows.Visibility.Visible;
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
            
            RefreshVisitsDataGrid(0);
        }



        /// <summary>
        /// Metoda pobierająca dane z bazy i inicjalizująca kontrolki odpowiedzialne za ich prezentację.
        /// </summary>
        private void GetDataFromDB()
        {
            // --> Tworzenie listy lekarzy.
            Dictionary<byte, string> doctorsDataList = db.GetDoctors();
            doctorsIdList = new List<byte>();

            if (doctorsDataList != null && doctorsDataList.Count > 0)
            {
                foreach (KeyValuePair<byte, string> doctorData in doctorsDataList)
                {
                    doctorsIdList.Add(doctorData.Key);
                    doctorsList.Items.Add(new ComboBoxItem().Content = doctorData.Value);
                    doctorsList2.Items.Add(new ComboBoxItem().Content = doctorData.Value);
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Brak lekarzy w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

                doctorsList.IsEnabled = false;
                doctorsList2.IsEnabled = false;
            }
            // <-- Tworzenie listy lekarzy.  

            //wypełnianie tabeli z wizytami:
            RefreshVisitsDataGrid(0);
        }



        /// <summary>
        /// Metoda wywoływana po kliknięciu przycisku "Wyczyść filtr".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearFilterButton2_Click(object sender, RoutedEventArgs e)
        {
            //wyczyszczenie pól filtra:
            patientNameTextBox2.Text = "";
            patientSurnameTextBox2.Text = "";
            doctorsList2.SelectedIndex = -1;
            visitStatusComboBox.SelectedIndex = 0;
            visitDate2.SelectedDate = null;
           
            //przywrócenie zawartości tabeli:
            if (registeredVisitsTable.DefaultView.Count > 0)
            {
                visitsDataGrid.ItemsSource = registeredVisitsTable.DefaultView;
            }
            clearFilterButton2.IsEnabled = false;
            findVisitButtonClicked = false;
        }



        /// <summary>
        /// Metoda obsługująca zmianę wybranego elementu w comboboksie służącym do wyboru lekarza w karcie "Zarejestrowane wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void doctorsList2_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (clearFilterButton2 != null)
                clearFilterButton2.IsEnabled = true;
        }



        /// <summary>
        /// Metoda obsługująca zmianę wybranego elementu w comboboksie służącym do wyboru lekarza w karcie "Rejestracja wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void doctorsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (doctorsList.SelectedIndex > -1)
            {
                if (visitDate.SelectedDate != null)
                {
                    //pobranie listy godzin zajętych u wybranego lekarza
                    hoursOfVisits = db.GetHoursOfVisits(doctorsIdList[doctorsList.SelectedIndex], visitDate.SelectedDate.Value);
                    hoursOfVisitsList.Items.Clear();

                    //zapobieganie wyjątkom, uproszczenie dalszej części metody, poinformowanie o wystąpieniu błędu
                    if (hoursOfVisits == null)
                    {
                        hoursOfVisits = new List<DateTime>();
                        System.Windows.MessageBox.Show("Wystąpił błąd podczas pobierania z bazy danych zajętych wizyt dla wybranego lekarza.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    //wpisanie zajętych godzin do listy obok listy lekarzy
                    foreach (DateTime hour in hoursOfVisits)
                    {
                        hoursOfVisitsList.Items.Add(new ComboBoxItem().Content = hour.Hour + ":" + (hour.Minute < 10 ? "0" + hour.Minute.ToString() : hour.Minute.ToString()));
                    }

                    if (visitTime.Value != null)
                    {
                        int i;

                        //sprawdzenie czy wskazana godzina jest już zajęta
                        for (i = 0; i < hoursOfVisits.Count; ++i)
                        {
                            if (visitTime.Value.Value.TimeOfDay >= hoursOfVisits[i].TimeOfDay && visitTime.Value.Value.TimeOfDay < hoursOfVisits[i].AddMinutes(30.0).TimeOfDay)
                            {
                                System.Windows.MessageBox.Show("Wybrana godzina wizyty jest już zajęta!", "Konflikt wizyt", MessageBoxButton.OK, MessageBoxImage.Warning);
                                break;
                            }
                        }

                        //jeśli wybrana godzina jest wolna oraz podano wszystkie inne, niezbędne informacje, to można zarejestrować wizytę
                        if (i == hoursOfVisits.Count && patientsDataGrid.SelectedIndex > -1)
                            registerVisitButton.IsEnabled = true;
                        else
                            registerVisitButton.IsEnabled = false;
                    }
                }
                else
                {
                    registerVisitButton.IsEnabled = false;
                    hoursOfVisitsList.Items.Clear();
                }
            }
            else
            {
                registerVisitButton.IsEnabled = false;
                hoursOfVisitsList.Items.Clear();
            }
        }



        /// <summary>
        /// Metoda obługująca zmianę wybranej daty w kontrolce określającej datę wizyty w karcie "Rejestracja wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            string newItem;
            
            if (visitDate.SelectedDate != null)
            {
                int? visNum;

                //zapisanie numeru obecnie wybranego lekarza (modyfikacja wybranego elementu powoduje jego odznaczenie)
                int currentDoctor = doctorsList.SelectedIndex;
                
                for (byte i = 0; i < doctorsList.Items.Count; ++i)
                {
                    //pobranie liczby wizyt zarejestrowanych dla danego lekarza na wybrany dzień
                    visNum = db.GetNumberOfVisits(doctorsIdList[i], visitDate.SelectedDate);

                    //dopisanie tej liczby obok imienia i nazwiska lekarza

                    newItem = (string)doctorsList.Items[i];

                    if (newItem[newItem.Length - 2] == ' ')
                        newItem = newItem.Substring(0, newItem.Length - 1);
                    else if (newItem[newItem.Length - 3] == ' ')
                        newItem = newItem.Substring(0, newItem.Length - 2);
                    else
                        newItem += " ";

                    newItem += (visNum != null) ? visNum.ToString() : "0";

                    doctorsList.Items[i] = new ComboBoxItem().Content = newItem;
                }

                if (doctorsList.SelectedIndex > -1)
                {
                    //pobranie listy godzin zajętych u wybranego lekarza
                    hoursOfVisits = db.GetHoursOfVisits(doctorsIdList[doctorsList.SelectedIndex], visitDate.SelectedDate.Value);
                    hoursOfVisitsList.Items.Clear();

                    //zapobieganie wyjątkom, uproszczenie dalszej części metody, poinformowanie o wystąpieniu błędu
                    if (hoursOfVisits == null)
                    {
                        hoursOfVisits = new List<DateTime>();
                        System.Windows.MessageBox.Show("Wystąpił błąd podczas pobierania z bazy danych zajętych wizyt dla wybranego lekarza.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    //wpisanie zajętych godzin do listy obok listy lekarzy
                    foreach (DateTime hour in hoursOfVisits)
                    {
                        hoursOfVisitsList.Items.Add(new ComboBoxItem().Content = hour.Hour + ":" + (hour.Minute < 10 ? "0" + hour.Minute.ToString() : hour.Minute.ToString()));
                    }

                    if (visitTime.Value != null)
                    {
                        int i;

                        //sprawdzenie czy wskazana godzina jest już zajęta
                        for (i = 0; i < hoursOfVisits.Count; ++i)
                        {
                            if (visitTime.Value.Value.TimeOfDay >= hoursOfVisits[i].TimeOfDay && visitTime.Value.Value.TimeOfDay < hoursOfVisits[i].AddMinutes(30.0).TimeOfDay)
                            {
                                System.Windows.MessageBox.Show("Wybrana godzina wizyty jest już zajęta!", "Konflikt wizyt", MessageBoxButton.OK, MessageBoxImage.Warning);
                                break;
                            }
                        }

                        //jeśli wybrana godzina jest wolna oraz podano wszystkie inne, niezbędne informacje, to można zarejestrować wizytę
                        if (i == hoursOfVisits.Count && patientsDataGrid.SelectedIndex > -1)
                            registerVisitButton.IsEnabled = true;
                        else
                            registerVisitButton.IsEnabled = false;
                    }
                }
                else
                {
                    registerVisitButton.IsEnabled = false;
                    hoursOfVisitsList.Items.Clear();
                }

                //przywrócenie na liście lekarza wybranego przed zmianami
                doctorsList.SelectedIndex = currentDoctor;
            }
            else
            {
                registerVisitButton.IsEnabled = false;
                hoursOfVisitsList.Items.Clear();

                //usunięcie liczb z listy lekarzy
                for (int i = 0; i < doctorsList.Items.Count; ++i)
                {
                    newItem = (string)doctorsList.Items[i];

                    if (newItem[newItem.Length - 2] == ' ')
                    {
                        newItem = newItem.Substring(0, newItem.Length - 2);
                        doctorsList.Items[i] = new ComboBoxItem().Content = newItem;
                    }
                    else if (newItem[newItem.Length - 3] == ' ')
                    {
                        newItem = newItem.Substring(0, newItem.Length - 3);
                        doctorsList.Items[i] = new ComboBoxItem().Content = newItem;
                    }
                }
            }           
        }



        /// <summary>
        /// Metoda obługująca zmianę wybranej godziny w kontrolce określającej czas wizyty w karcie "Rejestracja wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitTime_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (visitTime.Value != null && doctorsList.SelectedIndex > -1)
            {
                int i;

                //sprawdzenie czy wskazana godzina jest już zajęta
                for (i = 0; i < hoursOfVisits.Count; ++i)
                {
                    if (visitTime.Value.Value.TimeOfDay >= hoursOfVisits[i].TimeOfDay && visitTime.Value.Value.TimeOfDay < hoursOfVisits[i].AddMinutes(30.0).TimeOfDay)
                    {
                        System.Windows.MessageBox.Show("Wybrana godzina wizyty jest już zajęta!", "Konflikt wizyt", MessageBoxButton.OK, MessageBoxImage.Warning);
                        break;
                    }
                }

                //jeśli wybrana godzina jest wolna oraz podano wszystkie inne, niezbędne informacje, to można zarejestrować wizytę
                if (i == hoursOfVisits.Count && patientsDataGrid.SelectedIndex > -1 && visitDate.SelectedDate != null)
                    registerVisitButton.IsEnabled = true;
                else
                    registerVisitButton.IsEnabled = false;
            }
            else
            {
                registerVisitButton.IsEnabled = false;
            }
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Szukaj" przy filtrze wyszukującym pacjentów (karta "Rejestracja wizyty").
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void findPatientButton_Click(object sender, RoutedEventArgs e)
        {
            patientsDataGrid.Columns.Clear();
            if (patientsDataGrid.ItemsSource != null)
            {
                DataView src = (DataView)patientsDataGrid.ItemsSource;
                src.Dispose();
                patientsDataGrid.ItemsSource = null;
            }
            DataTable patientsTable = new DataTable();
            Dictionary<int, PatientData> patients;

            long pesel = long.Parse(peselTextBox.Text.Replace('_', '0'));
           
            if (pesel > 0)
            {
                if (peselTextBox.Text[0] == '_')
                {
                    patients = new Dictionary<int, PatientData>();
                    System.Windows.MessageBox.Show("Musisz podać pierwszą cyfrę numeru PESEL!", "Nieprawidłowy PESEL", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                    patients = db.GetPatients(patientNameTextBox1.Text, patientSurnameTextBox1.Text, pesel);
            }
            else
            {
                patients = db.GetPatients(patientNameTextBox1.Text, patientSurnameTextBox1.Text, null);
            }            

            patientsIdList = new List<int>();

            //kolumny tabeli:
            DataColumn nameColumn = new DataColumn("Imię", typeof(string));
            DataColumn surnameColumn = new DataColumn("Nazwisko", typeof(string));
            DataColumn dateOfBirthColum = new DataColumn("Data urodzenia", typeof(string));
            DataColumn peselColumn = new DataColumn("PESEL", typeof(string));
            DataColumn genderColumn = new DataColumn("Płeć", typeof(string));
            DataColumn streetColumn = new DataColumn("Ulica", typeof(string));
            DataColumn numberOfHouseColumn = new DataColumn("Numer domu", typeof(string));
            DataColumn numberOfFlatColumn = new DataColumn("Numer mieszkania", typeof(string));
            DataColumn postCodeColumn = new DataColumn("Kod pocztowy", typeof(string));
            DataColumn cityColumn = new DataColumn("Miejscowość", typeof(string));
            patientsTable.Columns.AddRange(new DataColumn[] {nameColumn, surnameColumn, dateOfBirthColum,
                    peselColumn, genderColumn, streetColumn, numberOfHouseColumn, numberOfFlatColumn,
                    postCodeColumn, cityColumn});

            //wiersze:
            foreach (KeyValuePair<int, PatientData> patientData in patients)
            {
                DataRow newRow = patientsTable.NewRow();
                newRow["Imię"] = patientData.Value.PatientName;
                newRow["Nazwisko"] = patientData.Value.PatientSurname;
                newRow["Data urodzenia"] = patientData.Value.PatientDateOfBirth;
                newRow["PESEL"] = patientData.Value.PatientPesel;
                newRow["Płeć"] = patientData.Value.PatientGender;
                newRow["Ulica"] = patientData.Value.PatientStreet;
                newRow["Numer domu"] = patientData.Value.PatientNumberOfHouse;
                newRow["Numer mieszkania"] = patientData.Value.PatientNumberOfFlat;
                newRow["Kod pocztowy"] = patientData.Value.PatientPostCode;
                newRow["Miejscowość"] = patientData.Value.PatientCity;
                patientsTable.Rows.Add(newRow);
                patientsIdList.Add(patientData.Key);
            }

            if (patientsTable.DefaultView.Count > 0)
            {
                patientsDataGrid.ItemsSource = patientsTable.DefaultView;
            }
            else
            {
                //dodawanie nagłówków do pustej tabeli:
                ClearPatientsDataGrid();
            }

        }

       

        /// <summary>
        /// Metoda czyszcząca zawartość kontrolki DataGrid zawierającej dane pacjentów (patientsDataGrid w karcie "Rejestracja wizyty").
        /// </summary>
        private void ClearPatientsDataGrid()
        {
            patientsDataGrid.Columns.Clear();
            
            if (patientsDataGrid.ItemsSource != null)
            {
                DataView src = (DataView)patientsDataGrid.ItemsSource;
                src.Dispose();
                patientsDataGrid.ItemsSource = null;
            }

            //kolumny tabeli:
            DataGridTextColumn nameColumn = new DataGridTextColumn();
            nameColumn.Header = "Imię";
            DataGridTextColumn surnameColumn = new DataGridTextColumn();
            surnameColumn.Header = "Nazwisko";
            DataGridTextColumn dateOfBirthColum = new DataGridTextColumn();
            dateOfBirthColum.Header = "Data urodzenia";
            DataGridTextColumn peselColumn = new DataGridTextColumn();
            peselColumn.Header = "PESEL";
            DataGridTextColumn genderColumn = new DataGridTextColumn();
            genderColumn.Header = "Płeć";
            DataGridTextColumn streetColumn = new DataGridTextColumn();
            streetColumn.Header = "Ulica";
            DataGridTextColumn numberOfHouseColumn = new DataGridTextColumn();
            numberOfHouseColumn.Header = "Numer domu";
            DataGridTextColumn numberOfFlatColumn = new DataGridTextColumn();
            numberOfFlatColumn.Header = "Numer mieszkania";
            DataGridTextColumn postCodeColumn = new DataGridTextColumn();
            postCodeColumn.Header = "Kod pocztowy";
            DataGridTextColumn cityColumn = new DataGridTextColumn();
            cityColumn.Header = "Miejscowość";

            patientsDataGrid.Columns.Add(nameColumn);
            patientsDataGrid.Columns.Add(surnameColumn);
            patientsDataGrid.Columns.Add(dateOfBirthColum);
            patientsDataGrid.Columns.Add(peselColumn);
            patientsDataGrid.Columns.Add(genderColumn);
            patientsDataGrid.Columns.Add(streetColumn);
            patientsDataGrid.Columns.Add(numberOfHouseColumn);
            patientsDataGrid.Columns.Add(numberOfFlatColumn);
            patientsDataGrid.Columns.Add(postCodeColumn);
            patientsDataGrid.Columns.Add(cityColumn);
        }


        /// <summary>
        /// Wypełnia tabelę w karcie "Rejestracja wizyty" danymi wszystkich pacjentów.
        /// </summary>
        void FillPatientsDataGrid()
        {
            patientsDataGrid.Columns.Clear();
            if (patientsDataGrid.ItemsSource != null)
            {
                DataView src = (DataView)patientsDataGrid.ItemsSource;
                src.Dispose();
                patientsDataGrid.ItemsSource = null;
            }
            DataTable patientsTable = new DataTable();
            Dictionary<int, PatientData> patients;



            patients = db.GetPatients("", "", null);                       
            patientsIdList = new List<int>();

            //kolumny tabeli:
            DataColumn nameColumn = new DataColumn("Imię", typeof(string));
            DataColumn surnameColumn = new DataColumn("Nazwisko", typeof(string));
            DataColumn dateOfBirthColum = new DataColumn("Data urodzenia", typeof(string));
            DataColumn peselColumn = new DataColumn("PESEL", typeof(string));
            DataColumn genderColumn = new DataColumn("Płeć", typeof(string));
            DataColumn streetColumn = new DataColumn("Ulica", typeof(string));
            DataColumn numberOfHouseColumn = new DataColumn("Numer domu", typeof(string));
            DataColumn numberOfFlatColumn = new DataColumn("Numer mieszkania", typeof(string));
            DataColumn postCodeColumn = new DataColumn("Kod pocztowy", typeof(string));
            DataColumn cityColumn = new DataColumn("Miejscowość", typeof(string));
            patientsTable.Columns.AddRange(new DataColumn[] {nameColumn, surnameColumn, dateOfBirthColum,
                    peselColumn, genderColumn, streetColumn, numberOfHouseColumn, numberOfFlatColumn,
                    postCodeColumn, cityColumn});

            //wiersze:
            foreach (KeyValuePair<int, PatientData> patientData in patients)
            {
                DataRow newRow = patientsTable.NewRow();
                newRow["Imię"] = patientData.Value.PatientName;
                newRow["Nazwisko"] = patientData.Value.PatientSurname;
                newRow["Data urodzenia"] = patientData.Value.PatientDateOfBirth;
                newRow["PESEL"] = patientData.Value.PatientPesel;
                newRow["Płeć"] = patientData.Value.PatientGender;
                newRow["Ulica"] = patientData.Value.PatientStreet;
                newRow["Numer domu"] = patientData.Value.PatientNumberOfHouse;
                newRow["Numer mieszkania"] = patientData.Value.PatientNumberOfFlat;
                newRow["Kod pocztowy"] = patientData.Value.PatientPostCode;
                newRow["Miejscowość"] = patientData.Value.PatientCity;
                patientsTable.Rows.Add(newRow);
                patientsIdList.Add(patientData.Key);
            }

            if (patientsTable.DefaultView.Count > 0)
            {
                patientsDataGrid.ItemsSource = patientsTable.DefaultView;
            }
            else
            {
                //dodawanie nagłówków do pustej tabeli:
                ClearPatientsDataGrid();
            }

        }




        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Wyczyść filtr" w karcie "Rejestracja wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearFilterButton1_Click(object sender, RoutedEventArgs e)
        {
            patientNameTextBox1.Text = "";
            patientSurnameTextBox1.Text = "";
            peselTextBox.Text = "";
            FillPatientsDataGrid();
            findPatientButton.IsEnabled = false;
        }



        /// <summary>
        /// Metoda obsługująca zmianę wyboru wiersza w tabeli zawierającej dane pacjentów w karcie "Rejestracja wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void patientsDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (registerVisitButton.IsEnabled == false)
            {
                if (patientsDataGrid.SelectedIndex > -1 && doctorsList.SelectedIndex > -1 && visitTime.Value != null)
                    registerVisitButton.IsEnabled = true;
            }
        }


                       
        /// <summary>
        /// Metoda obsługująca zmianę tekstu w polach tekstowych filtra w karcie "Rejestracja wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PatientFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (patientNameTextBox1.Text.Length > 0 || patientSurnameTextBox1.Text.Length > 0)
            {
                findPatientButton.IsEnabled = true;
                clearFilterButton1.IsEnabled = true;
            }
            else if (peselTextBox.Text == "___________")
            {
                findPatientButton.IsEnabled = false;
                clearFilterButton1.IsEnabled = false;
                FillPatientsDataGrid();
            }
        }



        /// <summary>
        /// Metoda obsługująca zmianę zawartości pola z PESEL-em w filtrze w karcie "Rejestracja wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void peselTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (findPatientButton != null && clearFilterButton1 != null)
            {
                if (peselTextBox.Text != "___________")
                {                    
                    findPatientButton.IsEnabled = true;                    
                    clearFilterButton1.IsEnabled = true;
                }
                else if (patientSurnameTextBox1.Text.Length == 0 && patientSurnameTextBox1.Text.Length == 0)
                {                   
                   findPatientButton.IsEnabled = false;                    
                   clearFilterButton1.IsEnabled = false;
                   FillPatientsDataGrid();
                }
            }
        }



        /// <summary>
        /// Metoda odświeżająca zawartość tabeli zawierającej dane dotyczące wizyt w karcie "Zarejestrowane wizyty".
        /// </summary>
        /// <param name="visitStatus"></param>
        private void RefreshVisitsDataGrid(byte visitStatus)
        {
            //wypełnianie tabeli z wizytami:
            registeredVisitsTable = new DataTable();
            Dictionary<int, VisitData> visits = db.GetVisits(visitStatus);
            visitsIdList = new List<int>();

            //kolumny tabeli:
            DataColumn patientNameColumn = new DataColumn("Imię", typeof(string));
            DataColumn patientSurnameColumn = new DataColumn("Nazwisko", typeof(string));
            DataColumn patientDateOfBirthColum = new DataColumn("Data urodzenia", typeof(string));
            DataColumn patientPeselColumn = new DataColumn("PESEL", typeof(string));
            DataColumn dateOfVisitColumn = new DataColumn("Data wizyty", typeof(string));
            DataColumn doctorColumn = new DataColumn("Lekarz", typeof(string));
            DataColumn statusColumn = new DataColumn("Stan wizyty", typeof(string));
            registeredVisitsTable.Columns.AddRange(new DataColumn[] {patientNameColumn, patientSurnameColumn, patientDateOfBirthColum,
                    patientPeselColumn, dateOfVisitColumn, doctorColumn, statusColumn});

            //wiersze:
            foreach (KeyValuePair<int, VisitData> visit in visits)
            {
                DataRow newRow = registeredVisitsTable.NewRow();
                newRow["Imię"] = visit.Value.PatientName;
                newRow["Nazwisko"] = visit.Value.PatientSurname;
                newRow["Data urodzenia"] = visit.Value.PatientDateOfBirth;
                newRow["PESEL"] = visit.Value.PatientPesel;
                newRow["Data wizyty"] = visit.Value.Date;
                newRow["Lekarz"] = visit.Value.Doctor;
                newRow["Stan wizyty"] = visit.Value.Status;
                registeredVisitsTable.Rows.Add(newRow);
                visitsIdList.Add(visit.Key);
            }

            //uprzednie czyszczenie tabeli z zarejestrowanymi wizytami
            if (visitsDataGrid.ItemsSource != null)
            {
                DataView src = (DataView)visitsDataGrid.ItemsSource;
                src.Dispose();
                visitsDataGrid.ItemsSource = null;
            }

            if (registeredVisitsTable.DefaultView.Count > 0)
            {
                visitsDataGrid.ItemsSource = registeredVisitsTable.DefaultView;
            }           
        }



        /// <summary>
        /// Metoda obsługująca zmianę tekstu w polach tekstowych filtra w karcie "Zarejestrowane wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void VisitFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (patientNameTextBox2.Text.Length > 0 || patientSurnameTextBox2.Text.Length > 0 || doctorsList2.SelectedIndex != -1)
            {
                findVisitButton.IsEnabled = true;
                clearFilterButton2.IsEnabled = true;
            }
            else if (patientNameTextBox2.Text.Length == 0 && patientSurnameTextBox2.Text.Length == 0 && doctorsList2.SelectedIndex == -1 && visitStatusComboBox.SelectedIndex == 0)
            {
                findVisitButton.IsEnabled = false;
                clearFilterButton2.IsEnabled = false;
            }
        }



        /// <summary>
        /// Metoda obsługująca zmianę wybranego stanu wizyty w comboboksie w filtrze w karcie "Zarejestrowane wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (visitStatusComboBox.SelectedIndex != 0) //0 - wartość domyślna, oznacza niezrealizowane wizyty
            {
                findVisitButton.IsEnabled = true;
                clearFilterButton2.IsEnabled = true;
            }
            else if (patientNameTextBox2.Text.Length == 0 && patientSurnameTextBox2.Text.Length == 0 && doctorsList2.SelectedIndex == -1)
            {
                if (findVisitButton != null && clearFilterButton2 != null)
                {
                    if (findVisitButtonClicked == false)
                    {
                        findVisitButton.IsEnabled = false;
                    }
                    clearFilterButton2.IsEnabled = false;
                }
            }
        }



        /// <summary>
        /// Metoda obsługująca zmianę daty w filtrze w karcie "Zarejestrowane wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitDate2_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (visitDate2.SelectedDate != null)
            {
                findVisitButton.IsEnabled = true;
                clearFilterButton2.IsEnabled = true;
            }
            else if (patientNameTextBox2.Text.Length == 0 && patientSurnameTextBox2.Text.Length == 0 && doctorsList2.SelectedIndex == -1 && visitStatusComboBox.SelectedIndex == 0)
            {
                findVisitButton.IsEnabled = false;
                clearFilterButton2.IsEnabled = false;
            }            
        }

        

        /// <summary>
        /// Metoda obsługująca klikniecie przycisku "Anuluj niedoszłe wizyty".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cancelUndoneVisitsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result;
            result = System.Windows.MessageBox.Show(this, "Czy na pewno anulować wszystkie dzisiejsze niezrealizowane wizyty", "Potwierdzenie", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes);
            if (result == MessageBoxResult.Yes)
            {
                if (db.CancelUndoneVisits() == true)
                {
                    System.Windows.MessageBox.Show(this, "Niezrealizowane wizyty zostały anulowane");
                }
                else
                {
                    System.Windows.MessageBox.Show(this, "Nie udało się anulować niezrealizowanych wizyt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

       
    }
}
