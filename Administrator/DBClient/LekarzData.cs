using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class LekarzData
    {
        public LekarzData(int id_lek, string nazwisko, string imie, string login, string haslo, DateTime aktywny, DateTime? wygasa, short kod_spec)
        {
            this.id_lek = id_lek;
            this.nazwisko = nazwisko;
            this.imie = imie;
            this.login = login;
            this.haslo = haslo;
            this.aktywny = aktywny;
            this.wygasa = wygasa;
            this.kod_spec = kod_spec;
        }

        public LekarzData(LekarzData previous)
        {
            this.id_lek    = previous.id_lek   ;
            this.nazwisko =  previous.nazwisko ;
            this.imie      = previous.imie     ;
            this.login     = previous.login    ;
            this.haslo     = previous.haslo    ;
            this.aktywny   = previous.aktywny  ;
            this.wygasa    = previous.wygasa   ;
            this.kod_spec = kod_spec;
        }

        public int id_lek { get; set; }
        public string nazwisko { get; set; }
        public string imie { get; set; }
        public string login { get; set; }
        public string haslo { get; set; }
        public DateTime? wygasa { get; set; }
        public DateTime aktywny { get; set; }
        public short kod_spec { get; set; }
    }
}
