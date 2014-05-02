using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DBConnectionLayer;

namespace test_client
{
    public partial class Form1 : Form
    {
        DBClient dbClient;
        
        public Form1()
        {
            InitializeComponent();
            dbClient = new DBClient();
        }

        private void etykieta_Click(object sender, EventArgs e)
        {
            etykieta.Text = "Nie klikaj mnie!";
        }

        private void button_Click(object sender, EventArgs e)
        {
            byte idl = 1;
            byte.TryParse(id_lek.Text, out idl);
            

            int idp = 3;
            int.TryParse(id_pac.Text, out idp);

            etykieta.Text = dbClient.RejestrujWizyte(data_rej.Value, idl, idp);
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
