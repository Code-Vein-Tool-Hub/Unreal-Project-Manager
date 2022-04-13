using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Unreal_Project_Manager
{
    public partial class FilePicker : Form
    {
        public List<string> Files = new List<string>();
        string basepath;
        public FilePicker(string inpath)
        {
            InitializeComponent();
            string[] files = Directory.GetFiles(inpath, "*.uasset", SearchOption.AllDirectories);
            basepath = inpath;
            
            foreach (string file in files)
            {
                treeView1.Nodes.Add(file.Substring(inpath.Length));
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Files = null;
            Close();
        }

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Checked)
            {
                Files.Add($"{basepath}{e.Node.Text}");
            }
            else
            {
                Files.Remove($"{basepath}{e.Node.Text}");
            }
        }
    }
}
