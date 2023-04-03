using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TLOU_PSARC_Tool.Core;

namespace TLOU_PSARC_Tool.Forms
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }


        void EnableOrDisableControls(bool Enabled)
        {
            button1.Enabled = Enabled;
            button2.Enabled = Enabled;
            checkBox1.Enabled = Enabled;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                EnableOrDisableControls(false);
                Export();
                EnableOrDisableControls(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Export()
        {
            OpenFileDialog fileDialog = new OpenFileDialog()
            {
                Title = "Select PSARC File:",
                Filter = "PSARC file (*.PSARC)|*.psarc"
            };

            SaveFileDialog fileMapDialog = new SaveFileDialog()
            {
                Title = "Select where you want to extract files:",
                FileName = "FilesMap.txt",
                Filter = "FilesMap.txt|FilesMap.txt"
            };

            if (fileDialog.ShowDialog() == DialogResult.OK && fileMapDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = "";
                if (File.Exists(fileMapDialog.FileName))
                {
                    File.Delete(fileMapDialog.FileName);
                }

                Directory.SetCurrentDirectory(Path.GetDirectoryName(fileMapDialog.FileName));

                var psarc = Psarc.Load(fileDialog.FileName);
                int i = 0;
                int EntriesCount = psarc.Entries.Count;
                foreach (var entry in psarc.Entries)
                {
                    Print($"[{++i} - {EntriesCount}] Exporting File '{entry.Key}' ");

                    if (!string.IsNullOrEmpty(Path.GetDirectoryName(entry.Key)))
                        Directory.CreateDirectory(Path.GetDirectoryName(entry.Key));

                    File.WriteAllBytes(entry.Key, psarc.GetFile(entry.Key));
                    File.AppendAllText("FilesMap.txt", entry.Key + Environment.NewLine);
                    Print("Done\r\n");

                    Application.DoEvents();
                }
                psarc.Dispose();
                MessageBox.Show("Done!");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (MessageBox.Show("Please remember to take a backup of your PSARC file\n Leave the files you want to import only in FilesMap.txt \n\n press Ok to countinue.", "Warning!", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) == DialogResult.OK)
                {
                    EnableOrDisableControls(false);
                    Import();
                    EnableOrDisableControls(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Import()
        {
            OpenFileDialog fileDialog = new OpenFileDialog
            {
                Title = "Select PSARC File:",
                Filter = "PSARC file (*.PSARC)|*.psarc"
            };

            OpenFileDialog fileMapDialog = new OpenFileDialog()
            {
                Title = "Select FilesMap.txt file:",
                FileName = "FilesMap.txt",
                Filter = "FilesMap.txt|FilesMap.txt"
            };

            if (fileDialog.ShowDialog() == DialogResult.OK && fileMapDialog.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = "";
                Directory.SetCurrentDirectory(Path.GetDirectoryName(fileMapDialog.FileName));
                var psarc = Psarc.Load(fileDialog.FileName);

                string[] files = File.ReadAllLines("FilesMap.txt").Where(x => !string.IsNullOrEmpty(x)).ToArray();

                //check files
                foreach (string entry in files)
                {
                    if (!File.Exists(entry))
                    {
                        throw new Exception($"Can't find this file \"{Path.GetFullPath(entry)}\"");
                    }

                    if (!psarc.Entries.ContainsKey(entry))
                    {
                        throw new Exception($"Can't find this file \"{Path.GetFullPath(entry)}\" entry in PSARC file");
                    }
                }

                int i = 0;
                foreach (string entry in files)
                {
                    Print($"[{++i} - {files.Length}] Importng File '{entry}' ");
                    psarc.ImportFile(entry, File.ReadAllBytes(entry));
                    Print("Done\r\n");
                }
                psarc.Save();
                psarc.Dispose();
                MessageBox.Show("Done!");
            }
        }


        private void Print(string text)
        {
            if (checkBox1.Checked)
                textBox1.AppendText(text);
        }

    }
}
