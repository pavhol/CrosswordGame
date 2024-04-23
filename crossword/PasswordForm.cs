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
    public partial class PasswordForm : Form
    {
        public bool IsPasswordCorrect { get; private set; }

        public PasswordForm()
        {
            InitializeComponent();
            SendToBack();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ShowInTaskbar = false;
            IsPasswordCorrect = false;
            this.MaximizeBox = false;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (config.AppSettings.Settings["Password"] == null)
            {
                config.AppSettings.Settings.Add("Password", "Admin");
                config.Save(ConfigurationSaveMode.Modified);
            }

            ConfigurationManager.RefreshSection("appSettings");
            if (textBox1.Text == config.AppSettings.Settings["Password"].Value)
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

        private void label2_Click(object sender, EventArgs e)
        {
            NewPassword new_password = new NewPassword();
            new_password.ShowDialog();
        }
    }
}