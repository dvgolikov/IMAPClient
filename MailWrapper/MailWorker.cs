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
        private Dictionary<TreeNode, IMailFolder> folders;
        private Dictionary<TreeNode, IMessageSummary> messages;
        private bool messagesArrived;
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
            }
            catch(Exception ex)
            {
                NewLogMessage?.Invoke(this, $"{DateTime.Now}: {ex.Message}");
            }

            if (Client.Capabilities.HasFlag(ImapCapabilities.UTF8Accept))
                await Client.EnableUTF8Async();

            folders = new Dictionary<TreeNode, IMailFolder>();
            messages = new Dictionary<TreeNode, IMessageSummary>();

            var folder = Client.GetFolder(Client.PersonalNamespaces[0]);

            var root = new TreeNode() { Name = "root", Text = host, Tag = folder };

            folders.Add(root, folder);

            Stopwatch stopWhatch = new Stopwatch();
            stopWhatch.Start();

            await ReadSubFolders(root).ConfigureAwait(false);

            stopWhatch.Stop();
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: {host} folders readed by {stopWhatch.ElapsedMilliseconds} ms.");

            UpdateFoldersTree?.Invoke(this, folders.Keys.Where(x => x.Name == "root").FirstOrDefault());
        }

        public async Task ReadSubFolders(TreeNode rootFolder)
        {

            if (!folders.TryGetValue(rootFolder, out var rootMailFolder)) return;

            var serverFolders= await rootMailFolder.GetSubfoldersAsync();

            foreach (var serverFolder in serverFolders)
            {
                await serverFolder.OpenAsync(FolderAccess.ReadWrite);
                await serverFolder.StatusAsync(StatusItems.Unread| StatusItems.Count);

                var node = new TreeNode
                {
                    Tag = serverFolder
                };

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
                
                serverFolder.MessageFlagsChanged += ServerFolder_MessageFlagsChanged;
                serverFolder.CountChanged += ServerFolder_CountChanged;
                serverFolder.Opened += ServerFolder_Opened;
                serverFolder.UnreadChanged += ServerFolder_UnreadChanged;

                await ReadSubFolders(node).ConfigureAwait(false);
            }
        }

        private void ServerFolder_UnreadChanged(object sender, EventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: New Unread message.");

        }

        private void ServerFolder_Opened(object sender, EventArgs e)
        {
            if(sender is IMailFolder mailFolder) NewLogMessage?.Invoke(this, $"{DateTime.Now}: {mailFolder.FullName} folder openned");
            else NewLogMessage?.Invoke(this, $"{DateTime.Now}: Uncnown folder openned");
        }

        private void ServerFolder_CountChanged(object sender, EventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Message count changed.");
            messagesArrived = true;
        }

        private void ServerFolder_MessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Message Number: {e.Index} flags changed."); 
        }

        public async Task ReadMails(TreeNode rootFolder)
        {
            if (!folders.TryGetValue(rootFolder, out var folder)) return;

            await folder.OpenAsync(FolderAccess.ReadWrite);

            var mailsTask =folder.FetchAsync(0, -1, SummaryItems);

            foreach (var message in await mailsTask)
            {
                var subNode = new TreeNode(message.Envelope.Subject);

                if (!message.Flags.Value.HasFlag(MessageFlags.Seen))
                    subNode.NodeFont = BoldFont;
                else
                    subNode.NodeFont = RegularFont;

                subNode.Tag = message;

                rootFolder.Nodes.Add(subNode);
            }
            await folder.CloseAsync().ConfigureAwait(false);
        }

        public async Task AddFolder(TreeNode rootFolder)
        {
            if (!folders.TryGetValue(rootFolder, out var folder)) return;

            rootFolder.Nodes.Add(await AddFolderAsync(folder));
        }

        public async Task<TreeNode> AddFolderAsync(IMailFolder rootFolder)
        {
            if (rootFolder == null) return default;

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
            if (!mailFolder.IsOpen) mailFolder.Open(FolderAccess.ReadWrite);

            MailRender mailRender = new MailRender();

            mailRender.RenderAsync(mailFolder, messageInfo.UniqueId, messageInfo.Body)
                .ContinueWith((t) =>
                {
                    UpdateWebBrowser(this, t.Result);
                });
        }

        public void SetMessageAsReaded(TreeNode treeNode)
        {
            if (treeNode.Tag is IMessageSummary messageInfo)
                if (treeNode.Parent.Tag is IMailFolder mailFolder)
                {
                    mailFolder.AddFlagsAsync(messageInfo.UniqueId, MessageFlags.Seen, true);
                }
        }
        public void SetMessageAsUnRead(TreeNode treeNode)
        {
            if (treeNode.Tag is IMessageSummary messageInfo)
                if (treeNode.Parent.Tag is IMailFolder mailFolder)
                {
                    mailFolder.RemoveFlagsAsync(messageInfo.UniqueId, MessageFlags.Seen, true);
                }
        }

        public async Task WaitForNewMessagesAsync()
        {
            //Client.NotifyAsync(true);


            //if (Client.Capabilities.HasFlag(ImapCapabilities.Idle))
            //{
            //    using (done = new CancellationTokenSource(new TimeSpan(0, 0, 10)))
            //        await Client.IdleAsync(done.Token, cancel.Token);
            //}
            //else
            //{
            //    await Task.Delay(new TimeSpan(0, 0, 10), cancel.Token);
            //    await Client.NoOpAsync(cancel.Token);
            //}
        }

        public bool IsFolder(TreeNode item)
        {
            return folders.Keys.Contains(item);
        }
        public bool IsMessage(TreeNode item)
        {
            return messages.Keys.Contains(item);
        }
    }
}
