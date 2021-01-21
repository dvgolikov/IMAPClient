using MailKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.MailWrapper
{
    class MailMessage : IMailPresentation
    {
        public IMailFolder serverFolder;
        public IMessageSummary serverMessage;

        private bool IsReaded;

        public string Text
        {
            get
            {
                return serverMessage.NormalizedSubject;
            }
        }
        public bool Accent
        {
            get
            {
                return (!serverMessage.Flags.Value.HasFlag(MessageFlags.Seen));
            }
        }
        public event EventHandler OnChange;

        public MailMessage(IMessageSummary serverMessage, IMailFolder serverFolder)
        {
            this.serverMessage = serverMessage;
            this.serverFolder = serverFolder;
            IsReaded = serverMessage.Flags.Value.HasFlag(MessageFlags.Seen);
        }
        public async Task<string> Render()
        {
            MailRender mailRender = new MailRender();

            return await mailRender.RenderAsync(serverFolder, serverMessage.UniqueId, serverMessage.Body);
        }

        public void Update()
        {
            if(serverMessage.Flags.Value.HasFlag(MessageFlags.Seen)!= IsReaded)
            {
                IsReaded = serverMessage.Flags.Value.HasFlag(MessageFlags.Seen);
                OnChange(this, EventArgs.Empty);
            }
        }

        public async Task SetState(bool readed)
        {
            await serverFolder.AddFlagsAsync(serverMessage.UniqueId, MessageFlags.Seen, !readed);
            Update();
        }


    }
}
