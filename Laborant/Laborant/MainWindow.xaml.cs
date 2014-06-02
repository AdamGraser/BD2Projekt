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

namespace Laborant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoginWindow loginWindow = new LoginWindow();
            if (LogIn() == true)
            {

            }
            else
            {

            }
        }

        private void logoutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = System.Windows.Visibility.Hidden;
            //TODO: wylogowanie z bazy danych            
            if (LogIn() == true)
            {
                //TODO: ponowne zalogowanie
                this.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                Environment.Exit(0);
            }
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }

        private void Lab_Back_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Lab_Save_Click(object sender, RoutedEventArgs e)
        {

        }

        private bool LogIn()
        {
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
                return true;
            return false;
        }
    }
}
