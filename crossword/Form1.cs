using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.IO;

namespace crossword
{
    public partial class Form1 : Form
    {
        private MainWindow _crosswordGM;
        private Records _records;
        XmlDocument doc = new XmlDocument();
        private string filename;
        Dictionary<string, string> _word_list;

        public Form1()
        {
            InitializeComponent();
            _word_list = new Dictionary<string, string>();
            InitialiseFromFile();
            _crosswordGM = new MainWindow(_word_list);
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

        private void button6_Click(object sender, EventArgs e)
        {
            _records = new Records();
            _records.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _crosswordGM.Close();
            _records.Close();
            Close();
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            PasswordForm passFM = new PasswordForm();
            passFM.ShowDialog();
            if ( passFM.IsPasswordCorrect )
            {
                // окно админа
            }
        }

        public void InitialiseFromFile()
        {
            string currentDir = System.Environment.CurrentDirectory;
            if (File.Exists(Path.Combine(currentDir, "WordList.xml")))
                filename = Path.Combine(currentDir, "WordList.xml");

            try
            {
                doc.Load(filename);
                var wordNodes = doc.SelectNodes("//Word");
                foreach (XmlNode wordNode in wordNodes)
                {
                    var wordName = wordNode.Attributes["name"].Value;
                    var wordDescription = wordNode.Attributes["description"].Value;
                    _word_list.Add(wordName, wordDescription);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading word file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }
    }
}
