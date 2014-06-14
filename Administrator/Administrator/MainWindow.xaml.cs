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

namespace Administrator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DBClient.DBClient db;



        public MainWindow()
        {
            InitializeComponent();

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();

                    //TODO: dopisać resztę (podobnie jak w kliencie rejestratorki i lekarza (lekarz wymagał kilku poprawek, nowa wersja pojawi się na repo dzisiaj 08.06.2014, ale wieczorem - Adam)).
                    //      resztę, czyli wrzucanie rzeczy do wszelkich list, czy innych combo box'ów, inicjalizacja zmiennych itp. - Adam
                    break;
                }
            }
        }



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
            Title = "Lekarz";
            Visibility = System.Windows.Visibility.Hidden;
            db = null;

            //TODO:
            //wyczyszczenie kontrolek i zmiennych zawierających ważne dane (dla bezpieczeństwa):

            while (true)
            {
                if (LogIn() == true)
                {
                    db = new DBClient.DBClient();
                    //TODO: dopisać resztę (podobnie jak w kliencie rejestratorki i lekarza (lekarz wymagał kilku poprawek, nowa wersja pojawi się na repo dzisiaj 08.06.2014, ale wieczorem - Adam)).
                    //      resztę, czyli wrzucanie rzeczy do wszelkich list, czy innych combo box'ów, inicjalizacja zmiennych itp. - Adam
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
            else if (hardExit.v == true) //zamknięcie okna logowania
                Environment.Exit(0);

            return false;
        }
    }
}
