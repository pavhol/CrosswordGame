using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace crossword
{
    public partial class SavedCrosswords : Form
    {
        public string file_name { get; private set; }
        public SavedCrosswords()
        {
            InitializeComponent();
            file_name = "";

            string directory_path = @"Saved";
            string[] files = Directory.GetFiles(directory_path);
            foreach (string filePath in files)
            {
                string fileName = Path.GetFileName(filePath);
                listBox1.Items.Add(fileName);
            }

            SendToBack();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ShowInTaskbar = false;
            this.MaximizeBox = false;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem != null)
            {
                file_name = listBox1.SelectedItem.ToString();
                Close();
            }
        }
    }
}
