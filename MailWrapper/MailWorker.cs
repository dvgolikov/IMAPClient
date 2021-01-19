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
        
        public MailFolder rootNode;

        public event EventHandler<string> NewLogMessage;
        public event EventHandler<bool> ConnectionState;
        public event EventHandler<bool> IdleState;
        public event EventHandler Update;

        private readonly ImapClient Client = new ImapClient(new ProtocolLogger("imap.txt"));
        private readonly SecureSocketOptions options = SecureSocketOptions.StartTlsWhenAvailable;
        private readonly ICredentials Credentials;
        private readonly string host;
        private readonly int port;

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
            cancel = new CancellationTokenSource();
        }

        public void Close()
        {
            cancel?.Cancel();
        }

        private void Client_MetadataChanged(object sender, MetadataChangedEventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Metadata Changed: {e.Metadata.Value}");
        }

        private void Client_Alert(object sender, AlertEventArgs e)
        {
            NewLogMessage?.Invoke(this, $"{DateTime.Now}: Alert: {e.Message}");
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
            if (!e.IsRequested) _ = ReconnectAsync();
        }

        public async Task ReconnectAsync()
        {
            // TODO: SSL validation
            Client.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            try
            {
                if (!Client.IsConnected)
                    await Client.ConnectAsync(host, port, options);

                if (!Client.IsAuthenticated)
                {
                    await Client.AuthenticateAsync(Credentials);
                }
            }
            catch(Exception ex)
            {
                NewLogMessage?.Invoke(this, $"{DateTime.Now}: {ex.Message}");
            }

            if (Client.Capabilities.HasFlag(ImapCapabilities.UTF8Accept))
                await Client.EnableUTF8Async();

            if (rootNode == null)
            {
                var folder = Client.GetFolder(Client.PersonalNamespaces[0]);

                rootNode = new MailFolder(folder);

                await rootNode.ReadFolder();

                await rootNode.ReadSubFolders();
                Update?.Invoke(this, EventArgs.Empty);
            }
            
        }

        public async Task WaitForNewMessagesAsync()
        {
            if (Client.Capabilities.HasFlag(ImapCapabilities.Idle))
            {
                try
                {
                    IdleState(this, true);
                    done = new CancellationTokenSource(new TimeSpan(0, 9, 0));
                        await Client.IdleAsync(done.Token, cancel.Token);
                    IdleState(this, false);
                }
                catch (Exception ex)
                {
                    NewLogMessage?.Invoke(this, $"{DateTime.Now}: {ex.Message}");
                }
                finally
                {
                    done.Dispose();
                    done = null;
                }
            }
            else
            {
                IdleState(this, true);
                await Task.Delay(new TimeSpan(0, 1, 0), cancel.Token);
                await Client.NoOpAsync(cancel.Token);
                IdleState(this, false);
            }
        }
    }
}
