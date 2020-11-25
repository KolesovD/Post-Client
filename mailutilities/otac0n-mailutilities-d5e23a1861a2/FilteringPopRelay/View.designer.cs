using System;

namespace FilteringPopRelay
{
    partial class View
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(Boolean disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(View));
            this.StatusPollingTimer = new System.Windows.Forms.Timer(this.components);
            this.PopServerGroupBox = new System.Windows.Forms.GroupBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.ServerStatus = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ServerStop = new System.Windows.Forms.Button();
            this.ServerStart = new System.Windows.Forms.Button();
            this.PopClientGroupBox = new System.Windows.Forms.GroupBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.ClientStatus = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.ClientStop = new System.Windows.Forms.Button();
            this.ClientStart = new System.Windows.Forms.Button();
            this.NotifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.TrayMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ShowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PopServerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.PopClientGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.TrayMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // StatusPollingTimer
            // 
            this.StatusPollingTimer.Enabled = true;
            this.StatusPollingTimer.Interval = 1000;
            this.StatusPollingTimer.Tick += new System.EventHandler(this.StatusPollingTimer_Tick);
            // 
            // PopServerGroupBox
            // 
            this.PopServerGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PopServerGroupBox.Controls.Add(this.pictureBox1);
            this.PopServerGroupBox.Controls.Add(this.ServerStatus);
            this.PopServerGroupBox.Controls.Add(this.label1);
            this.PopServerGroupBox.Controls.Add(this.ServerStop);
            this.PopServerGroupBox.Controls.Add(this.ServerStart);
            this.PopServerGroupBox.Location = new System.Drawing.Point(12, 12);
            this.PopServerGroupBox.Name = "PopServerGroupBox";
            this.PopServerGroupBox.Size = new System.Drawing.Size(278, 81);
            this.PopServerGroupBox.TabIndex = 0;
            this.PopServerGroupBox.TabStop = false;
            this.PopServerGroupBox.Text = "Pop Server";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::FilteringPopRelay.Properties.Resources.black_server_48x48;
            this.pictureBox1.Location = new System.Drawing.Point(6, 19);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(48, 48);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox1.TabIndex = 6;
            this.pictureBox1.TabStop = false;
            // 
            // ServerStatus
            // 
            this.ServerStatus.AutoSize = true;
            this.ServerStatus.Location = new System.Drawing.Point(141, 45);
            this.ServerStatus.Name = "ServerStatus";
            this.ServerStatus.Size = new System.Drawing.Size(13, 13);
            this.ServerStatus.TabIndex = 3;
            this.ServerStatus.Text = "?";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(95, 45);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Status:";
            // 
            // ServerStop
            // 
            this.ServerStop.Enabled = false;
            this.ServerStop.Location = new System.Drawing.Point(141, 19);
            this.ServerStop.Name = "ServerStop";
            this.ServerStop.Size = new System.Drawing.Size(75, 23);
            this.ServerStop.TabIndex = 1;
            this.ServerStop.Text = "Stop";
            this.ServerStop.UseVisualStyleBackColor = true;
            this.ServerStop.Click += new System.EventHandler(this.ServerStop_Click);
            // 
            // ServerStart
            // 
            this.ServerStart.Enabled = false;
            this.ServerStart.Location = new System.Drawing.Point(60, 19);
            this.ServerStart.Name = "ServerStart";
            this.ServerStart.Size = new System.Drawing.Size(75, 23);
            this.ServerStart.TabIndex = 0;
            this.ServerStart.Text = "Start";
            this.ServerStart.UseVisualStyleBackColor = true;
            this.ServerStart.Click += new System.EventHandler(this.ServerStart_Click);
            // 
            // PopClientGroupBox
            // 
            this.PopClientGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PopClientGroupBox.Controls.Add(this.pictureBox2);
            this.PopClientGroupBox.Controls.Add(this.ClientStatus);
            this.PopClientGroupBox.Controls.Add(this.label4);
            this.PopClientGroupBox.Controls.Add(this.ClientStop);
            this.PopClientGroupBox.Controls.Add(this.ClientStart);
            this.PopClientGroupBox.Location = new System.Drawing.Point(12, 99);
            this.PopClientGroupBox.Name = "PopClientGroupBox";
            this.PopClientGroupBox.Size = new System.Drawing.Size(278, 81);
            this.PopClientGroupBox.TabIndex = 1;
            this.PopClientGroupBox.TabStop = false;
            this.PopClientGroupBox.Text = "Pop Client";
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::FilteringPopRelay.Properties.Resources.email_48x48;
            this.pictureBox2.Location = new System.Drawing.Point(6, 19);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(48, 48);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            // 
            // ClientStatus
            // 
            this.ClientStatus.AutoSize = true;
            this.ClientStatus.Location = new System.Drawing.Point(141, 45);
            this.ClientStatus.Name = "ClientStatus";
            this.ClientStatus.Size = new System.Drawing.Size(13, 13);
            this.ClientStatus.TabIndex = 3;
            this.ClientStatus.Text = "?";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(95, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(40, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "Status:";
            // 
            // ClientStop
            // 
            this.ClientStop.Enabled = false;
            this.ClientStop.Location = new System.Drawing.Point(141, 19);
            this.ClientStop.Name = "ClientStop";
            this.ClientStop.Size = new System.Drawing.Size(75, 23);
            this.ClientStop.TabIndex = 1;
            this.ClientStop.Text = "Stop";
            this.ClientStop.UseVisualStyleBackColor = true;
            this.ClientStop.Click += new System.EventHandler(this.ClientStop_Click);
            // 
            // ClientStart
            // 
            this.ClientStart.Enabled = false;
            this.ClientStart.Location = new System.Drawing.Point(60, 19);
            this.ClientStart.Name = "ClientStart";
            this.ClientStart.Size = new System.Drawing.Size(75, 23);
            this.ClientStart.TabIndex = 0;
            this.ClientStart.Text = "Start";
            this.ClientStart.UseVisualStyleBackColor = true;
            this.ClientStart.Click += new System.EventHandler(this.ClientStart_Click);
            // 
            // NotifyIcon
            // 
            this.NotifyIcon.ContextMenuStrip = this.TrayMenu;
            this.NotifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("NotifyIcon.Icon")));
            this.NotifyIcon.Text = "Spam Sniper";
            this.NotifyIcon.Visible = true;
            this.NotifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
            // 
            // TrayMenu
            // 
            this.TrayMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ShowMenuItem,
            this.toolStripSeparator1,
            this.ExitMenuItem});
            this.TrayMenu.Name = "TrayMenu";
            this.TrayMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.TrayMenu.ShowImageMargin = false;
            this.TrayMenu.Size = new System.Drawing.Size(79, 54);
            // 
            // ShowMenuItem
            // 
            this.ShowMenuItem.Name = "ShowMenuItem";
            this.ShowMenuItem.Size = new System.Drawing.Size(78, 22);
            this.ShowMenuItem.Text = "&Show";
            this.ShowMenuItem.Click += new System.EventHandler(this.NotifyIcon_DoubleClick);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(75, 6);
            // 
            // ExitMenuItem
            // 
            this.ExitMenuItem.Name = "ExitMenuItem";
            this.ExitMenuItem.Size = new System.Drawing.Size(78, 22);
            this.ExitMenuItem.Text = "E&xit";
            this.ExitMenuItem.Click += new System.EventHandler(this.ExitMenuItem_Click);
            // 
            // View
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 193);
            this.Controls.Add(this.PopClientGroupBox);
            this.Controls.Add(this.PopServerGroupBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "View";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Spam Sniper Control Panel";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.View_FormClosing);
            this.Resize += new System.EventHandler(this.View_Resize);
            this.PopServerGroupBox.ResumeLayout(false);
            this.PopServerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.PopClientGroupBox.ResumeLayout(false);
            this.PopClientGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.TrayMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer StatusPollingTimer;
        private System.Windows.Forms.GroupBox PopServerGroupBox;
        private System.Windows.Forms.Button ServerStop;
        private System.Windows.Forms.Button ServerStart;
        private System.Windows.Forms.GroupBox PopClientGroupBox;
        private System.Windows.Forms.Button ClientStop;
        private System.Windows.Forms.Button ClientStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label ServerStatus;
        private System.Windows.Forms.Label ClientStatus;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NotifyIcon NotifyIcon;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.ContextMenuStrip TrayMenu;
        private System.Windows.Forms.ToolStripMenuItem ShowMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ExitMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}

