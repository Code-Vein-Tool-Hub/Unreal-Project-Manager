using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Diagnostics;
using System.Reflection;
using YamlDotNet.Serialization;

namespace Unreal_Project_Manager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            if (!File.Exists("UnrealPak.exe"))
                richTextBox1.AppendText("UnrealPak.exe Not Found\n");
            button1.Enabled = false;
            button2.Enabled = false;
            button3.Enabled = false;
            button4.Enabled = false;
            if (File.Exists("UPMSettings.yaml"))
            {
                settings = Settings.Read("UPMSettings.yaml");
                textBox2.Text = settings.BasePath;
            }
            else
            {
                settings = new Settings();
            }
            
        }
        Settings settings;
        ProjectSettings projectSettings;

        private void TS_Open_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
            {
                ofd.IsFolderPicker = true;
                ofd.RestoreDirectory = true;

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    MakeFileList(ofd.FileName);
                    textBox1.Text = ofd.FileName;
                    button1.Enabled = true;
                    button2.Enabled = true;
                    if (File.Exists("UnrealPak.exe"))
                        button3.Enabled = true;
                    button4.Enabled = true;
                    button5.Enabled = true;
                    button6.Enabled = true;
                    button7.Enabled = true;
                }
            }
        }

        private void MakeFileList(string inpath)
        {
            string[] files = Directory.GetFiles(inpath, "*", SearchOption.AllDirectories);
            treeView1.Nodes.Clear();

            foreach (string file in files)
            {
                if (file.Contains("UPMProjectSettings.yaml"))
                {
                    projectSettings = ProjectSettings.Read(file);
                    richTextBox1.AppendText("Loaded Project Settings from Directory\n");
                    continue;
                }
                treeView1.Nodes.Add(file.Replace($"{inpath}\\", ""), file.Replace($"{inpath}\\", ""));
            }

            if (projectSettings == null)
                projectSettings = new ProjectSettings();
            ProtectFiles();
        }

        private void ProtectFiles()
        {
            if (projectSettings == null)
                return;
            foreach (TreeNode node in treeView1.Nodes)
            {
                if (projectSettings.ProtectedFiles.Contains(node.Name))
                {
                    node.Text = node.Text + "*";
                }
            }
            richTextBox1.AppendText($"Protected {projectSettings.ProtectedFiles.Count} files from being updated\n");
        }

        private void GetDirectories(DirectoryInfo[] subDirs, TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                    GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);
            }
        }

        private static void processDirectory(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                processDirectory(directory);
                if (Directory.GetFiles(directory).Length == 0 &&
                    Directory.GetDirectories(directory).Length == 0)
                {
                    Directory.Delete(directory, false);
                }
            }
        }

        private void Remove_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Index < 0)
                return;

            if (File.Exists($"{textBox1.Text}\\{treeView1.SelectedNode.Text}"))
            {
                File.Delete($"{textBox1.Text}\\{treeView1.SelectedNode.Text}");
                treeView1.SelectedNode.Remove();
                processDirectory(textBox1.Text);
            }
        }

        private void Update_Click(object sender, EventArgs e)
        {
            richTextBox1.Clear();
            if (textBox2.Text.Length > 0)
            {
                string[] files = Directory.GetFiles(textBox2.Text, "*", SearchOption.AllDirectories);

                foreach (string file in files)
                {
                    string key = file.Replace($"{textBox2.Text}", "CodeVein");
                    if (treeView1.Nodes.ContainsKey(key))
                    {
                        int index = treeView1.Nodes.IndexOf(treeView1.Nodes[key]);
                        if (treeView1.Nodes[key].Text.Contains("*"))
                        {
                            richTextBox1.AppendText($"Skipped File: {Path.GetFileName(file)}\n");
                        }
                        else
                        {
                            File.Copy(file, file.Replace(textBox2.Text, $"{textBox1.Text}\\CodeVein"), true);
                            richTextBox1.AppendText($"Updated File: {Path.GetFileName(file)}\n");
                        }
                    }
                }
                MakeFileList(textBox1.Text);
            }
            else
            {
                richTextBox1.AppendText("Project Path Missing\n");
            }
            
        }

        private void CleanFolders_Click(object sender, EventArgs e)
        {
            if (textBox1.Text != null || textBox1.Text.Length > 0)
                processDirectory(textBox1.Text);
        }

        private void PakMod_Click(object sender, EventArgs e)
        {
            if (!File.Exists("UnrealPak.exe"))
                return;
            string outfile = "";

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.RestoreDirectory = true;
                sfd.Filter = "Pak File(*.pak)|*.pak";
                sfd.FileName = Path.GetFileName(textBox1.Text);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    outfile = sfd.FileName.Replace(Path.GetExtension(sfd.FileName), "");
                }
                else
                {
                    return;
                }
            }

            MakeBat();
            using (Process process = new Process())
            {
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.FileName = "Create-Pak.bat";
                process.StartInfo.Arguments = $"\"{textBox1.Text}\" \"{outfile}\"";
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();

                richTextBox1.AppendText(process.StandardOutput.ReadToEnd());
                process.WaitForExit();
            }
            File.Delete("Create-Pak.bat");
            File.Delete("filelist.txt");
        }

        private void MakeBat()
        {
            if (File.Exists("Create-Pak.bat"))
                File.Delete("Create-Pak.bat");
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream stream = assembly.GetManifestResourceStream("Unreal_Project_Manager.Create-Pak.bat");
            string bat;

            using (StreamReader sr = new StreamReader(stream))
            {
                bat = sr.ReadToEnd();
            }
            File.WriteAllText("Create-Pak.bat", bat);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            using (CommonOpenFileDialog ofd = new CommonOpenFileDialog())
            {
                ofd.IsFolderPicker = true;
                ofd.RestoreDirectory = true;
                ofd.Title = "Open Export Directory";

                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    settings.BasePath = ofd.FileName;
                    Settings.Save(settings);
                    textBox2.Text = settings.BasePath;
                }
            }
        }

        private bool CheckFiles(byte[] file1, byte[] file2)
        {
            return file1 == file2 ? true : false;
        }
        private bool CheckFiles(string path1, string path2)
        {
            byte[] file1 = File.ReadAllBytes(path1);
            byte[] file2 = File.ReadAllBytes(path2);
            return file1 == file2 ? true : false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;
            string file = treeView1.SelectedNode.Name;
            File.Copy(file, file.Replace(textBox2.Text, $"{textBox1.Text}\\CodeVein"), true);
            richTextBox1.AppendText($"Updated File: {Path.GetFileName(file)}\n");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Index < 0 || treeView1.SelectedNode == null)
                return;

            if (treeView1.SelectedNode.Text.EndsWith("*"))
            {
                treeView1.SelectedNode.Text = treeView1.SelectedNode.Text.Substring(0, treeView1.SelectedNode.Text.Length - 1); 
                projectSettings.ProtectedFiles.Remove(treeView1.SelectedNode.Name);
            }
            else
            {
                treeView1.SelectedNode.Text = treeView1.SelectedNode.Text + "*";
                projectSettings.ProtectedFiles.Add(treeView1.SelectedNode.Name);
            }
            ProjectSettings.Save(projectSettings, textBox1.Text);
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            using (FilePicker filePicker = new FilePicker(textBox2.Text))
            {
                filePicker.StartPosition = FormStartPosition.CenterParent;
                if (filePicker.ShowDialog() == DialogResult.OK)
                {
                    foreach (string file in filePicker.Files)
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(file.Replace(textBox2.Text, $"{textBox1.Text}\\CodeVein"))))
                            Directory.CreateDirectory(Path.GetDirectoryName(file.Replace(textBox2.Text, $"{textBox1.Text}\\CodeVein")));

                        File.Copy(file, file.Replace(textBox2.Text, $"{textBox1.Text}\\CodeVein"), true);
                        richTextBox1.AppendText($"Copied File: {Path.GetFileName(file)}\n");
                        string temp1 = file.Replace(".uasset",".uexp");
                        File.Copy(temp1, temp1.Replace(textBox2.Text, $"{textBox1.Text}\\CodeVein"), true);
                        richTextBox1.AppendText($"Copied File: {Path.GetFileName(temp1)}\n");
                        temp1 = file.Replace(".uasset", ".ubulk");
                        if (File.Exists(temp1))
                        {
                            File.Copy(temp1, temp1.Replace(textBox2.Text, $"{textBox1.Text}\\CodeVein"), true);
                            richTextBox1.AppendText($"Copied File: {Path.GetFileName(temp1)}\n");
                        }
                    }
                    MakeFileList(textBox1.Text);
                }
            }
        }
    }
}
