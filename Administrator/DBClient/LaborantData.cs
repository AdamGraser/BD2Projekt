using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class LaborantData
    {
        public LaborantData(int id_lab, string nazwisko, string imie, string login, string haslo, DateTime? wygasa, bool kier)
        {
            this.id_lab = id_lab;
            this.nazwisko = nazwisko;
            this.imie = imie;
            this.login = login;
            this.haslo = haslo;
            this.aktywny = DateTime.Now;
            this.wygasa = wygasa;
            this.kier = kier;
        }

        public LaborantData(LaborantData previous)
        {
            this.id_lab    = previous.id_lab   ;
            this.nazwisko =  previous.nazwisko ;
            this.imie      = previous.imie     ;
            this.login     = previous.login    ;
            this.haslo     = previous.haslo    ;
            this.aktywny   = previous.aktywny  ;
            this.wygasa    = previous.wygasa   ;
            this.kier = previous.kier;
        }

        public int id_lab { get; set; }
        public string nazwisko { get; set; }
        public string imie { get; set; }
        public string login { get; set; }
        public string haslo { get; set; }
        public DateTime? wygasa { get; set; }
        public DateTime aktywny { get; set; }
        public bool kier { get; set; }
    }
}
