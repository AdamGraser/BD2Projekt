using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DBConnectionLayer
{
    /// <summary>
    /// Klasa klienta bazy danych. Dostarcza interfejs programistyczny do realizacji funkcjonalności klienta systemu dla przychodni.
    /// Tworzy połączenie z bazą danych i wykonuje do niej odpowiednie zapytania, uprzednio sprawdzając poprawność danych.
    /// </summary>
    public class DBClient
    {
        SqlConnection connection;
        SqlTransaction transaction;
        Przychodnia.Przychodnia db;

        /// <summary>
        /// Domyślny konstruktor. Tworzy i otwiera połączenie z bazą danych.
        /// </summary>
        public DBClient()
        {
            connection = new SqlConnection(@"Server=BODACH\SQLEXPRESS; uid=sa; pwd=pass; Database=Przychodnia");

            db = new Przychodnia.Przychodnia(connection);
        }

        /*/// <summary>
        /// Domyślny destruktor. Zamyka połączenie z bazą danych.
        /// </summary>
        ~DBClient()
        {
            db.Dispose();
            
        }*/

        /// <summary>
        /// Dodaje do tabeli Wizyta nowy rekord z informacjami o nowej wizycie.
        /// </summary>
        /// <param name="data_rej">Data planowanej realizacji wizyty.</param>
        /// <param name="id_lek">ID lekarza, do którego zarejestrowano pacjenta (tabela Lekarz).</param>
        /// <param name="id_pac">ID zarejestrowanego pacjenta (tabela Pacjent).</param>
        /// <returns>Wartość true jeśli nowy rekord został pomyślnie dodany do tabeli. W razie wystąpienia błędu zwraca wartość false.</returns>
        public string RejestrujWizyte(DateTime data_rej, byte id_lek, int id_pac)
        {
            string retval = "Dodano nowy rekord.";

            connection.Open();
            
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            Przychodnia.Wizyta wiz = new Przychodnia.Wizyta();
            
            wiz.Data_rej = data_rej;
            wiz.Id_rej = 1;
            wiz.Id_lek = id_lek;
            wiz.Id_pac = id_pac;

            db.Wizytas.InsertOnSubmit(wiz);

            try
            {
                db.SubmitChanges();

                transaction.Commit();
            }
            catch (InvalidOperationException invOper)
            {
                Console.WriteLine("Transakcja została już zaakceptowana/odrzucona LUB połączenie zostało zerwane.");
                Console.WriteLine(invOper.Message);
                Console.WriteLine(invOper.Source);
                Console.WriteLine(invOper.HelpLink);
                Console.WriteLine(invOper.StackTrace);
                retval = "Transakcja została już zaakceptowana/odrzucona LUB połączenie zostało zerwane.";
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine("Wystąpił błąd przy dodawaniu nowego rekordu, np. niezgodność klucza obcego w tabeli podrzędnej z kluczem głównym w tabeli nadrzędnej.");
                Console.WriteLine(sqlEx.Message);
                Console.WriteLine(sqlEx.Source);
                Console.WriteLine(sqlEx.HelpLink);
                Console.WriteLine(sqlEx.StackTrace);
                retval = "Nieprawidłowe dane wejściowe.";
                transaction.Rollback();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wystąpił błąd podczas próby zaakceptowania transakcji.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.HelpLink);
                Console.WriteLine(ex.StackTrace);
                retval = "Wystąpił błąd podczas próby zaakceptowania transakcji.";
            }
            finally
            {
                connection.Close();
            }

            return retval;
        }
    }
}
