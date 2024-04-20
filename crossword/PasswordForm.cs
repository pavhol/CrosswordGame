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
    public partial class PasswordForm : Form
    {
        public bool IsPasswordCorrect { get; private set; }

        public PasswordForm()
        {
            InitializeComponent();
            IsPasswordCorrect = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "Admin")
            {
                IsPasswordCorrect = true;
                Close();
            }
            else
            {
                MessageBox.Show("Пароль введен неверно!");
                textBox1.Text = "";
            }
        }
    }
}