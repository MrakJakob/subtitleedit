using Newtonsoft.Json;
using Nikse.SubtitleEdit.Controls;
using Nikse.SubtitleEdit.Controls.Interfaces;
using Nikse.SubtitleEdit.Core.Common;
using Nikse.SubtitleEdit.Logic;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;
using MessageBox = Nikse.SubtitleEdit.Forms.SeMsgBox.MessageBox;
using Nikse.SubtitleEdit.Core.Forms;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class TrueBarAudioToText : Form
    {
        private readonly string _videoFileName;
        private readonly Subtitle _subtitle;
        private readonly bool _isVideo;
        private readonly bool _isAudio;
        private readonly int _audioTrackNumber;
        private readonly Form _parentForm;
        private string _accessToken;
        private string _sessionId;
        private Timer _statusCheckTimer;
        private readonly TrueBarAPI _trueBarAPI;
        private readonly SubtitleListView _subtitleListView1;
        private readonly FixDurationLimits _fixDurationLimits;
        private Button generate;
        private Label label1;
        private Button LoginButton;
        private Label label2;
        private Label label3;
        private TextBox username;
        private TextBox password;
        private LinkLabel linkLabel1;
        private ProgressBar progressBar1;
        private Label label4;
        private Label label5;
        private Button cancel;

        public TrueBarAudioToText(string videoFileName, Subtitle subtitle, int audioTrackNumber, Form parentForm, TrueBarAPI trueBarAPI)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, generate);
            _videoFileName = videoFileName;
            _subtitle = subtitle;
            _audioTrackNumber = audioTrackNumber;
            _parentForm = parentForm;
            _trueBarAPI = trueBarAPI;
            _statusCheckTimer = new Timer();
            _statusCheckTimer.Interval = 1000;
            _statusCheckTimer.Tick += StatusCheckTimer_Tick;
            _subtitleListView1 = new SubtitleListView();
            _fixDurationLimits = new FixDurationLimits(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, new List<double>());

        }

        public void InitializeComponent()
        {
            this.cancel = new System.Windows.Forms.Button();
            this.generate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.LoginButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.username = new System.Windows.Forms.TextBox();
            this.password = new System.Windows.Forms.TextBox();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.Location = new System.Drawing.Point(381, 237);
            this.cancel.Name = "cancel";
            this.cancel.Size = new System.Drawing.Size(134, 26);
            this.cancel.TabIndex = 0;
            this.cancel.Text = "Cancel";
            this.cancel.UseVisualStyleBackColor = true;
            this.cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // generate
            // 
            this.generate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.generate.Location = new System.Drawing.Point(241, 237);
            this.generate.Name = "generate";
            this.generate.Size = new System.Drawing.Size(134, 26);
            this.generate.TabIndex = 1;
            this.generate.Text = "Generate";
            this.generate.UseVisualStyleBackColor = true;
            this.generate.Click += new System.EventHandler(this.generate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(276, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Generate text from audio via True-bar speech recognition";
            // 
            // LoginButton
            // 
            this.LoginButton.Location = new System.Drawing.Point(281, 137);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(134, 26);
            this.LoginButton.TabIndex = 9;
            this.LoginButton.Text = "Login";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 125);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(145, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Password";
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(15, 141);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(127, 20);
            this.username.TabIndex = 12;
            this.username.Text = "jakob.mrak";
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(148, 141);
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(127, 20);
            this.password.TabIndex = 13;
            this.password.Text = "78Yio$3rt";
            this.password.UseSystemPasswordChar = true;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Location = new System.Drawing.Point(12, 64);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(86, 13);
            this.linkLabel1.TabIndex = 14;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "True-bar website";
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
            // 
            // progressBar1
            // 
            this.progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.progressBar1.Location = new System.Drawing.Point(15, 249);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(177, 14);
            this.progressBar1.TabIndex = 15;
            this.progressBar1.Visible = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 233);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(156, 233);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 13);
            this.label5.TabIndex = 17;
            // 
            // TrueBarAudioToText
            // 
            this.ClientSize = new System.Drawing.Size(527, 275);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.password);
            this.Controls.Add(this.username);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.generate);
            this.Controls.Add(this.cancel);
            this.MaximumSize = new System.Drawing.Size(543, 314);
            this.MinimumSize = new System.Drawing.Size(543, 314);
            this.Name = "TrueBarAudioToText";
            this.ShowIcon = false;
            this.Text = "Audio To Text";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private int GetProgress(string statusResponse)
        {
            // Parse the response to determine the progress
            var status = JsonConvert.DeserializeObject<dynamic>(statusResponse);

            if (status.status == "UPLOADING")
            {
                return -1;
            }
            var recordedSeconds = status?.recordedSeconds.Value;
            var processedSeconds = status?.processedSeconds.Value;
            if (recordedSeconds == null || processedSeconds == null)
            {
                return 0;
            }
            return (int)(100 * processedSeconds / recordedSeconds);
        }

        private async void StatusCheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Call the API to check the transcription status
                var statusResponse = await _trueBarAPI.CheckSessionStatusAsync(_sessionId, _accessToken);

                int progress = GetProgress(statusResponse);

                if (progress == -1)
                {
                    // Animate the progress bar during uploading
                    label4.Text = "Uploading...";
                    progressBar1.Style = ProgressBarStyle.Marquee;
                }
                else
                {
                    label4.Text = "Transcription progress...";
                    progressBar1.Style = ProgressBarStyle.Continuous;
                    progressBar1.Value = progress;
                    label5.Text = progress + " %";
                }


                // Parse the response to check if the transcription is complete
                if (IsTranscriptionComplete(statusResponse))
                {
                    _statusCheckTimer.Stop(); // Stop the timer
                    await RetrieveTranscriptionAsync(); // Retrieve the transcription
                }
            }
            catch (Exception ex)
            {
                _statusCheckTimer.Stop(); // Stop the timer on error
                MessageBox.Show($"An error occurred while checking the status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool IsTranscriptionComplete(string statusResponse)
        {
            var status = JsonConvert.DeserializeObject<dynamic>(statusResponse);
            return status?.status == "FINISHED";
        }


        private async Task RetrieveTranscriptionAsync()
        {
            try
            {
                // Call the API to retrieve the transcription
                var transcription = await _trueBarAPI.GetSessionTranscriptAsync(_sessionId, _accessToken);

                // Display or process the transcription
                MessageBox.Show("Transcription completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTranscriptionIntoSubtitle(transcription);
                this.DialogResult = DialogResult.OK; // Close the modal with OK result
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred while retrieving the transcription: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                generate.Enabled = true; // Re-enable the generate button
            }
        }

        public void LoadTranscriptionIntoSubtitle(string transcriptionJson)
        {
            _subtitle.Paragraphs.Clear(); // Clear the existing paragraphs
            try
            {
                // Parse the JSON
                List<SubtitleItem> transcriptionSegments = JsonConvert.DeserializeObject<List<SubtitleItem>>(transcriptionJson);

                if (transcriptionSegments == null)
                {
                    throw new InvalidOperationException("Invalid transcription format.");
                }

                foreach (var transcriptionSegment in transcriptionSegments)
                {
                    transcriptionSegment.content = transcriptionSegment.content.Replace("\\\"", "\""); // Fix escaped quotes
                    List<SubtitleDetails> subtitles = JsonConvert.DeserializeObject<List<SubtitleDetails>>(transcriptionSegment.content);

                    foreach (var subtitle in subtitles)
                    {
                        var paragraph = new Paragraph
                        {
                            StartTime = new TimeCode(subtitle.startTime * 1000), // Convert seconds to milliseconds
                            EndTime = new TimeCode(subtitle.endTime * 1000),     // Convert seconds to milliseconds
                            Text = subtitle.text
                        };
                        _subtitle.Paragraphs.Add(paragraph);
                    }
                }
                // Merge lines with same time codes
                var mergedSubtitle = MergeLinesWithSameTimeCodes.Merge(_subtitle, new List<int>(), out _, true, false, true, 1000, "en", new List<int>(), new Dictionary<int, bool>(), new Subtitle());
                // Fix long and short display times
                var fixedAndMergedSubtitle = _fixDurationLimits.Fix(mergedSubtitle);
                _subtitle.Paragraphs.Clear();
                _subtitle.Paragraphs.AddRange(fixedAndMergedSubtitle.Paragraphs);
            }
            catch (Exception ex)
            {
                // Handle parsing errors
                throw new InvalidOperationException("Failed to load transcription into subtitle.", ex);
            }
        }

        // Closing the form when transcription in progress
        private void CancelTranscriptionDialog()
        {
            var result = MessageBox.Show("Are you sure you want to cancel the transcription?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _statusCheckTimer.Stop(); // Stop the timer
                progressBar1.Visible = false; // Hide the progress bar
                progressBar1.Text = "Transcription canceled.";
                this.DialogResult = DialogResult.Cancel; // Close the modal with Cancel result
                this.Close();
            }
        }


        private void Cancel_Click(object sender, EventArgs e)
        {
            if (_statusCheckTimer.Enabled)
            {
                CancelTranscriptionDialog();
            }
            else
            {
                this.DialogResult = DialogResult.Cancel; // Close the modal with Cancel result
                this.Close();
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_statusCheckTimer.Enabled)
            {
                // Display warning when closing the form while transcription in progress
                CancelTranscriptionDialog();
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        private async void generate_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                MessageBox.Show("Please login first!");
                return;
            }

            if (string.IsNullOrWhiteSpace(_videoFileName))
            {
                MessageBox.Show("Please select a video file first!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!await _trueBarAPI.IsApiServerReachableAsync()){
                MessageBox.Show("No internet connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            // Disable the generate button while the request is in progress
            generate.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
         
            // Call the True-bar API to generate text from audio
            string response = await _trueBarAPI.UploadFileAsync(_accessToken, _videoFileName);

            try
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                // MessageBox.Show("Response: " + response);   
                if (jsonResponse.ContainsKey("sessionId"))
                {
                    // Store the session ID securely 
                    _sessionId = jsonResponse["sessionId"];
                    _statusCheckTimer.Start();
                    // transcription is in progress   
                }
                else if (jsonResponse.ContainsKey("error_description"))
                {
                    MessageBox.Show("Upload failed: " + jsonResponse["error_description"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    generate.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Unexpected response: " + response, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    generate.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing response: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Text))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            if (!await _trueBarAPI.IsApiServerReachableAsync()){
                MessageBox.Show("No internet connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // disable the login button while the request is in progress
            LoginButton.Enabled = false;
            // TODO: handle session expiration

            string response = await _trueBarAPI.Login(username.Text, password.Text);

            try
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

                if (jsonResponse.ContainsKey("access_token"))
                {
                    string accessToken = jsonResponse["access_token"];
                    MessageBox.Show("Login successful!");
                    // Put focus on the generate button
                    generate.Focus();
                    // Store the access token securely (e.g., in-memory or a secure storage)
                    _accessToken = accessToken;
                }
                else if (jsonResponse.ContainsKey("error_description"))
                {
                    MessageBox.Show("Login failed: " + jsonResponse["error_description"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    LoginButton.Enabled = true;
                }
                else
                {
                    MessageBox.Show("Unexpected response: " + response, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    LoginButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing response: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // open the True-bar website
            UiUtil.OpenUrl("https://demo.true-bar.si/");
        }
    }
}


public class SubtitleItem
{
    public int id { get; set; }
    public string content { get; set; }
}

public class SubtitleDetails
{
    public string text { get; set; }
    public double startTime { get; set; }
    public double endTime { get; set; }
    public bool spaceBefore { get; set; }
    public string speakerCode { get; set; }
    public Metadata metadata { get; set; }
}

public class Metadata
{
    public bool? postCapitalized { get; set; }
    public string source { get; set; }
}