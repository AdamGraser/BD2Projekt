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
        /// Pobiera z bazy dane potrzebne do logowania i sprawdza czy zgadzają się z podanymi parametrami.
        /// </summary>
        /// <param name="login">Login do wyszukania w bazie</param>
        /// <param name="passwordHash">Hash hasła</param>
        /// <returns>true - jeżeli użytkownik został znaleziony, false gdy podane parametry nie zgadzają się z zawartością bazy.</returns>
        public bool? FindUser(string login, byte[] passwordHash)
        {
            bool retval = false;
            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                string temp = System.Text.Encoding.ASCII.GetString(passwordHash);

                //Utworzenie zapytania.
                bool userExistsInDb = (from Rejestratorka in db.Rejestratorkas                                       
                                       where Rejestratorka.Login == login &&
                                             Rejestratorka.Haslo.StartsWith(temp) &&
                                             Rejestratorka.Haslo.EndsWith(temp)
                                       select new
                                       {
                                           rej_id = Rejestratorka.Id_rej
                                       }).Count() == 1;
                retval = userExistsInDb;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }
            return retval;
        }

        /// <summary>
        /// Pobiera z tabeli Pacjent imiona i nazwiska wszystkich pacjentów.
        /// </summary>
        /// <returns>Zwraca listę imion i nazwisk oddzielonych spacją lub null, jeśli wystąpił błąd.</returns>
        public List<string> GetPatients()
        {
            List<string> patientsList = new List<string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Pacjent in db.Pacjents
                            select new
                            {
                                imie = Pacjent.Imie,
                                nazwisko = Pacjent.Nazwisko
                            };

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (var pac in query)
                {
                    //Łączenie imion i nazwisk, zapisywanie ich.
                    patientsList.Add(pac.imie + " " + pac.nazwisko);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                patientsList = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return patientsList;
        }



        /// <summary>
        /// Pobiera z tabeli Pacjent szczegółowe informacje o wskazanym pacjencie.
        /// Zwrócona struktura następujące tekstowe indeksy: "pesel", "dataur", "adres".
        /// </summary>
        /// <param name="id_pac">ID pacjenta, którego szczegóły mają zostać zwrócone.</param>
        /// <returns>Zestaw szczegółów o wskazanym pacjencie lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<string, string> GetPatientDetails(int id_pac)
        {
            Dictionary<string, string> patientDetails = new Dictionary<string, string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Pacjent in db.Pacjents
                            where Pacjent.Id_pac == id_pac
                            select new
                            {
                                pesel = Pacjent.Pesel,
                                dataUr = Pacjent.Data_ur,
                                ulica = Pacjent.Ulica,
                                nrBud = Pacjent.Nr_bud,
                                nrMiesz = Pacjent.Nr_miesz,
                                kodPocz = Pacjent.Kod_pocz,
                                miasto = Pacjent.Miasto
                            };

                //Wykonanie zapytania.
                foreach (var p in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    patientDetails.Add("pesel", p.pesel.ToString());
                    patientDetails.Add("dataur", p.dataUr.ToString());
                    patientDetails.Add("adres", p.ulica + " " + p.nrBud.ToString() + (p.nrMiesz != null ? " " + p.nrMiesz.ToString() + ", " : ", ") + p.kodPocz + " " + p.miasto);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                patientDetails = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return patientDetails;
        }



        /// <summary>
        /// Pobiera z tabeli Lekarz imiona i nazwiska wszystkich lekarzy.
        /// </summary>
        /// <returns>Zwraca listę imion i nazwisk oddzielonych spacją lub null, jeśli wystąpił błąd.</returns>
        public List<string> GetDoctors()
        {
            List<string> doctorsList = new List<string>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                //Utworzenie zapytania.
                var query = from Lekarz in db.Lekarzs
                            select new
                            {
                                imie = Lekarz.Imie,
                                nazwisko = Lekarz.Nazwisko
                            };

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (var lek in query)
                {
                    //Łączenie imion i nazwisk, zapisywanie ich.
                    doctorsList.Add(lek.imie + " " + lek.nazwisko);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                doctorsList = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return doctorsList;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<VisitData> GetVisits()
        {
            List<VisitData> visitsList = new List<VisitData>();
            //string doctorName = null;
            //string doctorSurname = null;
            /*
            if (doctor != null)
            {
                doctorName = doctor.Substring(0, doctor.IndexOf(" "));
                doctorSurname = doctor.Substring(doctor.IndexOf(" ") + 1);
            }
            */
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
                            join Lekarz in db.Lekarzs on Wizyta.Id_lek equals Lekarz.Id_lek
                            where (Wizyta.Stan == null && Wizyta.Data_rej <= DateTime.Now)
                            select new
                            {
                                id = Wizyta.Id_wiz,
                                imie_pacjenta = Pacjent.Imie,
                                nazwisko_pacjenta = Pacjent.Nazwisko,
                                data_urodzenia = Pacjent.Data_ur,
                                pesel = Pacjent.Pesel,
                                dataRej = Wizyta.Data_rej,
                                imie_lekarza = Lekarz.Imie,
                                nazwisko_lekarza = Lekarz.Nazwisko
                            };

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (var vis in query)
                {
                    VisitData visData = new VisitData();
                    visData.PatientName = vis.imie_pacjenta;
                    visData.PatientSurname = vis.nazwisko_pacjenta;
                    visData.PatientDateOfBirth = vis.data_urodzenia.ToString();
                    visData.PatientPesel = vis.pesel.ToString();
                    visData.Date = vis.dataRej.ToString();
                    visData.Doctor = vis.imie_lekarza + " " + vis.nazwisko_lekarza;
                    visitsList.Add(visData);
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

        /// <summary>
        /// Metoda usuwająca wizytę z bazy.
        /// </summary>
        /// <param name="patientName">imię pacjenta</param>
        /// <param name="patientSurname">nazwisko pacjenta</param>
        /// <param name="patientDateOfBirth">data urodzenia pacjenta</param>
        /// <param name="patientPesel">PESEL pacjenta</param>
        /// <param name="dateOfVisit">data wizyty</param>
        /// <param name="doctor"></param>
        /// <returns></returns>
        public bool DeleteVisit(string patientName, string patientSurname, string patientPesel, string dateOfVisit, byte doctorID)
        {
            bool retval = true;            
            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Encja, aby odwzorować tabelę Wizyta.
            Przychodnia.Wizyta wiz = new Przychodnia.Wizyta();

            //Pobranie z bazy id pacjenta na podstawie imienia, nazwiska i PESEL-u.
            var patientIdQuery = from Pacjent in db.Pacjents
                        where Pacjent.Imie == patientName && Pacjent.Nazwisko == patientSurname && Pacjent.Pesel == long.Parse(patientPesel)
                        select Pacjent.Id_pac;

            foreach (int patientId in patientIdQuery)
            {
                wiz.Id_pac = patientId;
            }

            wiz.Data_rej = Convert.ToDateTime(dateOfVisit);
            wiz.Id_lek = doctorID;
                                              
            //Przygotowanie wszystkiego do wysłania.
            db.Wizytas.DeleteOnSubmit(wiz);

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
        /// Dodaje do bazy nowego pacjenta.
        /// </summary>
        /// <param name="name">imię pacjenta</param>
        /// <param name="surname">nazwisko pacjenta</param>
        /// <param name="dateOfBirth">data urodzenia pacjenta</param>
        /// <param name="pesel">PESEL pacjenta</param>
        /// <param name="numberOfHouse">numer domu pacjenta</param>
        /// <param name="numberOfFlat">numer mieszkania pacjenta</param>
        /// <param name="street">nazwa ulicy na której mieszka pacjent</param>
        /// <param name="postCode">kod pocztowy z adresu pacjenta</param>
        /// <param name="city">miasto pacjenta</param>
        /// <returns>Wartość true jeśli nowy rekord został pomyślnie dodany do tabeli. W razie wystąpienia błędu zwraca wartość false.</returns>
        public bool AddPatient(string name, string surname, DateTime dateOfBirth, string pesel, string numberOfHouse, string numberOfFlat, string street, string postCode, string city)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            //Encja, aby odwzorować tabelę Pacjent.
            Przychodnia.Pacjent pac = new Przychodnia.Pacjent();
            pac.Imie = name;
            pac.Nazwisko = surname;
            pac.Pesel = long.Parse(pesel);
            pac.Data_ur = dateOfBirth;
            pac.Nr_miesz = int.Parse(numberOfFlat);
            pac.Nr_bud = int.Parse(numberOfHouse);
            pac.Ulica = street;
            pac.Kod_pocz = postCode;
            pac.Miasto = city;
            

            db.Pacjents.InsertOnSubmit(pac);

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
    }
}
