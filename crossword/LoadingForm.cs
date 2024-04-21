using System;
using System.Windows.Forms;

namespace crossword
{
    partial class LoadingForm : Form
    {
        public LoadingForm()
        {
            InitializeComponent();
            SendToBack();
            StartPosition = FormStartPosition.CenterScreen;
            //FormBorderStyle = FormBorderStyle.FixedDialog;
            ShowInTaskbar = false;
            Show();
        }
    }
}
