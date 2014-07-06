using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Data.SqlClient;
using System.Data.Linq;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    /// <summary>
    /// Realizuje połączenie z bazą danych oraz wszystkie operacje na niej wykonywane.
    /// </summary>
    public class DBClient
    {
        SqlConnection connection;
        SqlTransaction transaction;
        Przychodnia.Przychodnia db;
        string name;                //przechowuje imię i nazwisko zalogowanego użytkownika



        /// <summary>
        /// Domyślny konstruktor. Tworzy i otwiera połączenie z bazą danych.
        /// </summary>
        public DBClient()
        {
            //Utworzenie połączenia do bazy danych.
            connection = new SqlConnection(@"Server=\SQLEXPRESS; uid=sa; pwd=; Database=Przychodnia");

            //Utworzenie obiektu reprezentującego bazę danych, który zawiera encje odpowiadające tabelom w bazie.
            db = new Przychodnia.Przychodnia(connection);

            name = null;
        }



        /// <summary>
        /// Zwraca imię i nazwisko zalogowanego użytkownika.
        /// </summary>
        public string UserName
        {
            get
            {
                return name;
            }
        }



        /// <summary>
        /// Przypisuje polu name wartość null.
        /// Jest to swoisty reset tego pola, który powinien dla bezpieczeństwa być wykonywany przy wylogowaniu.
        /// </summary>
        public void ResetClient()
        {
            name = null;
        }



        /// <summary>
        /// Zwalnia zasoby zajmowane przez pole "db: Przychodnia".
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }



        /// <summary>
        /// Sprawdza czy podane poświadczenia są prawidłowe oraz czy wskazane konto jest aktywne.
        /// Zapisywana jest również nazwa (imię i nazwisko) użytkownika we właściwości UserName.
        /// </summary>
        /// <param name="login">Login do wyszukania w bazie</param>
        /// <param name="passwordHash">Hash hasła</param>
        /// <returns>true jeżeli podane poświadczenia są prawidłowe, false jeżeli są nieprawidłowe, null jeśli wystąpił błąd..</returns>
        public bool? FindUser(string login, byte[] passwordHash)
        {
            bool? retval = false;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                string temp = System.Text.Encoding.ASCII.GetString(passwordHash);

                //Utworzenie zapytania.
                var query = from Administrator in db.Administrators
                            where Administrator.Login == login &&
                                  Administrator.Haslo.StartsWith(temp) &&
                                  Administrator.Haslo.Length == temp.Length
                            select new
                            {
                                id = Administrator.Id_adm,
                                imie = Administrator.Imie,
                                nazw = Administrator.Nazwisko
                            };

                byte id_adm = 0;

                //Sprawdzenie czy w bazie istnieje dokładnie 1 rekord z podanymi wartościami w kolumnach login i haslo.
                foreach (var q in query)
                {
                    if (id_adm == 0)
                    {
                        id_adm = q.id;
                        name = q.imie + " " + q.nazw;

                        retval = true;
                    }
                    else
                    {
                        name = null;
                        
                        retval = null;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                retval = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return retval;
        }
    }
}
