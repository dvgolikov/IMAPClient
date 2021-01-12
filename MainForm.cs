using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailClient.MailWrapper;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace MailClient
{
    public partial class MainForm : Form
    {
        private TreeNode SelectedTreeNode;
		private MailWorker mailClient;
		public MainForm()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
			var credentials = new NetworkCredential(LoginTextBox.Text.Trim(), PasswordTextBox.Text.Trim());
			var server = ServerTextBox.Text.Trim();
			int port;
			if (!string.IsNullOrEmpty(PortTextBox.Text))
				port = int.Parse(PortTextBox.Text);
			else
				port = 0; // default

			mailClient = new MailWorker(credentials, server, port);
            MailTreeView.Nodes.Add(mailClient.RootNode);

		}

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MailTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedTreeNode = e.Node;
            mailClient.ReadMails(SelectedTreeNode).ConfigureAwait(false);
        }

        private void AddFolderButton_Click(object sender, EventArgs e)
        {
            mailClient.AddFolder(SelectedTreeNode).ConfigureAwait(false);
        }
    }
}
