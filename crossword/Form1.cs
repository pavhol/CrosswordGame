using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace crossword
{
    public partial class Form1 : Form
    {
        private MainWindow _crosswordGM;

        public Form1()
        {
            InitializeComponent();
            _crosswordGM = new MainWindow();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _crosswordGM.StartGame(CrosswordSize.Small);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _crosswordGM.StartGame(CrosswordSize.Normal);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            _crosswordGM.StartGame(CrosswordSize.Large);
        }
    }
}
