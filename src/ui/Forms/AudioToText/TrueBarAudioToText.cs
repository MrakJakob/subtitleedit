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
        bool _processing = false;
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
            _statusCheckTimer = new Timer
            {
                Interval = 1000
            };
            _statusCheckTimer.Tick += StatusCheckTimer_Tick;
            _subtitleListView1 = new SubtitleListView();
            _fixDurationLimits = new FixDurationLimits(Configuration.Settings.General.SubtitleMinimumDisplayMilliseconds, Configuration.Settings.General.SubtitleMaximumDisplayMilliseconds, new List<double>());
            username.TabIndex = 0;
        }

        public void InitializeComponent()
        {
            cancel = new Button();
            generate = new Button();
            label1 = new Label();
            LoginButton = new Button();
            label2 = new Label();
            label3 = new Label();
            username = new TextBox();
            password = new TextBox();
            linkLabel1 = new LinkLabel();
            progressBar1 = new ProgressBar();
            label4 = new Label();
            label5 = new Label();
            SuspendLayout();
            // 
            // cancel
            // 
            cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            cancel.Location = new System.Drawing.Point(381, 237);
            cancel.Name = "cancel";
            cancel.Size = new System.Drawing.Size(134, 26);
            cancel.TabIndex = 0;
            cancel.Text = "Cancel";
            cancel.UseVisualStyleBackColor = true;
            cancel.Click += new System.EventHandler(Cancel_Click);
            // 
            // generate
            // 
            generate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            generate.Location = new System.Drawing.Point(241, 237);
            generate.Name = "generate";
            generate.Size = new System.Drawing.Size(134, 26);
            generate.TabIndex = 1;
            generate.Text = "Generate";
            generate.UseVisualStyleBackColor = true;
            generate.Click += new System.EventHandler(Generate_Click);
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(12, 40);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(276, 13);
            label1.TabIndex = 3;
            label1.Text = "Generate text from audio via True-bar speech recognition";
            // 
            // LoginButton
            // 
            LoginButton.Location = new System.Drawing.Point(297, 141);
            LoginButton.Name = "LoginButton";
            LoginButton.Size = new System.Drawing.Size(134, 26);
            LoginButton.TabIndex = 9;
            LoginButton.Text = "Login";
            LoginButton.UseVisualStyleBackColor = true;
            LoginButton.Click += new System.EventHandler(LoginButton_Click);
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new System.Drawing.Point(12, 125);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(55, 13);
            label2.TabIndex = 10;
            label2.Text = "Username";
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(145, 125);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(53, 13);
            label3.TabIndex = 11;
            label3.Text = "Password";
            // 
            // username
            // 
            username.Location = new System.Drawing.Point(15, 141);
            username.Name = "username";
            username.Size = new System.Drawing.Size(127, 20);
            username.TabIndex = 12;
            username.Text = "jakob.mrak";
            // 
            // password
            // 
            password.Location = new System.Drawing.Point(148, 141);
            password.Name = "password";
            password.PasswordChar = '*';
            password.Size = new System.Drawing.Size(127, 20);
            password.TabIndex = 13;
            password.Text = "78Yio$3rt";
            password.UseSystemPasswordChar = true;
            // 
            // linkLabel1
            // 
            linkLabel1.AutoSize = true;
            linkLabel1.Location = new System.Drawing.Point(12, 64);
            linkLabel1.Name = "linkLabel1";
            linkLabel1.Size = new System.Drawing.Size(86, 13);
            linkLabel1.TabIndex = 14;
            linkLabel1.TabStop = true;
            linkLabel1.Text = "True-bar website";
            linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(LinkLabel_LinkClicked);
            // 
            // progressBar1
            // 
            progressBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            progressBar1.Location = new System.Drawing.Point(15, 249);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(206, 14);
            progressBar1.TabIndex = 15;
            progressBar1.Visible = false;
            // 
            // label4
            // 
            label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label4.AutoSize = true;
            label4.Location = new System.Drawing.Point(12, 233);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(0, 13);
            label4.TabIndex = 16;
            // 
            // label5
            // 
            label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(185, 233);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(0, 13);
            label5.TabIndex = 17;
            // 
            // TrueBarAudioToText
            // 
            ClientSize = new System.Drawing.Size(527, 275);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(progressBar1);
            Controls.Add(linkLabel1);
            Controls.Add(password);
            Controls.Add(username);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(LoginButton);
            Controls.Add(label1);
            Controls.Add(generate);
            Controls.Add(cancel);
            MaximumSize = new System.Drawing.Size(543, 314);
            MinimumSize = new System.Drawing.Size(543, 314);
            Name = "TrueBarAudioToText";
            ShowIcon = false;
            Text = "Audio To Text";
            ResumeLayout(false);
            PerformLayout();

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
                _processing = false;
                DialogResult = DialogResult.OK; // Close the modal with OK result
                Close();
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
        private void CancelTranscriptionDialog(FormClosingEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to cancel the transcription process?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                _statusCheckTimer.Stop(); // Stop the timer
                progressBar1.Visible = false; // Hide the progress bar
                progressBar1.Text = "Transcription canceled.";
                _processing = false;

                if (e != null)
                {
                    e.Cancel = false; // Close the form
                }

                DialogResult = DialogResult.Yes; // Close the modal with Cancel result
                Close();
            }
        }


        private void Cancel_Click(object sender, EventArgs e)
        {
            if (_statusCheckTimer.Enabled || _processing)
            {
                CancelTranscriptionDialog(null);
            }
            else
            {
                DialogResult = DialogResult.Cancel; // Close the modal with Cancel result
                Close();
            }

        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (_statusCheckTimer.Enabled || _processing)
            {
                e.Cancel = true;
                // Display warning when closing the form while transcription in progress
                CancelTranscriptionDialog(e);
            }
            else
            {
                base.OnFormClosing(e);
            }
        }

        private async void Generate_Click(object sender, EventArgs e)
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

            if (!await _trueBarAPI.IsApiServerReachableAsync())
            {
                MessageBox.Show("No internet connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Disable the generate button while the request is in progress
            _processing = true;
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
                else if (jsonResponse.ContainsKey("error"))
                {
                    MessageBox.Show("Upload failed: " + jsonResponse["error"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _processing = false;
                    generate.Enabled = true;
                    progressBar1.Visible = false;
                }
                else
                {
                    MessageBox.Show("Unexpected response: " + response, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    _processing = false;
                    generate.Enabled = true;
                    progressBar1.Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error processing response: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _processing = false;
                generate.Enabled = true;
                progressBar1.Visible = false;
            }
        }

        private async void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Text))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            if (!await _trueBarAPI.IsApiServerReachableAsync())
            {
                MessageBox.Show("No internet connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // disable the login button while the request is in progress
            _processing = true;
            LoginButton.Enabled = false;
            // TODO: handle session expiration

            string response = await _trueBarAPI.Login(username.Text, password.Text);

            try
            {
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);
                _processing = false;

                if (jsonResponse.ContainsKey("access_token"))
                {
                    string accessToken = jsonResponse["access_token"];
                    MessageBox.Show("Login successful!");
                    // Put focus on the generate button
                    generate.Focus();
                    // Store the access token securely (e.g., in-memory or a secure storage)
                    _accessToken = accessToken;
                }
                else if (jsonResponse.ContainsKey("error"))
                {
                    MessageBox.Show("Login failed: " + jsonResponse["error"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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