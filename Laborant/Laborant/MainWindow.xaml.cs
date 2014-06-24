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
        bool done;                                    // determinuje czy laborant jest w trakcie wykonywania badania/edycji wykonanego badania (true) czy nie (false)
        Dictionary<int, List<byte>> labTestsAtVisit;  // kolekcja par <ID wizyty, liczba badań laboratoryjnych zleconych w trakcie tej wizyty> (tylko wizyty z dodatnią liczbą zleconych badań lab.)


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
                    labTestsAtVisit = new Dictionary<int,List<byte>>();
                    GetDataFromDB();
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
            //db.Dispose(); TO NIE DZIAUA!!!!!!!!!!!111111111oneoneeleven1
            db = null;

            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):
            ClearLabTestsLists();
            Lab_LabTestResult.Clear();

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

        // OBSLUGA KONTROLEK

        /// <summary>
        /// Obsługa kliknięcia przycisku "Wykonaj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Back_Click(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                if (db.ExecuteLabTest(currentVisitID, currentLabTestID, false))
                {
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
        /// Obsługa kliknięcia przycisku "Szukaj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            ClearLabTestsLists();
            GetDataFromDB();
        }

        private void clearFilterButton_Click(object sender, RoutedEventArgs e)
        {
            firstNameBox.Text = "";
            surnameBox.Text = "";
            stateComboBox.SelectedIndex = 0;
            DateTo.SelectedDate = null;
            DateFrom.SelectedDate = null;
        }

        private void DateFromChanged(object sender, RoutedEventArgs e) { }
        private void DateToChanged(object sender, RoutedEventArgs e) { }

        /// <summary>
        /// Czyści listę niewykonanych badań laboratoryjnych, ew. aktywuje ją i przywraca wyrównanie elementów do wartości domyślnych.
        /// Czyści również zbiór ID wizyt i ich list ID niewykonanych badań.
        /// </summary>
        private void ClearLabTestsLists()
        {
            Lab_LabTestsList.Items.Clear();
            Lab_LabTestsList.IsEnabled = true;
            Lab_LabTestsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Top;

            labTestsAtVisit.Clear();
        }



        /// <summary>
        /// Obsługa kliknięcia przycisków "Zatwierdź" i "Anuluj".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Lab_Save_Click(object sender, RoutedEventArgs e)
        {
            if (done == true)
            {
                Button s = (Button)sender;
                bool? save = db.SaveLabTest(currentVisitID, currentLabTestID, DateTime.Now, Lab_LabTestResult.Text, (bool?)s.Tag);

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
                    if (labTestsAtVisit[currentVisitID].Count == 1)
                        labTestsAtVisit.Remove(currentVisitID);
                    else
                        labTestsAtVisit[currentVisitID].Remove(currentLabTestID);

                    //Usuwanie szczegółowych informacji o zapisanym badaniu i usunięcie wyników tego badania.
                    Lab_LabTestOrderDate.Text = Lab_LabTestName.Text = Lab_LabTestDescription.Text = Lab_LabTestDoctorName.Text = "";
                    Lab_LabTestResult.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = false;

                    //Dezaktywacja przycisków
                    Lab_Save.IsEnabled = false;
                    Lab_Cancel.IsEnabled = false;
                }
                else if (save == false)
                    MessageBox.Show("Wystąpił błąd podczas próby zapisu wyniku badania laboratoryjnego i nie został on zapisany.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Podano nieprawidłowe dane dla funkcji zapisu wyników badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        /// <summary>
        /// Obsługa kliknięcia przycisków "Otwórz" z listy niewykonanych badań.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Lab_ExecuteLabTest_Click(object sender, RoutedEventArgs e)
        {
            //TODO: Poprawić warunek tak, żeby czytał to nie z ComboBoxa, a z zadania
            if (stateComboBox.Text == "Zlecone")
            {
                Lab_Save.IsEnabled = true;
                Lab_Cancel.IsEnabled = true;
            }
            else
            {
                Lab_Save.IsEnabled = false;
                Lab_Cancel.IsEnabled = false;
            }

            if (labTestsAtVisit.Count > 0)
                {
                    Button button = (Button)sender;
                    int num = 0;
                    currentLabTest = (int)button.Tag;

                    foreach (var t in labTestsAtVisit)
                    {
                        num += t.Value.Count;

                        if (num >= currentLabTest)
                        {
                            currentVisitID = t.Key;
                            break;
                        }

                    }

                    currentLabTestID = labTestsAtVisit[currentVisitID][labTestsAtVisit[currentVisitID].Count - 1 - (num - currentLabTest)];
                    done = true;

                    
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

            Dictionary<int, List<string>> tests = db.GetLabTests(firstNameBox.Text, surnameBox.Text, stateComboBox.Text, DateFrom.SelectedDate, DateTo.SelectedDate);

            if (tests != null && tests.Count > 0)
            {
                int labTestNumber = 0; //int wystarczy, hardcore na 4 mld niewykonanych badań lab. się nawet w Polsce nie zdarza :D

                //Zapisywanie list ID badań dla każdej wizyty
                    foreach (var t in tests)
                    {
                        labTestsAtVisit.Add(t.Key, db.GetLabTestsIDs(t.Key));
                        List<byte> dojpa = new List<byte>();
                        dojpa.Add(2);
                        labTestsAtVisit.Add(3,dojpa);

                        foreach (string str in t.Value)
                        {
                            ++labTestNumber;

                            ListBoxItem item = new ListBoxItem();
                            item.Content = str;
                            item.Margin = new Thickness(0.0, 0.0, 10.0, 0.0);

                            Lab_LabTestsList.Items.Add(item);

                            /* Każdy kolejny przycisk-klon ma w tagu zapisany nr badania, przy którym się znajduje, czyli w sumie numer wiersza listy, w którym się znajduje.
                                * Później foreach przez labTestsAtVisit dodając do kupy wartości, dopóki suma mniejsza od tagu tego przycisku - reszty chyba nie trzeba tłumaczyć.
                                * Wiemy w której wizycie to zostało zlecone (labTestsAtView.Key), a po wykonaniu 2 operacji odejmowania wiemy które to konkretnie badanie - p. Lab_ExecuteLabTest_Click*/
                            Button button = new Button();

                            button.Name = "Lab_ExecuteLabTest";
                            button.Content = "Otwórz";
                            button.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);
                            button.Tag = labTestNumber;
                            button.Click += Lab_ExecuteLabTest_Click;
                            Lab_LabTestsList.Items.Add(button);
                        }
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
            // <-- Tworzenie listy zleconych, niezrealizowanych badań laboratoryjnych
        }

    }
}

