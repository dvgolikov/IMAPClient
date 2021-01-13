using System;
using System.Collections.Generic;
using System.Drawing;
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
        private readonly Font RegularFont = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Regular);
        private readonly Font BoldFont = new Font(FontFamily.GenericSansSerif, 10f, FontStyle.Bold);

        const MessageSummaryItems SummaryItems = MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.BodyStructure;

        public event EventHandler<string> NewLogMessage;
        public event EventHandler<bool> ConnectionState;
        public event EventHandler<TreeNode> UpdateFoldersTree;
        public event EventHandler<string> UpdateWebBrowser;

        private readonly ImapClient Client = new ImapClient(new ProtocolLogger("imap.txt"));
        private SecureSocketOptions options = SecureSocketOptions.StartTlsWhenAvailable;
        private readonly ICredentials Credentials;
        private readonly string host;
        private readonly int port;
        private Dictionary<TreeNode, IMailFolder> folders;
        private readonly List<Task> workLoad;

        public MailWorker(ICredentials credentials, string server, int prt)
        {
            Credentials = credentials;
            host = server;
            port = prt;
            Client.Disconnected += OnClientDisconnected;
            Client.Connected += Client_Connected;
            Client.Alert += Client_Alert;
            Client.MetadataChanged += Client_MetadataChanged;
            options = SecureSocketOptions.SslOnConnect;
            workLoad = new List<Task>();
        }

        public void ConnectToServer()
        {
            workLoad.Add(ReconnectAsync(host, port, options));
        }

        private void Client_MetadataChanged(object sender, MetadataChangedEventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {e.Metadata.Value}");
        }

        private void Client_Alert(object sender, AlertEventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {e.Message}");
        }

        private void Client_Connected(object sender, ConnectedEventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {e}");

            ConnectionState(this, true);
        }

        private async void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            ConnectionState(this, false);
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {e.ToString()}");
            if (!e.IsRequested) workLoad.Add(ReconnectAsync(e.Host, e.Port, e.Options));
        }

        public async Task ReconnectAsync(string host, int port, SecureSocketOptions options)
        {
            // Note: for demo purposes, we're ignoring SSL validation errors (don't do this in production code)
            Client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            try
            {
                await Client.ConnectAsync(host, port, options);

                await Client.AuthenticateAsync(Credentials);
            }
            catch(Exception ex)
            {
                NewLogMessage?.Invoke(this, $"{DateTime.Now}: {ex.Message}");
            }

            if (Client.Capabilities.HasFlag(ImapCapabilities.UTF8Accept))
                await Client.EnableUTF8Async();

            folders = new Dictionary<TreeNode, IMailFolder>();

            var root = new TreeNode() { Name = "root", Text = host, Tag = Client.GetFolder(Client.PersonalNamespaces[0]) };

            folders.Add(root, root.Tag as IMailFolder);

            await ReadSubFolders(root).ConfigureAwait(false);

            UpdateFoldersTree?.Invoke(this, folders.Keys.Where(x => x.Name == "root").FirstOrDefault());
        }

        public async Task ReadSubFolders(TreeNode rootFolder)
        {
            if (!folders.TryGetValue(rootFolder, out var rootMailFolder)) return;

            var serverFolders= await rootMailFolder.GetSubfoldersAsync();

            foreach (var serverFolder in serverFolders)
            {
                await serverFolder.StatusAsync(StatusItems.Unread| StatusItems.Count);

                var node = new TreeNode();

                node.Tag = serverFolder;

                if (serverFolder.Count>0)
                {
                    if (serverFolder.Unread > 0)
                    {
                        node.Text = $"{serverFolder.Name} M:{serverFolder.Count}/U:{serverFolder.Unread}";
                        node.NodeFont = BoldFont;
                    }
                    else
                    {
                        node.Text = $"{serverFolder.Name} M:{serverFolder.Count}";
                        node.NodeFont = RegularFont;
                    }
                }
                else
                {
                    node.Text = $"{serverFolder.Name}";
                    node.NodeFont = RegularFont;
                }

                folders.Add(node, serverFolder);
                rootFolder.Nodes.Add(node);

                serverFolder.MessageFlagsChanged += UpdateMailFolders;
                serverFolder.CountChanged += UpdateMailFolders;

                await ReadSubFolders(node);
            }
        }

        private void UpdateMailFolders(object sender, EventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {e}"); ;
        }

        public async Task ReadMails(TreeNode rootFolder)
        {
            if (!folders.TryGetValue(rootFolder, out var folder)) return;

            if (!folder.IsOpen) folder.Open(FolderAccess.ReadWrite);

            var mailsTask =folder.FetchAsync(0, -1, SummaryItems);

            foreach (var message in await mailsTask)
            {
                var subNode = new TreeNode(message.Envelope.Subject);

                subNode.Tag = message;

                rootFolder.Nodes.Add(subNode);
            }
        }

        public async Task AddFolder(TreeNode rootFolder)
        {
            if (!folders.TryGetValue(rootFolder, out var folder)) return;

            rootFolder.Nodes.Add(await AddFolderAsync(folder));
        }

        public async Task<TreeNode> AddFolderAsync(IMailFolder rootFolder)
        {
            if (rootFolder == null) return default;

            if (rootFolder.IsOpen) await rootFolder.CloseAsync();

            await rootFolder.OpenAsync(FolderAccess.ReadWrite);

            var newFolder =await rootFolder.CreateAsync("NewFolder", true);
            var node = new TreeNode() { Text = newFolder.Name, Tag = newFolder };

            folders.Add(node, newFolder);

            return node;
        }
        public async Task Detete(TreeNode rootFolder)
        {
            if (!folders.TryGetValue(rootFolder, out var folder)) return;

            await folder.DeleteAsync();

            rootFolder.Remove();
        }

        public void RenderMailMessage(IMailFolder mailFolder, IMessageSummary messageInfo)
        {
            MailRender mailRender = new MailRender();

            mailRender.RenderAsync(mailFolder, messageInfo.UniqueId, messageInfo.Body)
                .ContinueWith((t) =>
                {
                    UpdateWebBrowser(this, t.Result);
                });
        }
    }
}
