using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace MailClient.MailWrapper
{
    class MailWorker
    {
        const MessageSummaryItems SummaryItems = MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.BodyStructure;

        private readonly ImapClient Client = new ImapClient(new ProtocolLogger("imap.txt"));
        private SecureSocketOptions options = SecureSocketOptions.StartTlsWhenAvailable;
        private ICredentials Credentials;
        private string host;
        private int port;

        public TreeNode RootNode
        {
            get
            {
                return folders.Keys.Where(x=>x.Name=="root").FirstOrDefault();
            }
        }

        private Dictionary<TreeNode,IMailFolder> folders;
        public MailWorker(ICredentials credentials, string server, int prt)
        {
            Credentials = credentials;
            host = server;
            port = prt;
            Client.Disconnected += OnClientDisconnected;
            options = SecureSocketOptions.SslOnConnect;
            ReconnectAsync(host, port, options).Wait();

        }

        private async void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            
            if (!e.IsRequested) await ReconnectAsync(e.Host, e.Port, e.Options);
        }

        public async Task ReconnectAsync(string host, int port, SecureSocketOptions options)
        {
            // Note: for demo purposes, we're ignoring SSL validation errors (don't do this in production code)
            Client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            await Client.ConnectAsync(host, port, options).ConfigureAwait(false);

            await Client.AuthenticateAsync(Credentials).ConfigureAwait(false);

            if (Client.Capabilities.HasFlag(ImapCapabilities.UTF8Accept))
                await Client.EnableUTF8Async();

            folders = new Dictionary<TreeNode, IMailFolder>();

            var root = new TreeNode() { Name = "root", Text = host, Tag = Client.GetFolder(Client.PersonalNamespaces[0]) };

            folders.Add(root, root.Tag as IMailFolder);
            await ReadSubFolders(root);
        }
        public async Task ReadSubFolders(TreeNode rootFolder)
        {
            var rootMailFolder = rootFolder.Tag as IMailFolder;
            if (rootMailFolder == null) return;

            var serverFolders= await rootMailFolder.GetSubfoldersAsync();

            foreach (var serverFolder in serverFolders)
            {
                var node = new TreeNode() { Text= serverFolder.Name, Tag= serverFolder };
                folders.Add(node, serverFolder);
                rootFolder.Nodes.Add(node);
                ReadSubFolders(node).ConfigureAwait(false);
            }
        }

        public async Task ReadMails(TreeNode rootFolder)
        {
            var folder = rootFolder.Tag as IMailFolder;
            if (folder == null) return;
            if (!folder.IsOpen) folder.Open(FolderAccess.ReadOnly);
            var mailsTask =folder.FetchAsync(0, -1, SummaryItems);

            foreach (var message in await mailsTask)
            {
                var subNode = new TreeNode(message.Envelope.Subject);
                rootFolder.Nodes.Add(subNode);
            }
        }

        public async Task AddFolder(TreeNode rootFolder)
        {
            var folder = rootFolder.Tag as IMailFolder;
            if (folder == null) return;
            rootFolder.Nodes.Add(AddFolder(folder));
        }

        public TreeNode AddFolder(IMailFolder rootFolder)
        {
            if (rootFolder == null) return default;

            if (rootFolder.IsOpen) rootFolder.Close();

            rootFolder.Open(FolderAccess.ReadWrite);

            var t = rootFolder.Create("NewFolder", true);
            var node = new TreeNode() { Text = t.Name, Tag = t };
            folders.Add(node, t);
            return node;

        }
    }
}
