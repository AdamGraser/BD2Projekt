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
                var query = from Lekarz in db.Lekarzs
                                       where Lekarz.Login == login &&
                                             Lekarz.Haslo.StartsWith(temp) &&
                                             Lekarz.Haslo.Length == temp.Length
                                       select Lekarz.Id_lek;

                byte lek_id = 0;
                
                foreach (byte q in query)
                {
                    if (lek_id == 0)
                    {
                        lek_id = q;
                        retval = true;
                    }
                    else
                    {
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
        /// <param name="id_lek">ID aktualnie zalogowanego lekarza (wizyty zarejestrowane do niego zostaną pobrane z bazy).</param>
        /// <returns>Zwraca listę dat, imion i nazwisk oddzielonych spacjami lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<int, string> GetVisits(byte id_lek)
        {
            Dictionary<int, string> visitsList = new Dictionary<int, string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Wizyta in db.Wizytas
                            join Pacjent in db.Pacjents on Wizyta.Id_pac equals Pacjent.Id_pac
                            where (Wizyta.Id_lek == id_lek && Wizyta.Stan == null && Wizyta.Data_rej >= DateTime.Now)
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
                    //Łączenie imion i nazwisk, zapisywanie ich.
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
        public bool ChangeVisitState(int id_wiz, bool nowy_stan)
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
        /// <param name="bad_fiz">Determinuje, czy lekarz wykonał badania fizykalne (czy odpowiednie pole obok opisu wizyty zostało zaznaczone).</param>
        /// <param name="diagnoza">Nowa diagnoza przedstawiona przez lekarza podczas tej wizyty.</param>
        /// <returns>True jeśli aktualizacja rekordu w tabeli powiodła się, false jeśli wystąpił błąd.</returns>
        public bool SaveVisit(int id_wiz, string opis, bool bad_fiz, string diagnoza)
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
                wiz.Stan = true;
                
                if (bad_fiz)
                    wiz.Data_wyk_bad = DateTime.Now;

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
        /// Dodaje do tabeli Badanie nowy rekord z informacjami o badaniu laboratoryjnym, zleconym w trakcie wskazanej wizyty.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której zostało zlecone to badanie.</param>
        /// <param name="id_bad">LP badania dla tej wizyty.</param>
        /// <param name="data_zle">Czas zlecenia badania.</param>
        /// <param name="opis">Opis (dodatkowe informacje) badania do wykonania.</param>
        /// <param name="kod">Kod badania w słowniku badań.</param>
        /// <returns>Wartość true jeśli nowy rekord został pomyślnie dodany do tabeli. W razie wystąpienia błędu zwraca wartość false.</returns>
        public bool AddLabTest(int id_wiz, byte id_bad, DateTime data_zle, string opis, short kod)
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



        /*
        /// <summary>
        /// Dodaje do tabeli Wizyta nowy rekord z informacjami o nowej wizycie.
        /// </summary>
        /// <param name="data_rej">Data planowanej realizacji wizyty.</param>
        /// <param name="id_lek">ID lekarza, do którego zarejestrowano pacjenta (tabela Lekarz).</param>
        /// <param name="id_pac">ID zarejestrowanego pacjenta (tabela Pacjent).</param>
        /// <returns>Wartość true jeśli nowy rekord został pomyślnie dodany do tabeli. W razie wystąpienia błędu zwraca wartość false.</returns>
        public bool AddVisit(DateTime data_rej, byte id_lek, int id_pac)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Encja, aby odwzorować tabelę Wizyta.
            Przychodnia.Wizyta wiz = new Przychodnia.Wizyta();

            wiz.Data_rej = data_rej;
            //Niech będzie 1, ja się tu nie będę bawił w implementację logowania.
            wiz.Id_rej = 1;
            wiz.Id_lek = id_lek;
            wiz.Id_pac = id_pac;

            //Przygotowanie wszystkiego do wysłania.
            db.Wizytas.InsertOnSubmit(wiz);

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
        */



        /*
        /// <summary>
        /// Pobiera z tabel Badanie i Sl_badan nazwy + opisy i daty zlecenia, zrealizowanych lub nie, badań laboratoryjnych.
        /// </summary>
        /// <param name="done">Determinuje czy pobierane mają być badania wykonane (true) czy niewykonane (false).</param>
        /// <returns>Zwraca listę dat zlecenia i nazw niezrealizowanych badań laboratoryjnych lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<int, List<string>> GetLabTests(bool done)
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
                var query = (done)
                            ?
                            from Badanie in db.Badanies
                            join Sl_badan in db.Sl_badans on Badanie.Kod equals Sl_badan.Kod
                            where Badanie.Id_lab != null
                            orderby Badanie.Id_wiz, Badanie.Id_bad
                            select new
                            {
                                idWiz = Badanie.Id_wiz,
                                dataZle = Badanie.Data_zle,
                                nazwa = Sl_badan.Nazwa,
                                opis = Sl_badan.Opis
                            }
                            :
                            from Badanie in db.Badanies
                            join Sl_badan in db.Sl_badans on Badanie.Kod equals Sl_badan.Kod
                            where Badanie.Id_lab == null
                            orderby Badanie.Id_wiz, Badanie.Id_bad
                            select new
                            {
                                idWiz = Badanie.Id_wiz,
                                dataZle = Badanie.Data_zle,
                                nazwa = Sl_badan.Nazwa,
                                opis = Sl_badan.Opis
                            };

                int iw = -1;
                List<string> lt = null;
                
                //Wykonanie zapytania.
                foreach (var b in query)
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
        /// <param name="id_lab">ID laboranta, który wykona to badanie lub null jeśli wykonanie badania ma być cofnięte.</param>
        /// <returns>True jeśli zmiany zostały pomyślnie wprowadzone lub false w przypadku wystąpienia błędu.</returns>
        public bool ExecuteLabTest(int id_wiz, byte id_bad, byte? id_lab)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Badanie in db.Badanies
                        where (Badanie.Id_wiz == id_wiz && Badanie.Id_bad == id_bad)
                        select Badanie;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Badanie bad in query)
            {
                //Dokonanie żądanych zmian.
                bad.Id_lab = id_lab;
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
        /// Rozpoczyna sprawdzanie badania (przypisuje ID kierownika laboratorium do badania) lub zmienia stan badania z powrotem na Niesprawdzone (wstawia NULL w miejsce ID kier. lab.).
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której to badanie zostało zlecone.</param>
        /// <param name="id_bad">L.p. badania dla tej wizyty.</param>
        /// <param name="id_klab">ID kierownika laboratorium, który sprawdzi to badanie lub null jeśli sprawdzenie badania ma być cofnięte.</param>
        /// <returns>True jeśli zmiany zostały pomyślnie wprowadzone lub false w przypadku wystąpienia błędu.</returns>
        public bool CheckLabTest(int id_wiz, byte id_bad, byte? id_klab)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Badanie in db.Badanies
                        where (Badanie.Id_wiz == id_wiz && Badanie.Id_bad == id_bad)
                        select Badanie;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Badanie bad in query)
            {
                //Dokonanie żądanych zmian.
                bad.Id_klab = id_klab;
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
        /// Funkcja aktualizuje dane dotyczące wskazanego badania laboratoryjnego.
        /// Jeśli w bazie pole Badanie.data_wyk_bad ma wartość NULL, pola data_wyk_bad oraz wynik są wymagane, pole zatw musi przyjąć wartość null lub false, a pole uwagi jest ignorowane.
        /// Jeśli w bazie pole Badanie.data_wyk_bad ma wartość różną od NULL, pole zatw musi przyjąć wartość true lub false, pole uwagi jest wymagane jeśli zatw == false, a pola data_wyk_bad i wynik są ignorowane.
        /// Pola id_wiz i id_bad są oczywiście zawsze wymagane, gdyż identyfikują rekord w tabeli Badanie.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której to badanie zostało zlecone.</param>
        /// <param name="id_bad">L.p. badania dla tej wizyty.</param>
        /// <param name="data_wyk_bad">Data wykonania badania (null jeśli nie trzeba).</param>
        /// <param name="wynik">Wynik badania (null jeśli nie trzeba).</param>
        /// <param name="zatw">
        /// Laborant: null jeśli badanie zatwierdzono, false jeśli anulowano.
        /// Kier. lab: true jeśli badanie zatwierdzono, false jeśli anulowano.
        /// </param>
        /// <param name="uwagi">Uwagi do sprawdzonego badania (null jeśli nie trzeba).</param>
        /// <returns>True jeśli cały proces przebiegł prawidłowo, false jeśli nastąpił błąd przy wysyłaniu danych/próbie zapisu danych w bazie, null jeśli podano nieprawidłowe dane.</returns>
        public bool? SaveLabTest(int id_wiz, byte id_bad, DateTime data_wyk_bad, string wynik, bool? zatw, string uwagi)
        {
            bool? retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Badanie in db.Badanies
                        where (Badanie.Id_wiz == id_wiz && Badanie.Id_bad == id_bad)
                        select Badanie;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Badanie bad in query)
            {
                //Sprawdzenie poprawności danych, dokonanie żądanych zmian.

                //Badanie zostało wykonane.
                if (bad.Data_wyk_bad == null)
                {
                    //Musi być data, wynik oraz brak zatwierdzenia jako kierownik, bo laborant tego zrobić nie może.
                    if (data_wyk_bad != null && wynik != null && wynik.Length > 0 && zatw != true)
                    {
                        bad.Data_wyk_bad = data_wyk_bad;
                        bad.Wynik = wynik;
                        bad.Zatw = zatw;
                    }
                    else
                        retval = null;
                }
                else //Badanie zostało sprawdzone przez kierownika.
                {
                    //Kierownik musi zatwierdzić lub anulować badanie (wtedy musi podać powód).
                    if (zatw == true || (zatw == false && uwagi != null && uwagi.Length > 0))
                    {
                        bad.Zatw = zatw;
                        bad.Uwagi = uwagi;
                    }
                    else
                        retval = null;
                }
            }

            try
            {
                if (retval == true)
                {
                    //Wysłanie zaktualizowanego rekordu. Rzuca SqlException, np. gdy klucz obcy nie odpowiada kluczowi głównemu w tabeli nadrzędnej.
                    db.SubmitChanges();

                    //Jeśli nie rzucił mięsem, dojdzie tutaj, czyli wszystko ok. Jeśli zostało już zacommitowane/rollbacknięte przez serwer, rzuci InvalidOper..., jeśli coś
                    //innego, rzuci Exception
                    transaction.Commit();
                }
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
         */
    }
}
