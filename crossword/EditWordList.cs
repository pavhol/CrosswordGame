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
    public partial class EditWordList : Form
    {
        private Dictionary<string, string> _word_list;

        public EditWordList(Dictionary<string, string> word_list)
        {
            InitializeComponent();
            SendToBack();
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            ShowInTaskbar = false;
            this.MaximizeBox = false;
            _word_list = word_list;
            UpdateDataGridView();
        }

        private void UpdateDataGridView()
        {
            dataGridView1.Rows.Clear();
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("1", "Слово");
            dataGridView1.Columns.Add("2", "Определение");
            foreach (var item in _word_list)
            {
                string[] row_data = { item.Key, item.Value };
                dataGridView1.Rows.Add(row_data);
            }

            dataGridView1.Columns[0].Width = 100;
            dataGridView1.Columns[1].Width = 550;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ( textBox1.Text.Length != 0 && textBox2.Text.Length != 0)
            {
                _word_list[textBox1.Text] = textBox2.Text;
                UpdateDataGridView();
            }
            else
                MessageBox.Show("Поля не могут быть пустыми!");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox1.Text.Length != 0 )
            {
                if (_word_list.Keys.Contains(textBox1.Text))
                {
                    _word_list.Remove(textBox1.Text);
                    UpdateDataGridView();
                }
                else
                    MessageBox.Show("Слова нет в словаре!");
            }
            else
                MessageBox.Show("Поля не могут быть пустыми!");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar))
                e.Handled = true;
        }
    }
}
