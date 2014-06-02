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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DBClient.DBClient db;
        int currentVisitID; // przechowuje ID wizyty, która właśnie się odbywa lub -1, jeśli lekarz nie przyjmuje teraz żadnego pacjenta/podczas której zlecone zostało badanie currentLabTestID
        byte currentLabTestID; // przechowuje ID badania lab. aktualnie wykonywanego przez laboranta lub -1 jeśli laborant nie wykonuje aktualnie żadnego badania lab.
        byte currentRow; // przechowuje nr wiersza listy zleconych badań laboratoryjnych, do którego wstawiona zostanie pozycja opisująca najnowsze, dopiero co zlecone badanie
        public MainWindow()
        {
            InitializeComponent();
            LoginWindow loginWindow = new LoginWindow();
            if (LogIn() == true)
            {
                db = new DBClient.DBClient();
                currentVisitID = -1;
                currentRow = 0;
                currentLabTestID = 0;
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

                    if (visits == null)
                        MessageBox.Show("Wystąpił błąd podczas pobierania listy wizyt.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                // <-- Tworzenie listy wizyt dla bieżąco zalogowanego lekarza.
            }
            else
            {
                Environment.Exit(0);
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

        private void VisitsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (currentVisitID == -1 && currentLabTestID == 0)
            {
                ListBoxItem item = (ListBoxItem)VisitsList.SelectedItem;
                string temp = (string)item.Content;

                string[] visit = temp.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                data_rej.Text = visit[0];
                nazwa_pac.Text = visit[1];
                stan.Text = "Nierozpoczęta";
            }
        }

        private void findVisitButton_Click(object sender, RoutedEventArgs e)
        {

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

        private bool LogIn()
        {
            LoginWindow loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
                return true;
            return false;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //TODO: wylogowanie z bazy danych
            Environment.Exit(0);
        }

        private void aboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            AboutDialog aboutDialog = new AboutDialog();
            aboutDialog.ShowDialog();
        }
    }
}
