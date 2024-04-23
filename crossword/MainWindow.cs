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
using System.Configuration;

namespace crossword_v2
{
    public partial class MainWindow : Form
    {
        Crossword _active_crossword;
        public static int blockSizePx = 21;
        public static Word selectedWord;
        public static MainWindow instance;
        CrosswordSize _size;
        Dictionary<string, string> _word_list;
        private DateTime startTime;
        public static int _balance;
        public static bool _hint = false;
        public MainWindow(Dictionary<string, string> word_list)
        {
            _active_crossword = new Crossword();
            instance = this;
            InitializeComponent();
            _word_list = word_list;
            startTime = DateTime.Now;
            timer1.Start();
        }
        public void StartGame(CrosswordSize size)
        {
            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            LoadingForm lf = new LoadingForm();
            _size = size;
            NewGame(size);
            startTime = DateTime.Now;
            lf.Close();
            if (_config.AppSettings.Settings["_balance"] != null)
            {
                _balance = Convert.ToInt32(_config.AppSettings.Settings["_balance"].Value);
            }
            else
            {
                _balance = 0;
            }
            toolStripStatusLabel2.Text = _balance.ToString();
            ShowDialog();
        }

        public void StartGame(IBlock[,] blocks, DateTime time, List<string> words, List<string> directions, List<Point> startpoints)
        {
            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            LoadingForm lf = new LoadingForm();

            bool is_used = true;
            foreach (var item in words)
            {
                if (!_word_list.Keys.Contains(item.ToLower()))
                {
                    is_used = false;
                    break;
                }
            }

            if (is_used == false)
            {
                MessageBox.Show("Не удалось открыть кроссворд!");
                return;
            }

            ContinueGame(blocks, words, directions, startpoints);
            startTime = DateTime.Now;
            startTime = startTime.Add(-(time.TimeOfDay));
            lf.Close();
            if (_config.AppSettings.Settings["_balance"] != null)
            {
                _balance = Convert.ToInt32(_config.AppSettings.Settings["_balance"].Value);
            }
            else
            {
                _balance = 0;
            }
            toolStripStatusLabel2.Text = _balance.ToString();
            ShowDialog();
        }

        private void NewGame(CrosswordSize size)
        {
            _active_crossword.GenerateNewCrossword(_word_list, size);
            RemakeTable();
            RemakeWords();
        }

        private void ContinueGame(IBlock[,] blocks, List<string> words, List<string> directions, List<Point> startpoints)
        {
            _active_crossword.ContinueCrossword(_word_list, blocks, words, directions, startpoints);
            RemakeTable();
            RemakeWords();
            _active_crossword.UpdateSolved(words, startpoints);
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
            else if (listBoxhorizontal.Items.Count == 0 && listBoxvertical.Items.Count == 0)
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
                            _balance += 5;
                            break;
                        case CrosswordSize.Normal:
                            _filename = "MediumRecords.txt";
                            _balance += 10;
                            break;
                        case CrosswordSize.Large:
                            _filename = "LargeRecords.txt";
                            _balance += 20;
                            break;
                        default:
                            _filename = "";
                            break;
                    }
                    toolStripStatusLabel2.Text = _balance.ToString();
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
            }
        }
        public void WriteConfig()
        {
            Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            if (_config.AppSettings.Settings["_balance"] == null)
            {
                _config.AppSettings.Settings.Add("_balance", _balance.ToString());
            }
            _config.AppSettings.Settings["_balance"].Value = _balance.ToString();
            _config.Save(ConfigurationSaveMode.Modified);
            ConfigurationManager.RefreshSection("appSettings");

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

        private void timer1_Tick(object sender, EventArgs e)
        {
            TimeSpan elapsedTime = DateTime.Now - startTime;
            toolStripStatusLabel1.Text = "Прошло времени: " + elapsedTime.ToString(@"hh\:mm\:ss");
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            WriteConfig();
            Hide();

            e.Cancel = true;
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string directory_path = @"Saved";
            if (!Directory.Exists(directory_path))
                Directory.CreateDirectory(directory_path);

            int num = 1;
            while (true)
            {
                string file_name = "Crossword" + Convert.ToString(num) + ".crs";
                if (!File.Exists(Path.Combine(directory_path, file_name)))
                {
                    FileStream fs = File.Create(Path.Combine(directory_path, file_name));
                    fs.Close();
                    MessageBox.Show("Кроссворд сохранен под именем " + file_name + "!");
                    break;
                }

                ++num;
            }

            IBlock[,] active_blocks = _active_crossword._blocks;
            string fileName = "Crossword" + Convert.ToString(num) + ".crs";

            using (FileStream fs = File.OpenWrite(Path.Combine(directory_path, fileName)))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    sw.WriteLine(elapsedTime.ToString(@"hh\:mm\:ss"));
                    for (int i = 0; i < active_blocks.GetLength(0); i++)
                    {
                        for (int j = 0; j < active_blocks.GetLength(1); j++)
                        {
                            IBlock block = active_blocks[i, j];
                            if (block is BlackBlock)
                            {
                                sw.Write("#");
                            }
                            else if (block is CharacterBlock)
                            {
                                if (block.IsCorrectAnswer())
                                    sw.Write(block.GetAnswer().ToString().ToUpper());
                                else
                                    sw.Write(block.GetAnswer().ToString().ToLower());
                            }
                        }
                        sw.WriteLine();
                    }

                }
            }

            Hide();
        }

        private void сдатьсяToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Hide();
        }

        private void подсказкаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_hint)
            {
                _balance += 5;
                toolStripStatusLabel2.Text = _balance.ToString();
                _hint = false;
                return;
            }

            if (_balance < 5)
                MessageBox.Show("Не хватает монет для подсказки (минимум 5)");
            else
            {
                _balance -= 5;
                toolStripStatusLabel2.Text = _balance.ToString();
                _hint = true;
            }
        }
    }
}