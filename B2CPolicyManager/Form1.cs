﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace B2CPolicyManager
{
    public partial class B2CPolicyManager : Form
    {
        public B2CPolicyManager()
        {
            InitializeComponent();
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void Button1_Click(object sender, EventArgs e)
        {
        FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (policyFolderLbl.Text != null)
            {
                fbd.SelectedPath = policyFolderLbl.Text;
            }
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.Folder = fbd.SelectedPath;
                Properties.Settings.Default.Save();
                policyFolderLbl.Text = fbd.SelectedPath;
                checkedPolicyList.Items.Clear();
                //var ext = new List<string> { ".xml" };
                //string[] fileEntries = Directory.GetFiles(policyFolderLbl.Text).Select(p => Path.GetFileName(p)..Where(fn => Path.GetExtension(fn) == ".xml")

                var allFilenames = Directory.EnumerateFiles(policyFolderLbl.Text).Select(p => Path.GetFileName(p));

                // Get all filenames that have a .txt extension, excluding the extension
                var fileEntries = allFilenames.Where(fn => Path.GetExtension(fn) == ".xml")
                                             .Select(fn => Path.GetFileName(fn))
                                             .ToArray();


                foreach (string file in fileEntries)
                {
                    checkedPolicyList.Items.Add(file);
                }
            }
        }

        private async void LoginBtn_Click(object sender, EventArgs e)
        {
            
            //Task<string> token = AuthenticationHelper.GetTokenForUserAsync();
            string token = await AuthenticationHelper.GetTokenForUserAsync();
            if (token != null)
            {
                HTTPResponse.AppendText("\r\nLogged in, getting Policies.\r\n");
                HttpResponseMessage response = null;
                response = await UserMode.HttpGetAsync(Constants.TrustFrameworkPolicesUri);
                string content = await response.Content.ReadAsStringAsync();
                HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                PolicyList pL = JsonConvert.DeserializeObject<PolicyList>(content);

                UpdatePolicyList(pL);
            }
        }

        private async void ListBtn_ClickAsync(object sender, EventArgs e)
        {
            string token = await AuthenticationHelper.GetTokenForUserAsync();
            if (token != null)
            {
                HTTPResponse.AppendText("\r\nGetting Policies.\r\n");
                HttpResponseMessage response = null;
                response = await UserMode.HttpGetAsync(Constants.TrustFrameworkPolicesUri);
                string content = await response.Content.ReadAsStringAsync();
                //HTTPResponse.AppendText(JToken.Parse(content).ToString(Formatting.Indented));
                //HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());

                if (response.IsSuccessStatusCode == true)
                {
                    HTTPResponse.AppendText("\r\nSuccessfully updated policy list.\r\n");
                }
                else
                {
                    HTTPResponse.AppendText(content);
                }


                PolicyList pL = JsonConvert.DeserializeObject<PolicyList>(content);

                UpdatePolicyList(pL);
            }
        }

        private async void DeleteSelectedBtn_Click(object sender, EventArgs e)
        {

            if (policyList.SelectedItem != null)
            {
                string token = await AuthenticationHelper.GetTokenForUserAsync();
                if (token != null)
                {
                    HTTPResponse.AppendText("\r\nDeleting " + policyList.SelectedItem.ToString() +"\r\n");
                    HttpResponseMessage response = null;
                    response = await UserMode.HttpDeleteIDAsync(Constants.TrustFrameworkPolicesUri, Constants.TrustFrameworkPolicyByIDUri, policyList.SelectedItem.ToString());
                    string content = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode == true)
                    {
                        HTTPResponse.AppendText("\r\nSuccessfully deleted " + policyList.SelectedItem.ToString() + "\r\n");
                    }
                    else
                    {
                        HTTPResponse.AppendText(content);
                    }

                    //HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                    response = await UserMode.HttpGetAsync(Constants.TrustFrameworkPolicesUri);
                    content = await response.Content.ReadAsStringAsync();
                    //HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode == true)
                    {
                        HTTPResponse.AppendText("\r\nSuccessfully updated policy list.\r\n");
                    }
                    else
                    {
                        HTTPResponse.AppendText(content);
                    }
                    PolicyList pL = JsonConvert.DeserializeObject<PolicyList>(content);
                    UpdatePolicyList(pL);
                }
            }
        }

        private void UpdatePolicyList(PolicyList myPolicies)
        {
            policyList.Items.Clear();
            foreach (Value policyValue in myPolicies.Value)
            {
                policyList.Items.Add(policyValue.Id);
            }
        }

        private async void DeleteAllBtn_Click(object sender, EventArgs e)
        {
            string token = await AuthenticationHelper.GetTokenForUserAsync();
            if (token != null)
            {
                HTTPResponse.AppendText("\r\nDeleting all custom polices from B2C.\r\n");
                HttpResponseMessage response = null;
                response = await UserMode.HttpGetAsync(Constants.TrustFrameworkPolicesUri);
                HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                string content = await response.Content.ReadAsStringAsync();
                PolicyList pL = JsonConvert.DeserializeObject<PolicyList>(content);
                foreach (Value policyValue in pL.Value)
                {
                    response = await UserMode.HttpDeleteIDAsync(Constants.TrustFrameworkPolicesUri, Constants.TrustFrameworkPolicyByIDUri, policyValue.Id);
                    string  log = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode == true)
                    {
                        HTTPResponse.AppendText("\r\nSuccessfully deleted " + policyValue.ToString() + "\r\n");
                    }
                    else
                    {
                        HTTPResponse.AppendText(log);
                    }

                }

                response = null;
                response = await UserMode.HttpGetAsync(Constants.TrustFrameworkPolicesUri);
                //HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                content = await response.Content.ReadAsStringAsync();
                pL = JsonConvert.DeserializeObject<PolicyList>(content);
                UpdatePolicyList(pL);
            }
        }

        private async void UploadFolderBtn_Click(object sender, EventArgs e)
        {
            if (policyFolderLbl.Text != "No Folder selected.")
            {
                HTTPResponse.AppendText("\r\nUploading selected policies\r\n");
                string token = await AuthenticationHelper.GetTokenForUserAsync();
                if (token != null)
                {
                    HttpResponseMessage response = null;
                    string[] fileEntries = checkedPolicyList.CheckedItems.OfType<string>().ToArray();
                    List<string> fileList = new List<string>(fileEntries);

                    //if we found ext file, move to top
                    var regexExtensions = @"\w*Extensions\w*";
                    var indexExtensions = -1;
                    indexExtensions = fileList.FindIndex(d => regexExtensions.Any(s => Regex.IsMatch(d.ToString(), regexExtensions)));
                    if (indexExtensions > -1)
                    {
                        fileList.Insert(0, fileList[indexExtensions]);
                        fileList.RemoveAt(indexExtensions + 1);
                    }

                    //if we found base file, move to top
                    var regexBase = @"\w*Base\w*";
                    var indexBase = -1;
                    indexBase = fileList.FindIndex(d => regexBase.Any(s => Regex.IsMatch(d.ToString(), regexBase)));
                    if (indexBase > -1)
                    {
                        fileList.Insert(0, fileList[indexBase]);
                        fileList.RemoveAt(indexBase + 1);
                    }

                    foreach (string file in fileList)
                    {
                        HTTPResponse.AppendText(String.Format("Uploading: {0}", file));
                    }
                    foreach (string file in fileList)
                    {
                        string xml = File.ReadAllText(policyFolderLbl.Text + @"\" + file);
                        response = await UserMode.HttpPostAsync(Constants.TrustFrameworkPolicesUri, xml);
                        if (response.IsSuccessStatusCode == false)
                        {
                            HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                        }
                        else
                        {
                            HTTPResponse.AppendText("\r\nSuccefully uploaded " + file +"\r\n");
                        }
                    }

                    response = await UserMode.HttpGetAsync(Constants.TrustFrameworkPolicesUri);
                    string content = await response.Content.ReadAsStringAsync();
                    //HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode == true)
                    {
                        HTTPResponse.AppendText("\r\nSuccessfully updated policy list.\r\n");
                    }
                    else
                    {
                        HTTPResponse.AppendText(content);
                    }

                    PolicyList pL = JsonConvert.DeserializeObject<PolicyList>(content);

                    UpdatePolicyList(pL);
                }
            }
        }

        private async void UpdateAllPolicesBtn_Click(object sender, EventArgs e)
        {

            if (policyFolderLbl.Text != "No Folder selected.")
            {
                HTTPResponse.AppendText("\r\nUpdating selected policies.\r\n");
                string token = await AuthenticationHelper.GetTokenForUserAsync();
                if (token != null)
                { 
                    HttpResponseMessage response = null;
                    string[] fileEntries = checkedPolicyList.CheckedItems.OfType<string>().ToArray();
                    List<string> fileList = new List<string>(fileEntries);

                    //if we found ext file, move to top
                    var regexExtensions = @"\w*Extensions\w*";
                    var indexExtensions = -1;
                    indexExtensions = fileList.FindIndex(d => regexExtensions.Any(s => Regex.IsMatch(d.ToString(), regexExtensions)));
                    if (indexExtensions > -1)
                    {
                        fileList.Insert(0, fileList[indexExtensions]);
                        fileList.RemoveAt(indexExtensions + 1);
                    }

                    //if we found base file, move to top
                    var regexBase = @"\w*Base\w*";
                    var indexBase = -1;
                    indexBase = fileList.FindIndex(d => regexBase.Any(s => Regex.IsMatch(d.ToString(), regexBase)));
                    if (indexBase > -1)
                    {
                        fileList.Insert(0, fileList[indexBase]);
                        fileList.RemoveAt(indexBase + 1);
                    }

                    foreach (string file in fileList)
                    {
                        string xml = File.ReadAllText(policyFolderLbl.Text + @"\" + file);
                        string fileName = file.Split('.')[0];
                        response = await UserMode.HttpPutIDAsync(Constants.TrustFrameworkPolicyByIDUriPUT, @"B2C_1A_" + fileName, xml);
                        if (response.IsSuccessStatusCode == false)
                        {
                            HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                        }
                        else
                        {
                            HTTPResponse.AppendText("\r\nSuccefully updated " + file + "\r\n");
                        }
                    }

                    response = await UserMode.HttpGetAsync(Constants.TrustFrameworkPolicesUri);
                    string content = await response.Content.ReadAsStringAsync();
                    //HTTPResponse.AppendText(await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode == true)
                    {
                        HTTPResponse.AppendText("\r\nSuccessfully updated policy list.\r\n");
                    }
                    else
                    {
                        HTTPResponse.AppendText(content);
                    }
                    
                    PolicyList pL = JsonConvert.DeserializeObject<PolicyList>(content);

                    UpdatePolicyList(pL);
                }
            }

        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void RefrshFileListBtn_Click(object sender, EventArgs e)
        {
            if (policyFolderLbl.Text != "No Folder selected.")
            {
                checkedPolicyList.Items.Clear();
                var allFilenames = Directory.EnumerateFiles(policyFolderLbl.Text).Select(p => Path.GetFileName(p));

                // Get all filenames that have a .txt extension, excluding the extension
                var fileEntries = allFilenames.Where(fn => Path.GetExtension(fn) == ".xml")
                                             .Select(fn => Path.GetFileName(fn))
                                             .ToArray();
                foreach (string file in fileEntries)
                {
                    checkedPolicyList.Items.Add(file);
                }
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (RunNowtxt.Text != "")
            {
                Clipboard.SetText(RunNowtxt.Text);
            }
        }

        private void AppIdtxt_TextChanged_1(object sender, EventArgs e)
        {
            //RunNowtxt.Text = ((TextBox)sender).Text;
            if (policyList.SelectedItem != null)
            {
                RunNowtxt.Text = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize?p={1}&client_id={2}&nonce=defaultNonce&redirect_uri={3}&scope=openid&response_type=id_token&prompt=login", tenantTxt.Text, policyList.SelectedItem.ToString(), AppIdtxt.Text, ReplyUrltxt.Text);
            }
            
            Properties.Settings.Default.B2CAppId = AppIdtxt.Text;
            Properties.Settings.Default.Save();
        }

        private void ReplyUrltxt_TextChanged(object sender, EventArgs e)
        {
            if (policyList.SelectedItem != null)
            {
                RunNowtxt.Text = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize?p={1}&client_id={2}&nonce=defaultNonce&redirect_uri={3}&scope=openid&response_type=id_token&prompt=login", tenantTxt.Text, policyList.SelectedItem.ToString(), AppIdtxt.Text, ReplyUrltxt.Text);
            }
            Properties.Settings.Default.ReplyUrl = ReplyUrltxt.Text;
            Properties.Settings.Default.Save();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (policyList.SelectedItem != null)
            {
                RunNowtxt.Text = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize?p={1}&client_id={2}&nonce=defaultNonce&redirect_uri={3}&scope=openid&response_type=id_token&prompt=login", tenantTxt.Text, policyList.SelectedItem.ToString(), AppIdtxt.Text, ReplyUrltxt.Text);
            }
            Properties.Settings.Default.TenantId = tenantTxt.Text;
            Properties.Settings.Default.Save();
        }

        private void RunNowtxt_TextChanged(object sender, EventArgs e)
        {

        }

        private void policyList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (policyList.SelectedItem != null)
            {
                if (tenantTxt.Text != null && tenantTxt.Text != "")
                {
                    Regex regex = new Regex(@"\w*");
                    Match match = regex.Match(tenantTxt.Text);
                    

                    RunNowtxt.Text = string.Format("https://{0}.b2clogin.com/{1}/oauth2/v2.0/authorize?p={2}&client_id={3}&nonce=defaultNonce&redirect_uri={4}&scope=openid&response_type=id_token&prompt=login", match.Value, tenantTxt.Text, policyList.SelectedItem.ToString(), AppIdtxt.Text, ReplyUrltxt.Text);

                }
                else
                {
                    RunNowtxt.Text = string.Format("https://login.microsoftonline.com/{0}/oauth2/v2.0/authorize?p={1}&client_id={2}&nonce=defaultNonce&redirect_uri={3}&scope=openid&response_type=id_token&prompt=login", tenantTxt.Text, policyList.SelectedItem.ToString(), AppIdtxt.Text, ReplyUrltxt.Text);
                }
            }
        }

        private void B2CPolicyManager_Load(object sender, EventArgs e)
        {
            this.AppIdtxt.Text = Properties.Settings.Default.B2CAppId;
            this.tenantTxt.Text = Properties.Settings.Default.TenantId;
            this.v2AppIDGraphtxt.Text = Properties.Settings.Default.V2AppId;
            this.ReplyUrltxt.Text = Properties.Settings.Default.ReplyUrl;
            this.policyFolderLbl.Text = Properties.Settings.Default.Folder;

            if (this.policyFolderLbl.Text != "No Folder selected.")
            {
                var allFilenames = Directory.EnumerateFiles(policyFolderLbl.Text).Select(p => Path.GetFileName(p));

                // Get all filenames that have a .txt extension, excluding the extension
                var fileEntries = allFilenames.Where(fn => Path.GetExtension(fn) == ".xml")
                                             .Select(fn => Path.GetFileName(fn))
                                             .ToArray();
                foreach (string file in fileEntries)
                {
                    checkedPolicyList.Items.Add(file);
                    //dd
                }
            }

        }

        private void v2AppIDGraphtxt_TextChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.V2AppId = v2AppIDGraphtxt.Text;
            Properties.Settings.Default.Save();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            HTTPResponse.Text = "";
        }

        private void VSCodeBtn_Click(object sender, EventArgs e)
        {
            if (this.policyFolderLbl.Text != "No Folder selected.")
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = string.Format("code"),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    Arguments = string.Format("\"{0}\"", policyFolderLbl.Text)
                };
                Process.Start(startInfo);
            }
        }

        private void EdgeBtn_Click(object sender, EventArgs e)
        {
            string command = String.Format("start chrome --incognito --new-window \"{0}\"", RunNowtxt.Text);
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c " + command)
            {
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(startInfo);
        }

        private void IEBtn_Click(object sender, EventArgs e)
        {
            string command = String.Format("start iexplore \"{0}\" -private", RunNowtxt.Text);
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c " + command)
            {
                WindowStyle = ProcessWindowStyle.Hidden
            };
            Process.Start(startInfo);
        }

        private void button3_Click(object sender, EventArgs e)
        {
        }

        private void button4_Click(object sender, EventArgs e)
        {
        }

        private void button5_Click(object sender, EventArgs e)
        {
        }

        private void button6_Click(object sender, EventArgs e)
        {
            int elementCount = checkedPolicyList.Items.Count;
            for (int i = 0; i < elementCount; i++)
            {
                checkedPolicyList.SetItemChecked(i, true);
            }
        }

        private void deselectAllPolicies_Click(object sender, EventArgs e)
        {
            int elementCount = checkedPolicyList.Items.Count;
            for (int i = 0; i < elementCount; i++)
            {
                checkedPolicyList.SetItemChecked(i, false);
            }
        }

        private void button6_Click_1(object sender, EventArgs e)
        {
            Regex regex = new Regex(@"\w*");
            Match match = regex.Match(tenantTxt.Text);
            if (policyList.SelectedItem != null) {
                string url = "https://b2csamlrp.azurewebsites.net/SP?" + "tenant=" + match.Value + "&policy=" + policyList.SelectedItem.ToString();
                string command = String.Format("start chrome --incognito --new-window \"{0}\"", url);
                ProcessStartInfo startInfo = new ProcessStartInfo("cmd", "/c " + command)
                {
                    WindowStyle = ProcessWindowStyle.Hidden
                };
                Process.Start(startInfo);
            }

        }

        private void button7_Click(object sender, EventArgs e)
        {
        }
    }
}
