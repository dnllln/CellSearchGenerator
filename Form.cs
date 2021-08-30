using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Diagnostics;

namespace CellSearchGenerator
{
    public partial class Form : System.Windows.Forms.Form
    {
        public Form()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
        }

        private void GenerateButton_Click(object sender, EventArgs e)
        {
            string data = TextBox.Text;
            Regex rxmonth = new("1?[0-9]{1}/");
            Regex rxyear = new("/20[0-9]{2}");

            try
            {
                int month = Convert.ToInt32(rxmonth.Match(data).Value[..^1]);
                int year = Convert.ToInt32(rxyear.Match(data).Value[1..]);

                if ((month >= 1 && month <= 12) || year >= 1000 && year <= 9999)
                {
                    string[] args = { month.ToString(), year.ToString() };
                    CellSearchGeneratorLogic.Program.GenMain(args);
                    string dest = String.Format("{0:00}_{1}.pdf", month, year);
                    MessageBox.Show(String.Format("'{0}' was generated in the Cell Searches directory.", dest));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Date must be in MM/YYYY format.");
            }
        }

        private void GitHubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("explorer", "https://github.com/dnllln/CellSearchGenerator");
        }
    }
}