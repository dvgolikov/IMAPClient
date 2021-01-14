
namespace MailClient
{
    partial class MainForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.ConnectButton = new System.Windows.Forms.Button();
            this.ServerTextBox = new System.Windows.Forms.TextBox();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.LoginTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.MailTreeView = new System.Windows.Forms.TreeView();
            this.logBox = new System.Windows.Forms.TextBox();
            this.webBrowser = new System.Windows.Forms.WebBrowser();
            this.AddFolderButton = new System.Windows.Forms.Button();
            this.AddMailButton = new System.Windows.Forms.Button();
            this.DeleteButton = new System.Windows.Forms.Button();
            this.connectedStatusLabel = new System.Windows.Forms.Label();
            this.SetReadedButton = new System.Windows.Forms.Button();
            this.SetUnReadedButton = new System.Windows.Forms.Button();
            this.DeleteMessageButton = new System.Windows.Forms.Button();
            this.checkButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ConnectButton
            // 
            this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ConnectButton.Location = new System.Drawing.Point(1096, 13);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(75, 23);
            this.ConnectButton.TabIndex = 0;
            this.ConnectButton.Text = "Connect";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_ClickAsync);
            // 
            // ServerTextBox
            // 
            this.ServerTextBox.Location = new System.Drawing.Point(59, 14);
            this.ServerTextBox.Name = "ServerTextBox";
            this.ServerTextBox.Size = new System.Drawing.Size(281, 20);
            this.ServerTextBox.TabIndex = 2;
            this.ServerTextBox.Text = "mx1.onlyoffice.com";
            // 
            // PortTextBox
            // 
            this.PortTextBox.Location = new System.Drawing.Point(381, 15);
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(50, 20);
            this.PortTextBox.TabIndex = 3;
            this.PortTextBox.Text = "993";
            // 
            // LoginTextBox
            // 
            this.LoginTextBox.Location = new System.Drawing.Point(479, 15);
            this.LoginTextBox.Name = "LoginTextBox";
            this.LoginTextBox.Size = new System.Drawing.Size(259, 20);
            this.LoginTextBox.TabIndex = 4;
            this.LoginTextBox.Text = "dmitriy.golikov@onlyoffice.com";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Server:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(346, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Port:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(437, 18);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Login:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(744, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Password:";
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(797, 15);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.Size = new System.Drawing.Size(293, 20);
            this.PasswordTextBox.TabIndex = 9;
            // 
            // MailTreeView
            // 
            this.MailTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.MailTreeView.Location = new System.Drawing.Point(12, 76);
            this.MailTreeView.Name = "MailTreeView";
            this.MailTreeView.Size = new System.Drawing.Size(403, 433);
            this.MailTreeView.TabIndex = 10;
            this.MailTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.MailTreeView_AfterSelect);
            // 
            // logBox
            // 
            this.logBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.logBox.Location = new System.Drawing.Point(12, 515);
            this.logBox.Multiline = true;
            this.logBox.Name = "logBox";
            this.logBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.logBox.Size = new System.Drawing.Size(1240, 62);
            this.logBox.TabIndex = 11;
            // 
            // webBrowser
            // 
            this.webBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser.Location = new System.Drawing.Point(421, 76);
            this.webBrowser.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser.Name = "webBrowser";
            this.webBrowser.Size = new System.Drawing.Size(831, 433);
            this.webBrowser.TabIndex = 12;
            // 
            // AddFolderButton
            // 
            this.AddFolderButton.Location = new System.Drawing.Point(153, 47);
            this.AddFolderButton.Name = "AddFolderButton";
            this.AddFolderButton.Size = new System.Drawing.Size(75, 23);
            this.AddFolderButton.TabIndex = 13;
            this.AddFolderButton.Text = "Add Folder";
            this.AddFolderButton.UseVisualStyleBackColor = true;
            this.AddFolderButton.Click += new System.EventHandler(this.AddFolderButton_Click);
            // 
            // AddMailButton
            // 
            this.AddMailButton.Location = new System.Drawing.Point(234, 47);
            this.AddMailButton.Name = "AddMailButton";
            this.AddMailButton.Size = new System.Drawing.Size(75, 23);
            this.AddMailButton.TabIndex = 14;
            this.AddMailButton.Text = "Add Mail";
            this.AddMailButton.UseVisualStyleBackColor = true;
            // 
            // DeleteButton
            // 
            this.DeleteButton.Location = new System.Drawing.Point(315, 47);
            this.DeleteButton.Name = "DeleteButton";
            this.DeleteButton.Size = new System.Drawing.Size(100, 23);
            this.DeleteButton.TabIndex = 15;
            this.DeleteButton.Text = "Delete Folder";
            this.DeleteButton.UseVisualStyleBackColor = true;
            this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
            // 
            // connectedStatusLabel
            // 
            this.connectedStatusLabel.AutoSize = true;
            this.connectedStatusLabel.Location = new System.Drawing.Point(12, 52);
            this.connectedStatusLabel.Name = "connectedStatusLabel";
            this.connectedStatusLabel.Size = new System.Drawing.Size(59, 13);
            this.connectedStatusLabel.TabIndex = 16;
            this.connectedStatusLabel.Text = "Connected";
            // 
            // SetReadedButton
            // 
            this.SetReadedButton.Location = new System.Drawing.Point(421, 47);
            this.SetReadedButton.Name = "SetReadedButton";
            this.SetReadedButton.Size = new System.Drawing.Size(100, 23);
            this.SetReadedButton.TabIndex = 17;
            this.SetReadedButton.Text = "Set as Readed";
            this.SetReadedButton.UseVisualStyleBackColor = true;
            this.SetReadedButton.Click += new System.EventHandler(this.SetReadedButton_Click);
            // 
            // SetUnReadedButton
            // 
            this.SetUnReadedButton.Location = new System.Drawing.Point(527, 47);
            this.SetUnReadedButton.Name = "SetUnReadedButton";
            this.SetUnReadedButton.Size = new System.Drawing.Size(100, 23);
            this.SetUnReadedButton.TabIndex = 18;
            this.SetUnReadedButton.Text = "Set as UnReaded";
            this.SetUnReadedButton.UseVisualStyleBackColor = true;
            this.SetUnReadedButton.Click += new System.EventHandler(this.SetUnReadedButton_Click);
            // 
            // DeleteMessageButton
            // 
            this.DeleteMessageButton.Location = new System.Drawing.Point(633, 47);
            this.DeleteMessageButton.Name = "DeleteMessageButton";
            this.DeleteMessageButton.Size = new System.Drawing.Size(100, 23);
            this.DeleteMessageButton.TabIndex = 19;
            this.DeleteMessageButton.Text = "Delete Message";
            this.DeleteMessageButton.UseVisualStyleBackColor = true;
            this.DeleteMessageButton.Click += new System.EventHandler(this.DeleteMessageButton_Click);
            // 
            // checkButton
            // 
            this.checkButton.Location = new System.Drawing.Point(72, 47);
            this.checkButton.Name = "checkButton";
            this.checkButton.Size = new System.Drawing.Size(75, 23);
            this.checkButton.TabIndex = 20;
            this.checkButton.Text = "Check";
            this.checkButton.UseVisualStyleBackColor = true;
            this.checkButton.Click += new System.EventHandler(this.checkButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 589);
            this.Controls.Add(this.checkButton);
            this.Controls.Add(this.DeleteMessageButton);
            this.Controls.Add(this.SetUnReadedButton);
            this.Controls.Add(this.SetReadedButton);
            this.Controls.Add(this.connectedStatusLabel);
            this.Controls.Add(this.DeleteButton);
            this.Controls.Add(this.AddMailButton);
            this.Controls.Add(this.AddFolderButton);
            this.Controls.Add(this.webBrowser);
            this.Controls.Add(this.logBox);
            this.Controls.Add(this.MailTreeView);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.LoginTextBox);
            this.Controls.Add(this.PortTextBox);
            this.Controls.Add(this.ServerTextBox);
            this.Controls.Add(this.ConnectButton);
            this.Name = "MainForm";
            this.Text = "IMAP Client";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.TextBox ServerTextBox;
        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.TextBox LoginTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.TreeView MailTreeView;
        private System.Windows.Forms.TextBox logBox;
        private System.Windows.Forms.WebBrowser webBrowser;
        private System.Windows.Forms.Button AddFolderButton;
        private System.Windows.Forms.Button AddMailButton;
        private System.Windows.Forms.Button DeleteButton;
        private System.Windows.Forms.Label connectedStatusLabel;
        private System.Windows.Forms.Button SetReadedButton;
        private System.Windows.Forms.Button SetUnReadedButton;
        private System.Windows.Forms.Button DeleteMessageButton;
        private System.Windows.Forms.Button checkButton;
    }
}

