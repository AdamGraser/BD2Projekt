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

namespace Laborant
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBClient.DBClient db;                 // klient bazy danych
        int currentVisitID;                           // przechowuje ID wizyty podczas której zlecone zostało badanie currentLabTestID
        byte currentLabTestID;                        // przechowuje ID badania lab. aktualnie wykonywanego przez laboranta lub -1 jeśli laborant nie wykonuje aktualnie żadnego badania lab.
        int currentLabTest;                           // przechowuje sumaryczny nr badania laboratoryjnego (sumowane są wszystkie niewykonane badania, zlecone we wszystkich wizytach, w kolejności rosnącej)
        List<int> visitIDs;                           // lista ID wizyt kolejnych badań
        List<byte> labTestIDs;                        // lista ID kolejnych badań
        byte currentState;                            // aktualny stan badań widocznych na liście

        /// <summary>
        /// Domyślny konstruktor. Obłsuguje logowanie. Inicjalizuje elementy interfejsu, klienta bazy danych oraz pola pomocnicze. Wypełnia odpowiednie elementy danymi.
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

                    Visibility = System.Windows.Visibility.Visible;

                    if (db.isKier()) // Ustawienie kontrolek widocznych tylko dla kierownika
                    {
                        Lab_Accept.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Lab_Accept.Visibility = System.Windows.Visibility.Hidden;
                    }

                    break;
                }                
            }

            Lab_Save.Tag = (bool?)null;
            Lab_Accept.Tag = (bool?)null;
            Lab_Cancel.Tag = (bool?)false;
        }



        /// <summary>
        /// Metoda obsługująca kliknięcie przycisku "Wyloguj się" na pasku menu.
        /// Powoduje ukrycie okna głównego i wyświetlenie okna logowania.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Title = "Laborant";
            Visibility = System.Windows.Visibility.Hidden;
            db.ResetIdLab();
            db.Dispose();
            db = null;

            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):
            ClearLabTestsLists();
            Lab_LabTestResult.Clear();
            clearFilterButton_Click(null, null);
            ClearControlls();

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();
                    GetDataFromDB();
                    Visibility = System.Windows.Visibility.Visible;

                    if (db.isKier()) // Ustawienie kontrolek widocznych tylko dla kierownika
                    {
                        Lab_Accept.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        Lab_Accept.Visibility = System.Windows.Visibility.Hidden;
                    }

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

        // OBSLUGA KONTROLEK
        /// <summary>
        /// Obsługa kliknięcia przycisku "Szukaj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            ClearLabTestsLists();
            GetDataFromDB();
            clearFilterButton.IsEnabled = true;
            currentState = (byte)stateComboBox.SelectedIndex;

            // Czyszczenie kontrolek
            ClearControlls();


            // Dezaktywacja
            Lab_Accept.IsEnabled = false;
            Lab_Cancel.IsEnabled = false;
            Lab_Save.IsEnabled = false;
        }

        private void clearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            stateComboBox.SelectedIndex = 0;
            DateTo.SelectedDate = null;
            DateFrom.SelectedDate = null;
            clearFilterButton.IsEnabled = false;
        }

        /// <summary>
        /// Czyści listę niewykonanych badań laboratoryjnych, ew. aktywuje ją i przywraca wyrównanie elementów do wartości domyślnych.
        /// Czyści również zbiór ID wizyt i ich list ID niewykonanych badań.
        /// </summary>
        private void ClearLabTestsLists()
        {
            Lab_LabTestsList.Items.Clear();
            Lab_LabTestsList.IsEnabled = true;
            Lab_LabTestsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;

            labTestIDs.Clear();
            visitIDs.Clear();
        }



        /// <summary>
        /// Obsługa kliknięcia przycisków "Wykonaj", "Zatwierdź" i "Anuluj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Save_Click(object sender, RoutedEventArgs e)
        {

            Button s = (Button)sender;

            byte stateToSave;

            if ((bool?)s.Tag == false) // Kliknięto "Anuluj"
            {
                if (currentState == 2) // Wykonane
                {
                    stateToSave = 3; // Anulowane KLAB

                    if (Lab_LabCancelInfo.Text.Length < 1) // Puste pole opisu
                    {
                        MessageBox.Show("Aby anulować badanie, należy podać powód!", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else
                {
                    stateToSave = 1; // Anulowane LAB

                    if (Lab_LabTestResult.Text.Length < 1) // Puste pole wyniku
                    {
                        MessageBox.Show("Aby anulować badanie, należy podać powód anulowania w rubryce \"Wynik\"!", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
            else // Kliknięto "Zatwierdź" lub "Wykonaj"
            {
                if (currentState == 2)
                {
                    stateToSave = 4; // Zatwierdzone
                }
                else
                {
                    stateToSave = 2; // Wykonane

                    if (Lab_LabTestResult.Text.Length < 1) // Puste pole wyniku
                    {
                        MessageBox.Show("Aby wykonać badanie, należy wpisać jego wynik!", "Błąd!", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }

            bool? save = db.SaveLabTest(currentVisitID, currentLabTestID, DateTime.Now, Lab_LabTestResult.Text, Lab_LabCancelInfo.Text, stateToSave, currentState);

            if (save == true)
            {
                //Usunięcie wybranego wiersza:
                Lab_LabTestsList.Items.RemoveAt(currentLabTest);
                visitIDs.RemoveAt(currentLabTest);
                labTestIDs.RemoveAt(currentLabTest);

                //Usuwanie szczegółowych informacji o zapisanym badaniu i usunięcie wyników tego badania.
                ClearControlls();
                
                currentLabTest = -1;
                currentVisitID = -1;
                currentLabTestID = 0;

                Lab_LabTestsList.UnselectAll();

                //Dezaktywacja przycisków
                Lab_Save.IsEnabled = false;
                Lab_Accept.IsEnabled = false;
                Lab_Cancel.IsEnabled = false;
            }
            else if (save == false)
                MessageBox.Show("Wystąpił błąd podczas próby zapisu wyniku badania laboratoryjnego i nie został on zapisany.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            else
                MessageBox.Show("Wystąpił błąd podczas próby zapisu wyniku - prawdopodobnie stan badania został zmieniony przez innego laboranta", "Zmieniony rekord", MessageBoxButton.OK, MessageBoxImage.Warning);
            
        }



        /// <summary>
        /// Obsługa kliknięcia badania z listy.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ListBoxSelectionChange(object sender, RoutedEventArgs e)
        {
            //Zeby nie robilo sie bez potrzeby
            if(Lab_LabTestsList.SelectedIndex < 0)
                return;

            //Aktywacja kontrolek
            if (currentState == 0) // Jeśli stan jest "zlecone"
            {
                Lab_Save.IsEnabled = true;
                Lab_Cancel.IsEnabled = true;
                Lab_Accept.IsEnabled = false;
                Lab_LabTestResult.IsEnabled = true;
                Lab_LabCancelInfo.IsEnabled = false;
            }
            else if ((currentState == 2) && (db.isKier() == true)) // Jeśli stan jest "wykonane" i jesteśmy kierownikiem
            {
                Lab_Cancel.IsEnabled = true;
                Lab_Save.IsEnabled = false;
                Lab_Accept.IsEnabled = true;
                Lab_LabTestResult.IsEnabled = false;
                Lab_LabCancelInfo.IsEnabled = true;
            }
            else
            {
                Lab_Save.IsEnabled = false;
                Lab_Cancel.IsEnabled = false;
                Lab_Accept.IsEnabled = false;
                Lab_LabTestResult.IsEnabled = false;
                Lab_LabCancelInfo.IsEnabled = false;
            }

            // Wyszukanie szczegółów konkretnego badania
            currentLabTest = Lab_LabTestsList.SelectedIndex;
            currentVisitID = visitIDs[currentLabTest];
            currentLabTestID = labTestIDs[currentLabTest];

            ListBoxItem item = (ListBoxItem)Lab_LabTestsList.Items.GetItemAt(currentLabTest);
            string temp = (string)item.Content;

            string[] labTest = temp.Split(new char[] { ' ' }, 3, StringSplitOptions.RemoveEmptyEntries);

            Lab_LabTestOrderDate.Text = labTest[0] + " " + labTest[1];
            Lab_LabTestName.Text = labTest[2];

            List<string> labTestDetails = db.GetLabTestDetails(currentVisitID, currentLabTestID);

            if (labTestDetails != null)
            {
                if (labTestDetails.Count > 0)
                {
                    Lab_LabTestDescription.Text = labTestDetails[0];
                    Lab_LabTestDoctorName.Text = labTestDetails[1] + " " + labTestDetails[2];

                    if (labTestDetails[3] == null)
                        Lab_LabTestResult.Text = "";
                    else
                        Lab_LabTestResult.Text = labTestDetails[3];

                    if (labTestDetails[4] == null)
                        Lab_LabCancelInfo.Text = "";
                    else
                        Lab_LabCancelInfo.Text = labTestDetails[4];

                    if (labTestDetails[5] == null)
                        Lab_LabTestExecuteDate.Text = "";
                    else
                        Lab_LabTestExecuteDate.Text = labTestDetails[5];

                }
                else
                    MessageBox.Show("Podano nieprawidłowe ID wizyty lub nieprawidłowe ID badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
                MessageBox.Show("Wystąpił błąd podczas pobierania szczegółów badania laboratoryjnego.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        // KONIEC OBSLUGI KONTROLEK

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
            else if (hardExit.v == true) //zamknięcie okna logowania
                Environment.Exit(0);

            return false;
        }



        /// <summary>
        /// Metoda pobierająca dane z bazy i inicjalizująca kontrolki odpowiedzialne za ich prezentację.
        /// <param name="undone">Jeśli "true", pobierane są badania niewykonane. Jeśli false, pobierane są badania wykonane przez zalogowanego laboranta.</param>
        /// </summary>
        private void GetDataFromDB()
        {
            // --> Tworzenie listy badań laboratoryjnych o wybranym stanie
            List<TestLabInfo> tests = db.GetLabTests(stateComboBox.SelectedIndex, DateFrom.SelectedDate, DateTo.SelectedDate);

            visitIDs = new List<int>();
            labTestIDs = new List<byte>();

            if (tests != null && tests.Count > 0)
            {
                //Zapisywanie list ID badań dla każdej wizyty
                foreach (var t in tests)
                {
                    // Dodanie badania do wyświetlanej tabeli
                    ListBoxItem item = new ListBoxItem();
                    item.Content = t.opis;
                    item.Margin = new Thickness(0.0, 0.0, 10.0, 0.0);
                    Lab_LabTestsList.Items.Add(item);

                    // Dodanie ID badania i wizyty do odpowiednich list
                    visitIDs.Add(t.id_wiz);
                    labTestIDs.Add(t.id_bad);
                }
            }
            else
            {
                Lab_LabTestsList.IsEnabled = false;
                Lab_LabTestsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                Lab_LabTestsList.Items.Add(new ListBoxItem().Content = "Brak badań odpowiadających podanym kryteriom!");
               
                if (tests == null)
                    MessageBox.Show("Wystąpił błąd podczas pobierania listy badań.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Czyści kontrolki
        /// </summary>
        private void ClearControlls()
        {
            Lab_LabTestOrderDate.Text = "";
            Lab_LabTestExecuteDate.Text = "";
            Lab_LabTestName.Text = "";
            Lab_LabTestDoctorName.Text = "";
            Lab_LabTestDescription.Text = "";
            Lab_LabTestResult.Text = "";
            Lab_LabCancelInfo.Text = "";
        }
    }
}

