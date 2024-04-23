using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows;
using System.IO;
using System.Xml;

namespace crossword_v1
{
    public partial class MainWindow : Form
    {
        Crossword _active_crossword;
        public static int blockSizePx = 21;
        public static Word selectedWord;
        public static MainWindow instance;
        Dictionary<string, string> _word_list;
        public MainWindow()
        {
            _word_list = new Dictionary<string, string>();
            InitialiseFromFile();
            _active_crossword = new Crossword();
            instance = this;
            InitializeComponent();
            NewGame();
        }

        public void InitialiseFromFile()
        {
            if (!File.Exists("WordList.xml"))
            {
                FileStream fs = File.Create("WordList.xml");
                fs.Close();
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load("WordList.xml");
                if ( doc.DocumentElement == null )
                {
                    MessageBox.Show("Пустой файл словаря!");
                    return;
                }

                var wordNodes = doc.SelectNodes("//Word");
                foreach (XmlNode wordNode in wordNodes)
                {
                    var wordName = wordNode.Attributes["name"].Value;
                    var wordDescription = wordNode.Attributes["description"].Value;
                    _word_list.Add(wordName.ToLower(), wordDescription);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading word file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
            }
        }

        private void NewGame()
        {
            _active_crossword.GenerateNewCrossword(_word_list);
            RemakeTable();
            RemakeWords();
        }

        public void RemakeWords()
        {
            listBoxhorizontal.Items.Clear();
            listBoxvertical.Items.Clear();

            List<Word> words = _active_crossword.words;
            for (int i = 0; i < words.Count; ++i)
            {
                if (words[i].GetDirection() == Direction.Horizontal)
                {
                    listBoxhorizontal.Items.Add(words[i]);
                    listBoxhorizontal.Size = new Size(listBoxhorizontal.Width, listBoxhorizontal.Items.Count * 20 + 30);

                }
                else
                {
                    listBoxvertical.Items.Add(words[i]);
                }

                words[i].onFinish += word => {
                    ListBox box = (word.GetDirection() == Direction.Horizontal) ? listBoxhorizontal : listBoxvertical;

                    box.Items.Remove(word);
                    SelectNextWord();
                };
            }
        }

        public void SelectWord(Word word)
        {
            listBoxhorizontal.SelectedItem = word;
            listBoxvertical.SelectedItem = word;
        }

        private void SelectNextWord()
        {
            if (listBoxhorizontal.Items.Count > 0)
            {
                listBoxhorizontal.SelectedIndex = 0;
            }
            else if (listBoxvertical.Items.Count > 0)
            {
                listBoxvertical.SelectedIndex = 0;
            }
        }

        public void RemakeTable()
        {
            IBlock[,] blocks = _active_crossword._blocks;

            int rowcount = blocks.GetLength(0);
            int columncount = blocks.GetLength(1);

            UI_TablePanel.Controls.Clear();
            UI_TablePanel.RowStyles.Clear();
            UI_TablePanel.ColumnStyles.Clear();

            UI_TablePanel.RowCount = rowcount;
            UI_TablePanel.ColumnCount = columncount;

            // Add rows
            for (int row = 0; row < rowcount; row++)
            {
                UI_TablePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, blockSizePx));
            }

            // Add columns
            for (int col = 0; col < columncount; col++)
            {
                UI_TablePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, blockSizePx));
            }


            for (int row = 0; row < rowcount; row++)
            {
                for (int col = 0; col < columncount; col++)
                {
                    UI_TablePanel.Controls.Add(blocks[row, col].GetVisualControl(), col, row);
                }
            }
        }

        private void SelectedWordChanged(Word high)
        {
            if (high != null)
            {
                if (selectedWord != null)
                {
                    selectedWord.DeSelect();
                }
                selectedWord = high;
                high.Select();
            }
        }

        private void listBoxhorizontal_SelectedIndexChanged(object sender, EventArgs e)
        {
            Word high = listBoxhorizontal.SelectedItem as Word;
            if (high != null)
            {
                SelectedWordChanged(high);
                listBoxvertical.ClearSelected();
            }
        }

        private void listBoxvertical_SelectedIndexChanged(object sender, EventArgs e)
        {
            Word high = listBoxvertical.SelectedItem as Word;
            if (high != null)
            {
                SelectedWordChanged(high);
                listBoxhorizontal.ClearSelected();
            }

        }

        private void новаяИграToolStripMenuItem3_Click(object sender, EventArgs e)
        {
            NewGame();
        }
    }
}