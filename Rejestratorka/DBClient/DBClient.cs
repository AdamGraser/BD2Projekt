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
        static byte id_rej;   //ID rejestratorki obecnie zalogowanej w systemie



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
        /// Przypisuje polu id_rej wartość 0 - jest to swoisty reset tego pola, który powinien dla bezpieczeństwa być wykonywany przy wylogowaniu.
        /// </summary>
        public void ResetIdRej()
        {
            id_rej = 0;
        }



        /// <summary>
        /// Zwalnia zasoby zajmowane przez pole "db: Przychodnia".
        /// </summary>
        public void Dispose()
        {
            db.Dispose();
        }



        /// <summary>
        /// Zlicza wizyty zarejestrowane na podany dzień do podanego lekarza.
        /// </summary>
        /// <param name="doctorID">ID lekarza, do którego zarejestrowane są zliczane wizyty</param>
        /// <param name="date">Dzień, z którego wizyty są zliczane</param>
        /// <returns>Liczba wizyt zarejestrowanych na podany dzień do podanego lekarza, lub null w przypadku wystąpienia błędu.</returns>
        public int? GetNumberOfVisits(byte doctorID, DateTime? date)
        {
            int? numberOfVisits = null;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {                             
                numberOfVisits = (from Wizyta in db.Wizytas                            
                            where (Wizyta.Stan == 0 && Wizyta.Id_lek == doctorID && Wizyta.Data_rej.Date == date.GetValueOrDefault().Date)
                            select Wizyta.Id_wiz).Count();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                numberOfVisits = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return numberOfVisits;
        }



        /// <summary>
        /// Pobiera z bazy godziny, na które zarejestrowane są wizyty do podanego lekarza w podanym dniu.
        /// </summary>
        /// <param name="id_lek">ID lekarza, dla którego pobierane są godziny wizyt.</param>
        /// <param name="day">Dzień, z którego godziny wizyt mają zostać pobrane.</param>
        /// <returns>Lista obiektów DateTime z godzinami zarejestrowanych wizyt i datą podaną w argumencie, posortowanych rosnąco.</returns>
        public List<DateTime> GetHoursOfVisits(byte id_lek, DateTime day)
        {
            List<DateTime> hoursOfVisits = new List<DateTime>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {
                var query = from Wizyta in db.Wizytas
                            where Wizyta.Id_lek == id_lek && Wizyta.Data_rej.Date == day
                            orderby Wizyta.Data_rej
                            select Wizyta.Data_rej;

                foreach (DateTime visitDate in query)
                {
                    hoursOfVisits.Add(visitDate);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                hoursOfVisits = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return hoursOfVisits;
        }



        /// <summary>
        /// Pobiera z bazy dane potrzebne do logowania i sprawdza czy zgadzają się z podanymi parametrami.
        /// </summary>
        /// <param name="login">Login do wyszukania w bazie</param>
        /// <param name="passwordHash">Hash hasła</param>
        /// <returns>true - jeżeli użytkownik został znaleziony, false gdy podane parametry nie zgadzają się z zawartością bazy, null jeśli wystąpił błąd.</returns>
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
                var query = from Rejestratorka in db.Rejestratorkas                                       
                          where Rejestratorka.Login == login &&
                                  Rejestratorka.Haslo.StartsWith(temp) &&
                                  Rejestratorka.Haslo.Length == temp.Length
                          select Rejestratorka.Id_rej;

                foreach (byte q in query)
                {
                    if (id_rej == 0)
                    {
                        id_rej = q;
                        retval = true;
                    }
                    else
                    {
                        id_rej = 0;
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
        /// Pobiera z tabeli Pacjent imiona i nazwiska wszystkich pacjentów.
        /// </summary>
        /// <returns>Zwraca listę imion i nazwisk oddzielonych spacją lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<int, PatientData> GetPatients(string name, string surname, long? pesel)
        {
            Dictionary<int, PatientData> patients = new Dictionary<int, PatientData>();

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {                        
                if (pesel != null && name.Length == 0 && surname.Length == 0)
                {
                    double temp = (long)pesel / 10000000000;
                    ++temp;
                    
                    //Utworzenie zapytania.
                    var query = from Pacjent in db.Pacjents
                                where Pacjent.Pesel >= pesel && Pacjent.Pesel < (long)temp * 10000000000
                                orderby Pacjent.Nazwisko
                                select new
                                {
                                    id = Pacjent.Id_pac,
                                    imie = Pacjent.Imie,
                                    nazwisko = Pacjent.Nazwisko,
                                    pesel = Pacjent.Pesel,
                                    plec = Pacjent.Plec,
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
                        PatientData patientData = new PatientData();
                        //Zapisanie wyników w odpowiednich elementach.
                        patientData.PatientName = p.imie;
                        patientData.PatientSurname = p.nazwisko;
                        patientData.PatientPesel = p.pesel.ToString();
                        if (p.plec == false)
                        {
                            patientData.PatientGender = "Mężczyzna";
                        }
                        else
                        {
                            patientData.PatientGender = "Kobieta";
                        }
                        patientData.PatientDateOfBirth = p.dataUr.ToShortDateString();
                        patientData.PatientCity = p.miasto;
                        patientData.PatientStreet = p.ulica;
                        patientData.PatientNumberOfHouse = p.nrBud;
                        patientData.PatientNumberOfFlat = p.nrMiesz;
                        patientData.PatientPostCode = p.kodPocz;
                        patients.Add(p.id, patientData);
                    }
                }
                else if ((name.Length > 0 || surname.Length > 0) && pesel == null)
                {
                    //Utworzenie zapytania.
                    var query = from Pacjent in db.Pacjents
                                where Pacjent.Imie.ToLower().StartsWith(name.ToLower()) && Pacjent.Nazwisko.ToLower().StartsWith(surname.ToLower())
                                select new
                                {
                                    id = Pacjent.Id_pac,
                                    imie = Pacjent.Imie,
                                    nazwisko = Pacjent.Nazwisko,
                                    pesel = Pacjent.Pesel,
                                    plec = Pacjent.Plec,
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
                        PatientData patientData = new PatientData();
                        //Zapisanie wyników w odpowiednich elementach.
                        patientData.PatientName = p.imie;
                        patientData.PatientSurname = p.nazwisko;
                        patientData.PatientPesel = p.pesel.ToString();
                        if (p.plec == false)
                        {
                            patientData.PatientGender = "Mężczyzna";
                        }
                        else
                        {
                            patientData.PatientGender = "Kobieta";
                        }
                        patientData.PatientDateOfBirth = p.dataUr.ToShortDateString();
                        patientData.PatientCity = p.miasto;
                        patientData.PatientStreet = p.ulica;
                        patientData.PatientNumberOfHouse = p.nrBud;
                        patientData.PatientNumberOfFlat = p.nrMiesz;
                        patientData.PatientPostCode = p.kodPocz;
                        patients.Add(p.id, patientData);
                    }
                }
                else
                {
                    //Utworzenie zapytania.
                    var query = from Pacjent in db.Pacjents
                                where Pacjent.Imie.ToLower().StartsWith(name.ToLower()) && Pacjent.Nazwisko.ToLower().StartsWith(surname.ToLower()) && Pacjent.Pesel >= pesel
                                select new
                                {
                                    id = Pacjent.Id_pac,
                                    imie = Pacjent.Imie,
                                    nazwisko = Pacjent.Nazwisko,
                                    pesel = Pacjent.Pesel,
                                    plec = Pacjent.Plec,
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
                        PatientData patientData = new PatientData();
                        //Zapisanie wyników w odpowiednich elementach.
                        patientData.PatientName = p.imie;
                        patientData.PatientSurname = p.nazwisko;
                        patientData.PatientPesel = p.pesel.ToString();
                        if (p.plec == false)
                        {
                            patientData.PatientGender = "Mężczyzna";
                        }
                        else
                        {
                            patientData.PatientGender = "Kobieta";
                        }
                        patientData.PatientDateOfBirth = p.dataUr.ToShortDateString();
                        patientData.PatientCity = p.miasto;
                        patientData.PatientStreet = p.ulica;
                        patientData.PatientNumberOfHouse = p.nrBud;
                        patientData.PatientNumberOfFlat = p.nrMiesz;
                        patientData.PatientPostCode = p.kodPocz;
                        patients.Add(p.id, patientData);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Console.WriteLine(e.HelpLink);
                Console.WriteLine(e.StackTrace);

                patients = null;
            }
            finally
            {
                //Zakończenie transakcji, zamknięcie połączenia z bazą danych, zwolnienie zasobów (po obu stronach).
                connection.Close();
            }

            return patients;
        }


        /*
        /// <summary>
        /// Pobiera z tabeli Pacjent szczegółowe informacje o wskazanym pacjencie.
        /// Zwrócona struktura następujące tekstowe indeksy: "pesel", "plec", "dataur", "adres".
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
                                plec = Pacjent.Plec,
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
                    patientDetails.Add("plec", p.plec ? "Kobieta" : "Mężczyzna");
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
        */


        /// <summary>
        /// Pobiera z tabeli Lekarz imiona i nazwiska wszystkich lekarzy.
        /// </summary>
        /// <returns>Zwraca listę imion i nazwisk oddzielonych spacją lub null, jeśli wystąpił błąd.</returns>
        public Dictionary<byte, string> GetDoctors()
        {
            Dictionary<byte, string> doctorsList = new Dictionary<byte, string>();            

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
                                imie = Lekarz.Imie,
                                nazwisko = Lekarz.Nazwisko,
                                id = Lekarz.Id_lek
                            };

                //Wykonanie zapytania, rekord po rekordzie.
                foreach (var lek in query)
                {                    
                    doctorsList.Add(lek.id, lek.imie + " " + lek.nazwisko);
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
        /// Wyszukuje w bazie danych wizyty, które się nie odbyły.
        /// </summary>
        /// <returns>Lista rekordów z tabeli Wizyta, które w kolumnie data_rej mają wartość mniejszą niż bieżący czas.</returns>
        public Dictionary<int, VisitData> GetVisits(byte status)
        {
            Dictionary<int, VisitData> visitsList = new Dictionary<int, VisitData>();
            
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
                            where Wizyta.Stan == status
                            orderby Wizyta.Data_rej descending
                            select new
                            {
                                id = Wizyta.Id_wiz,
                                imie_pacjenta = Pacjent.Imie,
                                nazwisko_pacjenta = Pacjent.Nazwisko,
                                data_urodzenia = Pacjent.Data_ur,
                                pesel = Pacjent.Pesel,
                                dataRej = Wizyta.Data_rej,
                                imie_lekarza = Lekarz.Imie,
                                nazwisko_lekarza = Lekarz.Nazwisko,
                                stan_wizyty = Wizyta.Stan
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
                    switch(vis.stan_wizyty)
                    {
                        case 0:
                            visData.Status = "Zarejestowana";
                            break;
                        case 1:
                            visData.Status = "Realizowana";
                            break;
                        case 2:
                            visData.Status = "Anulowana";
                            break;
                        case 3:
                            visData.Status = "Zakończona";
                            break;
                    }
                    visitsList.Add(vis.id, visData);
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

            /*
            //Pobranie z bazy id pacjenta na podstawie PESEL-u.
            var patientIdQuery = from Pacjent in db.Pacjents
                                 where Pacjent.Pesel == pesel
                                 select Pacjent.Id_pac;
            
            int patientId = -1; //inicjalizacja, ponieważ inaczej w poniższym zapytaniu patientId mogło być niezainicjalizowane

            foreach (int p in patientIdQuery)
            {
                patientId = p;
            }
            */

            //Encja, aby odwzorować tabelę Wizyta.
            Przychodnia.Wizyta wiz = new Przychodnia.Wizyta();

            wiz.Data_rej = data_rej;
            wiz.Id_rej = id_rej;
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


        /*
        /// <summary>
        /// Metoda usuwająca wizytę z bazy.
        /// </summary>
        /// <param name="visitID">ID wizyty</param>
        /// <returns></returns>
        public bool DeleteVisit(int visitID)
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            
            //Pobranie z bazy id pacjenta na podstawie PESEL-u.
            var patientIdQuery = from Pacjent in db.Pacjents
                        where Pacjent.Pesel == long.Parse(patientPesel)
                        select Pacjent.Id_pac;

            int patientId = -1; //inicjalizacja, ponieważ inaczej w poniższym zapytaniu patientId mogło być niezainicjalizowane
            
            foreach (int p in patientIdQuery)
            {
                patientId = p;
            }
            

            var query = from Wizyta in db.Wizytas
                        where Wizyta.Id_wiz == visitID
                        select Wizyta;

            //id_wiz jest kluczem głównym tabeli Wizyta, co zapewnia unikalność wartości w tej kolumnie - taka wizyta jest tylko jedna
            foreach (Przychodnia.Wizyta wiz in query)
            {
                db.Wizytas.DeleteOnSubmit(wiz);
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
        */


        /// <summary>
        /// Anuluje wizytę o wskazanym ID.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, która ma zostać usunięta.</param>
        /// <returns>true jeśli wizyta została pomyślnie usunięta z bazy danych, false jeśli wystąpił błąd.</returns>
        public bool CancelVisit(int id_wiz)
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
                wiz.Stan = 2; //zmiana stanu na "anulowana"
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
        public bool AddPatient(string name, string surname, DateTime dateOfBirth, string pesel, bool gender, string numberOfHouse, string numberOfFlat, string street, string postCode, string city)
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
            pac.Plec = gender;
            pac.Nr_miesz = numberOfFlat;
            pac.Nr_bud = numberOfHouse;
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



        public bool? IsAccountExpired(string login)
        {
            bool? retval = false;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            try
            {

                //Utworzenie zapytania.
                var query = from Lekarz in db.Lekarzs
                            where Lekarz.Login == login
                            select Lekarz.Wygasa;



                foreach (DateTime? date in query)
                {
                    if (date == null || date > DateTime.Now)
                    {
                        retval = false;
                    }
                    else
                    {
                        retval = true;
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


        public bool CancelUndoneVisits()
        {
            bool retval = true;

            //Łączenie się z bazą danych.
            connection.Open();

            //Rozpoczęcie transakcji z bazą danych, do wykorzystania przez LINQ to SQL.
            transaction = connection.BeginTransaction(IsolationLevel.RepeatableRead);
            db.Transaction = transaction;

            var query = from Wizyta in db.Wizytas
                        where Wizyta.Data_rej.Date == DateTime.Today
                        select Wizyta;

            //id_wiz jest kluczem głównym tabeli Wizyta, co zapewnia unikalność wartości w tej kolumnie - taka wizyta jest tylko jedna
            foreach (Przychodnia.Wizyta wiz in query)
            {
                wiz.Stan = 2; //zmiana stanu na "anulowana"
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
    }
}
