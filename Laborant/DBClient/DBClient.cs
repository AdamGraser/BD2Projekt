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
        static byte id_lab;   //ID laboranta obecnie zalogowanego w systemie

        /// <summary>
        /// Domyślny konstruktor. Tworzy i otwiera połączenie z bazą danych.
        /// </summary>
        public DBClient()
        {
            //Utworzenie połączenia do bazy danych.
            connection = new SqlConnection(@"Server=\SQLEXPRESS; uid=sa; pwd=; Database=Przychodnia");

            //Utworzenie obiektu reprezentującego bazę danych, który zawiera encje odpowiadające tabelom w bazie.
            db = new Przychodnia.Przychodnia(connection);
        }



        /// <summary>
        /// Zwalnia zasoby zajmowane przez pole "db: Przychodnia".
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }



        /// <summary>
        /// Pobiera z bazy dane potrzebne do logowania i sprawdza czy zgadzają się z podanymi parametrami.
        /// Jeśli dane są prawidłowe, zapisuje pobrane z bazy ID w polu id_lab.
        /// </summary>
        /// <param name="login">Login do wyszukania w bazie</param>
        /// <param name="passwordHash">Hash hasła</param>
        /// <returns>true - jeżeli użytkownik został znaleziony, false gdy podane parametry nie zgadzają się z zawartością bazy, null w przypadku wystąpienia błędu.</returns>
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
                var query = from Laborant in db.Laborants
                                       where Laborant.Login == login &&
                                             Laborant.Haslo.StartsWith(temp) &&
                                             Laborant.Haslo.Length == temp.Length
                                       select Laborant.Id_lab;

                id_lab = 0;

                //Sprawdzenie czy w bazie istnieje dokładnie 1 rekord z podanymi wartościami w kolumnach login i haslo.
                foreach (byte q in query)
                {
                    if (id_lab == 0)
                    {
                        id_lab = q;
                        retval = true;
                    }
                    else
                    {
                        id_lab = 0;
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



        /// <summary>
        /// Pobiera z tabel Badanie i Sl_badan nazwy + opisy i daty zlecenia, zrealizowanych lub nie, badań laboratoryjnych.
        /// </summary>
        /// <returns>Zwraca listę dat zlecenia i nazw wybranych badań laboratoryjnych lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<int, List<string>> GetLabTests(string docFirstName, string docSurname, string state, DateTime? dateFrom, DateTime? dateTo)
        {
            Dictionary<int, List<string>> labTests = new Dictionary<int, List<string>>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Badanie in db.Badanies
                            join Sl_badan in db.Sl_badans on Badanie.Kod equals Sl_badan.Kod
                            join Wizyta in db.Wizytas on Badanie.Id_wiz equals Wizyta.Id_wiz
                            join Lekarz in db.Lekarzs on Wizyta.Id_lek equals Lekarz.Id_lek
                            orderby Badanie.Id_wiz, Badanie.Id_bad
                            select new
                            {
                                idWiz = Badanie.Id_wiz,
                                dataZle = Badanie.Data_zle,
                                nazwa = Sl_badan.Nazwa,
                                opis = Sl_badan.Opis,
                                imieLek = Lekarz.Imie,
                                nazwLek = Lekarz.Nazwisko
                            };

                int iw = -1;
                List<string> lt = null;
                
                //TODO: Jakoś odfiltrować, żeby zwróciło tylko badania w konkretnym stanie

                //Wykonanie zapytania.
                foreach (var b in query)
                {
                    //Sprawdzenie, czy nasze zapytanie spełnia kryteria
                    if ( ((docFirstName == "") || (docFirstName == b.imieLek)) &&
                         ((docSurname == "") || (docSurname == b.nazwLek)) &&
                         ((dateFrom == null) || (b.dataZle >= dateFrom)) &&
                         ((dateTo == null) || (b.dataZle <= dateTo)) )
                    {
                        //Zapisanie wyników.
                        if (iw != b.idWiz)
                        {
                            iw = b.idWiz;

                            lt = new List<string>();

                            labTests.Add(iw, lt);
                        }

                        lt.Add(b.dataZle.ToString() + " " + b.nazwa + ", " + b.opis);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                labTests = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return labTests;
        }



        /// <summary>
        /// Tworzy listę ID wszystkich badań zleconych podczas wskazanej wizyty.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, dla której utworzona ma zostać lista ID badań.</param>
        /// <returns>Zwraca listę ID badań zleconych podczas wskazanej wizyty (może zawierać 0 elementów) lub null w przypadku wystąpienia błędu.</returns>
        public List<byte> GetLabTestsIDs(int id_wiz)
        {
            List<byte> testsIDs = new List<byte>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                var query = from Badanie in db.Badanies
                            where Badanie.Id_wiz == id_wiz
                            select Badanie.Id_bad;

                foreach (byte b in query)
                {
                    testsIDs.Add(b);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                testsIDs = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return testsIDs;
        }



        /// <summary>
        /// Pobiera z tabel Badanie i Lekarz imię i nazwisko lekarza, który zlecił to badanie, oraz podany przez niego opis (dodatkowe informacje dla laboranta).
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której to badanie zostało zlecone.</param>
        /// <param name="id_bad">L.p. badania dla tej wizyty.</param>
        /// <returns>Listę trzech informacji szczegółowych o badaniu w podanej kolejności: opis, imię lekarza, nazwisko lekarza, wynik badania; lub null w przypadku wystąpienia błędu.</returns>
        public List<string> GetLabTestDetails(int id_wiz, byte id_bad)
        {
            List<string> labTestDetails = new List<string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Badanie in db.Badanies
                            join Wizyta in db.Wizytas on Badanie.Id_wiz equals Wizyta.Id_wiz
                            join Lekarz in db.Lekarzs on Wizyta.Id_lek equals Lekarz.Id_lek
                            where (Badanie.Id_wiz == id_wiz && Badanie.Id_bad == id_bad)
                            select new
                            {
                                opis = Badanie.Opis,
                                imie = Lekarz.Imie,
                                nazwisko = Lekarz.Nazwisko,
                                wynik = Badanie.Wynik
                            };

                //Wykonanie zapytania.
                foreach (var l in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    labTestDetails.Add(l.opis);
                    labTestDetails.Add(l.imie);
                    labTestDetails.Add(l.nazwisko);
                    labTestDetails.Add(l.wynik);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                labTestDetails = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return labTestDetails;
        }



        /// <summary>
        /// Rozpoczyna badanie laboratoryjne (przypisuje ID laboranta do badania) lub zmienia stan badania z powrotem na Nierozpoczęte (wstawia NULL w miejsce ID laboranta).
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której to badanie zostało zlecone.</param>
        /// <param name="id_bad">L.p. badania dla tej wizyty.</param>
        /// <param name="execute">Determinuje czy w kolumnie id_lab wpisane ma być ID zalogowanego laboranta (true), czy wartość null (false).</param>
        /// <returns>True jeśli zmiany zostały pomyślnie wprowadzone lub false w przypadku wystąpienia błędu.</returns>
        public bool ExecuteLabTest(int id_wiz, byte id_bad, bool execute)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Badanie in db.Badanies
                        where Badanie.Id_wiz == id_wiz && Badanie.Id_bad == id_bad
                        select Badanie;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Badanie bad in query)
            {
                //Dokonanie żądanych zmian.
                bad.Id_lab = (execute) ? (byte?)id_lab : null;
            }

            try
            {
                //Wysłanie zaktualizowanego rekordu. Rzuca SqlException, np. gdy klucz obcy nie odpowiada kluczowi głównemu w tabeli nadrzędnej.
                db.SubmitChanges();

                //Jeśli nie rzucił mięsem, dojdzie tutaj, czyli wszystko ok. Jeśli zostało już zacommitowane/rollbacknięte przez serwer, rzuci InvalidOper..., jeśli coś
                //innego, rzuci Exception
                transaction.Commit();
            }
            catch (InvalidOperationException invOper)
            {
                Console.WriteLine("Transakcja została już zaakceptowana/odrzucona LUB połączenie zostało zerwane.");
                Console.WriteLine(invOper.Message);
                Console.WriteLine(invOper.Source);
                Console.WriteLine(invOper.HelpLink);
                Console.WriteLine(invOper.StackTrace);
                retval = false;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine("Wystąpił błąd przy dodawaniu nowego rekordu, np. niezgodność klucza obcego w tabeli podrzędnej z kluczem głównym w tabeli nadrzędnej.");
                Console.WriteLine(sqlEx.Message);
                Console.WriteLine(sqlEx.Source);
                Console.WriteLine(sqlEx.HelpLink);
                Console.WriteLine(sqlEx.StackTrace);
                retval = false;

                //Rollback, bo coś poszło nie tak.
                transaction.Rollback();

                //Zwolnienie zasobów, bo po co je zajmować.
                db.Dispose();

                //Utworzenie od razu nowego obiektu do użycia następnym razem.
                db = new Przychodnia.Przychodnia(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wystąpił błąd podczas próby zaakceptowania transakcji.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.HelpLink);
                Console.WriteLine(ex.StackTrace);
                retval = false;
            }
            finally
            {
                //Zawsze należy zamknąć połączenie.
                connection.Close();
            }

            return retval;
        }



        /// <summary>
        /// Przypisuje polu id_lab wartość 0 - jest to swoisty reset tego pola, który powinien dla bezpieczeństwa być wykonywany przy wylogowaniu.
        /// </summary>
        public void ResetIdLab()
        {
            id_lab = 0;
        }



        /// <summary>
        /// Funkcja aktualizuje dane dotyczące wskazanego badania laboratoryjnego. Argument "wynik" nie może być null.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której to badanie zostało zlecone.</param>
        /// <param name="id_bad">L.p. badania dla tej wizyty.</param>
        /// <param name="data_wyk_bad">Data wykonania badania (null jeśli nie trzeba).</param>
        /// <param name="wynik">Wynik badania.</param>
        /// <param name="zatw">null jeśli badanie zatwierdzono, false jeśli anulowano.</param>
        /// <returns>True jeśli cały proces przebiegł prawidłowo, false jeśli nastąpił błąd przy wysyłaniu danych/próbie zapisu danych w bazie, null jeśli podano null jako argument "wynik".</returns>
        public bool? SaveLabTest(int id_wiz, byte id_bad, DateTime data_wyk_bad, string wynik, bool? zatw)
        {
            bool? retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Badanie in db.Badanies
                        where Badanie.Id_wiz == id_wiz && Badanie.Id_bad == id_bad
                        select Badanie;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Badanie bad in query)
            {
                if (wynik != null)
                {
                    bad.Data_wyk_bad = data_wyk_bad;
                    bad.Wynik = wynik;
                    bad.Zatw = zatw;
                }
                else
                    retval = null;
            }

            try
            {
                //Wysłanie zaktualizowanego rekordu. Rzuca SqlException, np. gdy klucz obcy nie odpowiada kluczowi głównemu w tabeli nadrzędnej.
                db.SubmitChanges();

                //Jeśli nie rzucił mięsem, dojdzie tutaj, czyli wszystko ok. Jeśli zostało już zacommitowane/rollbacknięte przez serwer, rzuci InvalidOper..., jeśli coś
                //innego, rzuci Exception
                transaction.Commit();
            }
            catch (InvalidOperationException invOper)
            {
                Console.WriteLine("Transakcja została już zaakceptowana/odrzucona LUB połączenie zostało zerwane.");
                Console.WriteLine(invOper.Message);
                Console.WriteLine(invOper.Source);
                Console.WriteLine(invOper.HelpLink);
                Console.WriteLine(invOper.StackTrace);
                retval = false;
            }
            catch (SqlException sqlEx)
            {
                Console.WriteLine("Wystąpił błąd przy dodawaniu nowego rekordu, np. niezgodność klucza obcego w tabeli podrzędnej z kluczem głównym w tabeli nadrzędnej.");
                Console.WriteLine(sqlEx.Message);
                Console.WriteLine(sqlEx.Source);
                Console.WriteLine(sqlEx.HelpLink);
                Console.WriteLine(sqlEx.StackTrace);
                retval = false;

                //Rollback, bo coś poszło nie tak.
                transaction.Rollback();

                //Zwolnienie zasobów, bo po co je zajmować.
                db.Dispose();

                //Utworzenie od razu nowego obiektu do użycia następnym razem.
                db = new Przychodnia.Przychodnia(connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Wystąpił błąd podczas próby zaakceptowania transakcji.");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.Source);
                Console.WriteLine(ex.HelpLink);
                Console.WriteLine(ex.StackTrace);
                retval = false;
            }
            finally
            {
                //Zawsze należy zamknąć połączenie.
                connection.Close();
            }

            return retval;
        }
    }
}
