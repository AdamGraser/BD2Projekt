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
        static byte id_lek;         //ID lekarza obecnie zalogowanego w systemie
        static bool isExpired;      //determinuje ważność konta (false = aktywne, true = wygasło)
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
        /// Zwraca wartość determinującą ważność konta (false = aktywne, true = wygasło).
        /// </summary>
        public bool IsAccountExpired
        {
            get
            {
                return isExpired;
            }
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
        /// Przypisuje polu id_lek wartość 0, a polu idExpired wartość true.
        /// Jest to swoisty reset tych pól, który powinien dla bezpieczeństwa być wykonywany przy wylogowaniu.
        /// </summary>
        public void ResetClient()
        {
            id_lek = 0;
            isExpired = true;
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
        /// Informacja nt. aktywności konta jest zapisywana we właściwości IsAccountExpired.
        /// Zapisywana jest również nazwa (imię i nazwisko) użytkownika we właściwości UserName.
        /// </summary>
        /// <param name="login">Login do wyszukania w bazie</param>
        /// <param name="passwordHash">Hash hasła</param>
        /// <returns>true jeżeli podane poświadczenia są prawidłowe, false jeżeli są nieprawidłowe, null jeśli wystąpił błąd.</returns>
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
                var query = from Lekarz in db.Lekarzs
                            where Lekarz.Login == login &&
                                  Lekarz.Haslo.StartsWith(temp) &&
                                  Lekarz.Haslo.Length == temp.Length
                            select new
                            {
                                id = Lekarz.Id_lek,
                                exp = Lekarz.Wygasa,
                                imie = Lekarz.Imie,
                                nazw = Lekarz.Nazwisko
                            };

                //Sprawdzenie czy w bazie istnieje dokładnie 1 rekord z podanymi wartościami w kolumnach login i haslo.
                foreach (var q in query)
                {
                    if (id_lek == 0)
                    {
                        id_lek = q.id;
                        isExpired = (q.exp <= DateTime.Now);
                        name = q.imie + " " + q.nazw;

                        retval = true;
                    }
                    else
                    {
                        id_lek = 0;
                        isExpired = true;
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
             
 

        /// <summary>
        /// Pobiera z tabeli Wizyta i Pacjent daty wizyt oraz imiona i nazwiska pacjentów, dla których te wizyty zostały zarejestrowane.
        /// Dotyczy to tylko wizyt nieodbytych, które jeszcze mogą się odbyć (Wizyta.Stan == null &amp;&amp; Wizyta.Data_rej &lt; DateTime.today)
        /// </summary>
        /// <param name="patientName"></param>
        /// <param name="patientSurname"></param>
        /// <param name="visitDate"></param>
        /// <param name="visitStatus"></param>
        /// <returns>Zwraca listę dat, imion i nazwisk oddzielonych spacjami lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<int, string> GetVisits(string patientName, string patientSurname, byte visitStatus, DateTime? visitDate)
        {
            Dictionary<int, string> visitsList = new Dictionary<int, string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                if (patientName.Length > 0 || patientName.Length > 0)
                {
                    //Utworzenie zapytania.
                    var query = from Wizyta in db.Wizytas
                                join Pacjent in db.Pacjents on Wizyta.Id_pac equals Pacjent.Id_pac
                                where (Wizyta.Id_lek == id_lek && Wizyta.Stan == visitStatus && Wizyta.Data_rej.Date == visitDate && Pacjent.Imie.ToLower().StartsWith(patientName.ToLower()) && Pacjent.Nazwisko.ToLower().StartsWith(patientSurname.ToLower()))
                                orderby Wizyta.Data_rej descending
                                select new
                                {
                                    id = Wizyta.Id_wiz,
                                    dataRej = Wizyta.Data_rej,
                                    nazwisko = Pacjent.Nazwisko,
                                    imie = Pacjent.Imie
                                };

                    //Wykonanie zapytania, rekord po rekordzie.
                    foreach (var vis in query)
                    {
                        //Łączenie dat, imion i nazwisk, zapisywanie ich.
                        visitsList.Add(vis.id, vis.dataRej.ToString() + "  " + vis.nazwisko + " " + vis.imie);
                    }
                }
                else
                {
                    //Utworzenie zapytania.
                    var query = from Wizyta in db.Wizytas
                                join Pacjent in db.Pacjents on Wizyta.Id_pac equals Pacjent.Id_pac
                                where (Wizyta.Id_lek == id_lek && Wizyta.Stan == visitStatus && Wizyta.Data_rej.Date == visitDate.Value)
                                orderby Wizyta.Data_rej descending
                                select new
                                {
                                    id = Wizyta.Id_wiz,
                                    dataRej = Wizyta.Data_rej,
                                    nazwisko = Pacjent.Nazwisko,
                                    imie = Pacjent.Imie
                                };

                    //Wykonanie zapytania, rekord po rekordzie.
                    foreach (var vis in query)
                    {
                        //Łączenie dat, imion i nazwisk, zapisywanie ich.
                        visitsList.Add(vis.id, vis.dataRej.ToString() + "  " + vis.nazwisko + " " + vis.imie);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                visitsList = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return visitsList;
        }



        /// <summary>
        /// Pobiera z tabeli Sl_badan nazwy i opisy wszystkich badań laboratoryjnych.
        /// </summary>
        /// <returns>Zwraca listę nazw i opisów oddzielonych spacją lub null, jeśli wystąpił błąd.</returns>
        public List<string> GetLabTestsNames()
        {
            List<string> labTests = new List<string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Sl_badan in db.Sl_badans
                            select new
                            {
                                nazwa = Sl_badan.Nazwa,
                                opis = Sl_badan.Opis
                            };

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (var test in query)
                {
                    //Łączenie nazw i opisów badań, zapisywanie ich.
                    if(test.opis != null)
                        labTests.Add(test.nazwa + " " + test.opis);
                    else
                        labTests.Add(test.nazwa);
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
        /// Zmienia stan wskazanej wizyty na nowy.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, której stan ma być zmieniony.</param>
        /// <param name="nowy_stan">Nowy stan wizyty.</param>
        /// <returns>True jeśli aktualizacja rekordu w tabeli powiodła się, false jeśli wystąpił błąd.</returns>
        public bool ChangeVisitState(int id_wiz, byte nowy_stan)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Wizyta in db.Wizytas
                        where Wizyta.Id_wiz == id_wiz
                        select Wizyta;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Wizyta wiz in query)
            {
                //Dokonanie żądanych zmian.
                wiz.Stan = nowy_stan;
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
        /// Aktualizuje dane o wskazanej wizycie, zmienia jej stan na "zakończona".
        /// </summary>
        /// <param name="id_wiz">ID wizyty, która ma zostać zaktualizowana.</param>
        /// <param name="opis">Nowy opis wizyty.</param>
        /// <param name="diagnoza">Nowa diagnoza przedstawiona przez lekarza podczas tej wizyty.</param>
        /// <returns>True jeśli aktualizacja rekordu w tabeli powiodła się, false jeśli wystąpił błąd.</returns>
        public bool SaveVisit(int id_wiz, string opis, string diagnoza)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Wizyta in db.Wizytas
                        where Wizyta.Id_wiz == id_wiz
                        select Wizyta;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Wizyta wiz in query)
            {
                //Dokonanie żądanych zmian.
                wiz.Stan = 3; //zmiana stanu na "zakończona"
                
                if (opis != null && opis.Length > 0)
                    wiz.Opis = opis;

                if (diagnoza != null && diagnoza.Length > 0)
                    wiz.Diagnoza = diagnoza;
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
        /// Dodaje do tabeli Badanie nowy rekord z informacjami o badaniu, zleconym w trakcie wskazanej wizyty.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której zostało zlecone to badanie.</param>
        /// <param name="id_bad">LP badania dla tej wizyty.</param>
        /// <param name="data_zle">Czas zlecenia badania.</param>
        /// <param name="opis">Opis (dodatkowe informacje) badania do wykonania.</param>
        /// <param name="kod">Kod badania w słowniku badań.</param>
        /// <returns>Wartość true jeśli nowy rekord został pomyślnie dodany do tabeli. W razie wystąpienia błędu zwraca wartość false.</returns>
        public bool AddTest(int id_wiz, byte id_bad, DateTime data_zle, string opis, short kod)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Encja, aby odwzorować tabelę Badanie.
            Przychodnia.Badanie bad = new Przychodnia.Badanie();

            bad.Id_bad = id_bad;
            bad.Id_wiz = id_wiz;
            bad.Data_zle = data_zle;
            bad.Opis = opis;
            bad.Kod = kod;

            //Przygotowanie wszystkiego do wysłania.
            db.Badanies.InsertOnSubmit(bad);

            try
            {
                //Wysłanie danych (wykonanie inserta). Rzuca SqlException, np. gdy klucz obcy nie odpowiada kluczowi głównemu w tabeli nadrzędnej.
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
        /// Anuluje wizytę o wskazanym ID.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, która ma zostać usunięta.</param>
        /// <param name="opis">Opis wizyty (z założenia: powód anulowania wizyty).</param>
        /// <returns>true jeśli wizyta została pomyślnie usunięta z bazy danych, false jeśli wystąpił błąd.</returns>
        public bool CancelVisit(int id_wiz, string opis)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            var query = from Wizyta in db.Wizytas
                        where Wizyta.Id_wiz == id_wiz
                        select Wizyta;

            //id_wiz jest kluczem głównym tabeli Wizyta, co zapewnia unikalność wartości w tej kolumnie - taka wizyta jest tylko jedna
            foreach (Przychodnia.Wizyta wiz in query)
            {
                wiz.Stan = 2;    //zmiana stanu wizyty na "anulowana"
                wiz.Opis = opis; //zapisanie powodu anulowania wizyty
            }

            try
            {
                //Wysłanie danych (wykonanie inserta). Rzuca SqlException, np. gdy klucz obcy nie odpowiada kluczowi głównemu w tabeli nadrzędnej.
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
        /// Pobiera z bazy danych opis wskazanej wizyty, napisany przez lekarza w ramach wywiadu.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, której opis ma zostać pobrany.</param>
        /// <returns>Opis napisany przez lekarza podczas wskazanej wizyty lub null, jeśli wystąpił błąd.</returns>
        public string GetVisitDescription(int id_wiz)
        {
            string description = "";

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {                
                //Utworzenie zapytania.
                var query = from Wizyta in db.Wizytas                            
                            where Wizyta.Id_wiz == id_wiz
                            select Wizyta.Opis;

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (string vis in query)
                {
                    if (vis != null)
                        description = vis;
                }                
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                description = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return description;
        }



        /// <summary>
        /// Pobiera z bazy danych diagnozę wystawioną przez lekarza podczas wskazanej wizyty.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, z której diagnoza ma zostać pobrana.</param>
        /// <returns>Diagnozę wystawioną podczas wskazanej wizyty (może być pusta) lub null, jeśli wystąpił błąd.</returns>
        public string GetVisitDiagnosis(int id_wiz)
        {
            string diagnosis = "";

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Wizyta in db.Wizytas
                            where Wizyta.Id_wiz == id_wiz
                            select Wizyta.Diagnoza;

                //Pobranie i zapisanie diagnozy.
                foreach (string vis in query)
                {
                    if(vis != null)
                        diagnosis = vis;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                diagnosis = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return diagnosis;
        }
    }
}
