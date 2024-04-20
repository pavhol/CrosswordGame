using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlTypes;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace crossword
{
    public partial class Records : Form
    {
        public Records()
        {
            InitializeComponent();
        }

        private void Records_Load(object sender, EventArgs e)
        {
            dataGridView1.ColumnCount = 3;
            dataGridView1.Columns[0].HeaderText = "Легкий";
            dataGridView1.Columns[1].HeaderText = "Средний";
            dataGridView1.Columns[2].HeaderText = "Сложный";
            dataGridView1.Columns[0].DefaultCellStyle.BackColor = Color.LightGreen;
            dataGridView1.Columns[1].DefaultCellStyle.BackColor = Color.LightBlue;
            dataGridView1.Columns[2].DefaultCellStyle.BackColor = Color.OrangeRed;

            List<DateTime> _smallTime = new List<DateTime>();
            List<DateTime> _mediumTime = new List<DateTime>();
            List<DateTime> _largeTime = new List<DateTime>();
            using (StreamReader reader = new StreamReader("SmallRecords.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string timeString = line;
                    _smallTime.Add(DateTime.ParseExact(timeString, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            _smallTime.Sort();
            using (StreamReader reader = new StreamReader("MediumRecords.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string timeString = line;
                    _mediumTime.Add(DateTime.ParseExact(timeString, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            _mediumTime.Sort();
            using (StreamReader reader = new StreamReader("LargeRecords.txt"))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string timeString = line;
                    _largeTime.Add(DateTime.ParseExact(timeString, "HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture));
                }
            }
            _largeTime.Sort();
            try
            {
                for (int i = 0; i < _smallTime.Count; i++)
                {
                    dataGridView1.RowCount++;
                    dataGridView1.Rows[i].Cells[0].Value = _smallTime[i].ToString("HH:mm:ss");
                }
                for (int i = 0; i < _mediumTime.Count; i++)
                {
                    dataGridView1.RowCount++;
                    dataGridView1.Rows[i].Cells[1].Value = _mediumTime[i].ToString("HH:mm:ss");
                }
                for (int i = 0; i < _largeTime.Count; i++)
                {
                    dataGridView1.RowCount++;
                    dataGridView1.Rows[i].Cells[2].Value = _largeTime[i].ToString("HH:mm:ss");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
