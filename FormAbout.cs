﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace TacticalOpsQuickJoin {
    public partial class FormAbout : Form {
        public FormAbout() {
            InitializeComponent();

            this.BackColor = Color.FromArgb(32, 32, 32);
            this.ForeColor = Color.WhiteSmoke;
            lblVersion.ForeColor = Color.DarkGray;

            lblAboutInfo.Text = @"Tactical Ops Quick Joiner made by - (\/)(\/)-." + Environment.NewLine
                                + "Join your friends on the servers quicker." + Environment.NewLine + Environment.NewLine
                                + "Just add me on steam if you have any questions.";

            // Make links clickable
            lblAboutInfo.Text += Environment.NewLine + Environment.NewLine + "Website: ";
            var linkWebsite = new LinkLabel
            {
                Text = "http://www.jildert.com",
                Location = new Point(lblAboutInfo.Left + 60, lblAboutInfo.Bottom - 35),
                Size = new Size(150, 15),
                LinkColor = Color.LightSkyBlue,
                BackColor = Color.Transparent
            };
            linkWebsite.LinkClicked += (s, e) => Process.Start("http://www.jildert.com");
            this.Controls.Add(linkWebsite);

            var lblSteam = new Label
            {
                Text = "Steam: ",
                Location = new Point(lblAboutInfo.Left + 10, lblAboutInfo.Bottom - 20),
                Size = new Size(50, 15),
                ForeColor = Color.WhiteSmoke,
                BackColor = Color.Transparent
            };
            this.Controls.Add(lblSteam);

            var linkSteam = new LinkLabel
            {
                Text = "http://steamcommunity.com/id/jildert",
                Location = new Point(lblAboutInfo.Left + 60, lblAboutInfo.Bottom - 20),
                Size = new Size(200, 15),
                LinkColor = Color.LightSkyBlue,
                BackColor = Color.Transparent
            };
            linkSteam.LinkClicked += (s, e) => Process.Start("http://steamcommunity.com/id/jildert");
            this.Controls.Add(linkSteam);

            if (lblModInfo != null)
            {
                lblModInfo.Text = "--- MODDED BY LOCOS ---" + Environment.NewLine + Environment.NewLine
                                + "Modernized to .NET 8/10, implemented Async/Await for fast loading time," + Environment.NewLine
                                + "full Dark Mode/UI overhaul, fixed critical null-reference errors," + Environment.NewLine
                                + "and added Map Preview functionality with hover effects." + Environment.NewLine + Environment.NewLine
                                + "Enhanced server ping accuracy, improved bot detection," + Environment.NewLine
                                + "and modernized the entire codebase for better performance.";
                lblModInfo.ForeColor = Color.LightGreen;
            }
        }



        private void lblVersion_Click(object sender, EventArgs e) {

        }
    }
}