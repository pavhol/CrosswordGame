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

namespace crossword
{
    public partial class MainWindow : Form
    {
        Crossword activeCrossword = new Crossword();
        public static int blockSizePx = 21;
        public static Word selectedWord;
        public static MainWindow instance;
        CrosswordSize _size;
        private DateTime startTime; 

        public MainWindow(CrosswordSize size = CrosswordSize.Small)
        {
            instance = this;
            InitializeComponent();
            startTime = DateTime.Now;
            _size = size;
            timer1.Start();
            activeCrossword.OpenWordFile();
        }

        public void StartGame(CrosswordSize size)
        {
            NewGame(size);
            startTime = DateTime.Now;
            Show();
        }

        private void NewGame(CrosswordSize size)
        {
            activeCrossword.GenerateNewCrossword(size);
            RemakeTable();
            RemakeWords();
        }

        public void RemakeWords()
        {
            listBoxhorizontal.Items.Clear();
            listBoxvertical.Items.Clear();

            Word[] words = activeCrossword.GetWords();
            for (int i = 0; i < words.Length; ++i)
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
            else if (listBoxhorizontal.Items.Count == 0 && listBoxvertical.Items.Count==0)
            {
                TimeSpan elapsedTime = DateTime.Now - startTime;
                MessageBox.Show("Победа! Прошло времени: " + elapsedTime.ToString(@"hh\:mm\:ss"));
                try
                {
                    string _filename;
                    switch (_size)
                    {
                        case CrosswordSize.Small:
                            _filename = "SmallRecords.txt";
                            break;
                        case CrosswordSize.Normal:
                            _filename = "MediumRecords.txt";
                            break;
                        case CrosswordSize.Large:
                            _filename = "LargeRecords.txt";
                            break;
                        default:
                            _filename = "";
                            break;
                    }
                    using (StreamWriter writer = new StreamWriter(_filename, true))
                    {
                        
                        writer.WriteLine(elapsedTime.ToString(@"hh\:mm\:ss"));
                    }
                }
                catch (Exception)
                {

                    throw;
                }
                Hide();
                instance.Show();
            }
        }

        public void RemakeTable()
        {
            IBlock[,] blocks = activeCrossword.GetBlocks();

            int rowcount = blocks.GetLength(0);
            int columncount = blocks.GetLength(1);

            // Clear everything old first
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
                    UI_TablePanel.Controls.Add(blocks[row,col].GetVisualControl(), col, row);
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

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            toolStripStatusLabel1.Text = "Прошло времени: " + elapsedTime.ToString(@"hh\:mm\:ss");
            
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true; 
        }
    }
}
