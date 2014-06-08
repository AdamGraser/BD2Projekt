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

namespace KierownikLaboratorium
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBClient.DBClient db;       // klient bazy danych
        int currentVisitID;                 // przechowuje ID wizyty podczas której zlecone zostało badanie currentLabTestID
        byte currentLabTestID;              // przechowuje ID badania lab. aktualnie wykonywanego przez laboranta lub -1 jeśli laborant nie wykonuje aktualnie żadnego badania lab.
        int currentLabTest;                 // przechowuje sumaryczny nr badania laboratoryjnego (sumowane są wszystkie niewykonane badania, zlecone we wszystkich wizytach, w kolejności rosnącej)
        bool done;                          // determinuje czy rozpatrywane badanie laboratoryjne zostało wykonane czy nie (rozróżnienie dla użycia currentVisitID i currentLabTestID)       
        Dictionary<int, byte> doneLabTests; // kolekcja par <ID wizyty, liczba badań wykonanych spośród zleconych w trakcie tej wizyty>



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
                    doneLabTests = new Dictionary<int, byte>();
                    GetDataFromDB();
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
        /// Metoda obsługująca kliknięcie przycisku "Wyloguj się" na pasku menu.
        /// Powoduje ukrycie okna głównego i wyświetlenie okna logowania.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Title = "Kierownik laboratorium";
            Visibility = System.Windows.Visibility.Hidden;
            db = null;

            //TODO:
            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):

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
                Title += " - " + loginWindow.Login;
                return true;
            }
            else if (result == false) //zamknięcie okna logowania
                Environment.Exit(0);

            return false;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Klab_CheckLabTest_Click(object sender, RoutedEventArgs e)
        {
            if (done == false)
            {
                if (doneLabTests.Count > 0)
                {
                    Button button = (Button)sender;
                    int num = 0;
                    currentLabTest = (int)button.Tag;

                    foreach (var t in doneLabTests)
                    {
                        num += t.Value;

                        if (num >= currentLabTest)
                        {
                            currentVisitID = t.Key;
                            break;
                        }

                    }

                    currentLabTestID = (byte)(doneLabTests[currentVisitID] - (num - currentLabTest));
                    done = true;

                    if (db.CheckLabTest(currentVisitID, currentLabTestID, 1))
                    {
                        ListBoxItem item = (ListBoxItem)Klab_LabTestsList.Items.GetItemAt(currentLabTest * 2 - 2);
                        string temp = (string)item.Content;

                        string[] labTest = temp.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                        Klab_LabTestOrderDate.Text = labTest[0];
                        Klab_LabTestName.Text = labTest[1];

                        List<string> labTestDetails = db.GetLabTestDetails(currentVisitID, currentLabTestID);

                        if (labTestDetails != null)
                        {
                            if (labTestDetails.Count > 0)
                            {
                                Klab_LabTestDescription.Text = labTestDetails[0];
                                Klab_LabTestDoctorName.Text = labTestDetails[1] + " " + labTestDetails[2];
                                Klab_LabTestResult.Text = labTestDetails[3];
                            }
                            else
                                MessageBox.Show("Podano nieprawidłowe ID wizyty lub nieprawidłowe ID badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                            MessageBox.Show("Wystąpił błąd podczas pobierania szczegółów badania laboratoryjnego.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                        MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako Niesprawdzone.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Klab_Save_Click(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                Button s = (Button)sender;
                bool? save = db.SaveLabTest(currentVisitID, currentLabTestID, DateTime.Now, null, (bool?)s.Tag, Klab_LabTestRemarks.Text);

                if (save == true)
                {
                    int labTestNumber = currentLabTest;

                    //Dekrementacja właściwości Tag przycisków w wierszach kolejnych po usuwanym.
                    for (int i = currentLabTest * 2 + 1; i <= Klab_LabTestsList.Items.Count; i += 2)
                    {
                        Button button = (Button)Klab_LabTestsList.Items[i];
                        button.Tag = labTestNumber;

                        ++labTestNumber;
                    }

                    //Usunięcie wybranego wiersza:
                    labTestNumber = currentLabTest * 2 - 1;
                    //  druga kolumna
                    Klab_LabTestsList.Items.RemoveAt(labTestNumber);
                    --labTestNumber;
                    //  pierwsza kolumna.
                    Klab_LabTestsList.Items.RemoveAt(labTestNumber);

                    //Zaktualizowanie struktury zawierającej liczbę niewykonanych badań dla każdej wizyty.
                    if (doneLabTests[currentVisitID] == 1)
                        doneLabTests.Remove(currentVisitID);
                    else
                        --doneLabTests[currentVisitID];

                    //Usuwanie szczegółowych informacji o zapisanym badaniu i usunięcie wyników tego badania.
                    Klab_LabTestOrderDate.Text = Klab_LabTestName.Text = Klab_LabTestDescription.Text = Klab_LabTestDoctorName.Text = Klab_LabTestResult.Text = "";
                    Klab_LabTestRemarks.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = false;
                }
                else if (save == false)
                    MessageBox.Show("Wystąpił błąd podczas próby zapisu uwag do badania laboratoryjnego i nie zostały one zapisane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Podano nieprawidłowe dane dla funkcji zapisu badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Klab_Back_Click(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                if (db.CheckLabTest(currentVisitID, currentLabTestID, null))
                {
                    //Usuwanie szczegółowych informacji o badaniu i usunięcie danych wprowadzonych do pola na wyniki tego badania.
                    Klab_LabTestOrderDate.Text = Klab_LabTestName.Text = Klab_LabTestDescription.Text = Klab_LabTestDoctorName.Text = Klab_LabTestResult.Text = "";
                    Klab_LabTestRemarks.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = false;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako W trakcie sprawdzania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        /// <summary>
        /// Metoda pobierająca dane z bazy i inicjalizująca kontrolki odpowiedzialne za ich prezentację.
        /// </summary>
        void GetDataFromDB()
        {
            // --> Tworzenie listy wykonanych badań laboratoryjnych
            Dictionary<int, List<string>> tests = db.GetLabTests(true);

            if (tests != null && tests.Count > 0)
            {
                int labTestNumber = 0;

                foreach (var t in tests)
                {
                    doneLabTests.Add(t.Key, (byte)t.Value.Count);

                    foreach (string str in t.Value)
                    {
                        ++labTestNumber;

                        ListBoxItem item = new ListBoxItem();
                        item.Content = str;

                        Klab_LabTestsList.Items.Add(item);

                        Button button = new Button();
                        button.Name = "Klab_CheckLabTest";
                        button.Content = "Sprawdź";
                        button.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);
                        button.Tag = labTestNumber;
                        button.Click += Klab_CheckLabTest_Click;

                        Klab_LabTestsList.Items.Add(button);
                    }

                    Klab_Save.Tag = (bool?)true;
                    Klab_Cancel.Tag = (bool?)false;
                }
            }
            else
            {
                Klab_LabTestsList.IsEnabled = false;
                Klab_LabTestsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                Klab_LabTestsList.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                Klab_LabTestsList.Items.Add(new ListBoxItem().Content = "Brak badań do końca życia!");

                if (tests == null)
                    MessageBox.Show("Wystąpił błąd podczas pobierania listy badań do sprawdzenia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            // <-- Tworzenie listy wykonanych badań laboratoryjnych
        }
    }
}
