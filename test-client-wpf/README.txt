##########################################################################################################
#####################################      Wst�p do tej masakry      #####################################
##########################################################################################################

Oczywi�cie je�li chcecie to odpali�, musicie pozby� si� ��czenia z baz� danych w konstruktorze DBClient
i zast�pi� wszelkie wywo�ania metod klasy DBClient w MainWindow sztywnym podaniem danych.
Wy piszcie tych Waszych klient�w tak jak ma by�, czyli tak jak ja tu pisa�em z t� klas� DBClient,
korzystaj�c z klasy Przychodnia i innych. Je�li chcecie, mo�ecie potem te� w MainWindow (czy jak tam klas�
okna g��wnego nazwiecie) zakomentowa� wszystkie wywo�ania tych metod wykonuj�cych zapytania do bazy na
klasie Przychodnia i tych innych klasach Encji, a w ich miejsca te� wstawi� na sztywno jakie� warto�ci i
co nieco potestowa�. Testowanie i uruchamianie i to i tak b�dzie moja dzia�ka, bo ja mam baz� (po bo Wy
te� macie instalowa� SQL Server i tworzy� baz�, si� w to bawi�).
Aha, skoro ten, kto robi klienta admina (naj�atwiejszego) musi si� pom�czy� z Crystal Reports for Visual
Studio i porobi� wydruki jakie� (chyba jakie� przyk�ady da�em na grupie dyskusyjnej), to musicie wybra�
spo�r�d Was jednego, kt�ry zrobi jakiego� ma�ego helpa w html, co se b�dzie mo�na odpali� z poziomu
aplikacji (ze 2 funkcje programu opisa� tam, czy co�, bez zb�dnego pieprzenia si�).

Najwi�kszy syf narobi�em niew�a�ciwym nazewnictwem. Przepraszam. Dopiero podczas tworzenia karty lekarza
kapn��em si�, �e mog�em dawa� nazwy wg. schematu, kt�ry zastosowa�em w kartach lab. i klab., a nie
chcia�em ju� zmienia� wszystkiego, co ju� mia�em zrobione. Wy na szcz�cie nie b�dziecie mieli takich
problem�w, bo robicie zupe�nie osobne aplikacje (czyli najpewniej zupe�nie osobne solucje VS).

Umie�ci�em informacj� o osobie zalogowanej na pasku tytu�owym. Oczywi�cie jest to ustawione na sztywno,
nie bawi�em si� w logowanie (jeszcze by tego brakowa�o :P). Mo�na tak zrobi�, wtedy trzeba by doda� jaki�
chowalny toolbox z boku, kt�ry b�dzie zawiera� przycisk "Wyloguj", albo zrobi� 2 karty: jedna zawiera�aby
formularz logowania, a po zalogowaniu tylko przycisk "Wyloguj"; druga z kolei zawiera�aby t� g��wn� tre��
aplikacji, kt�ra oczywi�cie pojawia�aby si� tam dopiero po zalogowaniu.
Ale dobr� opcj� jest r�wnie� realizacja formularza logowania/przycisku "Wyloguj" na toolbarze u g�ry, a w
obszarze roboczym niech b�dzie tylko ta w�a�ciwa tre��. Cho� nie wiem jak wygl�da umieszczanie dowolnych
kontrolek w toolbarze.
Tak czy siak, jakkolwiek dziwne by nie by�y te imiona i nazwiska, wiedzcie, �e s� to dane testowe, kt�rych
mia�em u�y� w naszym projekcie z IO, a kt�rych tw�rc� jest Ferdek. Bartosz Fatyga :D

Zar�wno u lekarza, jak i u laboranta, z tych string�w, kt�re w DBClient tak �adnie skleja�em, wyci�gam
poszczeg�lne wyrazy (a konkretniej osobno daty oraz imiona z nazwiskami pacjent�w/nazwy z opisami bada�
laboratoryjnych). Wydaje si� to g�upie, bo stringi nie s� zbyt wydajne, ci�cie na tablic� to jest typowe
szukanie znak po znaku tego "delimitera". Ale robi� to, bo uwa�am, �e nale�y maksymalnie ograniczy� ilo��
danych pobieranych z bazy danych. Oczywi�cie to jest przesada przy tak ma�ej ilo�ci danych jak obiekt
DateTime i 2 kr�tkie stringi, ale mo�e si� zdarzy�, �e tych danych b�dzie gdzie� wi�cej, albo b�d�
pobierane cz�ciej. Koszt takiego dzia�ania jest proporcjonalnie ma�y do skomplikowania "delimitera" i
ilo�ci element�w, na jakie tnie si� string (dok�adnie 2, czyli po znalezieniu 1-go "delimitera" funkcja
zwraca dalej ca�� reszt� stringa), a nie jest to system czasu rzeczywistego, wi�c nie jest potrzebna
ca�kowicie natychmiastowa reakcja aplikacji. Baza danych odci��ona (o te pitu pitu). Je�li macie inne
zdanie na ten temat ("P*******sz, nie mam czasu na jakie� splity, ci�gn� z bazy wszystko, wal si�!"), to
spoko - to jest tylko klient testowy, pogl�dowy, kt�ry przedstawia niekt�re z mo�liwo�ci.

BTW: IntelliSense si� popieprzy�o i nie pokazywa�o mi nigdzie po stronie kodu �r�d�owego jednej kontrolki
     (Klab_LabTestRemarks), ale projekt si� skompilowa� o_O
     Wrzucam do listy bada� laboratoryjnych jakie lekarz mo�e zleci� "new ComboBoxItem().Content = l"
     (gdzie "l" to string kszta�tu "nazwa + opis"), ale gdy w innej metodzie robi� casta
     "(ComboBoxItem)LabTestsList.SelectedItem", to mi rzuca wyj�tkiem nieprawid�owego casta z dopiskiem,
     �e nie mog� rzutowa� stringa na ComboBoxItem o_O
     W innym miejscu mam odwrotn� sytuacj�: dodaj� do ListBox'a string, a wyrzuca mi potem wyj�tek, gdy
	 chc� SelectedItem rzutowa� do stringa, �e nie mog� rzutowa� do strina klasy ListBoxItem o_O



##########################################################################################################
##########################      Czym to si� r�ni od w�a�ciwych klient�w?      ###########################
##########################################################################################################

Opr�cz tego, �e tu w kartach uj��em 4 osobnych klient�w?

Nie zaimplementowa�em tutaj takiego elementu jak od�wie�anie list pacjent�w/wizyt/bada� laboratoryjnych.
We w�a�ciwych klientach trzeba to b�dzie robi� albo przy powrocie do ekranu zawieraj�cego tak� list� (jak
mniemam b�dzie to ekran g��wny aplikacji), czyli gdy ka�de kolejne ekrany b�d� otwierane w oknie g��wnym;
albo trzeba b�dzie da� przycisk "Od�wie�" przy tych listach, je�li kolejne ekrany b�d� otwierane w nowych
oknach (albo zamiast przycisku da� akcj� na rozwini�cie listy, czyli pewnie GotFocus, bo inaczej si�
chyba nie da).
Tak sobie my�l�, �e gdyby przyj�� t� konwencj� ograniczenia ilo�ci danych pobieranych z bazy, to trzeba
by t� funkcj� zrobi� tak, aby pobiera�a z bazy tylko to, co si� zmieni�o/jest nowe. Czyli musia�aby
istnie� jaka� struktura przechowuj�ca ID, przekazywa�oby si� do funkcji od�wie�aj�cej j� jako argument, a
ona pobiera�aby z bazy wszystko to, co spe�nia okre�lone warunki (np. nieodbyte wizyty) i ID tego rekordu
nie znajduje si� w tej strukturze.

Oczywi�cie brak tutaj jakiegokolwiek logowania si� do systemu.

Jedyna funkcja dodaj�ca co� do bazy lub zmieniaj�ca jakie� dane w bazie, kt�ra sprawdza czy argumenty
DateTime oraz string nie s� null'ami, to DBClient.DBClient.SaveLabTest. Ka�da taka funkcja powinna to
sprawdza�. Ale nie w klasie klienta db. Sprawdzanie poprawno�ci danych powinno oczywi�cie odbywa� si� po
stronie GUI (m�wi�c j�zykiem Javy: po stronie "kontrolera", a "model" to ma by� typowy get/set). No i
powinno to by� nie tylko sprawdzanie, czy null jest podany tam, gdzie w bazie go by� nie mo�e, ale r�wnie�
chodzi o kontrol� merytoryczn� danych, np. gdy laborant Zatwierdza b�d� Anuluje badanie lab., to wynik nie
mo�e by� pusty (ani null), albo gdy laborant klika "Wykonaj" przy badaniu, to (o czym wspomnia�em ni�ej)
trzeba sprawdzi� w bazie, czy Badanie.id_lab == null i je�li nie, to wy�wietli� odpowiednie info i usun��
t� pozycj� z listy bada� lub ca�� j� od�wie�y� (to jest gorsze, bo generuje by� mo�e niepotrzebny ruch).

GUI troch� tandetne, nie chcia�o mi si� a� tak stara�, zale�a�o mi tylko na u�yciu pewnych element�w oraz
na po�wiczeniu XAML'a. Pewnie sobie za jaki� czas wr�c� do tego i mo�e zrobi� ten DateTimePicker, �eby
wiedzie� jak wygl�da tworzenie w�asnych kontrolek. Cha�� tego GUI wida� chyba najbardziej na listach
bada� lab. u laboranta i kier. lab., �e s� 2 r�wne kolumny i uci�te teksty oraz prawie brak odst�p�w
mi�dzy datami i reszt�.
Je�li chodzi o te odst�py po dacie, to wynika z tego, �e te rzeczy zwracam w li�cie, to s� jedne stringi,
sklejone ze sob� daty i nazwiska/nazwy bada� i tak jest wsz�dzie. Nie wiem czemu tak zrobi�em, ale teraz
wiem, �e by� to z�y wyb�r. Wy zwracajcie daty i reszt� jako osobne stringi (mo�e te� by� List, po prostu
na przemian b�dzie data,nazwisko/badanie,data,nazw....).
Je�li chodzi o wewn�trzny layout ListBox'�w, to wybra�em UniformGrid, bo tam tylko liczb� kolumn podajesz
i tyle. Tak by�o po prostu szybciej. A jednak przyda�oby si�, aby druga kolumna (z przyciskiem) mia�a
szeroko�� "auto", a pierwsza kolumna mia�a szer. "*" (czyli w przypadku Waszych klient�w 1 (data) i
3 (przycisk) na "auto", a 2 na "*"). Trzeba wi�c jako ListBox.ItemsPanel.ItemsPanelTemplate ustawi� Grid.
Tylko jak zrobi� Grid z nieokre�lon� liczb� wierszy? Podobno tak:
http://stackoverflow.com/questions/10355013/dynamic-controls-in-wpf-adding-a-variable-number-of-controls
Na razie nie ogarniam tego, b�d� si� tego uczy� w tym samym czasie co custom controls.

[Rejestratorka]
- WPF, w przeciwie�stwie do Windows Forms, nie posiada kontrolki DateTimePicker, jest tylko DatePicker,
  czyli kalendarz do wyboru daty, oczywi�cie w�a�ciwo�� SelectedDate zwraca DateTime, kt�ry �adnie podaj�
  do bazy i widz� tam cudowne godziny 00:00:00
  Przykro mi, ale osoba, kt�ra b�dzie robi� klienta dla rejestratorki, b�dzie musia�a rozwi�za� ten
  problem. S� na to 3 sposoby:
  * samemu napisa� w�asn� kontrolk�, przy czym mo�na przyj�� 2 konwencje:
    # niepopierana przeze mnie konwencja rz�du sze�ciu (czy raczej pi�ciu, bo sekundy nas nie obchodz�)
      spin box'�w (edytowalne pole tekstowe z 2 ma�ymi przyciskami, in- i dekrementuj�cym warto�� pola,
      takie samo jak do ustawiania godziny w Windowsie)
    # maj�ce moje b�ogos�awie�stwo 2 spin box'y dodane pod kalendarzem (Calendar - jest taka kontrolka),
      kt�r� mo�na zobaczy� w tym, co mamy, czyli DatePicker
      (nie wiem czy tak si� da, �eby rozbudowa� DatePicker, pr�dzej trzeba to samemu od podstaw stworzy�)
  * skorzysta� z tej kontrolki, kt�ra wygl�da pono� tak samo jak ta z Windows Forms, ale nie potwierdz�,
    bo nie ma �adnego screena, a i tej z WinForms nie pami�tam:
    http://www.codeproject.com/Articles/132992/A-WPF-DateTimePicker-That-Works-Like-the-One-in-Wi
  * skorzysta� z tej kontrolki, kt�ra wygl�da ca�kiem przyzwoicie, cho� podejrzewam, �e ta poprzednia
    jest �adniejsza:
    http://wpftoolkit.codeplex.com/wikipage?title=DateTimePicker&referringTitle=Home
  Oczywi�cie je�li autor klienta dla rejestratorki skorzysta z gotowej kontrolki, niech to wyra�nie
  zaznaczy i do��czy link do tej kontrolki gdzie� w projekcie/na grupie dyskusyjnej, �eby nie by�o
  zdziwienia, �e si� co� nie kompiluje.

[Lekarz]
- trzeba klikn��, aby dana wizyta zosta�a w bazie oznaczona jako b�d�ca w trakcie realizacji, podczas gdy
  we w�a�ciwych klientach ma to dzia� si� automatycznie po dwukrotnym klikni�ciu na danej wizycie (czyli
  wy�wietleniu podobnie wygl�daj�cych jak tutaj szczeg��w wizyty (np. w nowym okienku))
- na li�cie znajduj� si� wszystkie nieodbyte wizyty, kt�re jeszcze mog� si� odby� (czas p�niejszy ni�
  bie��cy), a powinien jeszcze by� gdzie� obok przycisk "Dzi�", kt�ry wyrzuci z listy wszystko poza
  dzisiejszymi wizytami (w razie braku wizyt na dzi� mo�e sta� si� to, co ja zrobi�em dla przypadku, gdy
  dla danego lekarza w og�le nie ma w bazie �adnych wizyt)
- zrobi�em tak, �e po wybraniu badania laboratoryjnego i ew. wpisaniu opisu, gdy lekarz klika przycisk,
  to jest od razu w bazie zapisywane - mo�na zrobi� tak, �e klikni�cie tego przycisku dalej jest
  wymagane, ale powoduje tylko zapisanie tego zlecenia jako kolejnej pozycji na li�cie tu, po stronie
  aplikacji, a obok tego przycisku by�by drugi przycisk "Zapisz" i to on dopiero wysy�a�by to wszystko do
  bazy (trzeba stworzy� jak�� kolekcje implementuj�c� IEnumerable, najlepiej List<Badanie>, i przekaza�
  to do metody Przychodnia.Badanie.InsertAllOnSubmit), tylko, �e trzeba by sprawdza� kt�re zlecenia ju�
  zosta�y zapisane w bazie, a kt�re nie (je�li b�dzie czas, mo�na zaimplementowa� jeszcze obok ka�dego
  "zatwierdzonego" ju� przyciskiem "Zle� nowe badanie lab." przycisk do usuwania zlecenia albo checkbox,
  a usuni�cie si� zrobi po klikni�ciu "Zapisz", tylko trzeba sprawdza�, czy id_lab dla danego badania
  jest wci�� NULL)

[Laborant]
- nie sprawdzam, a powinienem, po klikni�ciu przez laboranta "Wykonaj" przy danym badaniu, czy id_lab dla
  danego badania wci�� jest NULL i dotyczy to zar�wno przypadku fantom�w (nieod�wie�ona lista, a kto� inny
  ju� klikn�� przy tym "Wykonaj"), jak i konieczno�ci sprawdzania czy na pewno to konkretne badanie dla
  danej wizyty zosta�o wybrane, co wynika ze struktury przechowuj�cej tylko ID wizyty i liczno�� bada�
  zleconych w ich trakcie, zamiast tablic ID bada� (co by�oby pami�ciowo nieefektywne), bo inaczej po
  wykonaniu 1-go badania z danej wizyty, gdy kliknie si� "Wykonaj" przy drugim, to dostanie si� lekarza i
  opis z pierwszej wizyty (wystarczy prze�ledzi� funkcj�, to wida� od razu)



##########################################################################################################
#######################      Informacje dodatkowe do poszczeg�lnych element�w      #######################
##########################################################################################################

DBClient.DBClient.GetPatients, .GetDoctors, .GetLabTestsNames, .GetUndoneLabTests:
- metody te zak�adaj� brak dziur w numeracji p�l Pacjent(id_pac), Lekarz(id_lek), Sl_badan(kod),
  Badanie(id_bad) (tu w zakresie ka�dego id_wiz)

DBClient.DBClient.GetLabTests:
- metoda ta nazywa�a si� pierwotnie GetUndoneLabTests, ale zgeneralizowa�em j�, tak jak poni�sz�, aby nie
  pisa� 2 razy praktycznie tego samego, bo o ile w poni�szym przypadku druga wersja pobiera�aby dodatkowe
  pole, o tyle tutaj r�nica by�aby tylko w warunku (Badanie.Id_lab == null vs. Badanie.Id_lab != null)
- tabela Wizyta posiada autonumeracj� indeksu id_wiz od warto�ci 1, wi�c �aden rekord w tabeli Badanie nie
  b�dzie posiada� w kolumnie id_wiz warto�ci mniejszej od 1 - jest to jedyne zabezpieczenie przed
  wyj�tkiem przy pr�bie dodania elementu

  int iw = -1;                         //inicjalizacja -1
  List<string> lt = null;              //inicjalizacja null
                
  //Wykonanie zapytania.
  foreach (var b in query)
  {
      //Zapisanie wynik�w.
      if (iw != b.idWiz)               //Je�li poprzednio by�o inne id_wiz ni� teraz, ale w pierwszej
      {                                //iteracji jest inaczej.
          iw = b.idWiz;                //Tutaj dopiero pierwsze id_wiz jest zapisywane do tej zmiennej
		                               //pomocniczej, a dotychczas ma ona warto�� -1.
          lt = new List<string>();     //Natomiast tutaj jest tworzona lista dla kolejnego id_wiz,

          labTests.Add(iw, lt);
      }

      //a je�li trafi si� (id_wiz == -1), to tutaj dalej (lt == null), wi�c tutaj rzuci NullReferenceExce.
	  lt.Add(b.dataZle.ToString() + " " + b.nazwa + ", " + b.opis);
  }

DBClient.DBClient.GetLabTestDetails:
- nie wiem czemu w GetPatientsDetails da�em Dictionary, tzn. wiadomo, �e indeksy stringowe s�
  wygodniejsze, ale stringi nie s� optymalne i to pewnie d�ugo trwa przy wi�kszych ilo�ciach element�w,
  dlatego tutaj zastosowa�em ju� List<string>, tylko trzeba zajrze� w opis funkcji w komentarzu, �eby
  zna� kolejno��
- zgeneralizowa�em funkcj� poprzez dodanie kolejnego zwracanego szczeg�u nt. badania, jakim jest wynik,
  �eby nie pisa� specjalnie drugiej funkcji, kt�ra robi to samo, co ta dla laboranta, tylko do tego
  jeszcze pobiera ten ca�y wynik; co w przypadku w�a�ciwych klient�w nie b�dzie mia�o miejsca, bo b�d� to
  osobne aplikacje

DBClient.DBClient.SaveLabTest:
- kolejna funkcja zgeneralizowana z lenistwa