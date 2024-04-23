using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;

namespace crossword_v2
{
    public partial class NewPassword : Form
    {
        public NewPassword()
        {
            InitializeComponent();
            SendToBack();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ShowInTaskbar = false;
            this.MaximizeBox = false;
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && e.KeyChar != (char)Keys.Back)
                e.Handled = true;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings["Password"] == null)
            {
                config.AppSettings.Settings.Add("Password", "Admin");
                config.Save(ConfigurationSaveMode.Modified);
            }

            if (textBox2.Text.Length < 3)
                MessageBox.Show("Пароль должен состоять хотя бы из 3 символов!");
            else
            {
                if (textBox1.Text.Equals(config.AppSettings.Settings["Password"].Value))
                {
                    config.AppSettings.Settings["Password"].Value = textBox2.Text;
                    config.Save(ConfigurationSaveMode.Modified);
                    Close();
                }
                else
                    MessageBox.Show("Старый пароль введен неверно!");
            }
        }
    }
}
