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
using System.Text.RegularExpressions;
using DBClient;
using System.Data;


namespace Administrator
{

/// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private DBClient.DBClient db;
        List<RejestratorkaData> rejlist;
        List<LekarzData> leklist;
        List<LaborantData> lablist;
        List<Sl_badData> badlist;
        List<Sl_specData> speclist;

        List<RejestratorkaData> rejmodlist;
        List<LekarzData> lekmodlist;
        List<LaborantData> labmodlist;
        List<Sl_badData> badmodlist;
        List<Sl_specData> specmodlist;


        public MainWindow()
        {
            InitializeComponent();

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();
                    getDataFromdb();
                    RejestratorkaTabItem.Focus();
                    break;
                }
            }
        }

        /// <summary>
        /// pobiera dane z bazy i zapelnia listy
        /// </summary>
        private void getDataFromdb()
        {
            rejlist = db.GetRejestratorkas();
            lablist = db.GetLaborants();
            leklist = db.GetLekarzs();
            badlist = db.GetSl_badans();
            speclist = db.GetSl_specs();

            rejmodlist = rejlist.ConvertAll(item => new RejestratorkaData(item));
            labmodlist = lablist.ConvertAll(item => new LaborantData(item));
            lekmodlist = leklist.ConvertAll(item => new LekarzData(item));
            badmodlist = badlist.ConvertAll(item => new Sl_badData(item));
            specmodlist = speclist.ConvertAll(item => new Sl_specData(item));

            if (rejlist != null && rejlist.Count > 0)
            {
                RejestratorkaGrid.ItemsSource = rejlist;
            }
            else
            {
                System.Windows.MessageBox.Show("wystąpił błąd podczas łączenia się z bazą. Dane rejestratoki będą niedostępne",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

                RejestratorkaGrid.IsEnabled = false;
            }

            if (lablist != null && lablist.Count > 0)
            {
                LaborantGrid.ItemsSource = lablist;
            }
            else
            {
                System.Windows.MessageBox.Show("wystąpił błąd podczas łączenia się z bazą. Dane laboranta będą niedostępne",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

                LaborantGrid.IsEnabled = false;
            }

            if (leklist != null && leklist.Count > 0)
            {
                LekarzGrid.ItemsSource = leklist;
            }
            else
            {
                System.Windows.MessageBox.Show("wystąpił błąd podczas łączenia się z bazą.Dane lekarza będą niedostępne",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

                LekarzGrid.IsEnabled = false;
            }

            if (badlist != null && badlist.Count > 0)
            {
                Sl_badGrid.ItemsSource = badlist;
            }
            else
            {
                System.Windows.MessageBox.Show("wystąpił błąd podczas łączenia się z bazą.Dane slownika badan będą niedostępne",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

                Sl_badGrid.IsEnabled = false;
            }

            if (speclist != null && speclist.Count > 0)
            {
                Sl_specGrid.ItemsSource = speclist;
            }
            else
            {
                System.Windows.MessageBox.Show("wystąpił błąd podczas łączenia się z bazą.Dane slownika specjalizacji będą niedostępne",
                                "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);

                Sl_specGrid.IsEnabled = false;
            }

        }

        /// <summary>
        /// Obsługa wciśnięcia przycisku nowy
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddEntryItem_Click(object sender, RoutedEventArgs e)
        { 
            //dodawanie rejestratorki
            if (RejestratorkaTabItem.IsFocused)
            {
                AddRejestratorkaDialog dialog = new AddRejestratorkaDialog();
                bool? dialogResult = dialog.ShowDialog();
                if (dialogResult == true)
                {
                    
                    if ( db.AddRejestratorka(dialog.login,dialog.haslo,(DateTime)dialog.wygasa,dialog.imie,dialog.nazwisko) )
                    {
                        System.Windows.MessageBox.Show("Rejestratorka została pomyślnie dodany do bazy danych.", "Dodanie nowej rejestratorki", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Nie udało się dodać nowej rejestratorki do bazy danych!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            //dodawanie lekarza
            if (LekarzTabItem.IsFocused)
            {
                AddLekarzDialog dialog = new AddLekarzDialog(ref speclist);
                bool? dialogResult = dialog.ShowDialog();
                if (dialogResult == true)
                {

                    if (db.AddLekarz(dialog.login, dialog.haslo, (DateTime)dialog.wygasa, dialog.imie, dialog.nazwisko,dialog.kod_spec))
                    {
                        System.Windows.MessageBox.Show("Lekarz został pomyślnie dodany do bazy danych.", "Dodanie nowego lekarza", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Nie udało się dodać nowego lekarza do bazy danych!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            //dodawanie laboranta
            if (LaborantTabItem.IsFocused)
            {
                AddLaborantDialog dialog = new AddLaborantDialog();
                bool? dialogResult = dialog.ShowDialog();
                if (dialogResult == true)
                {

                    if (db.AddLaborant(dialog.login, dialog.haslo, (DateTime)dialog.wygasa, dialog.imie, dialog.nazwisko, (bool)dialog.kier))
                    {
                        System.Windows.MessageBox.Show("Laborant został pomyślnie dodany do bazy danych.", "Dodanie nowego laboranta", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Nie udało się dodać nowego laboranta do bazy danych!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            //dodawanie do slownika badan
            if (BadaniaTabItem.IsFocused)
            {
                AddBadDialog dialog = new AddBadDialog();
                bool? dialogResult = dialog.ShowDialog();
                if (dialogResult == true)
                {

                    if (db.AddSl_badan(dialog.nazwa,dialog.opis,(bool)dialog.lab))
                    {
                        System.Windows.MessageBox.Show("Badanie zostało pomyślnie dodany do bazy danych.", "Dodanie nowego badania", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Nie udało się dodać nowego badania do bazy danych!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            //dodawanie do slownika specjalizacji
            if (SpecTabItem.IsFocused)
            {
                AddSpecDialog dialog = new AddSpecDialog();
                bool? dialogResult = dialog.ShowDialog();
                if (dialogResult == true)
                {

                    if (db.AddSl_spec(dialog.nazwa))
                    {
                        System.Windows.MessageBox.Show("Specjalizacja została pomyślnie dodana do bazy danych.", "Dodanie nowej specjalizacji", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Nie udało się dodać nowej specjalizacji do bazy danych!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }

            getDataFromdb();
        }

        /// <summary>
        /// obsługa przycisku zapisz zmiany
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveChangesItem_Click(object sender, RoutedEventArgs e)
        {
            List<RejestratorkaData> rejToBeRemoved = new List<RejestratorkaData>();
            List<LekarzData> lekToBeRemoved = new List<LekarzData>();
            List<LaborantData> labToBeRemoved = new List<LaborantData>();
            List<Sl_badData> badToBeRemoved = new List<Sl_badData>();
            List<Sl_specData> specToBeRemoved = new List<Sl_specData>();

            //szukanie wierwszy takich samych (nie zmodyfikowanych)

            foreach (RejestratorkaData r in rejmodlist)
            {
                RejestratorkaData temp = rejlist.Find(x => x.id_rej == r.id_rej);

                if (temp != null)
                {
                    if( temp.haslo.Length == r.haslo.Length && temp.imie == r.imie && temp.nazwisko == r.nazwisko && temp.login == r.login && temp.wygasa == r.wygasa)
                    {
                        rejToBeRemoved.Add(temp);
                    }
                }
            }

            foreach (LekarzData r in lekmodlist)
            {
                LekarzData temp = leklist.Find(x => x.id_lek == r.id_lek);
                if (temp != null)
                {
                    if( temp.haslo.Length == r.haslo.Length && temp.imie == r.imie && temp.nazwisko == r.nazwisko && temp.login == r.login && temp.wygasa == r.wygasa && temp.kod_spec == r.kod_spec)
                    {
                        lekToBeRemoved.Add(temp);
                    }
                }
            }

            foreach (LaborantData r in labmodlist)
            {
                LaborantData temp = lablist.Find(x => x.id_lab == r.id_lab);
                if (temp != null)
                {
                    if( temp.haslo.Length == r.haslo.Length && temp.imie == r.imie && temp.nazwisko == r.nazwisko && temp.login == r.login && temp.wygasa == r.wygasa && temp.kier == r.kier)
                    {
                        labToBeRemoved.Add(temp);
                    }
                }
            }

            foreach (Sl_badData r in badmodlist)
            {
                Sl_badData temp = badlist.Find(x => x.kod == r.kod);
                if (temp != null)
                {
                    if(temp.lab == r.lab && temp.nazwa == r.nazwa && temp.opis == r.opis)
                    {
                        badToBeRemoved.Add(temp);
                    }
                }
            }

            foreach (Sl_specData r in specmodlist)
            {
                Sl_specData temp = speclist.Find(x => x.kod_spec == r.kod_spec);
                if (temp != null)
                {
                    if(temp.nazwa == r.nazwa)
                    {
                        specToBeRemoved.Add(temp);
                    }
                }
            }

            //usuwanie wierszy niezmodyfikowanych z list pozmienianych

            foreach( RejestratorkaData r in rejToBeRemoved)
            {
                rejlist.Remove(r);
            }
            foreach (LekarzData r in lekToBeRemoved)
            {
                leklist.Remove(r);
            }
            foreach (LaborantData r in labToBeRemoved)
            {
                lablist.Remove(r);
            }
            foreach (Sl_specData r in specToBeRemoved)
            {
                speclist.Remove(r);
            }
            foreach (Sl_badData r in badToBeRemoved)
            {
                badlist.Remove(r);
            }

            // powinny zostac same wiersze zmodyfikowane w oryginalnych listach teraz nalezy zupdejtowac baze

            foreach( RejestratorkaData r in rejlist)
            {
                db.UpdateRejestratorka(r.id_rej,r.imie,r.nazwisko,r.login,r.haslo,r.wygasa);
            }
            foreach (LekarzData r in leklist)
            {
                db.UpdateLekarz(r.id_lek,r.imie,r.nazwisko,r.login,r.haslo,r.wygasa,r.kod_spec);
            }
            foreach (LaborantData r in lablist)
            {
                db.UpdateLaborant(r.id_lab,r.imie,r.nazwisko,r.login,r.haslo,r.wygasa,r.kier);
            }
            foreach (Sl_specData r in speclist)
            {
                db.UpdateSl_spec(r.kod_spec,r.nazwa);
            }
            foreach (Sl_badData r in badlist)
            {
                db.UpdateSl_badan(r.kod,r.nazwa,r.opis,r.lab);
            }

            //odswiezenie wszystkich danych i przygotowanie wszystkiego do nastepnych operacji
            //tl;dr dawacz baze na nowo

            getDataFromdb();
        }

        /// <summary>
        /// obsługa przycisku about
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
            Title = "Administrator";
            Visibility = System.Windows.Visibility.Hidden;
            db.ResetClient();
            db.Dispose();
            db = null;

            rejlist.Clear();
            leklist.Clear();
            lablist.Clear();
            badlist.Clear();
            speclist.Clear();

            rejmodlist.Clear();
            lekmodlist.Clear();
            labmodlist.Clear();
            badmodlist.Clear();
            specmodlist.Clear();

            DataView src;
            if (RejestratorkaGrid.ItemsSource != null)
            {
                src = (DataView)RejestratorkaGrid.ItemsSource;
                src.Dispose();
                RejestratorkaGrid.ItemsSource = null;
            }

            if (LaborantGrid.ItemsSource != null)
            {
                src = (DataView)LaborantGrid.ItemsSource;
                src.Dispose();
                LaborantGrid.ItemsSource = null;
            }

            if (LekarzGrid.ItemsSource != null)
            {
                src = (DataView)LekarzGrid.ItemsSource;
                src.Dispose();
                LekarzGrid.ItemsSource = null;
            }

            if (Sl_badGrid.ItemsSource != null)
            {
                src = (DataView)Sl_badGrid.ItemsSource;
                src.Dispose();
                Sl_badGrid.ItemsSource = null;
            }

            if (Sl_specGrid.ItemsSource != null)
            {
                src = (DataView)Sl_specGrid.ItemsSource;
                src.Dispose();
                Sl_specGrid.ItemsSource = null;
            }
            
            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();
                    getDataFromdb();
                    RejestratorkaTabItem.Focus();
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
        
        private void LekarzGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if ( (string)e.Column.Header == "Specjalizacja")
            {
                TextBox box = e.EditingElement as TextBox;
                string toCheck = box.Text;
                int result;
                if(int.TryParse(toCheck,out result) == false)
                {
                    MessageBox.Show("Wartość musi być wartością numeryczną!");
                    e.Cancel = true;
                }
            }
        }
    }
}
