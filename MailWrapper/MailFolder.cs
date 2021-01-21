using MailKit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.MailWrapper
{
    class MailFolder : IMailPresentation
    {
        const MessageSummaryItems SummaryItems = MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags | MessageSummaryItems.BodyStructure;

        public string Name;

        public IMailFolder serverFolder;
        public List<MailFolder> Nodes;
        public List<MailMessage> Messages;

        public event EventHandler OnChange;

        public string Text
        {
            get
            {
                if (serverFolder.Count > 0)
                {
                    if (serverFolder.Unread > 0)
                    {
                        return $"{serverFolder.Name} M:{serverFolder.Count}/U:{serverFolder.Unread}";
                    }
                    else
                    {
                        return $"{serverFolder.Name} M:{serverFolder.Count}";
                    }
                }
                else
                {
                    return $"{serverFolder.Name}";
                }
            }
        }

        public bool Accent => (serverFolder.Unread > 0);

        public MailFolder(IMailFolder serverFolder)
        {
            this.serverFolder = serverFolder;

            Nodes = new List<MailFolder>();
            Messages = new List<MailMessage>();
        }

        public async Task ReadFolder()
        {
            if (!serverFolder.IsOpen)
            {
                try
                {
                    await serverFolder.OpenAsync(FolderAccess.ReadWrite);
                }
                catch(Exception ex)
                {

                }

                serverFolder.MessageFlagsChanged += ServerFolder_MessageFlagsChanged;
                serverFolder.CountChanged += ServerFolder_CountChanged;
                serverFolder.UnreadChanged += ServerFolder_UnreadChanged;
            }

            await serverFolder.StatusAsync(StatusItems.Unread | StatusItems.Count);
        }

        private void ServerFolder_UnreadChanged(object sender, EventArgs e)
        {
            ReadMails();
        }

        private void ServerFolder_CountChanged(object sender, EventArgs e)
        {
            ReadMails();
        }

        private void ServerFolder_MessageFlagsChanged(object sender, MessageFlagsChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public async Task<long> ReadSubFolders()
        {
            Stopwatch stopWhatch = new Stopwatch();
            stopWhatch.Start();

            var serverFolders = await serverFolder.GetSubfoldersAsync();

            foreach (var folder in serverFolders)
            {
                if (Nodes.Count(x=>x.Name==folder.Name) == 0)
                {
                    var newFolder = new MailFolder(folder);
                    Nodes.Add(newFolder);

                    await newFolder.ReadFolder();

                    await newFolder.ReadSubFolders();
                }
            }

            stopWhatch.Stop();
            return stopWhatch.ElapsedMilliseconds;
        }

        public async Task<long> ReadMails()
        {
            Stopwatch stopWhatch = new Stopwatch();
            stopWhatch.Start();

            try
            {
                var mailsTask = serverFolder.FetchAsync(0, -1, SummaryItems);

                foreach (var message in await mailsTask)
                {
                    if(Messages.Count(x=>x.serverMessage.UniqueId==message.UniqueId)==0)
                    {
                        Messages.Add(new MailMessage(message, serverFolder));
                    }
                }
            }
            catch (Exception ex)
            {
                return 0;
            }

            stopWhatch.Stop();
            return stopWhatch.ElapsedMilliseconds;
        }

        public void Update()
        {
            throw new NotImplementedException();
        }

        public async Task Detete(MailMessage message)
        {
                await serverFolder.AddFlagsAsync(message.serverMessage.UniqueId, MessageFlags.Deleted, true);
        }

        public async Task AddFolders(string Name)
        {
                //if (!(rootFolder.Tag is IMailFolder rootMailFolder)) return;

                //var newMailFolder = await rootMailFolder.CreateAsync(name, true);
                //var newNode = new TreeNode() { Text = name, Tag = newMailFolder };

                //folders.Add(newMailFolder, newNode);
                //rootFolder.Nodes.Add(newNode);

        }
    }
}
