namespace steamBackup.Forms
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

        private string _updName = @"N/A";
        private string _updVersion = @"N/A";
        private string _updUrl = @"N/A";
        private string _updChangeList = @"N/A";

        public UpdateWizard()
        {
            InitializeComponent();
        }

        private void UpdateWizard_Shown(object sender, EventArgs e)
        {
            this.Refresh();

            var data = GetJsonFile(@"http://du-z.com/sbtRelease.json");

            var thisVer = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            // check to make sure we have a valid json file.
            if (data == null)
            {
                infoBox.Text = Resources.ToolUpdateDownloadFail;
                return;
            }
            else
            {
                try
                {
                    while (data.Read())
                    {
                        if (data.Value == null) continue;

                        switch (data.Value.ToString())
                        {
                            case "name":
                                data.Read();
                                _updName = data.Value.ToString();
                                break;
                            case "version":
                                data.Read();
                                _updVersion = data.Value.ToString();
                                break;
                            case "url":
                                data.Read();
                                _updUrl = data.Value.ToString();
                                break;
                            case "changelist":
                                data.Read();
                                _updChangeList = data.Value.ToString();
                                break;
                        }
                    }
                }
                catch
                {
                    infoBox.Text = Resources.ToolUpdateBadJson;
                    return;
                }
                
            if (thisVer.Equals(_updVersion))
            {
                infoBox.Text = Resources.ToolUpdateNoNewVersion;
            }
            else
            {
                infoBox.Text = "Update available: " + _updName + "\n\n" + _updChangeList + "\n" + Resources.ChangeListLegend;
                btnUpdate.Enabled = true;
            }

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

            infoBox.Text = "Downloading Update.";
            this.Refresh();

            // Download the Web resource and save it into the current filesystem folder.
            try
            {
                myWebClient.DownloadFile(_updUrl, downloadLocation);
            }
            catch
            {
                infoBox.Text = "Error downloading update.";
                btnCancel.Enabled = true;
                return;
            }


            infoBox.Text = "Successfully Downloaded Update";

            Application.Exit();
            System.Diagnostics.Process.Start(@"rsc\update.bat");
        }
    }
}
