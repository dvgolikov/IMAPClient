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
        private readonly Font RegularFont = new Font("Arial", 8f, FontStyle.Regular);
        private readonly Font BoldFont = new Font("Arial", 8f, FontStyle.Bold);
        private Dictionary<IMailPresentation, TreeNode> TreeNodes;

        private TreeNode SelectedTreeNode;
		private MailWorker mailClient;
		public MainForm()
        {
            InitializeComponent();
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

            ConnectButton.Enabled = false;

            mailClient.NewLogMessage += MailClient_NewLogMessage;
            mailClient.ConnectionState += MailClient_ConnectionState;
            mailClient.IdleState += MailClient_IdleState;
            mailClient.Update += MailClient_Update;

            mailClient.ReconnectAsync();
        }

        private void MailClient_Update(object sender, EventArgs e)
        {
            this.BeginInvoke(new Action(() =>
            {
                if(MailTreeView.Nodes.Count==0)
                {
                    var rootNode = CreateNode(mailClient.rootNode);
                    MailTreeView.Nodes.Add(rootNode);
                }
            }));
        }
        private TreeNode CreateNode(MailWrapper.MailFolder rootNode)
        {
            TreeNode result = new TreeNode() { Text = rootNode.Text, Tag = rootNode };
            result.NodeFont = (rootNode.Accent) ? BoldFont : RegularFont;
            rootNode.OnChange += Node_OnChange;
            foreach (var folder in rootNode.Nodes)
            {
                result.Nodes.Add(CreateNode(folder));
            }
            return result;
        }



        private void Node_OnChange(object sender, EventArgs e)
        {
            if(sender is IMailPresentation node)
            {
                if (TreeNodes.TryGetValue(node, out var treeNode))
                {
                    treeNode.Text = node.Text;
                    treeNode.NodeFont = (node.Accent) ? BoldFont : RegularFont;
                }
            }
        }

        private void MailClient_ConnectionState(object sender, bool e)
        {
            this.BeginInvoke(new Action(() =>
            {
                connectedStatusLabel.Text = (e) ? "Connected" : "Disconnected";
                connectedStatusLabel.BackColor = (e) ? Color.Green : Color.Red;
            }));
        }

        private void MailClient_IdleState(object sender, bool e)
        {
            this.BeginInvoke(new Action(() =>
            {
                IdleLabel.Text = (e) ? "Idle: Off" : "Idle: On";
                IdleLabel.BackColor = (e) ? Color.Green : Color.Red;
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
            if (SelectedTreeNode.Tag is MailMessage message)
            {
                webBrowser.DocumentText = message.Render().Result;

                return;
            }

            if (SelectedTreeNode.Tag is MailWrapper.MailFolder folder)
            {
                folder.ReadMails();
               // foreach()
            }
            
        }

        //private TreeNode CreateOrUpdateMailNode(MailWrapper.MailFolder rootNode)
        //{
        //    TreeNode result = new TreeNode();
        //    result.Tag = rootNode;
        //    treeNode.Text = node.Text;
        //    treeNode.NodeFont = (node.Accent) ? BoldFont : RegularFont;
        //    rootNode.OnChange += RootNode_OnChange;
        //    foreach (var folder in rootNode.Nodes)
        //    {
        //        result.Nodes.Add(CreateNode(folder));
        //    }
        //    return result;
        //}

        //MessageNodes

        private void AddFolderButton_Click(object sender, EventArgs e)
        {
            //mailClient.AddFolder(SelectedTreeNode);
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            //mailClient.Detete(SelectedTreeNode);
        }

        private void SetReadedButton_Click(object sender, EventArgs e)
        {
            //mailClient.SetMessageAsReaded(SelectedTreeNode);
        }

        private void SetUnReadedButton_Click(object sender, EventArgs e)
        {
            //mailClient.SetMessageAsUnRead(SelectedTreeNode);
        }

        private void checkButton_Click(object sender, EventArgs e)
        {
            mailClient.WaitForNewMessagesAsync().ConfigureAwait(false);
        }
    }
}
