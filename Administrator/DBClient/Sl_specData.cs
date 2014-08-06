using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBClient
{
    public class Sl_specData
    {
        public Sl_specData(short kod_spec, string nazwa)
        {
            this.kod_spec = kod_spec;
            this.nazwa = nazwa;
        }
        public short kod_spec { get; set; }
        public string nazwa { get; set; }

        public Sl_specData(Sl_specData previous)
        {
            this.kod_spec = previous.kod_spec;
            this.nazwa = previous.nazwa;
        }
        
        public override string ToString()
        {
            return nazwa;
        }
    }
}
