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

namespace Client
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBClient.DBClient db;                                   // klient bazy danych
        bool isTabLekLoaded, isTabLabLoaded, isTabKlabLoaded;   // determinują czy dana karta aplikacji została wybrana po raz pierwszy
        int currentVisitID;                                     // przechowuje ID wizyty, która właśnie się odbywa lub -1, jeśli lekarz nie przyjmuje teraz żadnego pacjenta/podczas której zlecone zostało badanie currentLabTestID
        byte currentRow;                                        // przechowuje nr wiersza listy zleconych badań laboratoryjnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co zlecone badanie
        byte currentLabTestID;                                  // przechowuje ID badania lab. aktualnie wykonywanego przez laboranta lub -1 jeśli laborant nie wykonuje aktualnie żadnego badania lab.
        int currentLabTest;                                     // przechowuje sumaryczny nr badania laboratoryjnego (sumowane są wszystkie niewykonane badania, zlecone we wszystkich wizytach, w kolejności rosnącej)
        Dictionary<int, byte> labTestsAtVisit;                  // kolekcja par <ID wizyty, liczba badań laboratoryjnych zleconych w trakcie tej wizyty> (tylko wizyty z dodatnią liczbą zleconych badań lab.)
        Dictionary<int, byte> doneLabTests;                     // kolekcja par <ID wizyty, liczba badań
        bool? done;                                             // determinuje czy rozpatrywane badanie laboratoryjne zostało wykonane czy nie (rozróżnienie dla użycia currentVisitID i currentLabTestID w kartach TabLab i TabKlab)



        /// <summary>
        /// Domyślny konstruktor. Inicjalizuje elementy interfejsu, klienta bazy danych oraz pola pomocnicze. Wypełnia odpowiednie elementy danymi.
        /// </summary>
        public MainWindow()
        {
            db = new DBClient.DBClient();
            isTabLekLoaded = isTabLabLoaded = isTabKlabLoaded = false;
            currentVisitID = -1;
            currentRow = 0;
            currentLabTestID = 0;
            labTestsAtVisit = new Dictionary<int, byte>();
            doneLabTests = new Dictionary<int, byte>();
            done = null;

            InitializeComponent();

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
                MessageBox.Show("Brak pacjentów w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            // <-- Tworzenie listy pacjentów.

            // --> Tworzenie listy lekarzy.
            List<string> doctors = db.GetDoctors();

            if (doctors != null && doctors.Count > 0)
            {
                foreach (string d in doctors)
                {
                    DoctorsList.Items.Add(new ComboBoxItem().Content = d);
                }
            }
            else
                MessageBox.Show("Brak lekarzy w bazie danych lub wystąpił błąd podczas łączenia się z bazą. Skontaktuj się z administratorem systemu.",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            // <-- Tworzenie listy lekarzy.

            //Ustawienie informacji o osobie zalogowanej w tytule okna.
            Title = "Zalogowano: Alicja Psikuta - rejestratorka";
        }



        private void AddPatient_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Nie da się dodać pacjenta.", "Niezaimplementowana funkcja", MessageBoxButton.YesNo, MessageBoxImage.Error);
        }



        private void RegisterVisit_Click(object sender, RoutedEventArgs e)
        {
            if(VisitDate.SelectedDate == null)
            {
                MessageBox.Show("Nie podano daty odbycia się wizyty!", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                if (db.AddVisit((DateTime)VisitDate.SelectedDate, (byte)(DoctorsList.SelectedIndex + 1), PatientsList.SelectedIndex + 1))
                {
                    MessageBox.Show("Zarejestrowano nową wizytę.", "Informacja", MessageBoxButton.OK, MessageBoxImage.Information);
                    PatientsList.SelectedIndex = -1;
                    DoctorsList.SelectedIndex = -1;
                    VisitDate.SelectedDate = null;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas rejestrowania wizyty i nie została ona zarejestrowana.", "Błąd rejestracji", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



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
                    MessageBox.Show("Pacjent o podanym numerze nie istnieje.", "Nieprawidłowy nr pacjenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PatientDetails.IsExpanded = false;
                }
            }
            else
            {
                MessageBox.Show("Wystąpił błąd podczas pobierania szczegółowych danych o pacjencie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                PatientDetails.IsExpanded = false;
            }
        }



        private void PatientDetails_Collapsed(object sender, RoutedEventArgs e)
        {
            PatientName.Text = "";
            PatientPesel.Text = "";
            PatientBirthDate.Text = "";
            PatientAddress.Text = "";
        }



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
                        MessageBox.Show("Pacjent o podanym numerze nie istnieje.", "Nieprawidłowy nr pacjenta", MessageBoxButton.OK, MessageBoxImage.Warning);
                        PatientDetails.IsExpanded = false;
                    }
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas pobierania szczegółowych danych o pacjencie.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PatientDetails.IsExpanded = false;
                }
            }
        }



        private void TabRej_GotFocus(object sender, RoutedEventArgs e)
        {
            Title = "Zalogowano: Alicja Psikuta - rejestratorka";
        }



        private void TabLek_GotFocus(object sender, RoutedEventArgs e)
        {
            Title = "Zalogowano: Henryk Mały - lekarz";

            if (TabLek.IsSelected && !isTabLekLoaded)
            {
                isTabLekLoaded = true;

                // --> Tworzenie listy wizyt dla bieżąco zalogowanego lekarza.
                Dictionary<int, string> visits = db.GetVisits(1);

                if (visits != null && visits.Count > 0)
                {
                    foreach (var v in visits)
                    {
                        ListBoxItem item = new ListBoxItem();
                        item.Content = v.Value;
                        item.Tag = v.Key;

                        VisitsList.Items.Add(item);
                    }
                }
                else
                {
                    VisitsList.IsEnabled = false;
                    VisitsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    VisitsList.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                    VisitsList.Items.Add(new ListBoxItem().Content = "Brak wizyt do końca życia!");

                    if(visits == null)
                        MessageBox.Show("Wystąpił błąd podczas pobierania listy wizyt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // <-- Tworzenie listy wizyt dla bieżąco zalogowanego lekarza.

                // --> Tworzenie listy badań laboratoryjnych.
                List<string> labTests = db.GetLabTestsNames();

                if (labTests != null && labTests.Count > 0)
                {
                    foreach (string l in labTests)
                    {
                        LabTestsList.Items.Add(new ComboBoxItem().Content = l);
                    }
                }
                else
                {
                    LabTestsList.IsEnabled = false;

                    LabTestDesc.Text = "Brak badań w bazie danych";
                    LabTestDesc.IsEnabled = false;

                    OrderLaboratoryTest.IsEnabled = false;

                    if(labTests == null)
                        MessageBox.Show("Wystąpił błąd podczas pobierania listy badań laboratoryjnych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // <-- Tworzenie listy badań laboratoryjnych.
            }
        }



        private void VisitsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentVisitID == -1 && currentLabTestID == 0)
            {
                ListBoxItem item = (ListBoxItem)VisitsList.SelectedItem;
                string temp = (string)item.Content;

                string[] visit = temp.Split(new char [] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                data_rej.Text = visit[0];
                nazwa_pac.Text = visit[1];
                stan.Text = "Nierozpoczęta";
            }
        }



        private void ChangeVisitState_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID == -1 && currentLabTestID == 0)
            {
                ListBoxItem item = (ListBoxItem)VisitsList.SelectedItem;
                currentVisitID = (int)item.Tag;

                if (db.ChangeVisitState(currentVisitID, false))
                {
                    stan.Text = "W trakcie realizacji";

                    VisitsList.Items.RemoveAt(VisitsList.SelectedIndex);
                }
                else
                {
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu wizyty i nie został on zmieniony.", "Błąd akceptacji wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
                    currentVisitID = -1;
                }
            }
        }



        private void SaveVisit_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID > -1 && currentLabTestID == 0)
            {
                if (db.SaveVisit(currentVisitID, opis.Text, (bool)PhysicalTestDone.IsChecked, diagnoza.Text))
                {
                    data_rej.Text = nazwa_pac.Text = stan.Text = "";

                    opis.Clear();
                    diagnoza.Clear();
                    PhysicalTestDone.IsChecked = false;

                    LabTestDesc.Clear();
                    LabTestsList.SelectedIndex = -1;

                    while (LaboratoryTests.Children.Count > 2)
                        LaboratoryTests.Children.RemoveAt(0);
                    
                    currentVisitID = -1;

                    VisitsList.SelectedIndex = -1;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zapisu szczegółów wizyty i nie zostały one zapisane.", "Błąd aktualizacji wizyty", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



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



        private void TabLab_GotFocus(object sender, RoutedEventArgs e)
        {
            Title = "Zalogowano: Bartosz Fatyga - laborant";

            if (TabLab.IsSelected && !isTabLabLoaded)
            {
                isTabLabLoaded = true;

                // --> Tworzenie listy zleconych, niezrealizowanych badań laboratoryjnych
                Dictionary<int, List<string>> tests = db.GetLabTests(false);

                if (tests != null && tests.Count > 0)
                {
                    int labTestNumber = 0; //int wystarczy, hardcore na 4 mld niewykonanych badań lab. się nawet w Polsce nie zdarza :D
                    
                    foreach (var t in tests)
                    {
                        //Później potrzebne będzie tylko w trakcie której wizyty ile badań zlecono.
                        labTestsAtVisit.Add(t.Key, (byte)t.Value.Count);

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
                            button.Content = "Wykonaj";
                            button.Padding = new Thickness(5.0, 0.0, 5.0, 0.0);
                            button.Tag = labTestNumber;
                            button.Click += Lab_ExecuteLabTest_Click;

                            Lab_LabTestsList.Items.Add(button);
                        }

                        Lab_Save.Tag = (bool?)null;
                        Lab_Cancel.Tag = (bool?)false;
                    }
                }
                else
                {
                    Lab_LabTestsList.IsEnabled = false;
                    Lab_LabTestsList.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                    Lab_LabTestsList.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                    Lab_LabTestsList.Items.Add(new ListBoxItem().Content = "Brak badań do końca życia!");

                    if(tests == null)
                        MessageBox.Show("Wystąpił błąd podczas pobierania listy badań do wykonania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // <-- Tworzenie listy zleconych, niezrealizowanych badań laboratoryjnych
            }
        }



        void Lab_ExecuteLabTest_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID == -1 && currentLabTestID == 0 && done == null)
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
                    done = false;

                    if (db.ExecuteLabTest(currentVisitID, currentLabTestID, 1))
                    {
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



        private void Lab_Save_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID > -1 && currentLabTestID > 0 && done == false)
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
                    done = null;
                }
                else if (save == false)
                    MessageBox.Show("Wystąpił błąd podczas próby zapisu wyniku badania laboratoryjnego i nie został on zapisany.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Podano nieprawidłowe dane dla funkcji zapisu wyników badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void Lab_Back_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID > -1 && currentLabTestID > 0 && done == false)
            {
                if (db.ExecuteLabTest(currentVisitID, currentLabTestID, null))
                {
                    //Usuwanie szczegółowych informacji o badaniu i usunięcie danych wprowadzonych do pola na wyniki tego badania.
                    Lab_LabTestOrderDate.Text = Lab_LabTestName.Text = Lab_LabTestDescription.Text = Lab_LabTestDoctorName.Text = "";
                    Lab_LabTestResult.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = null;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako W trakcie wykonywania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void TabKlab_GotFocus(object sender, RoutedEventArgs e)
        {
            Title = "Zalogowano: Adam Czarny - kierownik laboratorium";

            if (TabKlab.IsSelected && !isTabKlabLoaded)
            {
                isTabKlabLoaded = true;

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



        private void Klab_CheckLabTest_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID == -1 && currentLabTestID == 0 && done == null)
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



        private void Klab_Save_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID > -1 && currentLabTestID > 0 && done == true)
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
                    done = null;
                }
                else if (save == false)
                    MessageBox.Show("Wystąpił błąd podczas próby zapisu uwag do badania laboratoryjnego i nie zostały one zapisane.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                else
                    MessageBox.Show("Podano nieprawidłowe dane dla funkcji zapisu badania.", "Nieprawidłowe dane", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }



        private void Klab_Back_Click(object sender, RoutedEventArgs e)
        {
            if (currentVisitID > -1 && currentLabTestID > 0 && done == true)
            {
                if (db.CheckLabTest(currentVisitID, currentLabTestID, null))
                {
                    //Usuwanie szczegółowych informacji o badaniu i usunięcie danych wprowadzonych do pola na wyniki tego badania.
                    Klab_LabTestOrderDate.Text = Klab_LabTestName.Text = Klab_LabTestDescription.Text = Klab_LabTestDoctorName.Text = Klab_LabTestResult.Text = "";
                    Klab_LabTestRemarks.Clear();

                    currentLabTest = -1;
                    currentVisitID = -1;
                    currentLabTestID = 0;
                    done = null;
                }
                else
                    MessageBox.Show("Wystąpił błąd podczas zmiany stanu badania laboratoryjnego i jest ono wciąż oznaczone jako W trakcie sprawdzania.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
