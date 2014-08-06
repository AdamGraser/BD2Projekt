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
using System.Security.Cryptography;

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
            connection = new SqlConnection(@"Server=BODACH\SQLEXPRESS; uid=sa; pwd=Gresiulina; Database=Przychodnia");

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

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////  Rejestratorka  ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Dodaje nowego użytkownika do tabeli Rejestratorka.
        /// </summary>
        /// <param name="login">login rejestratorki</param>
        /// <param name="haslo">haslo</param>
        /// <param name="wygasa">data wygaśnięcia konta</param>
        /// <param name="imie">imie</param>
        /// <param name="nazwisko">nazwisko</param>
        /// <returns>true jeśli pomyślnie dodano nowego użytkownika, false jeśli wystąpił błąd</returns>
        public bool AddRejestratorka(string login, string haslo, DateTime? wygasa, string imie, string nazwisko)
        {
            bool retval = true;

            HashAlgorithm sha = HashAlgorithm.Create("SHA512");
            byte[] _hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(haslo));

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            int seed = (from Rejestratorka in db.Rejestratorkas select Rejestratorka.Id_rej).Count();

            //Encja, aby odwzorować tabelę Pacjent.
            Przychodnia.Rejestratorka rej = new Przychodnia.Rejestratorka();
            rej.Id_rej = (byte)(seed + 1);
            rej.Imie = imie;
            rej.Nazwisko = nazwisko;
            rej.Haslo = System.Text.Encoding.ASCII.GetString(_hash);
            rej.Login = login;
            rej.Wygasa = wygasa;
            rej.Aktywny = DateTime.Now;

            db.Rejestratorkas.InsertOnSubmit(rej);

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
        /// Aktualizuje rejestratorkę o podanym ID.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="nowe_imie">imie</param>
        /// <param name="nowe_nazwisko">nazwisko</param>
        /// <param name="nowy_login">login</param>
        /// <param name="nowe_haslo">haslo</param>
        /// <param name="nowe_wygasa">data wygaśnięcia konta</param>
        /// <returns>true jeśli pomyślnie zaktualizowano wybranego użytkownika, false jeśli wystąpił błąd</returns>
        public bool UpdateRejestratorka(int id, string nowe_imie, string nowe_nazwisko, string nowy_login, string nowe_haslo, DateTime? nowe_wygasa)
        {
            bool retval = true;

            HashAlgorithm sha = HashAlgorithm.Create("SHA512");
            byte[] _hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(nowe_haslo));

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Rejestratorka in db.Rejestratorkas
                        where Rejestratorka.Id_rej == id
                        select Rejestratorka;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Rejestratorka rej in query)
            {
                //Dokonanie żądanych zmian.
                rej.Imie = nowe_imie;
                rej.Nazwisko = nowe_nazwisko;
                rej.Login = nowy_login;
                rej.Haslo = _hash.ToString();
                rej.Wygasa = nowe_wygasa;
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
        /// Pobiera liste wszystkich rejestratorek z bazy
        /// </summary>
        /// <returns>Lista rejestratorek lub null, jeśli wystąpił błąd.</returns>
        public List<RejestratorkaData> GetRejestratorkas()
        {
            List <RejestratorkaData> rejestratorkas = new List<RejestratorkaData>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {

                //Utworzenie zapytania.
                var query = from Rejestratorka in db.Rejestratorkas
                            orderby Rejestratorka.Nazwisko
                            select new
                            {
                                id = Rejestratorka.Id_rej,
                                imie = Rejestratorka.Imie,
                                nazwisko = Rejestratorka.Nazwisko,
                                login = Rejestratorka.Login,
                                aktywny = Rejestratorka.Aktywny,
                                wygasa = Rejestratorka.Wygasa
                            };


                //Wykonanie zapytania.
                foreach (var p in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    RejestratorkaData rejestratorkaData = new RejestratorkaData(p.id, p.nazwisko, p.imie, p.login, "", p.wygasa);
                    rejestratorkas.Add(rejestratorkaData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                rejestratorkas = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return rejestratorkas;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////    Laborant     ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Dodaje nowego użytkownika do tabeli Laborant.
        /// </summary>
        /// <param name="login">login laboranta</param>
        /// <param name="haslo">haslo</param>
        /// <param name="wygasa">data wygaśnięcia konta</param>
        /// <param name="imie">imie</param>
        /// <param name="nazwisko">nazwisko</param>
        /// <param name="kod_spec">kod specjalizacji</param>
        /// <returns>true jeśli pomyślnie dodano nowego użytkownika, false jeśli wystąpił błąd</returns>
        public bool AddLaborant(string login, string haslo, DateTime? wygasa, string imie, string nazwisko,bool kier)
        {
            DateTime aktywny = DateTime.Now;

            bool retval = true;

            HashAlgorithm sha = HashAlgorithm.Create("SHA512");
            byte[] _hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(haslo));

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            int seed = (from Laborant in db.Laborants select Laborant.Id_lab).Count();
            

            //Encja, aby odwzorować tabelę Pacjent.
            Przychodnia.Laborant lab = new Przychodnia.Laborant();
            lab.Id_lab = (byte)(seed + 1);
            lab.Imie = imie;
            lab.Nazwisko = nazwisko;
            lab.Haslo = System.Text.Encoding.ASCII.GetString(_hash);
            lab.Login = login;
            lab.Wygasa = wygasa;
            lab.Aktywny = aktywny;
            lab.Kier = kier;

            db.Laborants.InsertOnSubmit(lab);

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
        /// Aktualizuje laboranta o podanym ID.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="nowe_imie">imie</param>
        /// <param name="nowe_nazwisko">nazwisko</param>
        /// <param name="nowy_login">login</param>
        /// <param name="nowe_haslo">haslo</param>
        /// <param name="nowe_wygasa">data wygaśnięcia konta</param>
        /// <returns>true jeśli pomyślnie zaktualizowano wybranego użytkownika, false jeśli wystąpił błąd</returns>
        public bool UpdateLaborant(int id, string nowe_imie, string nowe_nazwisko, string nowy_login, string nowe_haslo, DateTime? nowe_wygasa, bool kier)
        {
            bool retval = true;

            HashAlgorithm sha = HashAlgorithm.Create("SHA512");
            byte[] _hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(nowe_haslo));

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Laborant in db.Laborants
                        where Laborant.Id_lab == id
                        select Laborant;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Laborant lab in query)
            {
                //Dokonanie żądanych zmian.
                lab.Imie = nowe_imie;
                lab.Nazwisko = nowe_nazwisko;
                lab.Login = nowy_login;
                lab.Haslo = _hash.ToString();
                lab.Wygasa = nowe_wygasa;
                lab.Kier = kier;
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
        /// Pobiera liste wszystkich laborantów z bazy
        /// </summary>
        /// <returns>Lista laborantów lub null, jeśli wystąpił błąd.</returns>
        public List<LaborantData> GetLaborants()
        {
            List<LaborantData> laborants = new List<LaborantData>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {

                //Utworzenie zapytania.
                var query = from Laborant in db.Laborants
                            orderby Laborant.Nazwisko
                            select new
                            {
                                id = Laborant.Id_lab,
                                imie = Laborant.Imie,
                                nazwisko = Laborant.Nazwisko,
                                login = Laborant.Login,
                                aktywny = Laborant.Aktywny,
                                wygasa = Laborant.Wygasa,
                                kier = Laborant.Kier
                            };


                //Wykonanie zapytania.
                foreach (var p in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    LaborantData labData = new LaborantData(p.id, p.nazwisko, p.imie, p.login, "", p.wygasa, p.kier);
                    laborants.Add(labData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                laborants = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return laborants;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////     Lekarz      ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Dodaje nowego użytkownika do tabeli Lekarz.
        /// </summary>
        /// <param name="login">login lekarza</param>
        /// <param name="haslo">haslo</param>
        /// <param name="wygasa">data wygaśnięcia konta</param>
        /// <param name="imie">imie</param>
        /// <param name="nazwisko">nazwisko</param>
        /// <param name="kod_spec">kod specjalizacji</param>
        /// <returns>true jeśli pomyślnie dodano nowego użytkownika, false jeśli wystąpił błąd</returns>
        public bool AddLekarz(string login, string haslo, DateTime? wygasa, string imie, string nazwisko, short kod_spec)
        {
            DateTime aktywny = DateTime.Now;

            bool retval = true;

            HashAlgorithm sha = HashAlgorithm.Create("SHA512");
            byte[] _hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(haslo));

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            int seed = (from Lekarz in db.Lekarzs select Lekarz.Id_lek).Count();

            //Encja, aby odwzorować tabelę Pacjent.
            Przychodnia.Lekarz lek = new Przychodnia.Lekarz();
            lek.Id_lek = (byte)(seed + 1);
            lek.Imie = imie;
            lek.Nazwisko = nazwisko;
            lek.Haslo = System.Text.Encoding.ASCII.GetString(_hash);
            lek.Login = login;
            lek.Wygasa = wygasa;
            lek.Aktywny = aktywny;
            lek.Kod_spec = kod_spec;

            db.Lekarzs.InsertOnSubmit(lek);

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
        /// Aktualizuje lekarza o podanym ID.
        /// </summary>
        /// <param name="id">id</param>
        /// <param name="nowe_imie">imie</param>
        /// <param name="nowe_nazwisko">nazwisko</param>
        /// <param name="nowy_login">login</param>
        /// <param name="nowe_haslo">haslo</param>
        /// <param name="nowe_wygasa">data wygaśnięcia konta</param>
        /// <returns>true jeśli pomyślnie zaktualizowano wybranego użytkownika, false jeśli wystąpił błąd</returns>
        public bool UpdateLekarz(int id, string nowe_imie, string nowe_nazwisko, string nowy_login, string nowe_haslo, DateTime? nowe_wygasa, short kod_spec)
        {
            bool retval = true;

            HashAlgorithm sha = HashAlgorithm.Create("SHA512");
            byte[] _hash = sha.ComputeHash(System.Text.Encoding.ASCII.GetBytes(nowe_haslo));

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Lekarz in db.Lekarzs
                        where Lekarz.Id_lek == id
                        select Lekarz;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Lekarz lek in query)
            {
                //Dokonanie żądanych zmian.
                lek.Imie = nowe_imie;
                lek.Nazwisko = nowe_nazwisko;
                lek.Login = nowy_login;
                lek.Haslo = _hash.ToString();
                lek.Wygasa = nowe_wygasa;
                lek.Kod_spec = kod_spec;
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
        /// Pobiera liste wszystkich lekarzy z bazy
        /// </summary>
        /// <returns>Lista lekarzy lub null, jeśli wystąpił błąd.</returns>
        public List<LekarzData> GetLekarzs()
        {
            List<LekarzData> lekarzs = new List<LekarzData>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {

                //Utworzenie zapytania.
                var query = from Lekarz in db.Lekarzs
                            orderby Lekarz.Nazwisko
                            select new
                            {
                                id = Lekarz.Id_lek,
                                imie = Lekarz.Imie,
                                nazwisko = Lekarz.Nazwisko,
                                login = Lekarz.Login,
                                aktywny = Lekarz.Aktywny,
                                wygasa = Lekarz.Wygasa,
                                kod_spec = Lekarz.Kod_spec
                            };


                //Wykonanie zapytania.
                foreach (var p in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    LekarzData lekarzData = new LekarzData(p.id, p.nazwisko, p.imie, p.login, "", p.wygasa, p.kod_spec);
                    lekarzs.Add(lekarzData);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                lekarzs = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return lekarzs;
        }

        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////    Sl badan     ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Dodaje nowy rekord do słownika badań.
        /// </summary>
        /// <param name="lab">determinuje typ dodawanego badania (true = laboratoryjne, false = fizykalne)</param>
        /// <param name="nazwa">nazwa badania</param>
        /// <param name="opis">opis badania</param>
        /// <returns>true jeśli pomyślnie dodano nową pozycję do słownika, false jeśli wystąpił błąd</returns>
        public bool AddSl_badan(string nazwa, string opis, bool lab)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Encja, aby odwzorować tabelę Pacjent.
            Przychodnia.Sl_badan bad = new Przychodnia.Sl_badan();

            bad.Lab = lab;
            bad.Opis = opis;
            bad.Nazwa = nazwa;

            db.Sl_badans.InsertOnSubmit(bad);

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
        /// Aktualizuje element slownika o podanym kodzie.
        /// </summary>
        /// <param name="kod">kod pozycji w słowniku</param>
        /// <param name="lab">determinuje typ dodawanego badania (true = laboratoryjne, false = fizykalne)</param>
        /// <param name="nazwa">nowa nazwa pozycji w słowniku</param>
        /// <param name="opis">nowy opis pozycji w słowniku</param>
        /// <returns>true jeśli pomyślnie zaktualizowano wybraną pozycję słownika, false jeśli wystąpił błąd</returns>
        public bool UpdateSl_badan(short kod, string nazwa, string opis, bool lab)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Sl_badan in db.Sl_badans
                        where Sl_badan.Kod == kod
                        select Sl_badan;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Sl_badan bad in query)
            {
                //Dokonanie żądanych zmian.
                bad.Lab = lab;
                bad.Nazwa = nazwa;
                bad.Opis = opis;
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
        /// Pobiera liste wszystkich pozycji ze slownika badan
        /// </summary>
        /// <returns>Lista pozycji ze słownika badań lub null, jeśli wystąpił błąd.</returns>
        public List<Sl_badData> GetSl_badans()
        {
            List<Sl_badData> badans = new List<Sl_badData>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {

                //Utworzenie zapytania.
                var query = from Sl_badan in db.Sl_badans
                            orderby Sl_badan.Kod
                            select Sl_badan;


                //Wykonanie zapytania.
                foreach (Przychodnia.Sl_badan p in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    Sl_badData Data = new Sl_badData(p.Kod, p.Nazwa, p.Opis, p.Lab);
                    badans.Add(Data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                badans = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return badans;
        }


        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////    Sl spec      ///////////////////////////////////////////////////////
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Dodaje nowy rekord do slownika specjalizacji
        /// </summary>
        /// <param name="nazwa">nazwa specjalizacji</param>
        /// <returns>true jeśli pomyślnie dodano nową pozycję do słownika, false jeśli wystąpił błąd</returns>
        public bool AddSl_spec(string nazwa)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Encja, aby odwzorować tabelę Pacjent.
            Przychodnia.Sl_specjalizacji spec = new Przychodnia.Sl_specjalizacji();

            spec.Nazwa = nazwa;

            db.Sl_specjalizacjis.InsertOnSubmit(spec);

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
        /// Aktualizuje element slownika specjalizacji o podanym kodzie
        /// </summary>
        /// <param name="kod">kod pozycji w słowniku</param>
        /// <param name="nazwa">nowa nazwa dla pozycji w słowniku</param>
        /// <returns>true jeśli pomyślnie zaktualizowano wybraną pozycję słownika, false jeśli wystąpił błąd</returns>
        public bool UpdateSl_spec(short kod, string nazwa)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Utworzenie zapytania - pobranie z tabeli rekordu, który ma być zmieniony.
            var query = from Sl_specjalizacji in db.Sl_specjalizacjis
                        where Sl_specjalizacji.Kod_spec == kod
                        select Sl_specjalizacji;

            //Wykonanie zapytania w pętli foreach.
            foreach (Przychodnia.Sl_specjalizacji spec in query)
            {
                //Dokonanie żądanych zmian.
                spec.Nazwa = nazwa;
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
        /// Pobiera liste wszystkich pozycji ze slownika specjalizacji
        /// </summary>
        /// <returns>Lista pozycji ze słownika specjalizacji lub null, jeśli wystąpił błąd.</returns>
        public List<Sl_specData> GetSl_specs()
        {
            List<Sl_specData> specs = new List<Sl_specData>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {

                //Utworzenie zapytania.
                var query = from Sl_specjalizacji in db.Sl_specjalizacjis
                            orderby Sl_specjalizacji.Kod_spec
                            select Sl_specjalizacji;


                //Wykonanie zapytania.
                foreach (Przychodnia.Sl_specjalizacji p in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    Sl_specData Data = new Sl_specData(p.Kod_spec, p.Nazwa);
                    specs.Add(Data);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                specs = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return specs;
        }
    }
}
