using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class Sl_badData
    {
        public Sl_badData(short kod, string nazwa, string opis, bool lab)
        {
            this.kod = kod;
            this.nazwa = nazwa;
            this.opis = opis;
            this.lab = lab;
        }

        public Sl_badData(Sl_badData previous)
        {
            this.kod = previous.kod;
            this.nazwa = previous.nazwa;
            this.opis = previous.opis;
            this.lab = previous.lab;
        }

        public short kod { get; set; }
        public string nazwa { get; set; }
        public string opis { get; set; }
        public bool lab { get; set; }

    }
}
