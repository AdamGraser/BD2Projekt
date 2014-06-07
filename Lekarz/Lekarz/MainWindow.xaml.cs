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
        byte currentLabTestID; // przechowuje ID badania lab. aktualnie wykonywanego przez laboranta lub -1 jeśli laborant nie wykonuje aktualnie żadnego badania lab.
        byte currentRow;       // przechowuje nr wiersza listy zleconych badań laboratoryjnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co zlecone badanie



        /// <summary>
        /// Domyślny konstruktor. Tworzy i otwiera połączenie z bazą danych.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();

            if (LogIn() == true)
            {
                db = new DBClient.DBClient();
                currentVisitID = -1;
                currentRow = 0;
                currentLabTestID = 0;
                GetDataFromDB();                
            }            
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Przyjmij wizytę".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeVisitState_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID == -1 && currentLabTestID == 0)
            {
                ListBoxItem item = (ListBoxItem)visitsList.SelectedItem;
                currentVisitID = (int)item.Tag;

                if (db.ChangeVisitState(currentVisitID, false))
                {
                    stan.Text = "W trakcie realizacji";

                    visitsList.Items.RemoveAt(visitsList.SelectedIndex);
                    changeVisitStateButton.IsEnabled = false;
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

            if (db.AddLabTest(currentVisitID, (byte)(currentRow + 1), lTT, LabTestDesc.Text, (short)(LabTestsList.SelectedIndex + 1)))
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

                LaboratoryTests.Children.Insert((int)currentRow, savedLabTest);

                ++currentRow;

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
            if (currentVisitID > -1 && currentLabTestID == 0)
            {
                bool physicalTestIsDone = false; //tymczasowo
                if (db.SaveVisit(currentVisitID, opis.Text, physicalTestIsDone, diagnoza.Text))
                {
                    data_rej.Text = nazwa_pac.Text = stan.Text = "";

                    opis.Clear();
                    diagnoza.Clear();                    

                    LabTestDesc.Clear();
                    LabTestsList.SelectedIndex = -1;

                    while (LaboratoryTests.Children.Count > 2)
                        LaboratoryTests.Children.RemoveAt(0);

                    currentVisitID = -1;

                    visitsList.SelectedIndex = -1;
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
            if (currentVisitID == -1 && currentLabTestID == 0)
            {
                ListBoxItem item = (ListBoxItem)visitsList.SelectedItem;
                string temp = (string)item.Content;

                string[] visit = temp.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                data_rej.Text = visit[0];
                nazwa_pac.Text = visit[1];
                stan.Text = "Nierozpoczęta";
                changeVisitStateButton.IsEnabled = true;
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
            LabTestsList.SelectedItem = null;
            LabTestsList.SelectedIndex = -1;
            LabTestDesc.Text = "";
            PhiTestsList.SelectedItem = null;
            PhiTestsList.SelectedIndex = -1;
            PhiTestDesc.Text = "";

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
        /// Metoda obsługująca wyświetalanie okna dialogowego odpowiedzialnego za logowanie do systemu.
        /// </summary>
        /// <returns></returns>
        private bool LogIn()
        {
            LoginWindow loginWindow = new LoginWindow();

            bool? result = loginWindow.ShowDialog();

            if (result == true)
            {
                Title += " - " + loginWindow.Login;
                return true;
            }
            else if (result == false) //zamknięcie okna logowania
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
    }
}
