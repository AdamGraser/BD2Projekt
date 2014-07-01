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
    public class TestLabInfo
    {
        public int id_wiz;
        public byte id_bad;
        public string opis;
    }
  
    /// <summary>
    /// Realizuje połączenie z bazą danych oraz wszystkie operacje na niej wykonywane.
    /// </summary>
    public class DBClient
    {
        SqlConnection connection;
        SqlTransaction transaction;
        Przychodnia.Przychodnia db;
        static byte id_lab;   //ID laboranta obecnie zalogowanego w systemie
        static bool kier; //Informacja, czy dany laborant jest kierownikiem

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

        public void ResetIdLab()
        {
            id_lab = 0;
        }

        /// <summary>
        /// Akcesor do informacji nt. kierownika
        /// </summary>
        /// <returns></returns>
        public bool isKier()
        {
            return kier;
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
                            select new
                            {
                                id = Laborant.Id_lab,
                                kier = Laborant.Kier
                            };

                id_lab = 0;

                //Sprawdzenie czy w bazie istnieje dokładnie 1 rekord z podanymi wartościami w kolumnach login i haslo.
                foreach (var q in query)
                {
                    if (id_lab == 0)
                    {
                        id_lab = q.id;
                        kier = q.kier;
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
        /// Pobiera z tabel Badanie i Sl_badan nazwy + opisy i daty zlecenia badań laboratoryjnych o określonym stanie.
        /// </summary>
        /// <returns>Zwraca listę dat zlecenia i nazw wybranych badań laboratoryjnych lub null, jeśli wystąpił błąd.</returns>
        public List<TestLabInfo> GetLabTests(int state, DateTime? dateFrom, DateTime? dateTo)
        {
            List<TestLabInfo> labTests = new List<TestLabInfo>();

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
                            orderby Badanie.Id_wiz, Badanie.Id_bad
                            where (Badanie.Stan == state) && (Sl_badan.Lab == true)
                            select new
                            {
                                idWiz = Badanie.Id_wiz,
                                idBad = Badanie.Id_bad,
                                dataZle = Badanie.Data_zle,
                                nazwa = Sl_badan.Nazwa,
                                opis = Sl_badan.Opis
                            };

                //Żeby pokazało daty do godziny 0:00 dnia następnego
                DateTime? dT = dateTo == null ? null : (DateTime?)dateTo.Value.AddDays(1);

                //Wykonanie zapytania.
                foreach (var b in query)
                {
                    //Sprawdzenie, czy nasze zapytanie spełnia kryteria
                    if ( ((dateFrom == null) || (b.dataZle >= dateFrom)) &&
                         ((dT == null) || (b.dataZle < dT)))
                    {
                        //Zapisanie wyników.

                        TestLabInfo tli = new TestLabInfo();
                        tli.id_wiz = b.idWiz;
                        tli.id_bad = b.idBad;
                        tli.opis = b.dataZle.ToString() + " " + b.nazwa + ", " + b.opis;
                        labTests.Add(tli);
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
        /// Pobiera z tabel Badanie i Lekarz szczegóły badania.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której to badanie zostało zlecone.</param>
        /// <param name="id_bad">L.p. badania dla tej wizyty.</param>
        /// <returns>Listę trzech informacji szczegółowych o badaniu w podanej kolejności:
        /// - opis
        /// - imię lekarza
        /// - nazwisko lekarza
        /// - wynik badania
        /// - informacje od kierownika dot. anulowania
        /// W przypadku wystąpienia błędu zwraca null.</returns>
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
                                wynik = Badanie.Wynik,
                                kier_info = Badanie.Uwagi,
                                data_wyk_bad = Badanie.Data_wyk_bad
                            };

                //Wykonanie zapytania.
                foreach (var l in query)
                {
                    //Zapisanie wyników w odpowiednich elementach.
                    labTestDetails.Add(l.opis);
                    labTestDetails.Add(l.imie);
                    labTestDetails.Add(l.nazwisko);
                    labTestDetails.Add(l.wynik);
                    labTestDetails.Add(l.kier_info);
                    labTestDetails.Add(l.data_wyk_bad.ToString());
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
        /// Funkcja aktualizuje dane dotyczące wskazanego badania laboratoryjnego. Argument "wynik" nie może być null.
        /// </summary>
        /// <param name="id_wiz">ID wizyty, w trakcie której to badanie zostało zlecone.</param>
        /// <param name="id_bad">L.p. badania dla tej wizyty.</param>
        /// <param name="data_wyk_bad">Data wykonania badania (null jeśli nie trzeba).</param>
        /// <param name="wynik">Wynik badania.</param>
        /// <param name="zatw">null jeśli badanie zatwierdzono, false jeśli anulowano.</param>
        /// <returns>True jeśli cały proces przebiegł prawidłowo, false jeśli nastąpił błąd przy wysyłaniu danych/próbie zapisu danych w bazie, null jeśli w bazie coś się zmieniło.</returns>
        public bool? SaveLabTest(int id_wiz, byte id_bad, DateTime data_wyk_bad, string wynik, string powod_anul, byte stan_po_zmianie, byte stan_przed_zmiana)
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
                if (stan_przed_zmiana == bad.Stan)
                {
                    if (stan_po_zmianie == 2) // Jeśli to jest wykonanie badania, trzeba przypisać datę
                        bad.Data_wyk_bad = data_wyk_bad;

                    bad.Wynik = wynik;
                    bad.Uwagi = powod_anul;
                    bad.Stan = stan_po_zmianie;
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
    
    /*
    // TYLKO DO DEBUGOWANIA, GDY NIE MA POLACZENIA Z BAZA DANYCH!!!
    public class DBClient
    {
        private class FikcyjneBadanie
        {
            public int id_wid;
            public byte id_bad;
            public byte stan;
            public DateTime dataZle;
            public DateTime? dataWyk;
            public string typ_badania;
            public string opis;
            public string wynik;
            public string uwagi;
            public string imielek;
            public string nazwlek;
        }

        static bool kier; //Informacja, czy dany laborant jest kierownikiem
        List<FikcyjneBadanie> fikcyjnaBaza;

        public DBClient()
        {
            fikcyjnaBaza = new List<FikcyjneBadanie>();
            FikcyjneBadanie nowe;

            nowe = new FikcyjneBadanie();
            nowe.stan = 0;
            nowe.id_wid = 1;
            nowe.id_bad = 1;
            nowe.imielek = "Adam";
            nowe.nazwlek = "Kubełek";
            nowe.dataWyk = null;
            nowe.opis = "Pilne!";
            nowe.typ_badania = "Badanie krwi";
            nowe.dataZle = new DateTime(2014, 6, 13);
            nowe.wynik = "";
            nowe.uwagi = "";
            fikcyjnaBaza.Add(nowe);

            nowe = new FikcyjneBadanie();
            nowe.stan = 0;
            nowe.id_wid = 1;
            nowe.id_bad = 2;
            nowe.imielek = "Adam";
            nowe.nazwlek = "Kubełek";
            nowe.dataWyk = null;
            nowe.opis = "";
            nowe.typ_badania = "Badanie moczu";
            nowe.dataZle = new DateTime(2014, 6, 13);
            nowe.wynik = "";
            nowe.uwagi = "";
            fikcyjnaBaza.Add(nowe);

            nowe = new FikcyjneBadanie();
            nowe.stan = 0;
            nowe.id_wid = 2;
            nowe.id_bad = 1;
            nowe.imielek = "Jan";
            nowe.nazwlek = "Kowalski";
            nowe.dataWyk = null;
            nowe.opis = "Nie musi być dokładne";
            nowe.typ_badania = "Prześwietlenie ręki";
            nowe.dataZle = new DateTime(2014, 3, 12);
            nowe.wynik = "";
            nowe.uwagi = "";
            fikcyjnaBaza.Add(nowe);
        }

        /// <summary>
        /// Zwalnia zasoby zajmowane przez pole "db: Przychodnia".
        /// </summary>
        public void Dispose()
        {
        }

        /// <summary>
        /// Akcesor do informacji nt. kierownika
        /// </summary>
        /// <returns></returns>
        public bool isKier()
        {
            return kier;
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
            bool? retval = true;

            if (login == "kier")
            {
                kier = true;
            }
            else
            {
                kier = false;
            }

            return retval;
        }



        /// <summary>
        /// Pobiera z tabel Badanie i Sl_badan nazwy + opisy i daty zlecenia, zrealizowanych lub nie, badań laboratoryjnych.
        /// </summary>
        /// <returns>Zwraca listę dat zlecenia i nazw wybranych badań laboratoryjnych lub null, jeśli wystąpił błąd.</returns>
        public List<TestLabInfo> GetLabTests(int state, DateTime? dateFrom, DateTime? dateTo)
        {
            List<TestLabInfo> labTests = new List<TestLabInfo>();

            foreach (FikcyjneBadanie b in fikcyjnaBaza)
            {
                if (b.stan == state)
                {
                    TestLabInfo doDodania = new TestLabInfo();
                    doDodania.id_bad = b.id_bad;
                    doDodania.id_wiz = b.id_wid;
                    doDodania.opis = b.dataZle.ToString() + " " + b.typ_badania;
                    labTests.Add(doDodania);
                }
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
            List<string> labTestDetails = null;

            foreach (FikcyjneBadanie b in fikcyjnaBaza)
            {
                if ((b.id_bad == id_bad) && (b.id_wid == id_wiz))
                {
                    labTestDetails = new List<string>();

                    //Zapisanie wyników w odpowiednich elementach.
                    labTestDetails.Add(b.opis);
                    labTestDetails.Add(b.imielek);
                    labTestDetails.Add(b.nazwlek);
                    labTestDetails.Add(b.wynik);
                    labTestDetails.Add(b.uwagi);
                    labTestDetails.Add(b.dataWyk.ToString());

                    break;
                }
            }
            return labTestDetails;
        }

        public void ResetIdLab()
        {
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
        public bool? SaveLabTest(int id_wiz, byte id_bad, DateTime data_wyk_bad, string wynik, string powod_anul, byte stan_po_zmianie, byte stan_przed_zmiana)
        {
            bool? retval = false;

            foreach (FikcyjneBadanie b in fikcyjnaBaza)
            {
                if ((b.id_bad == id_bad) && (b.id_wid == id_wiz))
                {
                    b.wynik = wynik;
                    b.uwagi = powod_anul;
                    b.stan = stan_po_zmianie;
                    b.dataWyk = data_wyk_bad;

                    retval = true;
                    break;
                }
            }

            return retval;
        }
    }
    */
}
