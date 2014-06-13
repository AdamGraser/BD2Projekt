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
using DBClient;

namespace Lekarz
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBClient.DBClient db;  // klient bazy danych
        int currentVisitID;    // przechowuje ID wizyty, która właśnie się odbywa lub -1, jeśli lekarz nie przyjmuje teraz żadnego pacjenta/podczas której zlecone zostało badanie currentLabTestID
        byte currentLabRow;    // przechowuje nr wiersza listy zleconych badań laboratoryjnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co zlecone badanie
        int currentPhyRow;     // przechowuje nr wiersza listy wykonanych badań fizykalnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co wykonane badanie



        /// <summary>
        /// Domyślny konstruktor. Tworzy i otwiera połączenie z bazą danych.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();

                    currentVisitID = -1;
                    currentLabRow = 0;
                    currentPhyRow = 0;

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

                    break;
                }
            }
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Przyjmij wizytę".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeVisitState_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID == -1)
            {
                ListBoxItem item = (ListBoxItem)visitsList.SelectedItem;
                currentVisitID = (int)item.Tag;

                if (db.ChangeVisitState(currentVisitID, false))
                {
                    stan.Text = "W trakcie realizacji";

                    visitsList.Items.RemoveAt(visitsList.SelectedIndex);
                    changeVisitStateButton.IsEnabled = false;

                    diagnosisExpander.IsEnabled = true;
                    laboratoryTestsExpander.IsEnabled = true;
                    physicalTestsExpander.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu wizyty i nie został on zmieniony.", "Błąd akceptacji wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
                    currentVisitID = -1;
                }
            }
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Zleć badanie lab.".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OrderLaboratoryTest_Click(object sender, RoutedEventArgs e)
        {
            DateTime lTT = DateTime.Now;

            if (db.AddLabTest(currentVisitID, (byte)(currentLabRow + 1), lTT, LabTestDesc.Text, (short)(LabTestsList.SelectedIndex + 1)))
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
                labTestTime.Text = lTT.ToString();
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
        private void SaveVisit_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID > -1)
            {
                bool physicalTestIsDone;
                string visDesc = opis.Text;

                //Dodanie badań fizykalnych do opisu wizyty.
                if (PhysicalTests.Children.Count > 2)
                {
                    visDesc += "\n\n***************************\n\nBadania fizykalne:\n";
                    physicalTestIsDone = true;
                    TextBox temp;

                    for (int i = 0; i < PhysicalTests.Children.Count - 2; ++i)
                    {
                        temp = (TextBox)PhysicalTests.Children[i];
                        visDesc += "\n" + temp.Text;
                    }
                }
                else
                    physicalTestIsDone = false;

                
                //Czyszczenie elementów i list, zwijanie i wyłączanie expander'ów.
                if (db.SaveVisit(currentVisitID, visDesc, physicalTestIsDone, diagnoza.Text))
                {
                    data_rej.Text = nazwa_pac.Text = stan.Text = "";

                    opis.Clear();
                    diagnoza.Clear();                    

                    LabTestDesc.Clear();
                    LabTestsList.SelectedIndex = -1;

                    PhyTestDesc.Clear();

                    while (LaboratoryTests.Children.Count > 2)
                        LaboratoryTests.Children.RemoveAt(0);

                    while (PhysicalTests.Children.Count > 2)
                        PhysicalTests.Children.RemoveAt(0);

                    currentVisitID = -1;

                    visitsList.Items.RemoveAt(visitsList.SelectedIndex);
                    visitsList.SelectedIndex = -1;

                    diagnosisExpander.IsEnabled = false;
                    diagnosisExpander.IsExpanded = false;
                    laboratoryTestsExpander.IsEnabled = false;
                    laboratoryTestsExpander.IsExpanded = false;
                    physicalTestsExpander.IsEnabled = false;
                    physicalTestsExpander.IsExpanded = false;
                    VisitExpander.IsEnabled = false;
                    VisitExpander.IsExpanded = false;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zapisu szczegółów wizyty i nie zostały one zapisane.", "Błąd aktualizacji wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
                 
            }
        }



        /// <summary>
        /// Metoda obsługująca zmianę selekcji wizyty w listboksie.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void visitsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentVisitID == -1)
            {
                ListBoxItem item = (ListBoxItem)visitsList.SelectedItem;
                string temp = (string)item.Content;

                string[] visit = temp.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                data_rej.Text = visit[0];
                nazwa_pac.Text = visit[1];
                stan.Text = "Nierozpoczęta";
                changeVisitStateButton.IsEnabled = true;

                VisitExpander.IsEnabled = true;
                VisitExpander.IsExpanded = true;
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
            db = null;

            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):
            visitsList.Items.Clear();
            data_rej.Text = "";
            nazwa_pac.Text = "";
            stan.Text = "";
            opis.Text = "";
            diagnoza.Text = "";
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
                    currentVisitID = -1;
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
            RefBool hardExit = new RefBool();
            LoginWindow loginWindow = new LoginWindow(hardExit);

            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                Title += " - " + loginWindow.Login;
                return true;
            }
            else if (hardExit.v == true) //zamknięcie okna logowania powoduje zamknięcie aplikacji
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
        /// Metoda obsługująca klikniecie przycisku "Odśwież dane".
        /// Klikniecie przycisku powoduje pobranie aktualnych danych z bazy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            visitsList.Items.Clear();
            GetDataFromDB();
        }



        /// <summary>
        /// Metoda pobierająca dane z bazy i inicjalizująca kontrolki odpowiedzialne za ich prezentację.
        /// </summary>
        private void GetDataFromDB()
        {
            // --> Tworzenie listy wizyt dla bieżąco zalogowanego lekarza.
            Dictionary<int, string> visits = db.GetVisits(1);

            if (visits != null && visits.Count > 0)
            {
                foreach (var v in visits)
                {
                    ListBoxItem item = new ListBoxItem();
                    item.Content = v.Value;
                    item.Tag = v.Key;

                    visitsList.Items.Add(item);
                }
            }
            else
            {
                visitsList.IsEnabled = false;
                visitsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                visitsList.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                visitsList.Items.Add(new ListBoxItem().Content = "Brak wizyt do końca życia!");

                if (visits == null)
                    MessageBox.Show("Wystąpił błąd podczas pobierania listy wizyt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            // <-- Tworzenie listy wizyt dla bieżąco zalogowanego lekarza. 
        }



        /// <summary>
        /// Metoda obsługująca zdarzenie kliknięcia "Zapisz badanie fiz."
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void savePhysicalTestButton_Click(object sender, RoutedEventArgs e)
        {
            TextBlock phyTest = new TextBlock();
            phyTest.Text = PhyTestDesc.Text;
            phyTest.TextWrapping = TextWrapping.Wrap;

            LaboratoryTests.Children.Insert(currentPhyRow, phyTest);

            ++currentPhyRow;

            PhyTestDesc.Clear();
        }
    }
}
