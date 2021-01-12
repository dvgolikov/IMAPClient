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

using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace MailClient
{
    public partial class MainForm : Form
    {
		const MessageSummaryItems SummaryItems = MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.BodyStructure;
		public MainForm()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, EventArgs e)
        {
			var options = SecureSocketOptions.StartTlsWhenAvailable;
			options = SecureSocketOptions.SslOnConnect;

			int port;

			if (!string.IsNullOrEmpty(PortTextBox.Text))
				port = int.Parse(PortTextBox.Text);
			else
				port = 0; // default

			Program.Credentials = new NetworkCredential(LoginTextBox.Text.Trim(), PasswordTextBox.Text.Trim());
				

			try
			{
				Program.Client.ServerCertificateValidationCallback = (senderFake, certificate, chain, sslPolicyErrors) => true;

				Program.Client.ConnectAsync(ServerTextBox.Text.Trim(), port, options).Wait();

				Program.Client.AuthenticateAsync(Program.Credentials).Wait();

				if (Program.Client.Capabilities.HasFlag(ImapCapabilities.UTF8Accept))
					Program.Client.EnableUTF8Async();

			}
			catch
			{
				MessageBox.Show("Failed to Authenticate to server.");
				return;
			}

			var userFolders = Program.Client.GetFolder(Program.Client.PersonalNamespaces[0]);
			var subfolders = userFolders.GetSubfoldersAsync().Result;

			MailTreeView.PathSeparator = userFolders.DirectorySeparator.ToString();

			

			foreach (var subfolder in subfolders)
			{
				var node = new TreeNode(subfolder.Name + " " + subfolder.Unread.ToString()) { Tag = subfolder, ToolTipText = subfolder.FullName  };
				var wer=subfolder.GetSubfoldersAsync().Result;
				if (!subfolder.IsOpen)
					subfolder.OpenAsync(FolderAccess.ReadOnly).Wait();
				var summaries = subfolder.FetchAsync(0, -1, SummaryItems).Result;
				
				foreach (var message in summaries)
				{

					var subNode = new TreeNode(message.Envelope.Subject) ;


					node.Nodes.Add(subNode);
				}
				MailTreeView.Nodes.Add(node);
				
			}

		}

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
