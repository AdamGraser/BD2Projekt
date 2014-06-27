using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class FictionDatabase
    {
            static bool kier; //Informacja, czy dany laborant jest kierownikiem

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

                TestLabInfo nazwy = new TestLabInfo();
                nazwy.id_bad = 1;
                nazwy.id_wiz = 1;
                nazwy.short_opis = "12_13_14 Tereferekuku, ważne!";
                labTests.Add(nazwy);
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
                testsIDs.Add(2);
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

                        //Zapisanie wyników w odpowiednich elementach.
                        labTestDetails.Add("To badanie jest bardzo ważne");
                        labTestDetails.Add("Adam");
                        labTestDetails.Add("Kubełek");
                        labTestDetails.Add("Dostateczny");
                        labTestDetails.Add("Kierownik nie ma uwag");

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
                bool? retval = true;

                return retval;
            }
        }
    }

