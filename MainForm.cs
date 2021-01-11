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
				var node = new TreeNode(subfolder.Name) { Tag = subfolder, ToolTipText = subfolder.FullName };

				MailTreeView.Nodes.Add(node);

			}

		}


    }
}
