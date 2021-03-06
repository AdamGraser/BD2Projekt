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
using DBClient;

namespace Lekarz
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBClient.DBClient db;                // klient bazy danych
        List<int> visitsIdList;
        Dictionary<short, string> labTests;  // lista kodów i nazw + opisów badań laboratoryjnych
        Dictionary<short, string> phyTests;  // lista kodów i nazw + opisów badań fizykalnych
        int currentLabRow;                   // przechowuje nr wiersza listy zleconych badań laboratoryjnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co zlecone badanie
        int currentPhyRow;                   // przechowuje nr wiersza listy wykonanych badań fizykalnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co wykonane badanie
        byte currentTestId;                  // przechowuje nr badania dla bieżącej wizyty
        bool findVisitButtonClicked = false;
        int selectedVisitStatus;
        PacjentWizytyBadania dataSet;
        PacjentWizytyBadaniaTableAdapters.PacjentWizytyBadaniaTableAdapter tableAdapter;
        PacjentHistoria report;


        /// <summary>
        /// Domyślny konstruktor. Tworzy i otwiera połączenie z bazą danych.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            reportViewer.Owner = this;

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();

                    currentLabRow = 0;
                    currentPhyRow = 0;
                    currentTestId = 1;

                    visitDate.SelectedDate = DateTime.Today;

                    GetDataFromDB();

                    // Tworzenie list badań fizykalnych i laboratoryjnych
                    labTests = db.GetLabTests();

                    if (labTests != null && labTests.Count > 0)
                        LabTestsList.ItemsSource = labTests;
                    else
                    {
                        LabTestsList.IsEnabled = false;

                        LabTestDesc.Text = "Brak badań w bazie danych";
                        LabTestDesc.IsEnabled = false;

                        orderLaboratoryTestButton.IsEnabled = false;

                        if (labTests == null)
                            MessageBox.Show("Wystąpił błąd podczas pobierania listy badań laboratoryjnych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    phyTests = db.GetPhyTests();

                    if (phyTests != null && phyTests.Count > 0)
                        PhyTestsList.ItemsSource = phyTests;
                    else
                    {
                        PhyTestsList.IsEnabled = false;

                        PhyTestResult.Text = "Brak badań w bazie danych";
                        PhyTestResult.IsEnabled = false;

                        savePhysicalTestButton.IsEnabled = false;

                        if (phyTests == null)
                            MessageBox.Show("Wystąpił błąd podczas pobierania listy badań fizykalnych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    break;
                }
            }
        }


        
        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Zleć badanie lab.".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void orderLaboratoryTest_Click(object sender, RoutedEventArgs e)
        {
            if (LabTestsList.SelectedIndex > -1)
            {
                if (db.AddTest(visitsIdList[visitsList.SelectedIndex], currentTestId, DateTime.Now, LabTestDesc.Text, labTests.Keys.ElementAt(LabTestsList.SelectedIndex), true))
                {
                    Grid savedLabTest = new Grid();
                    RowDefinition row = new RowDefinition();
                    row.Height = GridLength.Auto;
                    savedLabTest.RowDefinitions.Add(row);
                    ColumnDefinition col1 = new ColumnDefinition();
                    col1.Width = new GridLength(0.4, GridUnitType.Star);
                    ColumnDefinition col2 = new ColumnDefinition();
                    col2.Width = GridLength.Auto;
                    ColumnDefinition col3 = new ColumnDefinition();
                    col3.Width = new GridLength(0.6, GridUnitType.Star);
                    savedLabTest.ColumnDefinitions.Add(col1);
                    savedLabTest.ColumnDefinitions.Add(col2);
                    savedLabTest.ColumnDefinitions.Add(col3);

                    TextBlock labTest = new TextBlock();
                    labTest.Text = labTests.ElementAt(LabTestsList.SelectedIndex).Value;
                    labTest.TextWrapping = TextWrapping.Wrap;
                    Grid.SetRow(labTest, 0);        //globalne statyczne właściwości, lol
                    Grid.SetColumn(labTest, 0);     //ponoć powinno się to robić przed dodawaniem elementu do siatki, żeby się nic nie posypało
                    savedLabTest.Children.Add(labTest); //i tak też czynię

                    TextBlock labTestTime = new TextBlock();
                    labTestTime.Text = DateTime.Now.ToString();
                    labTestTime.Margin = new Thickness(5.0, 0.0, 5.0, 0.0);
                    Grid.SetRow(labTestTime, 0);
                    Grid.SetColumn(labTestTime, 1);
                    savedLabTest.Children.Add(labTestTime);

                    TextBlock labTestDesc = new TextBlock();
                    labTestDesc.Text = LabTestDesc.Text;
                    labTestDesc.TextWrapping = TextWrapping.Wrap;
                    Grid.SetRow(labTestDesc, 0);
                    Grid.SetColumn(labTestDesc, 2);
                    savedLabTest.Children.Add(labTestDesc);

                    LaboratoryTests.Children.Insert(currentLabRow, savedLabTest);

                    ++currentTestId;
                    ++currentLabRow;

                    LabTestDesc.Clear();
                    LabTestsList.SelectedIndex = -1;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zapisu zlecenia badania laboratoryjnego i nie zostało ono zapisane.", "Błąd zapisu zlecenia", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Należy wybrać badanie laboratoryjne z listy.", "Wybierz badanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                LabTestsList.Focus();
            }
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku zapisz.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (visitDescription.Text != null && visitDescription.Text.Length > 0)
            {
                //Zapisanie wizyty, czyszczenie elementów i list, zwijanie i wyłączanie expander'ów.
                if (db.SaveVisit(visitsIdList[visitsList.SelectedIndex], visitDescription.Text, diagnosis.Text))
                {
                    findVisitButton.IsEnabled = true;
                    todayButton.IsEnabled = true;

                    data_rej.Text = nazwa_pac.Text = "";

                    visitDescription.Clear();
                    diagnosis.Clear();

                    LabTestDesc.Clear();
                    LabTestsList.SelectedIndex = -1;

                    PhyTestResult.Clear();
                    PhyTestsList.SelectedIndex = -1;

                    while (LaboratoryTests.Children.Count > 2)
                        LaboratoryTests.Children.RemoveAt(0);

                    while (PhysicalTests.Children.Count > 2)
                        PhysicalTests.Children.RemoveAt(0);

                    currentLabRow = 0;
                    currentPhyRow = 0;
                    currentTestId = 1;

                    diagnosisExpander.IsEnabled = false;
                    diagnosisExpander.IsExpanded = false;
                    laboratoryTestsExpander.IsEnabled = false;
                    laboratoryTestsExpander.IsExpanded = false;
                    physicalTestsExpander.IsEnabled = false;
                    physicalTestsExpander.IsExpanded = false;
                    visitsHistory.IsEnabled = false;
                    VisitExpander.IsEnabled = false;
                    VisitExpander.IsExpanded = false;

                    visitsIdList.RemoveAt(visitsList.SelectedIndex);
                    visitsList.Items.RemoveAt(visitsList.SelectedIndex);
                    visitsList.SelectedIndex = -1;
                    cancelVisitButton.IsEnabled = false;
                    saveVisitButton.IsEnabled = false;
                    RapTab.IsEnabled = false;

                    if (visitsList.Items.Count == 0)
                    {
                        visitsList.Items.Add(new ListBoxItem().Content = "Brak wizyt!");
                    }
                    else
                    {
                        clearFilterButton.IsEnabled = true;
                        visitsList.IsEnabled = true;
                    }
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zapisu szczegółów wizyty i nie zostały one zapisane.", "Błąd aktualizacji wizyty", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Przed zakończeniem wizyty należy podać jej opis.", "Brak opisu wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
                visitDescription.Focus();
            }
        }



        /// <summary>
        /// Metoda obsługująca zmianę zaznaczenia na liście wizyt.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (visitsList.SelectedIndex > -1)
            {
                if (selectedVisitStatus == 0)
                {
                    beginVisitButton.IsEnabled = true;
                    beginVisitButton.Content = "Rozpocznij wizytę";
                }
                else
                {
                    if (selectedVisitStatus == 1)
                    {
                        beginVisitButton.IsEnabled = true;
                        beginVisitButton.Content = "Kontynuuj wizytę";
                    }
                    else
                    {
                        beginVisitButton.IsEnabled = false;

                        visitDescription.IsEnabled = false;
                        visitDescription.Text = db.GetVisitDescription(visitsIdList[visitsList.SelectedIndex]);
                        VisitExpander.IsEnabled = true;
                        VisitExpander.IsExpanded = true;

                        diagnosis.IsEnabled = false;
                        diagnosis.Text = db.GetVisitDiagnosis(visitsIdList[visitsList.SelectedIndex]);
                        diagnosisExpander.IsEnabled = true;

                        laboratoryTestsExpander.IsEnabled = false;
                        physicalTestsExpander.IsEnabled = false;
                    }
                }

                ListBoxItem item = (ListBoxItem)visitsList.SelectedItem;
                string temp = (string)item.Content;

                string[] visit = temp.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

                data_rej.Text = visit[0] + " " + visit[1];
                nazwa_pac.Text = visit[2];
            }
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Wyloguj się" na pasku menu.
        /// Powoduje ukrycie okna głównego i wyświetlenie okna logowania.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            //przycisk "Zakończ wizytę" jest aktywny tylko wtedy, gdy jakaś wizyta się odbywa
            if (saveVisitButton.IsEnabled == true)
                MessageBox.Show("Nie można wylogować się w trakcie odbywania się wizyty.", "Wizyta w toku", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
            {
                Title = "Lekarz";
                Visibility = System.Windows.Visibility.Hidden;
                db.ResetClient();
                db.Dispose();
                db = null;

                //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):
                visitsList.Items.Clear();
                data_rej.Text = "";
                nazwa_pac.Text = "";
                visitDescription.Text = "";
                diagnosis.Text = "";
                LabTestsList.SelectedIndex = -1;
                LabTestDesc.Text = "";
                PhyTestResult.Text = "";

                diagnosisExpander.IsEnabled = false;
                diagnosisExpander.IsExpanded = false;
                laboratoryTestsExpander.IsEnabled = false;
                laboratoryTestsExpander.IsExpanded = false;
                physicalTestsExpander.IsEnabled = false;
                physicalTestsExpander.IsExpanded = false;
                VisitExpander.IsEnabled = false;
                VisitExpander.IsExpanded = false;

                while (true)
                {
                    if (LogIn() == true)
                    {
                        db = new DBClient.DBClient();
                        currentLabRow = 0;
                        currentPhyRow = 0;
                        GetDataFromDB();
                        Visibility = System.Windows.Visibility.Visible;
                        break;
                    }
                }
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
        /// Metoda pobierająca dane z bazy i inicjalizująca kontrolki odpowiedzialne za ich prezentację.
        /// </summary>
        private void GetDataFromDB()
        {
            Dictionary<int, string> visits = db.GetVisits(patientNameTextBox.Text, patientSurnameTextBox.Text, (byte)visitStatusComboBox.SelectedIndex, visitDate.SelectedDate);

            if (visitsList.Items != null)
            {
                visitsList.Items.Clear();
            }

            if (visits != null && visits.Count > 0)
            {
                visitsIdList = new List<int>();

                visitsList.IsEnabled = true;

                foreach (var v in visits)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = v.Value;
                    item.Tag = v.Key;

                    visitsList.Items.Add(item);
                    visitsIdList.Add(v.Key);
                }
            }
            else
            {
                visitsList.IsEnabled = false;
                visitsList.Items.Add(new ListBoxItem().Content = "Brak wizyt!");

                if (visits == null)
                    MessageBox.Show("Wystąpił błąd podczas pobierania listy wizyt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        /// <summary>
        /// Metoda obsługująca zdarzenie kliknięcia "Zapisz badanie fiz."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePhysicalTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (PhyTestsList.SelectedIndex > -1)
            {
                if (PhyTestResult.Text.Length > 0)
                {
                    if (db.AddTest(visitsIdList[visitsList.SelectedIndex], currentTestId, DateTime.Now, PhyTestResult.Text, phyTests.Keys.ElementAt(PhyTestsList.SelectedIndex), false))
                    {
                        Grid savedPhyTest = new Grid();
                        RowDefinition row = new RowDefinition();
                        row.Height = GridLength.Auto;
                        savedPhyTest.RowDefinitions.Add(row);
                        ColumnDefinition col1 = new ColumnDefinition();
                        col1.Width = new GridLength(0.4, GridUnitType.Star);
                        ColumnDefinition col2 = new ColumnDefinition();
                        col2.Width = GridLength.Auto;
                        ColumnDefinition col3 = new ColumnDefinition();
                        col3.Width = new GridLength(0.6, GridUnitType.Star);
                        savedPhyTest.ColumnDefinitions.Add(col1);
                        savedPhyTest.ColumnDefinitions.Add(col2);
                        savedPhyTest.ColumnDefinitions.Add(col3);

                        TextBlock phyTest = new TextBlock();
                        phyTest.Text = phyTests.ElementAt(PhyTestsList.SelectedIndex).Value;
                        phyTest.TextWrapping = TextWrapping.Wrap;
                        Grid.SetRow(phyTest, 0);        //globalne statyczne właściwości, lol
                        Grid.SetColumn(phyTest, 0);     //ponoć powinno się to robić przed dodawaniem elementu do siatki, żeby się nic nie posypało
                        savedPhyTest.Children.Add(phyTest); //i tak też czynię

                        TextBlock phyTestTime = new TextBlock();
                        phyTestTime.Text = DateTime.Now.ToString();
                        phyTestTime.Margin = new Thickness(5.0, 0.0, 5.0, 0.0);
                        Grid.SetRow(phyTestTime, 0);
                        Grid.SetColumn(phyTestTime, 1);
                        savedPhyTest.Children.Add(phyTestTime);

                        TextBlock phyTestResult = new TextBlock();
                        phyTestResult.Text = PhyTestResult.Text;
                        phyTestResult.TextWrapping = TextWrapping.Wrap;
                        Grid.SetRow(phyTestResult, 0);
                        Grid.SetColumn(phyTestResult, 2);
                        savedPhyTest.Children.Add(phyTestResult);

                        PhysicalTests.Children.Insert(currentPhyRow, savedPhyTest);

                        ++currentTestId;
                        ++currentPhyRow;

                        PhyTestResult.Clear();
                        PhyTestsList.SelectedIndex = -1;
                    }
                    else
                        MessageBox.Show("Wystąpił błąd podczas zapisu wykonanego badania laboratoryjnego i nie zostało ono zapisane.", "Błąd zapisu badania", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show("Należy podać wynik badania fizykalnego.", "Brak wyniku", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PhyTestResult.Focus();
                }
            }
            else
            {
                MessageBox.Show("Należy wybrać badanie fizykalne z listy.", "Wybierz badanie", MessageBoxButton.OK, MessageBoxImage.Warning);
                PhyTestsList.Focus();
            }
        }

       

        
        private void VisitFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (patientNameTextBox.Text.Length > 0 || patientSurnameTextBox.Text.Length > 0)
            {
                findVisitButton.IsEnabled = true;
                clearFilterButton.IsEnabled = true;
            }
            else if (patientNameTextBox.Text.Length == 0 && patientSurnameTextBox.Text.Length == 0 && visitStatusComboBox.SelectedIndex == 0 && visitDate.SelectedDate == DateTime.Today)
            {
                findVisitButton.IsEnabled = false;
                clearFilterButton.IsEnabled = false;
            }
        }

     

        private void visitDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (findVisitButton != null && clearFilterButton != null)
            {
                if (findVisitButtonClicked || visitDate.SelectedDate != DateTime.Today)
                {
                    findVisitButton.IsEnabled = true;
                    clearFilterButton.IsEnabled = true;
                }
                else if (patientNameTextBox.Text.Length == 0 && patientSurnameTextBox.Text.Length == 0 && visitStatusComboBox.SelectedIndex == 0)
                {
                    findVisitButton.IsEnabled = false;
                    clearFilterButton.IsEnabled = false;
                }
                InvalidateVisual();
            }
        }
        


        private void visitStatusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (findVisitButton != null && clearFilterButton != null)
            {
                if (visitStatusComboBox.SelectedIndex > 0)
                {
                    findVisitButton.IsEnabled = true;
                    clearFilterButton.IsEnabled = true;
                }
                else if (patientNameTextBox.Text.Length == 0 && patientSurnameTextBox.Text.Length == 0 && visitDate.SelectedDate == DateTime.Today)
                {
                    if (findVisitButtonClicked == false)
                    {
                        findVisitButton.IsEnabled = false;
                        clearFilterButton.IsEnabled = false;
                    }
                    else
                    {
                        findVisitButton.IsEnabled = true;
                        clearFilterButton.IsEnabled = true;
                    }
                }
            }
        }



        private void beginVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedVisitStatus == 0)
            {
                if (db.ChangeVisitState(visitsIdList[visitsList.SelectedIndex], 1))
                {
                    cancelVisitButton.IsEnabled = true;
                    saveVisitButton.IsEnabled = true;
                    RapTab.IsEnabled = true;
                    beginVisitButton.IsEnabled = false;
                    clearFilterButton.IsEnabled = false;
                    findVisitButton.IsEnabled = false;
                    todayButton.IsEnabled = false;
                    visitsList.IsEnabled = false;

                    visitDescription.IsEnabled = true;
                    diagnosis.IsEnabled = true;

                    diagnosisExpander.IsEnabled = true;
                    laboratoryTestsExpander.IsEnabled = true;
                    physicalTestsExpander.IsEnabled = true;
                    VisitExpander.IsEnabled = true;
                    VisitExpander.IsExpanded = true;
                    visitsHistory.IsEnabled = true;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu wizyty i nie został on zmieniony.", "Błąd rozpoczęcia wizyty", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else if (selectedVisitStatus == 1)
            {
                List<BadanieInfo> visitsTests = db.GetVisitTests(visitsIdList[visitsList.SelectedIndex]);

                if (visitsTests != null)
                {
                    cancelVisitButton.IsEnabled = true;
                    saveVisitButton.IsEnabled = true;
                    RapTab.IsEnabled = true;
                    beginVisitButton.IsEnabled = false;
                    clearFilterButton.IsEnabled = false;
                    findVisitButton.IsEnabled = false;
                    todayButton.IsEnabled = false;
                    visitsList.IsEnabled = false;

                    visitDescription.IsEnabled = true;
                    diagnosis.IsEnabled = true;

                    currentTestId = (byte)(visitsTests.Count + 1);

                    foreach (BadanieInfo bad in visitsTests)
                    {
                        if (bad.lab) //badanie laboratoryjne
                        {
                            Grid savedLabTest = new Grid();
                            RowDefinition row = new RowDefinition();
                            row.Height = GridLength.Auto;
                            savedLabTest.RowDefinitions.Add(row);
                            ColumnDefinition col1 = new ColumnDefinition();
                            col1.Width = new GridLength(0.4, GridUnitType.Star);
                            ColumnDefinition col2 = new ColumnDefinition();
                            col2.Width = GridLength.Auto;
                            ColumnDefinition col3 = new ColumnDefinition();
                            col3.Width = new GridLength(0.6, GridUnitType.Star);
                            savedLabTest.ColumnDefinitions.Add(col1);
                            savedLabTest.ColumnDefinitions.Add(col2);
                            savedLabTest.ColumnDefinitions.Add(col3);

                            TextBlock labTest = new TextBlock();
                            labTest.Text = labTests[bad.kod];
                            labTest.TextWrapping = TextWrapping.Wrap;
                            Grid.SetRow(labTest, 0);        //globalne statyczne właściwości, lol
                            Grid.SetColumn(labTest, 0);     //ponoć powinno się to robić przed dodawaniem elementu do siatki, żeby się nic nie posypało
                            savedLabTest.Children.Add(labTest); //i tak też czynię

                            TextBlock labTestTime = new TextBlock();
                            labTestTime.Text = bad.data_zle.ToString();
                            labTestTime.Margin = new Thickness(5.0, 0.0, 5.0, 0.0);
                            Grid.SetRow(labTestTime, 0);
                            Grid.SetColumn(labTestTime, 1);
                            savedLabTest.Children.Add(labTestTime);

                            TextBlock labTestDesc = new TextBlock();
                            labTestDesc.Text = bad.opis;
                            labTestDesc.TextWrapping = TextWrapping.Wrap;
                            Grid.SetRow(labTestDesc, 0);
                            Grid.SetColumn(labTestDesc, 2);
                            savedLabTest.Children.Add(labTestDesc);

                            LaboratoryTests.Children.Insert(currentLabRow, savedLabTest);

                            ++currentLabRow;
                        }
                        else         //badanie fizykalne
                        {
                            Grid savedPhyTest = new Grid();
                            RowDefinition row = new RowDefinition();
                            row.Height = GridLength.Auto;
                            savedPhyTest.RowDefinitions.Add(row);
                            ColumnDefinition col1 = new ColumnDefinition();
                            col1.Width = new GridLength(0.4, GridUnitType.Star);
                            ColumnDefinition col2 = new ColumnDefinition();
                            col2.Width = GridLength.Auto;
                            ColumnDefinition col3 = new ColumnDefinition();
                            col3.Width = new GridLength(0.6, GridUnitType.Star);
                            savedPhyTest.ColumnDefinitions.Add(col1);
                            savedPhyTest.ColumnDefinitions.Add(col2);
                            savedPhyTest.ColumnDefinitions.Add(col3);

                            TextBlock phyTest = new TextBlock();
                            phyTest.Text = phyTests[bad.kod];
                            phyTest.TextWrapping = TextWrapping.Wrap;
                            Grid.SetRow(phyTest, 0);        //globalne statyczne właściwości, lol
                            Grid.SetColumn(phyTest, 0);     //ponoć powinno się to robić przed dodawaniem elementu do siatki, żeby się nic nie posypało
                            savedPhyTest.Children.Add(phyTest); //i tak też czynię

                            TextBlock phyTestTime = new TextBlock();
                            phyTestTime.Text = bad.data_zle.ToString();
                            phyTestTime.Margin = new Thickness(5.0, 0.0, 5.0, 0.0);
                            Grid.SetRow(phyTestTime, 0);
                            Grid.SetColumn(phyTestTime, 1);
                            savedPhyTest.Children.Add(phyTestTime);

                            TextBlock phyTestResult = new TextBlock();
                            phyTestResult.Text = bad.wynik;
                            phyTestResult.TextWrapping = TextWrapping.Wrap;
                            Grid.SetRow(phyTestResult, 0);
                            Grid.SetColumn(phyTestResult, 2);
                            savedPhyTest.Children.Add(phyTestResult);

                            PhysicalTests.Children.Insert(currentPhyRow, savedPhyTest);

                            ++currentPhyRow;
                        }
                    }

                    diagnosisExpander.IsEnabled = true;
                    laboratoryTestsExpander.IsEnabled = true;
                    physicalTestsExpander.IsEnabled = true;
                    VisitExpander.IsEnabled = true;
                    VisitExpander.IsExpanded = true;
                    visitsHistory.IsEnabled = true;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas pobierania z bazy danych badań dla tej wizyty.", "Błąd pobierania badań", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        private void cancelVisitButton_Click(object sender, RoutedEventArgs e)
        {
            if (visitDescription.Text != null && visitDescription.Text.Length > 0)
            {
                if (db.CancelVisit(visitsIdList[visitsList.SelectedIndex], visitDescription.Text))
                {
                    MessageBox.Show("Wizyta została anulowana.", "Anulowanie wizyty", MessageBoxButton.OK, MessageBoxImage.Information);

                    findVisitButton.IsEnabled = true;
                    todayButton.IsEnabled = true;

                    data_rej.Text = nazwa_pac.Text = "";

                    visitDescription.Clear();
                    diagnosis.Clear();

                    LabTestDesc.Clear();
                    LabTestsList.SelectedIndex = -1;

                    PhyTestResult.Clear();
                    PhyTestsList.SelectedIndex = -1;

                    while (LaboratoryTests.Children.Count > 2)
                        LaboratoryTests.Children.RemoveAt(0);

                    while (PhysicalTests.Children.Count > 2)
                        PhysicalTests.Children.RemoveAt(0);

                    currentLabRow = 0;
                    currentPhyRow = 0;
                    currentTestId = 1;

                    diagnosisExpander.IsEnabled = false;
                    diagnosisExpander.IsExpanded = false;
                    laboratoryTestsExpander.IsEnabled = false;
                    laboratoryTestsExpander.IsExpanded = false;
                    physicalTestsExpander.IsEnabled = false;
                    physicalTestsExpander.IsExpanded = false;
                    visitsHistory.IsEnabled = false;
                    VisitExpander.IsEnabled = false;
                    VisitExpander.IsExpanded = false;

                    visitsIdList.RemoveAt(visitsList.SelectedIndex);
                    visitsList.Items.RemoveAt(visitsList.SelectedIndex);
                    visitsList.SelectedIndex = -1;
                    cancelVisitButton.IsEnabled = false;
                    saveVisitButton.IsEnabled = false;
                    RapTab.IsEnabled = false;

                    if (visitsList.Items.Count == 0)
                    {
                        visitsList.Items.Add(new ListBoxItem().Content = "Brak wizyt!");
                    }
                    else if (findVisitButtonClicked)
                    {
                        clearFilterButton.IsEnabled = true;
                        visitsList.IsEnabled = true;
                    }
                }
                else
                    MessageBox.Show("Nie udało się anulować wizyty!", "Błąd - anulowanie wizyty", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show("Przed anulowaniem wizyty należy podać powód jej anulowania.", "Brak uzasadnienia", MessageBoxButton.OK, MessageBoxImage.Warning);
                visitDescription.Focus();
            }
        }



        private void findVisitButton_Click(object sender, RoutedEventArgs e)
        {
            GetDataFromDB();

            findVisitButtonClicked = true;
            selectedVisitStatus = visitStatusComboBox.SelectedIndex;

            //wyczyszczenie, zwinięcie i dezaktywacja expanderów "Wizyta" i "Diagnoza", dezaktywacja przycisku "Rozpocznij wizytę"
            data_rej.Text = nazwa_pac.Text = "";
            visitDescription.Clear();
            VisitExpander.IsExpanded = false;
            VisitExpander.IsEnabled = false;
            diagnosis.Clear();
            diagnosisExpander.IsExpanded = false;
            diagnosisExpander.IsEnabled = false;
            beginVisitButton.IsEnabled = false;
        }



        private void clearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            //wyczyszczenie pól filtra:
            patientNameTextBox.Text = "";
            patientSurnameTextBox.Text = "";           
            visitStatusComboBox.SelectedIndex = 0;
            visitDate.SelectedDate = DateTime.Today;
            clearFilterButton.IsEnabled = false;

            //wyczyszczenie, zwinięcie i dezaktywacja expanderów "Wizyta" i "Diagnoza", dezaktywacja przycisku "Rozpocznij wizytę"
            data_rej.Text = nazwa_pac.Text = "";
            visitDescription.Clear();
            VisitExpander.IsExpanded = false;
            VisitExpander.IsEnabled = false;
            diagnosis.Clear();
            diagnosisExpander.IsExpanded = false;
            diagnosisExpander.IsEnabled = false;
            beginVisitButton.IsEnabled = false;
            
            if (findVisitButtonClicked == true)
            {
                GetDataFromDB();
            }

            findVisitButtonClicked = false;
        }



        /// <summary>
        /// Obsługa zdarzenia zamykania okna głównego aplikacji.
        /// Anuluje zamknięcie okna i wyświetla odpowiednią informację, jeśli zdarzenie to ma miejsce w trakcie odbywania się wizyty.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //przycisk "Zakończ wizytę" jest aktywny tylko wtedy, gdy jakaś wizyta się odbywa
            if (saveVisitButton.IsEnabled == true)
            {
                MessageBox.Show("Nie można zamknąć aplikacji w trakcie odbywania się wizyty.", "Wizyta w toku", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Cancel = true;
            }
        }



        /// <summary>
        /// Obsługa zdarzenia kliknięcia przycisku "Dziś".
        /// Ustawia dzisiejszą datę jako datę odbycia się wizyty w filtrze.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void todayButton_Click(object sender, RoutedEventArgs e)
        {
            visitDate.SelectedDate = DateTime.Today;
        }



        /// <summary>
        /// Obsługa zdarzenia kliknięcia przycisku "Historia wizyt pacjenta".
        /// Ładuje dane o wizytach i badaniach wskazanego pacjenta do raportu, odświeża widok przeglądarki raportów.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitsHistory_Click(object sender, RoutedEventArgs e)
        {
            //raz, że jest to logiczne, a dwa, że przed wyświetleniem 1-go raportu, jeszcze przed ustawieniem ReportSource przeglądarce, trzeba "fizycznie" ją wyświetlić
            RapTab.Focus();
            
            if (reportViewer.ViewerCore.ReportSource == null)
            {
                //utworzenie raportu
                report = new PacjentHistoria();
                //utworzenie struktury zbioru danych
                dataSet = new PacjentWizytyBadania();
                //utworzenie adaptera ładującego dane do tabeli w data secie
                tableAdapter = new PacjentWizytyBadaniaTableAdapters.PacjentWizytyBadaniaTableAdapter();

                //wypełnienie tabeli data setu danymi o wizytach i badaniach pacjenta o podanym ID
                tableAdapter.Fill(dataSet._PacjentWizytyBadania, db.GetPatientId(visitsIdList[visitsList.SelectedIndex]));

                //ustawienie naszego data setu z wypełnioną tabelą jako źródło raportu
                report.SetDataSource(dataSet);

                //ustawienie naszego raportu jako źródłowego w przeglądarce
                reportViewer.ViewerCore.ReportSource = report;
            }
            else
            {
                tableAdapter.Fill(dataSet._PacjentWizytyBadania, db.GetPatientId(visitsIdList[visitsList.SelectedIndex]));

                //odświeżenie raportu w przeglądarce
                reportViewer.ViewerCore.RefreshReport();
            }
        }        
    }
}
