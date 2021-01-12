using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace MailClient.MailWrapper
{
    class MailWorker
    {
        public static readonly ImapClient Client = new ImapClient(new ProtocolLogger("imap.txt"));
        public static ICredentials Credentials;
        public MailWorker()
        {
            Client.Disconnected += OnClientDisconnected;

        }

        private async void OnClientDisconnected(object sender, DisconnectedEventArgs e)
        {
            if (!e.IsRequested) await ReconnectAsync(e.Host, e.Port, e.Options);
        }

        public static async Task ReconnectAsync(string host, int port, SecureSocketOptions options)
        {
            // Note: for demo purposes, we're ignoring SSL validation errors (don't do this in production code)
            Client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            await Client.ConnectAsync(host, port, options);

            await Client.AuthenticateAsync(Credentials);

            if (Client.Capabilities.HasFlag(ImapCapabilities.UTF8Accept))
                await Client.EnableUTF8Async();

            CurrentTask = Task.FromResult(true);
        }
    }
}
