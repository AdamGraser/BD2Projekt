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
            connection = new SqlConnection(@"Server=BODACH\SQLEXPRESS; uid=sa; pwd=Gresiulina; Database=Przychodnia");

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
        /// </summary>
        /// <param name="patientName">Imię/początek imienia pacjentów - kryterium wyszukiwania</param>
        /// <param name="patientSurname">Nazwisko/początek nazwiska - kryterium wyszukiwania</param>
        /// <param name="visitDate">Data odbycia się wizyty - kryterium wyszukiwania</param>
        /// <param name="visitStatus">
        /// Stan wizyty - kryterium wyszukiwania. Dostępne stany:
        /// * Zarejestrowana
        /// * Realizowana
        /// * Anulowana
        /// * Zakończona
        /// </param>
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
                if ((patientName.Length > 0 || patientSurname.Length > 0) && visitDate != null)
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
                else if (visitDate != null)
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
                else
                {
                    //Utworzenie zapytania.
                    var query = from Wizyta in db.Wizytas
                                join Pacjent in db.Pacjents on Wizyta.Id_pac equals Pacjent.Id_pac
                                where (Wizyta.Id_lek == id_lek && Wizyta.Stan == visitStatus && Pacjent.Imie.ToLower().StartsWith(patientName.ToLower()) && Pacjent.Nazwisko.ToLower().StartsWith(patientSurname.ToLower()))
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
        /// Pobiera z tabeli Sl_badan wszystkie badania laboratoryjne.
        /// </summary>
        /// <returns>Zwraca listę par &lt;kod, nazwa opis&gt; lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<short, string> GetLabTests()
        {
            Dictionary<short, string> labTests = new Dictionary<short, string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Sl_badan in db.Sl_badans
                            where Sl_badan.Lab == true
                            select Sl_badan;

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (var test in query)
                {
                    //Łączenie nazw i opisów badań, zapisywanie ich.
                    if(test.Opis != null)
                        labTests.Add(test.Kod, test.Nazwa + " " + test.Opis);
                    else
                        labTests.Add(test.Kod, test.Nazwa);
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
        /// Pobiera z tabeli Sl_badan wszystkie badania fizykalne.
        /// </summary>
        /// <returns>Zwraca listę par &lt;kod, nazwa opis&gt; lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<short, string> GetPhyTests()
        {
            Dictionary<short, string> phyTests = new Dictionary<short, string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Sl_badan in db.Sl_badans
                            where Sl_badan.Lab == false
                            select Sl_badan;

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (var test in query)
                {
                    //Łączenie nazw i opisów badań, zapisywanie ich.
                    if (test.Opis != null)
                        phyTests.Add(test.Kod, test.Nazwa + " " + test.Opis);
                    else
                        phyTests.Add(test.Kod, test.Nazwa);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                phyTests = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return phyTests;
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
        /// <param name="lab">
        /// Determinuje czy badanie jest laboratoryjne (true) czy fizykalne (false).
        /// Badania fizykalne: data zlecenia jest jednocześnie datą wykonania badania, opis to wynik.
        /// </param>
        /// <returns>Wartość true jeśli nowy rekord został pomyślnie dodany do tabeli. W razie wystąpienia błędu zwraca wartość false.</returns>
        public bool AddTest(int id_wiz, byte id_bad, DateTime data_zle, string opis, short kod, bool lab)
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
            bad.Kod = kod;

            if (lab)
                bad.Opis = opis;
            else
            {
                bad.Opis = "";
                bad.Wynik = opis;
                bad.Data_wyk_bad = data_zle;
            }

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



        /// <summary>
        /// Pobiera z bazy danych podstawowe informacje o laboratoryjnych i fizykalnych badaniach, zleconych/wykonanych w trakcie wskazanej wizyty.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, dla której lista badań ma być pobrana.</param>
        /// <returns>Zwraca listę badań dla danej wizyty (może być pusta) lub null w przypadku błędu.</returns>
        public List<BadanieInfo> GetVisitTests(int id_wiz)
        {
            List<BadanieInfo> visitsTests = new List<BadanieInfo>();

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
                            where Badanie.Id_wiz == id_wiz
                            select new
                            {
                                lab = Sl_badan.Lab,
                                kod = Badanie.Kod,
                                data_zle = Badanie.Data_zle,
                                opis = Badanie.Opis,
                                wynik = Badanie.Wynik
                            };

                //Pobranie i zapisanie informacji o kolejnych badaniach (o ile jakieś istnieją).
                foreach (var bad in query)
                {
                    visitsTests.Add(new BadanieInfo(bad.lab, bad.kod, bad.data_zle, bad.opis, bad.wynik));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                visitsTests = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return visitsTests;
        }



        /// <summary>
        /// Pobiera z bazy danych ID pacjenta, dla którego zarejestrowana została wskazana wizyta.
        /// </summary>
        /// <param name="id_wiz">ID wizyty</param>
        /// <returns>
        /// Zwraca ID pacjenta, dla którego zarejestrowana została wskazana wizyta,
        /// 0 jeśli nie odnaleziono w bazie wizyty o podanym ID,
        /// -1 w przypadku wystąpienia błędu.
        /// </returns>
        public int GetPatientId(int id_wiz)
        {
            int id_pac = 0;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                var query = from Wizyta in db.Wizytas
                            where Wizyta.Id_wiz == id_wiz
                            select Wizyta.Id_pac;

                //Pobranie i zapisanie ID pacjenta.
                foreach (int i in query)
                {
                    id_pac = i;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                id_pac = -1;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return id_pac;
        }
    }
}
