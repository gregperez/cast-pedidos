using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Servicio_Cast_Pedidos_Test
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Servicio_Cast_Pedidos.Pedidos service = new Servicio_Cast_Pedidos.Pedidos();
            service.OnStartTest();
        }
    }
}
