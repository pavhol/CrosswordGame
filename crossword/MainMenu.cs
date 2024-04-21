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
using System.Globalization;

namespace crossword
{
    public partial class MainMenu : Form
    {
        private MainWindow _crosswordGM;
        private Records _records;
        XmlDocument doc = new XmlDocument();
        private string filename;
        Dictionary<string, string> _word_list;

        public MainMenu()
        {
            InitializeComponent();
            _word_list = new Dictionary<string, string>();
            InitialiseFromFile();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            _crosswordGM = new MainWindow(_word_list);
            _crosswordGM.WindowState = FormWindowState.Maximized;
            _crosswordGM.FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Hide();
            _crosswordGM.StartGame(CrosswordSize.Small);
            Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Hide();
            _crosswordGM.StartGame(CrosswordSize.Normal);
            Show();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Hide();
            _crosswordGM.StartGame(CrosswordSize.Large);
            Show();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            _records = new Records();
            _records.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _crosswordGM.Close();
            Close();
        }
        
        private void button5_Click(object sender, EventArgs e)
        {
            PasswordForm passFM = new PasswordForm();
            passFM.ShowDialog();
            if ( passFM.IsPasswordCorrect )
            {
                EditWordList _edit_word_listW = new EditWordList(_word_list);
                _edit_word_listW.ShowDialog();
                SaveToFile();
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

        public void SaveToFile()
        {
            string currentDir = System.Environment.CurrentDirectory;
            if (File.Exists(Path.Combine(currentDir, "WordList.xml")))
                filename = Path.Combine(currentDir, "WordList.xml");

            try
            {
                doc.DocumentElement.RemoveAll();
                foreach (var item in _word_list)
                {
                    XmlNode word_node = doc.CreateElement("Word");
                    word_node.Attributes.Append(doc.CreateAttribute("name")).Value = item.Key;
                    word_node.Attributes.Append(doc.CreateAttribute("description")).Value = item.Value;
                    doc.DocumentElement.AppendChild(word_node);
                }

                doc.Save(filename);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving word file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            string directory_path = @"Saved";
            if (!Directory.Exists(directory_path))
                Directory.CreateDirectory(directory_path);

            string[] files = Directory.GetFiles(directory_path);
            if (files.Length == 0)
            {
                MessageBox.Show("Сохраненные уровни отсутствуют!");
                return;
            }

            SavedCrosswords saved_crosswords = new SavedCrosswords();
            saved_crosswords.ShowDialog();
            if ( saved_crosswords.file_name.Length > 0)
            {
                IBlock[,] blocks;
                DateTime time;
                using (FileStream fs = File.OpenRead(Path.Combine(directory_path, saved_crosswords.file_name)))
                {
                    using (StreamReader sr = new StreamReader(fs))
                    {
                        string time_string = sr.ReadLine();
                        time = DateTime.ParseExact(time_string, @"hh\:mm\:ss", DateTimeFormatInfo.CurrentInfo);

                        List<string> lines = new List<string>();
                        string line;
                        while ((line = sr.ReadLine()) != null)
                            lines.Add(line);

                        blocks = new IBlock[lines.Count, lines[0].Length];
                        for (int i = 0; i < lines.Count; i++)
                        {
                            for (int j = 0; j < lines[i].Length; j++)
                            {
                                if ( lines[i][j] == '#')
                                    blocks[i, j] = new BlackBlock();
                                else
                                {
                                    if ( char.IsUpper(lines[i][j]) )
                                    {
                                        blocks[i, j] = new CharacterBlock(char.ToUpper(lines[i][j]));
                                        blocks[i, j].SetConfirmed();
                                    }
                                    else
                                        blocks[i, j] = new CharacterBlock(char.ToUpper(lines[i][j]));
                                }
                            }
                        }
                    }
                }

                File.Delete(Path.Combine(directory_path, saved_crosswords.file_name));

                Hide();
                _crosswordGM.StartGame(blocks, time);
                Show();
            }
        }
    }
}
