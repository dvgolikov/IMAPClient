using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace MailClient.MailWrapper
{
    class MailWorker
    {
        CancellationTokenSource cancel;
        CancellationTokenSource done;

        private TreeNode rootNode;

        private readonly Font RegularFont = new Font("Arial", 8f, FontStyle.Regular);
        private readonly Font BoldFont = new Font("Arial", 8f, FontStyle.Bold);

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
        private Dictionary<IMailFolder, TreeNode> folders;
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
            cancel = new CancellationTokenSource();
        }

        private void Inbox_CountChanged(object sender, EventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: New Message detected.");
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
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Connected.");

            ConnectionState(this, true);
        }

        private void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            ConnectionState(this, false);
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Disconected.");
            if (!e.IsRequested) workLoad.Add(ReconnectAsync(e.Host, e.Port, e.Options));
        }

        public async Task ReconnectAsync(string host, int port, SecureSocketOptions options)
        {
            // TODO: SSL validation
            Client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                await Client.ConnectAsync(host, port, options);

                await Client.AuthenticateAsync(Credentials);

                await Client.Inbox.OpenAsync(FolderAccess.ReadWrite);
                Client.Inbox.CountChanged += Inbox_CountChanged;
            }
            catch(Exception ex)
            {
                NewLogMessage?.Invoke(this, $"{DateTime.Now}: {ex.Message}");
            }

            if (Client.Capabilities.HasFlag(ImapCapabilities.UTF8Accept))
                await Client.EnableUTF8Async();

            folders = new Dictionary<IMailFolder, TreeNode>();

            var folder = Client.GetFolder(Client.PersonalNamespaces[0]);

            rootNode = new TreeNode() { Name = "root", Text = host, Tag = folder };

            folders.Add(folder, rootNode);

            Stopwatch stopWhatch = new Stopwatch();
            stopWhatch.Start();

            await ReadSubFolders(rootNode).ConfigureAwait(false);

            stopWhatch.Stop();
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {host} folders readed by {stopWhatch.ElapsedMilliseconds} ms.");

            UpdateFoldersTree?.Invoke(this, rootNode);
        }

        public async Task ReadSubFolders(TreeNode rootFolder)
        {
            if (!(rootFolder.Tag is IMailFolder rootMailFolder)) return;

            var serverFolders= await rootMailFolder.GetSubfoldersAsync();

            foreach (var serverFolder in serverFolders)
            {
                if(!await OpenMailFolder(serverFolder)) return;

                await serverFolder.StatusAsync(StatusItems.Unread| StatusItems.Count);

                var nodeTree = CreateOrUppdate(serverFolder);

                rootFolder.Nodes.Add(nodeTree);
                
                serverFolder.MessageFlagsChanged += ServerFolder_MessageFlagsChanged;
                serverFolder.CountChanged += ServerFolder_CountChanged;
                serverFolder.Opened += ServerFolder_Opened;
                serverFolder.UnreadChanged += ServerFolder_UnreadChanged;

                await ReadSubFolders(nodeTree).ConfigureAwait(false);
            }
        }

        public TreeNode CreateOrUppdate(IMailFolder folder)
        {
            if (!folders.TryGetValue(folder, out var treeNode))
            {
                treeNode = new TreeNode { Tag = folder };
                folders.Add(folder, treeNode);
            }

            if (folder.Count > 0)
            {
                if (folder.Unread > 0)
                {
                    treeNode.Text = $"{folder.Name} M:{folder.Count}/U:{folder.Unread}";
                    treeNode.NodeFont = BoldFont;
                }
                else
                {
                    treeNode.Text = $"{folder.Name} M:{folder.Count}";
                    treeNode.NodeFont = RegularFont;
                }
            }
            else
            {
                treeNode.Text = $"{folder.Name}";
                treeNode.NodeFont = RegularFont;
            }

            return treeNode;
        }

        private void ServerFolder_UnreadChanged(object sender, EventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: New Unread message.");
            if (sender is IMailFolder rootMailFolder)
                CreateOrUppdate(rootMailFolder);
        }

        private void ServerFolder_Opened(object sender, EventArgs e)
        {
            if(sender is IMailFolder mailFolder) NewLogMessage?.Invoke(this, $"{DateTime.Now}: {mailFolder.FullName} folder openned");
            else NewLogMessage?.Invoke(this, $"{DateTime.Now}: Uncnown folder openned");
        }

        private void ServerFolder_CountChanged(object sender, EventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Message count changed.");
            if (sender is IMailFolder rootMailFolder)
                CreateOrUppdate(rootMailFolder);
        }

        private void ServerFolder_MessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Message Number: {e.Index} flags changed.");
            if (sender is IMailFolder rootMailFolder)
                CreateOrUppdate(rootMailFolder);
        }

        public async Task ReadMails(TreeNode rootFolder)
        {
            if (!(rootFolder.Tag is IMailFolder rootMailFolder)) return;

            if (!await OpenMailFolder(rootMailFolder)) return;

            Stopwatch stopWhatch = new Stopwatch();
            stopWhatch.Start();

            var mailsTask = rootMailFolder.FetchAsync(0, -1, SummaryItems);

            foreach (var message in await mailsTask)
            {
                TreeNode messageNode;

                if (!rootFolder.Nodes.ContainsKey(message.UniqueId.Id.ToString()))
                {
                    messageNode = new TreeNode { Text = message.Envelope.Subject, Name= message.UniqueId.Id.ToString(), Tag= message };

                    rootFolder.Nodes.Add(messageNode);
                }
                else messageNode = rootFolder.Nodes[message.UniqueId.Id.ToString()];

                if (!message.Flags.Value.HasFlag(MessageFlags.Seen)) messageNode.NodeFont = BoldFont;
                else messageNode.NodeFont = RegularFont;
            }

            stopWhatch.Stop();
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {rootMailFolder.Name} folders mail readed by {stopWhatch.ElapsedMilliseconds} ms.");
        }

        public async Task<bool> OpenMailFolder(IMailFolder folder)
        {
            try
            {
                if (!folder.IsOpen) await folder.OpenAsync(FolderAccess.ReadWrite);
                return true;
            }
            catch (Exception ex)
            {
                NewLogMessage?.Invoke(this, $"{DateTime.Now}: ReadMails Exception: {ex.Message}.");
            }
            return false;
        }

        public async Task AddFolder(TreeNode rootFolder, string name="NewFolder")
        {
            if (!(rootFolder.Tag is IMailFolder rootMailFolder)) return;

            var newMailFolder = await rootMailFolder.CreateAsync(name, true);
            var newNode = new TreeNode() { Text = name, Tag = newMailFolder };

            folders.Add(newMailFolder, newNode);
            rootFolder.Nodes.Add(newNode);
        }

        public async Task Detete(TreeNode rootFolder)
        {
            if ((rootFolder.Tag is IMailFolder rootMailFolder))
            {
                await rootMailFolder.DeleteAsync();

                rootFolder.Remove();

                return;
            }
            if ((rootFolder.Tag is IMessageSummary messageInfo))
            {
                if (!(rootFolder.Parent.Tag is IMailFolder parentMailFolder)) return;
                await parentMailFolder.AddFlagsAsync(messageInfo.UniqueId, MessageFlags.Deleted, true);

                rootFolder.Remove();
            }


        }

        public void RenderMailMessage(IMailFolder mailFolder, IMessageSummary messageInfo)
        {
            if (!mailFolder.IsOpen) mailFolder.Open(FolderAccess.ReadWrite);

            MailRender mailRender = new MailRender();

            mailRender.RenderAsync(mailFolder, messageInfo.UniqueId, messageInfo.Body)
                .ContinueWith((t) =>
                {
                    UpdateWebBrowser(this, t.Result);
                });
        }

        public async Task SetMessageAsReaded(TreeNode treeNode)
        {
            if (treeNode.Tag is IMessageSummary messageInfo)
                if (treeNode.Parent.Tag is IMailFolder mailFolder)
                {
                    await mailFolder.AddFlagsAsync(messageInfo.UniqueId, MessageFlags.Seen, true);

                    CreateOrUppdate(mailFolder);

                    await ReadMails(treeNode.Parent);
                }
        }
        public async Task SetMessageAsUnRead(TreeNode treeNode)
        {
            if (treeNode.Tag is IMessageSummary messageInfo)
                if (treeNode.Parent.Tag is IMailFolder mailFolder)
                {
                    await mailFolder.RemoveFlagsAsync(messageInfo.UniqueId, MessageFlags.Seen, true);

                    CreateOrUppdate(mailFolder);

                    await ReadMails(treeNode.Parent);
                }
        }

        public async Task WaitForNewMessagesAsync()
        {
            if (Client.Capabilities.HasFlag(ImapCapabilities.Idle))
            {
                using (done = new CancellationTokenSource(new TimeSpan(0, 9, 0)))
                    await Client.IdleAsync(done.Token, cancel.Token);
            }
            else
            {
                await Task.Delay(new TimeSpan(0, 1, 0), cancel.Token);
                await Client.NoOpAsync(cancel.Token);
            }
        }
    }
}
