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

        private string updName = @"N/A";
        private string updVersion = @"N/A";
        private string updUrl = @"N/A";
        private string updChangeList = @"N/A";

        public UpdateWizard()
        {
            InitializeComponent();
        }

        private void UpdateWizard_Shown(object sender, EventArgs e)
        {
            this.Refresh();

            var data = getJsonFile(@"http://du-z.com/sbtRelease.json");

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
                        if (data.Value != null)
                        {
                            if (data.Value.ToString() == "name")
                            {
                                data.Read();
                                updName = data.Value.ToString();
                            }
                            else if (data.Value.ToString() == "version")
                            {
                                data.Read();
                                updVersion = data.Value.ToString();
                            }
                            else if (data.Value.ToString() == "url")
                            {
                                data.Read();
                                updUrl = data.Value.ToString();
                            }
                            else if (data.Value.ToString() == "changelist")
                            {
                                data.Read();
                                updChangeList = data.Value.ToString();
                            }
                        }
                    }
                }
                catch
                {
                    infoBox.Text = Resources.ToolUpdateBadJson;
                    return;
                }
                
            if (thisVer.Equals(updVersion))
            {
                infoBox.Text = Resources.ToolUpdateNoNewVersion;
            }
            else
            {
                infoBox.Text = "Update available: " + updName + "\n\n" + updChangeList + "\n" + Resources.ChangeListLegend;
                btnUpdate.Enabled = true;
            }

            }
        }

        private JsonTextReader getJsonFile(string url)
        {
            JsonTextReader reader = null;
            using (var webClient = new System.Net.WebClient())
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
            
            var DownloadLocation = Application.StartupPath + @"/rsc/update.7z";

            // Create a new WebClient instance.
            var myWebClient = new WebClient();

            infoBox.Text = "Downloading Update.";
            this.Refresh();

            // Download the Web resource and save it into the current filesystem folder.
            try
            {
                myWebClient.DownloadFile(updUrl, DownloadLocation);
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
