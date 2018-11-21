using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SetupHelper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetSolutionPath(Directory.GetCurrentDirectory());
            SetPluginVersion();
        }


        private void SetPluginVersion()
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            txtVersion.Text = version;
        }

        private void SetSolutionPath(String path)
        {
            bool found = false;
            while (path != null && !found)
            {
                String[] files = Directory.GetFiles(path, "*.sln");
                if (files.Length > 0)
                {
                    found = true;
                    txtSolutionPath.Text = path;
                }
                else
                {
                    path = Path.GetDirectoryName(path);
                }
            }
            if (!found)
            {
                MessageBox.Show("Solution directory must contains file *.sln file");
            }

        }

        private void btnUpdateVersion_Click(object sender, EventArgs e)
        {
            String[] filePaths = Directory.GetFiles(txtSolutionPath.Text, "AssemblyInfo.cs", SearchOption.AllDirectories);
            PrintLog("Found " + filePaths.Length + " files");
            foreach (String filePath in filePaths)
            {
                if (filePath.Contains("alm-octane-csharp-rest-sdk"))
                {
                    PrintLog("Skipping "  + filePath);
                    //don't update sdk as it submodule
                    continue;
                }

                String content = File.ReadAllText(filePath);
                String content2 = content;
                content2 = Regex.Replace(content2, "\n\\[assembly: AssemblyVersion(.*?)\\]", "\n[assembly: AssemblyVersion(\"" + txtVersion.Text + "\")]");
                content2 = Regex.Replace(content2, "\n\\[assembly: AssemblyFileVersion(.*?)\\]", "\n[assembly: AssemblyFileVersion(\"" + txtVersion.Text + "\")]");
                File.WriteAllText(filePath, content2);
                PrintLog("Updated " + filePath);
            }
        }

        private void PrintLog(String text)
        {
            richTextBox1.AppendText(text);
            richTextBox1.AppendText("\n");
        }

        private void btnSelectPath_Click(object sender, EventArgs e)
        {
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.SelectedPath = Directory.GetCurrentDirectory();
                fbd.ShowNewFolderButton = false;
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    SetSolutionPath(fbd.SelectedPath);
                }
            }
        }
    }
}
