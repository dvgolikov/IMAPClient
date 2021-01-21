using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailClient.MailWrapper
{
    interface IMailPresentation
    {
        string Text { get;}
        bool Accent { get; }

        event EventHandler OnChange;

        void Update();
    }
}
