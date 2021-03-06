﻿namespace steamBackup.Forms
{
    using System;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Windows.Forms;
    using Newtonsoft.Json;
    using steamBackup.Properties;

    public partial class UpdateWizard : Form
    {

        private string m_updName = @"N/A";
        private string m_updVersion = @"N/A";
        private string m_updUrl = @"N/A";
        private string m_updChangeList = @"N/A";

        public UpdateWizard()
        {
            InitializeComponent();
        }

        private void UpdateWizard_Shown(object sender, EventArgs e)
        {
            this.Refresh();

            // https://docs.google.com/document/d/1d-ftHzMkjHXzmlAdjmm8XK1ZvTLK5TVYREMN2fzx2Jg/edit
            var data = GetJsonFile(@"https://docs.google.com/document/d/1d-ftHzMkjHXzmlAdjmm8XK1ZvTLK5TVYREMN2fzx2Jg/export?format=txt");

            var thisVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // check to make sure we have a valid json file.
            if (data == null)
            {
                infoBox.Text = Resources.ToolUpdateDownloadFail;
                return;
            }

            try
            {
                while (data.Read())
                {
                    if (data.Value == null) continue;

                    switch (data.Value.ToString())
                    {
                        case "name":
                            data.Read();
                            m_updName = data.Value.ToString();
                            break;
                        case "version":
                            data.Read();
                            m_updVersion = data.Value.ToString();
                            break;
                        case "url":
                            data.Read();
                            m_updUrl = data.Value.ToString();
                            break;
                        case "changelist":
                            data.Read();
                            m_updChangeList = data.Value.ToString();
                            break;
                    }
                }
            }
            catch
            {
                infoBox.Text = Resources.ToolUpdateBadJson;
                return;
            }
                
            if (thisVer.Equals(m_updVersion))
            {
                infoBox.Text = Resources.ToolUpdateNoNewVersion;
            }
            else
            {
                infoBox.Text = string.Format(Resources.UpdateAvailable, m_updName, m_updChangeList, Resources.ChangeListLegend, Environment.NewLine);
                btnUpdate.Enabled = true;
            }
        }

        private static JsonTextReader GetJsonFile(string url)
        {
            JsonTextReader reader;
            using (var webClient = new WebClient())
            {
                try
                {

                    var jsonStr = webClient.DownloadString(url);
                    // Now parse with JSON.Net

                    reader = new JsonTextReader(new StringReader(jsonStr));
                }
                catch
                {
                    return null;
                }
            }

            return reader;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            btnUpdate.Enabled = false;
            btnCancel.Enabled = false;
            
            var downloadLocation = Application.StartupPath + @"/rsc/update.7z";

            // Create a new WebClient instance.
            var myWebClient = new WebClient();

            infoBox.Text = Resources.DownloadingUpdate;
            this.Refresh();

            // Download the Web resource and save it into the current filesystem folder.
            try
            {
                myWebClient.DownloadFile(m_updUrl, downloadLocation);
            }
            catch
            {
                infoBox.Text = Resources.DownloadingUpdateError;
                btnCancel.Enabled = true;
                return;
            }

            infoBox.Text = Resources.DownloadingUpdateSuccess;

            Application.Exit();
            System.Diagnostics.Process.Start(@"rsc\update.bat");
        }
    }
}
