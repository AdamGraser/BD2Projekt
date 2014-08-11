using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class BadanieInfo
    {
        public BadanieInfo(bool lab, short kod, DateTime data_zle, string opis, string wynik)
        {
            this.lab = lab;
            this.kod = kod;
            this.data_zle = data_zle;
            this.opis = opis;
            this.wynik = wynik;
        }
        
        public bool lab { get; set; }
        public short kod { get; set; }
        public DateTime data_zle { get; set; }
        public string opis { get; set; }
        public string wynik { get; set; }
    }
}
