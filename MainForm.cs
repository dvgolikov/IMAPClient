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
            ChangeConnectedState(false);
        }

        private void ConnectButton_ClickAsync(object sender, EventArgs e)
        {
			var credentials = new NetworkCredential(LoginTextBox.Text.Trim(), PasswordTextBox.Text.Trim());
			var server = ServerTextBox.Text.Trim();
			int port;
			if (!string.IsNullOrEmpty(PortTextBox.Text))
				port = int.Parse(PortTextBox.Text);
			else
				port = 0; // default

			mailClient = new MailWorker(credentials, server, port);

            mailClient.NewLogMessage += MailClient_NewLogMessage;
            mailClient.ConnectionState += MailClient_ConnectionState;
            mailClient.UpdateFoldersTree += MailClient_UpdateFoldersTree;
            mailClient.UpdateWebBrowser += MailClient_UpdateWebBrowser;
            mailClient.UpdateForm += MailClient_UpdateForm;
            mailClient.ReconnectAsync();
        }

        private void MailClient_UpdateForm(object sender, Action e)
        {
            this.BeginInvoke(e);
        }

        private void MailClient_UpdateWebBrowser(object sender, string e)
        {
            this.BeginInvoke(new Action(() =>
            {
                webBrowser.DocumentText = e;
            }));

        }

        private void MailClient_UpdateFoldersTree(object sender, TreeNode e)
        {
            this.BeginInvoke(new Action(() =>
                {
                    MailTreeView.Nodes.Clear();
                    MailTreeView.Nodes.Add(e);
                }));
        }

        private void MailClient_ConnectionState(object sender, bool e)
        {
            this.BeginInvoke(new Action(() =>
            {
                ChangeConnectedState(e);
            }));
        }

        private void MailClient_NewLogMessage(object sender, string e)
        {
            this.BeginInvoke(new Action(() =>
            {
                logBox.Text += e + Environment.NewLine;
            }));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }

        private void MailTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelectedTreeNode = e.Node;
            if (SelectedTreeNode.Name == "root") return;
            if (SelectedTreeNode.Tag is IMessageSummary messageInfo)
            {
                if (!(SelectedTreeNode.Parent.Tag is IMailFolder mailFolder)) return;

                mailClient.RenderMailMessage(mailFolder, messageInfo);

                return;
            }

            if (SelectedTreeNode.Tag is IMailFolder folder)
            {

                mailClient.ReadMails(SelectedTreeNode);
            }
            
        }

        private void AddFolderButton_Click(object sender, EventArgs e)
        {
            mailClient.AddFolder(SelectedTreeNode);//.ConfigureAwait(false);
        }

        private void ChangeConnectedState(bool State)
        {
            connectedStatusLabel.Text = (State) ? "Connected" : "Disconnected";
            connectedStatusLabel.BackColor = (State) ? Color.Green : Color.Red;
            ServerTextBox.Enabled = !State;
            PortTextBox.Enabled = !State;
            LoginTextBox.Enabled = !State;
            PasswordTextBox.Enabled = !State;
            ConnectButton.Enabled = !State;
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            mailClient.Detete(SelectedTreeNode);
        }

        private void SetReadedButton_Click(object sender, EventArgs e)
        {
            mailClient.SetMessageAsReaded(SelectedTreeNode);
        }

        private void SetUnReadedButton_Click(object sender, EventArgs e)
        {
            mailClient.SetMessageAsUnRead(SelectedTreeNode);
        }

        private void DeleteMessageButton_Click(object sender, EventArgs e)
        {

        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            Task.Run(new Action(()=> { mailClient.WaitForNewMessagesAsync(); }));//.ConfigureAwait(false);
        }
    }
}
