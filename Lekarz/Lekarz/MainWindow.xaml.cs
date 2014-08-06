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
        DBClient.DBClient db;  // klient bazy danych
        List<int> visitsIdList;
        byte currentLabRow;    // przechowuje nr wiersza listy zleconych badań laboratoryjnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co zlecone badanie
        int currentPhyRow;     // przechowuje nr wiersza listy wykonanych badań fizykalnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co wykonane badanie
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

                    visitDate.SelectedDate = DateTime.Today;

                    GetDataFromDB();

                    // --> Tworzenie listy badań laboratoryjnych.
                    List<string> labTestsNames = db.GetLabTestsNames();

                    if (labTestsNames != null && labTestsNames.Count > 0)
                    {
                        foreach (string l in labTestsNames)
                        {
                            LabTestsList.Items.Add(new ComboBoxItem().Content = l);
                        }
                    }
                    else
                    {
                        LabTestsList.IsEnabled = false;

                        LabTestDesc.Text = "Brak badań w bazie danych";
                        LabTestDesc.IsEnabled = false;

                        orderLaboratoryTestButton.IsEnabled = false;

                        if (labTestsNames == null)
                            MessageBox.Show("Wystąpił błąd podczas pobierania listy badań laboratoryjnych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    // <-- Tworzenie listy badań laboratoryjnych.

                    // --> Tworzenie listy badań fizykalnych.
                    List<string> phyTestsNames = db.GetPhyTestsNames();

                    if (phyTestsNames != null && phyTestsNames.Count > 0)
                    {
                        foreach (string l in phyTestsNames)
                        {
                            PhyTestsList.Items.Add(new ComboBoxItem().Content = l);
                        }
                    }
                    else
                    {
                        PhyTestsList.IsEnabled = false;

                        PhyTestDesc.Text = "Brak badań w bazie danych";
                        PhyTestDesc.IsEnabled = false;

                        savePhysicalTestButton.IsEnabled = false;

                        if (labTestsNames == null)
                            MessageBox.Show("Wystąpił błąd podczas pobierania listy badań fizykalnych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    // <-- Tworzenie listy badań fizykalnych.

                    /*tableAdapter.Fill(dataSet._PacjentWizytyBadania);

                    CrystalDecisions.CrystalReports.Engine.ReportDocument doc = new CrystalDecisions.CrystalReports.Engine.ReportDocument();
                    doc.Load("E:\\Adam\\House of dog\\studia\\bd-projekt\\Lekarz\\Lekarz\\PacjentHistoria.rpt");*/
                    
                    //report = new PacjentHistoria();
                    //dataSet = new PacjentWizytyBadania();
                    //tableAdapter = new PacjentWizytyBadaniaTableAdapters.PacjentWizytyBadaniaTableAdapter();
                    //tableAdapter.Connection = new System.Data.SqlClient.SqlConnection(@"Server=BODACH\SQLEXPRESS; uid=sa; pwd=Gresiulina; Database=Przychodnia");
                    
                    //tableAdapter.Fill(report.pacjentWizytyBadania._PacjentWizytyBadania, 1);
                    //report.pacjentWizytyBadaniaTableAdapter.Connection = new System.Data.SqlClient.SqlConnection(@"Server=BODACH\SQLEXPRESS; uid=sa; pwd=Gresiulina; Database=Przychodnia");
                    //report.pacjentWizytyBadaniaTableAdapter.Fill(report.pacjentWizytyBadania._PacjentWizytyBadania, 1);

                    //report.SetDataSource(tableAdapter.GetData(1) as System.Data.DataTable);
                    
                    //reportViewer.ViewerCore.ReportSource = report;

                    /*testReport = new Test();
                    dataSet = new PacjentWizytyBadania();
                    //dataSet.WriteXml("E:\\PacjentWizytyBadania.xml", System.Data.XmlWriteMode.WriteSchema);
                    tableAdapter = new PacjentWizytyBadaniaTableAdapters.PacjentWizytyBadaniaTableAdapter();
                    System.Data.SqlClient.SqlConnection conn = tableAdapter.Connection;
                    tableAdapter.Fill(dataSet._PacjentWizytyBadania, 1);
                    //testReport.pacjentWizytyBadania = new PacjentWizytyBadania();
                    //tableAdapter.Fill(testReport.pacjentWizytyBadania._PacjentWizytyBadania, 1);

                    //tableAdapter.Connection = new System.Data.SqlClient.SqlConnection("Data Source=BODACH\\SQLEXPRESS;Initial Catalog=Przychodnia;User ID=sa");
                    //testReport.SetDataSource(tableAdapter.GetData(1) as System.Data.DataTable);
                    testReport.SetDataSource(dataSet);
                    //testReport.SetDatabaseLogon("sa", "Gresiulina", "BODACH\\SQLEXPRESS", "Przychodnia");

                    reportViewer.ViewerCore.ReportSource = testReport;*/

                    // WORKAROUND -->
                    System.Reflection.FieldInfo tooltipField = typeof(SAPBusinessObjects.WPF.Viewer.DocumentView).GetField("m_tooltip", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                    if (tooltipField != null)
                    {
                        System.Reflection.FieldInfo reportAlbumField = typeof(SAPBusinessObjects.WPF.Viewer.ViewerCore).GetField("reportAlbum", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        if (reportAlbumField != null)
                        {
                            if (reportViewer.ViewerCore.ViewCount > 0)
                            {
                                SAPBusinessObjects.WPF.Viewer.DocumentView currentView = ((SAPBusinessObjects.WPF.Viewer.ReportAlbum)reportAlbumField.GetValue(reportViewer.ViewerCore)).ReportViews[0];

                                if (tooltipField.GetValue(currentView) == null)
                                    tooltipField.SetValue(currentView, new System.Windows.Controls.ToolTip());
                            }

                        }

                    }
                    // <-- WORKAROUND

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
            if (db.AddTest(visitsIdList[visitsList.SelectedIndex], (byte)(currentLabRow + 1), DateTime.Now, LabTestDesc.Text, (short)(LabTestsList.SelectedIndex + 1)))
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
                labTest.Text = (string)LabTestsList.SelectedItem;
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

                LaboratoryTests.Children.Insert((int)currentLabRow, savedLabTest);

                ++currentLabRow;

                LabTestDesc.Clear();
                LabTestsList.SelectedIndex = -1;
            }
            else
                MessageBox.Show("Wystąpił błąd podczas zapisu zlecenia badania laboratoryjnego i nie zostało ono zapisane.", "Błąd zapisu zlecenia", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                    data_rej.Text = nazwa_pac.Text = "";

                    visitDescription.Clear();
                    diagnosis.Clear();

                    LabTestDesc.Clear();
                    LabTestsList.SelectedIndex = -1;

                    PhyTestDesc.Clear();

                    while (LaboratoryTests.Children.Count > 2)
                        LaboratoryTests.Children.RemoveAt(0);

                    while (PhysicalTests.Children.Count > 2)
                        PhysicalTests.Children.RemoveAt(0);

                    diagnosisExpander.IsEnabled = false;
                    diagnosisExpander.IsExpanded = false;
                    laboratoryTestsExpander.IsEnabled = false;
                    laboratoryTestsExpander.IsExpanded = false;
                    physicalTestsExpander.IsEnabled = false;
                    physicalTestsExpander.IsExpanded = false;
                    VisitExpander.IsEnabled = false;
                    VisitExpander.IsExpanded = false;

                    visitsIdList.RemoveAt(visitsList.SelectedIndex);
                    visitsList.Items.RemoveAt(visitsList.SelectedIndex);
                    visitsList.SelectedIndex = -1;
                    cancelVisitButton.IsEnabled = false;
                    saveVisitButton.IsEnabled = false;
                    RapTab.IsEnabled = false;
                    todayButton.IsEnabled = true;

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
                MessageBox.Show("Przed zakończeniem wizyty należy podać jej opis.", "Brak opisu wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
        }



        /// <summary>
        /// Metoda obsługująca zmianę selekcji wizyty w listboksie.
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
                }
                else
                {
                    beginVisitButton.IsEnabled = false;
                    visitDescription.IsEnabled = false;
                    visitDescription.Text = db.GetVisitDescription(visitsIdList[visitsList.SelectedIndex]);
                    diagnosis.IsEnabled = false;
                    diagnosis.Text = db.GetVisitDiagnosis(visitsIdList[visitsList.SelectedIndex]);

                    VisitExpander.IsEnabled = true;
                    VisitExpander.IsExpanded = true;

                    if (selectedVisitStatus != 1)
                        diagnosisExpander.IsEnabled = true;

                    laboratoryTestsExpander.IsEnabled = false;
                    physicalTestsExpander.IsEnabled = false;
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
            PhyTestDesc.Text = "";

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
                    MessageBox.Show("Wystąpił błąd podczas pobierania listy wizyt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        /// <summary>
        /// Metoda obsługująca zdarzenie kliknięcia "Zapisz badanie fiz."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePhysicalTestButton_Click(object sender, RoutedEventArgs e)
        {
            if (db.AddTest(visitsIdList[visitsList.SelectedIndex], (byte)(currentPhyRow + 1), DateTime.Now, PhyTestDesc.Text, (short)(PhyTestsList.SelectedIndex + 1)))
            {
                TextBlock phyTest = new TextBlock();
                phyTest.Text = PhyTestDesc.Text;
                phyTest.TextWrapping = TextWrapping.Wrap;

                PhysicalTests.Children.Insert(currentPhyRow, phyTest);

                ++currentPhyRow;

                PhyTestDesc.Clear();
            }
            else
                MessageBox.Show("Wystąpił błąd podczas zapisu zlecenia badania laboratoryjnego i nie zostało ono zapisane.", "Błąd zapisu zlecenia", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                if (visitStatusComboBox.SelectedIndex != 0)
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
            cancelVisitButton.IsEnabled = true;
            saveVisitButton.IsEnabled = true;
            RapTab.IsEnabled = true;
            beginVisitButton.IsEnabled = false;
            clearFilterButton.IsEnabled = false;
            findVisitButton.IsEnabled = false;
            todayButton.IsEnabled = false;
            visitsList.IsEnabled = false;

            if (db.ChangeVisitState(visitsIdList[visitsList.SelectedIndex], 1))
            {
                visitDescription.IsEnabled = true;
                diagnosis.IsEnabled = true;
                   

                diagnosisExpander.IsEnabled = true;
                laboratoryTestsExpander.IsEnabled = true;
                physicalTestsExpander.IsEnabled = true;
                VisitExpander.IsEnabled = true;
                VisitExpander.IsExpanded = true;
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas zmiany stanu wizyty i nie został on zmieniony.", "Błąd akceptacji wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Przed anulowaniem wizyty należy podać powód jej anulowania.", "Brak opisu", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                MessageBox.Show("Nie można zamknąć aplikacji w trakcie odbywania się wizyty.", "Ostrzeżenie", MessageBoxButton.OK, MessageBoxImage.Warning);
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
