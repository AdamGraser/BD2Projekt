using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class RejestratorkaData
    {
        public RejestratorkaData(int id_rej, string nazwisko, string imie, string login, string haslo, DateTime? wygasa)
        {
            this.id_rej = id_rej;
            this.nazwisko = nazwisko;
            this.imie = imie;
            this.login = login;
            this.haslo = haslo;
            this.aktywny = DateTime.Now;
            this.wygasa = wygasa;
        }
        public RejestratorkaData(RejestratorkaData previous)
        {
            this.id_rej    = previous.id_rej   ;
            this.nazwisko =  previous.nazwisko ;
            this.imie      = previous.imie     ;
            this.login     = previous.login    ;
            this.haslo     = previous.haslo    ;
            this.aktywny   = previous.aktywny  ;
            this.wygasa    = previous.wygasa   ;
        }


        public int id_rej { get; set; }
        public string nazwisko { get; set; }
        public string imie { get; set; }
        public string login { get; set; }
        public string haslo { get; set; }
        public DateTime? wygasa { get; set; }
        public DateTime aktywny { get; set; }
    }

}
