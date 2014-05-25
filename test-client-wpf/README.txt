##########################################################################################################
#####################################      Wstêp do tej masakry      #####################################
##########################################################################################################

Oczywiœcie jeœli chcecie to odpaliæ, musicie pozbyæ siê ³¹czenia z baz¹ danych w konstruktorze DBClient
i zast¹piæ wszelkie wywo³ania metod klasy DBClient w MainWindow sztywnym podaniem danych.
Wy piszcie tych Waszych klientów tak jak ma byæ, czyli tak jak ja tu pisa³em z t¹ klas¹ DBClient,
korzystaj¹c z klasy Przychodnia i innych. Jeœli chcecie, mo¿ecie potem te¿ w MainWindow (czy jak tam klasê
okna g³ównego nazwiecie) zakomentowaæ wszystkie wywo³ania tych metod wykonuj¹cych zapytania do bazy na
klasie Przychodnia i tych innych klasach Encji, a w ich miejsca te¿ wstawiæ na sztywno jakieœ wartoœci i
co nieco potestowaæ. Testowanie i uruchamianie i to i tak bêdzie moja dzia³ka, bo ja mam bazê (po bo Wy
te¿ macie instalowaæ SQL Server i tworzyæ bazê, siê w to bawiæ).
Aha, skoro ten, kto robi klienta admina (naj³atwiejszego) musi siê pomêczyæ z Crystal Reports for Visual
Studio i porobiæ wydruki jakieœ (chyba jakieœ przyk³ady da³em na grupie dyskusyjnej), to musicie wybraæ
spoœród Was jednego, który zrobi jakiegoœ ma³ego helpa w html, co se bêdzie mo¿na odpaliæ z poziomu
aplikacji (ze 2 funkcje programu opisaæ tam, czy coœ, bez zbêdnego pieprzenia siê).

Najwiêkszy syf narobi³em niew³aœciwym nazewnictwem. Przepraszam. Dopiero podczas tworzenia karty lekarza
kapn¹³em siê, ¿e mog³em dawaæ nazwy wg. schematu, który zastosowa³em w kartach lab. i klab., a nie
chcia³em ju¿ zmieniaæ wszystkiego, co ju¿ mia³em zrobione. Wy na szczêœcie nie bêdziecie mieli takich
problemów, bo robicie zupe³nie osobne aplikacje (czyli najpewniej zupe³nie osobne solucje VS).

Umieœci³em informacjê o osobie zalogowanej na pasku tytu³owym. Oczywiœcie jest to ustawione na sztywno,
nie bawi³em siê w logowanie (jeszcze by tego brakowa³o :P). Mo¿na tak zrobiæ, wtedy trzeba by dodaæ jakiœ
chowalny toolbox z boku, który bêdzie zawiera³ przycisk "Wyloguj", albo zrobiæ 2 karty: jedna zawiera³aby
formularz logowania, a po zalogowaniu tylko przycisk "Wyloguj"; druga z kolei zawiera³aby tê g³ówn¹ treœæ
aplikacji, która oczywiœcie pojawia³aby siê tam dopiero po zalogowaniu.
Ale dobr¹ opcj¹ jest równie¿ realizacja formularza logowania/przycisku "Wyloguj" na toolbarze u góry, a w
obszarze roboczym niech bêdzie tylko ta w³aœciwa treœæ. Choæ nie wiem jak wygl¹da umieszczanie dowolnych
kontrolek w toolbarze.
Tak czy siak, jakkolwiek dziwne by nie by³y te imiona i nazwiska, wiedzcie, ¿e s¹ to dane testowe, których
mia³em u¿yæ w naszym projekcie z IO, a których twórc¹ jest Ferdek. Bartosz Fatyga :D

Zarówno u lekarza, jak i u laboranta, z tych stringów, które w DBClient tak ³adnie skleja³em, wyci¹gam
poszczególne wyrazy (a konkretniej osobno daty oraz imiona z nazwiskami pacjentów/nazwy z opisami badañ
laboratoryjnych). Wydaje siê to g³upie, bo stringi nie s¹ zbyt wydajne, ciêcie na tablicê to jest typowe
szukanie znak po znaku tego "delimitera". Ale robiê to, bo uwa¿am, ¿e nale¿y maksymalnie ograniczyæ iloœæ
danych pobieranych z bazy danych. Oczywiœcie to jest przesada przy tak ma³ej iloœci danych jak obiekt
DateTime i 2 krótkie stringi, ale mo¿e siê zdarzyæ, ¿e tych danych bêdzie gdzieœ wiêcej, albo bêd¹
pobierane czêœciej. Koszt takiego dzia³ania jest proporcjonalnie ma³y do skomplikowania "delimitera" i
iloœci elementów, na jakie tnie siê string (dok³adnie 2, czyli po znalezieniu 1-go "delimitera" funkcja
zwraca dalej ca³¹ resztê stringa), a nie jest to system czasu rzeczywistego, wiêc nie jest potrzebna
ca³kowicie natychmiastowa reakcja aplikacji. Baza danych odci¹¿ona (o te pitu pitu). Jeœli macie inne
zdanie na ten temat ("P*******sz, nie mam czasu na jakieœ splity, ci¹gnê z bazy wszystko, wal siê!"), to
spoko - to jest tylko klient testowy, pogl¹dowy, który przedstawia niektóre z mo¿liwoœci.

BTW: IntelliSense siê popieprzy³o i nie pokazywa³o mi nigdzie po stronie kodu Ÿród³owego jednej kontrolki
     (Klab_LabTestRemarks), ale projekt siê skompilowa³ o_O
     Wrzucam do listy badañ laboratoryjnych jakie lekarz mo¿e zleciæ "new ComboBoxItem().Content = l"
     (gdzie "l" to string kszta³tu "nazwa + opis"), ale gdy w innej metodzie robiê casta
     "(ComboBoxItem)LabTestsList.SelectedItem", to mi rzuca wyj¹tkiem nieprawid³owego casta z dopiskiem,
     ¿e nie mogê rzutowaæ stringa na ComboBoxItem o_O
     W innym miejscu mam odwrotn¹ sytuacjê: dodajê do ListBox'a string, a wyrzuca mi potem wyj¹tek, gdy
	 chcê SelectedItem rzutowaæ do stringa, ¿e nie mogê rzutowaæ do strina klasy ListBoxItem o_O



##########################################################################################################
##########################      Czym to siê ró¿ni od w³aœciwych klientów?      ###########################
##########################################################################################################

Oprócz tego, ¿e tu w kartach uj¹³em 4 osobnych klientów?

Nie zaimplementowa³em tutaj takiego elementu jak odœwie¿anie list pacjentów/wizyt/badañ laboratoryjnych.
We w³aœciwych klientach trzeba to bêdzie robiæ albo przy powrocie do ekranu zawieraj¹cego tak¹ listê (jak
mniemam bêdzie to ekran g³ówny aplikacji), czyli gdy ka¿de kolejne ekrany bêd¹ otwierane w oknie g³ównym;
albo trzeba bêdzie daæ przycisk "Odœwie¿" przy tych listach, jeœli kolejne ekrany bêd¹ otwierane w nowych
oknach (albo zamiast przycisku daæ akcjê na rozwiniêcie listy, czyli pewnie GotFocus, bo inaczej siê
chyba nie da).
Tak sobie myœlê, ¿e gdyby przyj¹æ tê konwencjê ograniczenia iloœci danych pobieranych z bazy, to trzeba
by tê funkcjê zrobiæ tak, aby pobiera³a z bazy tylko to, co siê zmieni³o/jest nowe. Czyli musia³aby
istnieæ jakaœ struktura przechowuj¹ca ID, przekazywa³oby siê do funkcji odœwie¿aj¹cej j¹ jako argument, a
ona pobiera³aby z bazy wszystko to, co spe³nia okreœlone warunki (np. nieodbyte wizyty) i ID tego rekordu
nie znajduje siê w tej strukturze.

Oczywiœcie brak tutaj jakiegokolwiek logowania siê do systemu.

Jedyna funkcja dodaj¹ca coœ do bazy lub zmieniaj¹ca jakieœ dane w bazie, która sprawdza czy argumenty
DateTime oraz string nie s¹ null'ami, to DBClient.DBClient.SaveLabTest. Ka¿da taka funkcja powinna to
sprawdzaæ. Ale nie w klasie klienta db. Sprawdzanie poprawnoœci danych powinno oczywiœcie odbywaæ siê po
stronie GUI (mówi¹c jêzykiem Javy: po stronie "kontrolera", a "model" to ma byæ typowy get/set). No i
powinno to byæ nie tylko sprawdzanie, czy null jest podany tam, gdzie w bazie go byæ nie mo¿e, ale równie¿
chodzi o kontrolê merytoryczn¹ danych, np. gdy laborant Zatwierdza b¹dŸ Anuluje badanie lab., to wynik nie
mo¿e byæ pusty (ani null), albo gdy laborant klika "Wykonaj" przy badaniu, to (o czym wspomnia³em ni¿ej)
trzeba sprawdziæ w bazie, czy Badanie.id_lab == null i jeœli nie, to wyœwietliæ odpowiednie info i usun¹æ
tê pozycjê z listy badañ lub ca³¹ j¹ odœwie¿yæ (to jest gorsze, bo generuje byæ mo¿e niepotrzebny ruch).

GUI trochê tandetne, nie chcia³o mi siê a¿ tak staraæ, zale¿a³o mi tylko na u¿yciu pewnych elementów oraz
na poæwiczeniu XAML'a. Pewnie sobie za jakiœ czas wrócê do tego i mo¿e zrobiê ten DateTimePicker, ¿eby
wiedzieæ jak wygl¹da tworzenie w³asnych kontrolek. Cha³ê tego GUI widaæ chyba najbardziej na listach
badañ lab. u laboranta i kier. lab., ¿e s¹ 2 równe kolumny i uciête teksty oraz prawie brak odstêpów
miêdzy datami i reszt¹.
Jeœli chodzi o te odstêpy po dacie, to wynika z tego, ¿e te rzeczy zwracam w liœcie, to s¹ jedne stringi,
sklejone ze sob¹ daty i nazwiska/nazwy badañ i tak jest wszêdzie. Nie wiem czemu tak zrobi³em, ale teraz
wiem, ¿e by³ to z³y wybór. Wy zwracajcie daty i resztê jako osobne stringi (mo¿e te¿ byæ List, po prostu
na przemian bêdzie data,nazwisko/badanie,data,nazw....).
Jeœli chodzi o wewnêtrzny layout ListBox'ów, to wybra³em UniformGrid, bo tam tylko liczbê kolumn podajesz
i tyle. Tak by³o po prostu szybciej. A jednak przyda³oby siê, aby druga kolumna (z przyciskiem) mia³a
szerokoœæ "auto", a pierwsza kolumna mia³a szer. "*" (czyli w przypadku Waszych klientów 1 (data) i
3 (przycisk) na "auto", a 2 na "*"). Trzeba wiêc jako ListBox.ItemsPanel.ItemsPanelTemplate ustawiæ Grid.
Tylko jak zrobiæ Grid z nieokreœlon¹ liczb¹ wierszy? Podobno tak:
http://stackoverflow.com/questions/10355013/dynamic-controls-in-wpf-adding-a-variable-number-of-controls
Na razie nie ogarniam tego, bêdê siê tego uczy³ w tym samym czasie co custom controls.

[Rejestratorka]
- WPF, w przeciwieñstwie do Windows Forms, nie posiada kontrolki DateTimePicker, jest tylko DatePicker,
  czyli kalendarz do wyboru daty, oczywiœcie w³aœciwoœæ SelectedDate zwraca DateTime, który ³adnie podajê
  do bazy i widzê tam cudowne godziny 00:00:00
  Przykro mi, ale osoba, która bêdzie robiæ klienta dla rejestratorki, bêdzie musia³a rozwi¹zaæ ten
  problem. S¹ na to 3 sposoby:
  * samemu napisaæ w³asn¹ kontrolkê, przy czym mo¿na przyj¹æ 2 konwencje:
    # niepopierana przeze mnie konwencja rzêdu szeœciu (czy raczej piêciu, bo sekundy nas nie obchodz¹)
      spin box'ów (edytowalne pole tekstowe z 2 ma³ymi przyciskami, in- i dekrementuj¹cym wartoœæ pola,
      takie samo jak do ustawiania godziny w Windowsie)
    # maj¹ce moje b³ogos³awieñstwo 2 spin box'y dodane pod kalendarzem (Calendar - jest taka kontrolka),
      któr¹ mo¿na zobaczyæ w tym, co mamy, czyli DatePicker
      (nie wiem czy tak siê da, ¿eby rozbudowaæ DatePicker, prêdzej trzeba to samemu od podstaw stworzyæ)
  * skorzystaæ z tej kontrolki, która wygl¹da ponoæ tak samo jak ta z Windows Forms, ale nie potwierdzê,
    bo nie ma ¿adnego screena, a i tej z WinForms nie pamiêtam:
    http://www.codeproject.com/Articles/132992/A-WPF-DateTimePicker-That-Works-Like-the-One-in-Wi
  * skorzystaæ z tej kontrolki, która wygl¹da ca³kiem przyzwoicie, choæ podejrzewam, ¿e ta poprzednia
    jest ³adniejsza:
    http://wpftoolkit.codeplex.com/wikipage?title=DateTimePicker&referringTitle=Home
  Oczywiœcie jeœli autor klienta dla rejestratorki skorzysta z gotowej kontrolki, niech to wyraŸnie
  zaznaczy i do³¹czy link do tej kontrolki gdzieœ w projekcie/na grupie dyskusyjnej, ¿eby nie by³o
  zdziwienia, ¿e siê coœ nie kompiluje.

[Lekarz]
- trzeba klikn¹æ, aby dana wizyta zosta³a w bazie oznaczona jako bêd¹ca w trakcie realizacji, podczas gdy
  we w³aœciwych klientach ma to dziaæ siê automatycznie po dwukrotnym klikniêciu na danej wizycie (czyli
  wyœwietleniu podobnie wygl¹daj¹cych jak tutaj szczegó³ów wizyty (np. w nowym okienku))
- na liœcie znajduj¹ siê wszystkie nieodbyte wizyty, które jeszcze mog¹ siê odbyæ (czas póŸniejszy ni¿
  bie¿¹cy), a powinien jeszcze byæ gdzieœ obok przycisk "Dziœ", który wyrzuci z listy wszystko poza
  dzisiejszymi wizytami (w razie braku wizyt na dziœ mo¿e staæ siê to, co ja zrobi³em dla przypadku, gdy
  dla danego lekarza w ogóle nie ma w bazie ¿adnych wizyt)
- zrobi³em tak, ¿e po wybraniu badania laboratoryjnego i ew. wpisaniu opisu, gdy lekarz klika przycisk,
  to jest od razu w bazie zapisywane - mo¿na zrobiæ tak, ¿e klikniêcie tego przycisku dalej jest
  wymagane, ale powoduje tylko zapisanie tego zlecenia jako kolejnej pozycji na liœcie tu, po stronie
  aplikacji, a obok tego przycisku by³by drugi przycisk "Zapisz" i to on dopiero wysy³a³by to wszystko do
  bazy (trzeba stworzyæ jak¹œ kolekcje implementuj¹c¹ IEnumerable, najlepiej List<Badanie>, i przekazaæ
  to do metody Przychodnia.Badanie.InsertAllOnSubmit), tylko, ¿e trzeba by sprawdzaæ które zlecenia ju¿
  zosta³y zapisane w bazie, a które nie (jeœli bêdzie czas, mo¿na zaimplementowaæ jeszcze obok ka¿dego
  "zatwierdzonego" ju¿ przyciskiem "Zleæ nowe badanie lab." przycisk do usuwania zlecenia albo checkbox,
  a usuniêcie siê zrobi po klikniêciu "Zapisz", tylko trzeba sprawdzaæ, czy id_lab dla danego badania
  jest wci¹¿ NULL)

[Laborant]
- nie sprawdzam, a powinienem, po klikniêciu przez laboranta "Wykonaj" przy danym badaniu, czy id_lab dla
  danego badania wci¹¿ jest NULL i dotyczy to zarówno przypadku fantomów (nieodœwie¿ona lista, a ktoœ inny
  ju¿ klikn¹³ przy tym "Wykonaj"), jak i koniecznoœci sprawdzania czy na pewno to konkretne badanie dla
  danej wizyty zosta³o wybrane, co wynika ze struktury przechowuj¹cej tylko ID wizyty i licznoœæ badañ
  zleconych w ich trakcie, zamiast tablic ID badañ (co by³oby pamiêciowo nieefektywne), bo inaczej po
  wykonaniu 1-go badania z danej wizyty, gdy kliknie siê "Wykonaj" przy drugim, to dostanie siê lekarza i
  opis z pierwszej wizyty (wystarczy przeœledziæ funkcjê, to widaæ od razu)



##########################################################################################################
#######################      Informacje dodatkowe do poszczególnych elementów      #######################
##########################################################################################################

DBClient.DBClient.GetPatients, .GetDoctors, .GetLabTestsNames, .GetUndoneLabTests:
- metody te zak³adaj¹ brak dziur w numeracji pól Pacjent(id_pac), Lekarz(id_lek), Sl_badan(kod),
  Badanie(id_bad) (tu w zakresie ka¿dego id_wiz)

DBClient.DBClient.GetLabTests:
- metoda ta nazywa³a siê pierwotnie GetUndoneLabTests, ale zgeneralizowa³em j¹, tak jak poni¿sz¹, aby nie
  pisaæ 2 razy praktycznie tego samego, bo o ile w poni¿szym przypadku druga wersja pobiera³aby dodatkowe
  pole, o tyle tutaj ró¿nica by³aby tylko w warunku (Badanie.Id_lab == null vs. Badanie.Id_lab != null)
- tabela Wizyta posiada autonumeracjê indeksu id_wiz od wartoœci 1, wiêc ¿aden rekord w tabeli Badanie nie
  bêdzie posiada³ w kolumnie id_wiz wartoœci mniejszej od 1 - jest to jedyne zabezpieczenie przed
  wyj¹tkiem przy próbie dodania elementu

  int iw = -1;                         //inicjalizacja -1
  List<string> lt = null;              //inicjalizacja null
                
  //Wykonanie zapytania.
  foreach (var b in query)
  {
      //Zapisanie wyników.
      if (iw != b.idWiz)               //Jeœli poprzednio by³o inne id_wiz ni¿ teraz, ale w pierwszej
      {                                //iteracji jest inaczej.
          iw = b.idWiz;                //Tutaj dopiero pierwsze id_wiz jest zapisywane do tej zmiennej
		                               //pomocniczej, a dotychczas ma ona wartoœæ -1.
          lt = new List<string>();     //Natomiast tutaj jest tworzona lista dla kolejnego id_wiz,

          labTests.Add(iw, lt);
      }

      //a jeœli trafi siê (id_wiz == -1), to tutaj dalej (lt == null), wiêc tutaj rzuci NullReferenceExce.
	  lt.Add(b.dataZle.ToString() + " " + b.nazwa + ", " + b.opis);
  }

DBClient.DBClient.GetLabTestDetails:
- nie wiem czemu w GetPatientsDetails da³em Dictionary, tzn. wiadomo, ¿e indeksy stringowe s¹
  wygodniejsze, ale stringi nie s¹ optymalne i to pewnie d³ugo trwa przy wiêkszych iloœciach elementów,
  dlatego tutaj zastosowa³em ju¿ List<string>, tylko trzeba zajrzeæ w opis funkcji w komentarzu, ¿eby
  znaæ kolejnoœæ
- zgeneralizowa³em funkcjê poprzez dodanie kolejnego zwracanego szczegó³u nt. badania, jakim jest wynik,
  ¿eby nie pisaæ specjalnie drugiej funkcji, która robi to samo, co ta dla laboranta, tylko do tego
  jeszcze pobiera ten ca³y wynik; co w przypadku w³aœciwych klientów nie bêdzie mia³o miejsca, bo bêd¹ to
  osobne aplikacje

DBClient.DBClient.SaveLabTest:
- kolejna funkcja zgeneralizowana z lenistwa