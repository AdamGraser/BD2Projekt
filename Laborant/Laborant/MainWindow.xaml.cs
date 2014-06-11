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
        private DBClient.DBClient db;          // klient bazy danych
        int currentVisitID;                    // przechowuje ID wizyty podczas której zlecone zostało badanie currentLabTestID
        byte currentLabTestID;                 // przechowuje ID badania lab. aktualnie wykonywanego przez laboranta lub -1 jeśli laborant nie wykonuje aktualnie żadnego badania lab.
        int currentLabTest;                    // przechowuje sumaryczny nr badania laboratoryjnego (sumowane są wszystkie niewykonane badania, zlecone we wszystkich wizytach, w kolejności rosnącej)
        bool done;                             // determinuje czy laborant jest w trakcie wykonywania badania/edycji wykonanego badania (true) czy nie (false)
        Dictionary<int, byte> labTestsAtVisit; // kolekcja par <ID wizyty, liczba badań laboratoryjnych zleconych w trakcie tej wizyty> (tylko wizyty z dodatnią liczbą zleconych badań lab.)
        Dictionary<int, byte> labTestsAtVisit1; // kolekcja par <ID wizyty, liczba badań laboratoryjnych zleconych w trakcie tej wizyty> (tylko wizyty z dodatnią liczbą zleconych badań lab.)


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
                    labTestsAtVisit = new Dictionary<int, byte>();
                    labTestsAtVisit1 = new Dictionary<int, byte>();
                    GetDataFromDB(true);
                    GetDataFromDB(false);
                    break;
                }                
            }

            Lab_Save.Tag = (bool?)null;
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
            db = null;

            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):
            Lab_LabTestsList.Items.Clear();
            Lab_LabTestsList1.Items.Clear();
            Lab_LabTestResult.Clear();
            Lab_LabTestResult1.Clear();

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();
                    GetDataFromDB(true);
                    GetDataFromDB(false);
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


        // OBSLUGA KONTROLEK Z PIERWSZEJ ZAKLADKI

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Back_Click(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                byte result = db.ExecuteLabTest(currentVisitID, currentLabTestID, null);
                if (result != 0)
                {
                    currentLabTestID = result;

                    //Usuwanie szczegółowych informacji o badaniu i usunięcie danych wprowadzonych do pola na wyniki tego badania.
                    Lab_LabTestOrderDate.Text = Lab_LabTestName.Text = Lab_LabTestDescription.Text = Lab_LabTestDoctorName.Text = "";
                    Lab_LabTestResult.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = false;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako W trakcie wykonywania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Refresh_Click(object sender, RoutedEventArgs e)
        {
            Lab_LabTestsList.Items.Clear();
            GetDataFromDB(true);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Save_Click(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                Button s = (Button)sender;
                bool? save = db.SaveLabTest(currentVisitID, currentLabTestID, DateTime.Now, Lab_LabTestResult.Text, (bool?)s.Tag, null);

                if (save == true)
                {
                    int labTestNumber = currentLabTest;

                    //Dekrementacja właściwości Tag przycisków w wierszach kolejnych po usuwanym.
                    for (int i = currentLabTest * 2 + 1; i <= Lab_LabTestsList.Items.Count; i += 2)
                    {
                        Button button = (Button)Lab_LabTestsList.Items[i];
                        button.Tag = labTestNumber;

                        ++labTestNumber;
                    }

                    //Usunięcie wybranego wiersza:
                    labTestNumber = currentLabTest * 2 - 1;
                    //  druga kolumna
                    Lab_LabTestsList.Items.RemoveAt(labTestNumber);
                    --labTestNumber;
                    //  pierwsza kolumna.
                    Lab_LabTestsList.Items.RemoveAt(labTestNumber);

                    //Zaktualizowanie struktury zawierającej liczbę niewykonanych badań dla każdej wizyty.
                    if (labTestsAtVisit[currentVisitID] == 1)
                        labTestsAtVisit.Remove(currentVisitID);
                    else
                        --labTestsAtVisit[currentVisitID];

                    //Usuwanie szczegółowych informacji o zapisanym badaniu i usunięcie wyników tego badania.
                    Lab_LabTestOrderDate.Text = Lab_LabTestName.Text = Lab_LabTestDescription.Text = Lab_LabTestDoctorName.Text = "";
                    Lab_LabTestResult.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = false;
                }
                else if (save == false)
                    MessageBox.Show("Wystąpił błąd podczas próby zapisu wyniku badania laboratoryjnego i nie został on zapisany.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Podano nieprawidłowe dane dla funkcji zapisu wyników badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // OBSLUGA KONTROLEK Z DRUGIEJ ZAKLADKI

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Back_Click1(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                byte result = db.ExecuteLabTest(currentVisitID, currentLabTestID, null);
                if (result != 0)
                {
                    currentLabTestID = result;
                    //Usuwanie szczegółowych informacji o badaniu i usunięcie danych wprowadzonych do pola na wyniki tego badania.
                    Lab_LabTestOrderDate.Text = Lab_LabTestName1.Text = Lab_LabTestDescription1.Text = Lab_LabTestDoctorName1.Text = "";
                    Lab_LabTestResult.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = false;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako W trakcie wykonywania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Refresh_Click1(object sender, RoutedEventArgs e)
        {
            Lab_LabTestsList1.Items.Clear();
            GetDataFromDB(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Save_Click1(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                Button s = (Button)sender;
                bool? save = db.SaveLabTest(currentVisitID, currentLabTestID, DateTime.Now, Lab_LabTestResult.Text, (bool?)s.Tag, null);

                if (save == true)
                {
                    int labTestNumber = currentLabTest;

                    //Usuwanie szczegółowych informacji o zapisanym badaniu i usunięcie wyników tego badania.
                    Lab_LabTestOrderDate1.Text = Lab_LabTestName1.Text = Lab_LabTestDescription1.Text = Lab_LabTestDoctorName1.Text = "";
                    Lab_LabTestResult1.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = false;
                }
                else if (save == false)
                    MessageBox.Show("Wystąpił błąd podczas próby zapisu wyniku badania laboratoryjnego i nie został on zapisany.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Podano nieprawidłowe dane dla funkcji zapisu wyników badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // KONIEC OBSLUGI KONTROLEK

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
        /// Metoda pobierająca dane z bazy i inicjalizująca kontrolki odpowiedzialne za ich prezentację.
        /// <param name="undone">Jeśli "true", pobierane są badania niewykonane. Jeśli false - wykonane przez zalogowanego laboranta.</param>
        /// </summary>
        private void GetDataFromDB(bool undone)
        {
            // --> Tworzenie listy zleconych, niezrealizowanych badań laboratoryjnych
            Dictionary<int, List<string>> tests = db.GetLabTests(!undone);

            if (tests != null && tests.Count > 0)
            {
                int labTestNumber = 0; //int wystarczy, hardcore na 4 mld niewykonanych badań lab. się nawet w Polsce nie zdarza :D

                foreach (var t in tests)
                {
                    //Później potrzebne będzie tylko w trakcie której wizyty ile badań zlecono.
                    if (undone)
                        labTestsAtVisit.Add(t.Key, (byte)t.Value.Count);
                    else
                        labTestsAtVisit1.Add(t.Key, (byte)t.Value.Count);

                    foreach (string str in t.Value)
                    {
                        ++labTestNumber;

                        ListBoxItem item = new ListBoxItem();
                        item.Content = str;
                        item.Margin = new Thickness(0.0, 0.0, 10.0, 0.0);

                        if (undone)
                            Lab_LabTestsList.Items.Add(item);
                        else
                            Lab_LabTestsList1.Items.Add(item);

                        /* Każdy kolejny przycisk-klon ma w tagu zapisany nr badania, przy którym się znajduje, czyli w sumie numer wiersza listy, w którym się znajduje.
                         * Później foreach przez labTestsAtVisit dodając do kupy wartości, dopóki suma mniejsza od tagu tego przycisku - reszty chyba nie trzeba tłumaczyć.
                         * Wiemy w której wizycie to zostało zlecone (labTestsAtView.Key), a po wykonaniu 2 operacji odejmowania wiemy które to konkretnie badanie - p. Lab_ExecuteLabTest_Click*/
                        Button button = new Button();
                        if (undone)
                        {
                            button.Name = "Lab_ExecuteLabTest";
                            button.Content = "Wykonaj";
                            button.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);
                            button.Tag = labTestNumber;
                            button.Click += Lab_ExecuteLabTest_Click;
                            Lab_LabTestsList.Items.Add(button);
                        }
                        else
                        {
                            button.Name = "Lab_EditLabTest";
                            button.Content = "Edytuj";
                            button.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);
                            button.Tag = labTestNumber;
                            button.Click += Lab_EditLabTest_Click;
                            Lab_LabTestsList1.Items.Add(button);
                        }
                    }
                }
            }
            else
            {
                Lab_LabTestsList.IsEnabled = false;
                Lab_LabTestsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                Lab_LabTestsList.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                Lab_LabTestsList.Items.Add(new ListBoxItem().Content = "Brak badań do końca życia!");

                if (tests == null)
                    MessageBox.Show("Wystąpił błąd podczas pobierania listy badań do wykonania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            // <-- Tworzenie listy zleconych, niezrealizowanych badań laboratoryjnych
        }

        void Lab_ExecuteLabTest_Click(object sender, RoutedEventArgs e)
        {
            if (done == false)
            {
                if (labTestsAtVisit.Count > 0)
                {
                    Button button = (Button)sender;
                    int num = 0;
                    currentLabTest = (int)button.Tag;

                    foreach (var t in labTestsAtVisit)
                    {
                        num += t.Value;

                        if (num >= currentLabTest)
                        {
                            currentVisitID = t.Key;
                            break;
                        }

                    }

                    currentLabTestID = (byte)(labTestsAtVisit[currentVisitID] - (num - currentLabTest));
                    done = true;

                    byte result = db.ExecuteLabTest(currentVisitID, currentLabTestID, null);
                    if (result != 0)
                    {
                        currentLabTestID = result;
                        /* Przypominam: labTestNumber to jednocześnie nr wiersza na liście badań. Elementy w ListBox'ie są numerowane od 0, to zawsze integery, niezależnie od
                         * layout'u listy (zawsze od lewej do prawej, z góry na dół).
                         * [string do wydobycia][button]<nr wiersza> no i wiadomo, te liczby to indeksy właśnie:
                         * [0][1]<1>: 1 * 2 - 2 = 0
                         * [2][3]<2>: 2 * 2 - 2 = 2
                         * ...
                         * [12][13]<7>: 7 * 2 - 2 = 12
                         * Jak widać, poniższy wzór zawsze da w wyniku indeks elementu w lewej kolumnie.*/
                        ListBoxItem item = (ListBoxItem)Lab_LabTestsList.Items.GetItemAt(currentLabTest * 2 - 2);
                        string temp = (string)item.Content;

                        string[] labTest = temp.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                        Lab_LabTestOrderDate.Text = labTest[0];
                        Lab_LabTestName.Text = labTest[1];

                        List<string> labTestDetails = db.GetLabTestDetails(currentVisitID, currentLabTestID);

                        if (labTestDetails != null)
                        {
                            if (labTestDetails.Count > 0)
                            {
                                Lab_LabTestDescription.Text = labTestDetails[0];
                                Lab_LabTestDoctorName.Text = labTestDetails[1] + " " + labTestDetails[2];
                            }
                            else
                                MessageBox.Show("Podano nieprawidłowe ID wizyty lub nieprawidłowe ID badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                            MessageBox.Show("Wystąpił błąd podczas pobierania szczegółów badania laboratoryjnego.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                        MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako Niewykonane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }


        void Lab_EditLabTest_Click(object sender, RoutedEventArgs e)
        {
            if (done == false)
            {
                if (labTestsAtVisit1.Count > 0)
                {
                    Button button = (Button)sender;
                    int num = 0;
                    currentLabTest = (int)button.Tag;

                    foreach (var t in labTestsAtVisit)
                    {
                        num += t.Value;

                        if (num >= currentLabTest)
                        {
                            currentVisitID = t.Key;
                            break;
                        }

                    }

                    currentLabTestID = (byte)(labTestsAtVisit1[currentVisitID] - (num - currentLabTest));
                    done = true;

                    byte result = db.ExecuteLabTest(currentVisitID, currentLabTestID, null);
                    if (result != 0)
                    {
                        currentLabTestID = result;
                        /* Przypominam: labTestNumber to jednocześnie nr wiersza na liście badań. Elementy w ListBox'ie są numerowane od 0, to zawsze integery, niezależnie od
                         * layout'u listy (zawsze od lewej do prawej, z góry na dół).
                         * [string do wydobycia][button]<nr wiersza> no i wiadomo, te liczby to indeksy właśnie:
                         * [0][1]<1>: 1 * 2 - 2 = 0
                         * [2][3]<2>: 2 * 2 - 2 = 2
                         * ...
                         * [12][13]<7>: 7 * 2 - 2 = 12
                         * Jak widać, poniższy wzór zawsze da w wyniku indeks elementu w lewej kolumnie.*/
                        ListBoxItem item = (ListBoxItem)Lab_LabTestsList1.Items.GetItemAt(currentLabTest * 2 - 2);
                        string temp = (string)item.Content;

                        string[] labTest = temp.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                        Lab_LabTestOrderDate1.Text = labTest[0];
                        Lab_LabTestName1.Text = labTest[1];

                        List<string> labTestDetails = db.GetLabTestDetails(currentVisitID, currentLabTestID);

                        if (labTestDetails != null)
                        {
                            if (labTestDetails.Count > 0)
                            {
                                Lab_LabTestDescription1.Text = labTestDetails[0];
                                Lab_LabTestDoctorName1.Text = labTestDetails[1] + " " + labTestDetails[2];
                            }
                            else
                                MessageBox.Show("Podano nieprawidłowe ID wizyty lub nieprawidłowe ID badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                        else
                            MessageBox.Show("Wystąpił błąd podczas pobierania szczegółów badania laboratoryjnego.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                        MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako Niewykonane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }

    }
}

