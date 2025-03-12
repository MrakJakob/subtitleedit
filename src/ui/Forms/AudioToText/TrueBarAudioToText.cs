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
using Nikse.SubtitleEdit.Core.AudioToText;
using System.Linq;
using Nikse.SubtitleEdit.Core.SubtitleFormats;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Nikse.SubtitleEdit.Forms.AudioToText
{
    public sealed partial class TrueBarAudioToText : Form
    {
        private readonly string _videoFileName;
        private string _audioFileName;
        private readonly Subtitle _subtitle;
        private readonly bool _isVideo;
        private readonly bool _isAudio;
        private readonly int _audioTrackNumber;
        private readonly Form _parentForm;
        private string _accessToken;
        private string _sessionId;
        private string _jobId;
        private readonly Timer _statusCheckTimer;
        private readonly TrueBarAPI _trueBarAPI;
        private readonly TrueBarSubtitlerAPI _trueBarSubtitlerAPI;
        private readonly SubtitleListView _subtitleListView1;
        private readonly FixDurationLimits _fixDurationLimits;
        bool _processing = false;
        private readonly List<string> _filesToDelete;
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
        private Label label6;
        private CheckBox checkBox1;
        private CheckBox checkBox2;
        private CheckBox checkBox3;
        private CheckBox checkBox4;
        private ComboBox comboBox1;
        private CheckBox checkBox5;
        private Button cancel;

        public TrueBarAudioToText(string videoFileName, Subtitle subtitle, int audioTrackNumber, Form parentForm, TrueBarSubtitlerAPI trueBarSubtitlerAPI)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, generate);
            _videoFileName = videoFileName;
            _subtitle = subtitle;
            _audioTrackNumber = audioTrackNumber;
            _parentForm = parentForm;
            _trueBarSubtitlerAPI = trueBarSubtitlerAPI;
            _statusCheckTimer = new Timer
            {
                Interval = 1000
            };
            _statusCheckTimer.Tick += StatusCheckTimer_Tick;
            
            _filesToDelete = new List<string>();
            username.TabIndex = 0;
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
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cancel
            // 
            this.cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel.Location = new System.Drawing.Point(381, 323);
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
            this.generate.Location = new System.Drawing.Point(241, 323);
            this.generate.Name = "generate";
            this.generate.Size = new System.Drawing.Size(134, 26);
            this.generate.TabIndex = 1;
            this.generate.Text = "Generate";
            this.generate.UseVisualStyleBackColor = true;
            this.generate.Click += new System.EventHandler(this.Generate_Click);
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
            this.LoginButton.Location = new System.Drawing.Point(381, 137);
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
            this.label2.Location = new System.Drawing.Point(12, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(55, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Username";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(185, 116);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Password";
            // 
            // username
            // 
            this.username.Location = new System.Drawing.Point(15, 141);
            this.username.Name = "username";
            this.username.Size = new System.Drawing.Size(155, 20);
            this.username.TabIndex = 12;
            // 
            // password
            // 
            this.password.Location = new System.Drawing.Point(188, 141);
            this.password.Name = "password";
            this.password.PasswordChar = '*';
            this.password.Size = new System.Drawing.Size(155, 20);
            this.password.TabIndex = 13;
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
            this.progressBar1.Location = new System.Drawing.Point(15, 335);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(206, 14);
            this.progressBar1.TabIndex = 15;
            this.progressBar1.Visible = false;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 319);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(0, 13);
            this.label4.TabIndex = 16;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(185, 319);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(0, 13);
            this.label5.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 180);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(45, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Settings";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(15, 206);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(139, 17);
            this.checkBox1.TabIndex = 20;
            this.checkBox1.Text = "Voice Activity Detection";
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Location = new System.Drawing.Point(15, 229);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(83, 17);
            this.checkBox2.TabIndex = 21;
            this.checkBox2.Text = "Punctuation";
            this.checkBox2.UseVisualStyleBackColor = true;
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Location = new System.Drawing.Point(15, 252);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(101, 17);
            this.checkBox3.TabIndex = 22;
            this.checkBox3.Text = "Denormalization";
            this.checkBox3.UseVisualStyleBackColor = true;
            // 
            // checkBox4
            // 
            this.checkBox4.AutoSize = true;
            this.checkBox4.Checked = true;
            this.checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox4.Location = new System.Drawing.Point(15, 275);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(155, 17);
            this.checkBox4.TabIndex = 23;
            this.checkBox4.Text = "Speaker Change Detection";
            this.checkBox4.UseVisualStyleBackColor = true;
            // 
            // comboBox1
            // 
            this.comboBox1.Enabled = false;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(209, 204);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(134, 21);
            this.comboBox1.TabIndex = 24;
            this.comboBox1.Text = "Slovenščina (sl-SI)";
            // 
            // checkBox5
            // 
            this.checkBox5.AutoSize = true;
            this.checkBox5.Enabled = false;
            this.checkBox5.Location = new System.Drawing.Point(381, 206);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(119, 17);
            this.checkBox5.TabIndex = 25;
            this.checkBox5.Text = "Translate to English";
            this.checkBox5.UseVisualStyleBackColor = true;
            // 
            // TrueBarAudioToText
            // 
            this.ClientSize = new System.Drawing.Size(527, 361);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label6);
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
            this.MaximumSize = new System.Drawing.Size(543, 400);
            this.MinimumSize = new System.Drawing.Size(543, 400);
            this.Name = "TrueBarAudioToText";
            this.ShowIcon = false;
            this.Text = "Audio To Text";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // THIS CODE IS REUSED FROM THE WHISPER AUDIO TO TEXT FORM (START)
        private Process GetFfmpegProcess(string videoFileName, int audioTrackNumber, string outWaveFile)
        {
            if (!File.Exists(Configuration.Settings.General.FFmpegLocation) && Configuration.IsRunningOnWindows)
            {
                return null;
            }

            var audioParameter = string.Empty;
            if (audioTrackNumber > 0)
            {
                audioParameter = $"-map 0:a:{audioTrackNumber}";
            }

            // TODO: handle center channel only
            // labelFC.Text = string.Empty;
            var fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ac 1 -ab 32k -af volume=1.75 -f wav {2} \"{1}\"";
            // if (_useCenterChannelOnly)
            // {
            //     fFmpegWaveTranscodeSettings = "-i \"{0}\" -vn -ar 16000 -ab 32k -af volume=1.75 -af \"pan=mono|c0=FC\" -f wav {2} \"{1}\"";
            //     labelFC.Text = "FC";
            // }

            //-i indicates the input
            //-vn means no video output
            //-ar 44100 indicates the sampling frequency.
            //-ab indicates the bit rate (in this example 160kb/s)
            //-af volume=1.75 will boot volume... 1.0 is normal
            //-ac 2 means 2 channels
            // "-map 0:a:0" is the first audio stream, "-map 0:a:1" is the second audio stream

            var exeFilePath = Configuration.Settings.General.FFmpegLocation;
            if (!Configuration.IsRunningOnWindows)
            {
                exeFilePath = "ffmpeg";
            }

            var parameters = string.Format(fFmpegWaveTranscodeSettings, videoFileName, outWaveFile, audioParameter);
            return new Process
            {
                StartInfo = new ProcessStartInfo(exeFilePath, parameters)
                {
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                }
            };
        }
        private string GenerateWavFile(string videoFileName, int audioTrackNumber)
        {
            if (videoFileName.EndsWith(".wav"))
            {
                try
                {
                    using (var waveFile = new WavePeakGenerator(videoFileName))
                    {
                        if (waveFile.Header != null && waveFile.Header.SampleRate == 16000 && waveFile.Header.NumberOfChannels == 1)
                        {
                            return videoFileName;
                        }
                    }
                }
                catch
                {
                    // ignore
                }
            }

            var ffmpegLog = new StringBuilder();
            var outWaveFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".wav");
            _filesToDelete.Add(outWaveFile);
            var process = GetFfmpegProcess(videoFileName, audioTrackNumber, outWaveFile);

            process.ErrorDataReceived += (sender, args) =>
            {
                ffmpegLog.AppendLine(args.Data);
            };

            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.BeginErrorReadLine();

            double seconds = 0;
            // buttonCancel.Visible = true;
            try
            {
                process.PriorityClass = ProcessPriorityClass.Normal;
            }
            catch
            {
                // ignored
            }

            // _cancel = false;
            string targetDriveLetter = null;
            if (Configuration.IsRunningOnWindows)
            {
                var root = Path.GetPathRoot(outWaveFile);
                if (root.Length > 1 && root[1] == ':')
                {
                    targetDriveLetter = root.Remove(1);
                }
            }

            while (!process.HasExited)
            {
                Application.DoEvents();
                System.Threading.Thread.Sleep(100);
                seconds += 0.1;

                Invalidate();

                if (targetDriveLetter != null && seconds > 1 && Convert.ToInt32(seconds) % 10 == 0)
                {
                    try
                    {
                        var drive = new DriveInfo(targetDriveLetter);
                        if (drive.IsReady)
                        {
                            if (drive.AvailableFreeSpace < 50 * 1000000) // 50 mb
                            {
                                // TODO: handle low disk space
                                // labelInfo.ForeColor = Color.Red;
                                // labelInfo.Text = LanguageSettings.Current.AddWaveform.LowDiskSpace;
                            }
                        }
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            if (!File.Exists(outWaveFile))
            {
                SeLogger.WhisperInfo("Generated wave file not found: " + outWaveFile + Environment.NewLine +
                               "ffmpeg: " + process.StartInfo.FileName + Environment.NewLine +
                               "Parameters: " + process.StartInfo.Arguments + Environment.NewLine +
                               "OS: " + Environment.OSVersion + Environment.NewLine +
                               "64-bit: " + Environment.Is64BitOperatingSystem + Environment.NewLine +
                               "ffmpeg exit code: " + process.ExitCode + Environment.NewLine +
                               "ffmpeg log: " + ffmpegLog);
            }

            return outWaveFile;
        }
        // THIS CODE IS REUSED FROM THE WHISPER AUDIO TO TEXT FORM (END)

        // Login to the True-bar (Subtitler) API
        private async void LoginButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(username.Text) || string.IsNullOrWhiteSpace(password.Text))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            // if (!await _trueBarAPI.IsApiServerReachableAsync())
            // {
            //     MessageBox.Show("No internet connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //     return;
            // }

            // Disable the login button while the request is in progress
            _processing = true;
            LoginButton.Enabled = false;
            // TODO: handle session expiration

            // Call the new Subtitler API to login
            string response = await _trueBarSubtitlerAPI.Login(username.Text, password.Text);

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

        private async void Generate_Click(object sender, EventArgs e)
        {
            // Check if the user is logged in
            if (string.IsNullOrWhiteSpace(_accessToken))
            {
                MessageBox.Show("Please login first!");
                return;
            }

            // Check if the video file is selected
            if (string.IsNullOrWhiteSpace(_videoFileName))
            {
                MessageBox.Show("Please select a video file first!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Check if the API server is reachable 
            // if (!await _trueBarSubtitlerAPI.IsApiServerReachableAsync())
            // {
            //     MessageBox.Show("No internet connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //     return;
            // }

            
            _processing = true;
            // Disable the generate button while the request is in progress
            generate.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            label4.Text = "Generating WAV file...";
            
            // Check if the the uploaded file is .wav, if not, convert it to .wav
            _audioFileName = _videoFileName != null ? GenerateWavFile(_videoFileName, _audioTrackNumber) : null;

            try
            {
                string response = await _trueBarSubtitlerAPI.UploadFileAsync(_accessToken, _audioFileName, checkBox1.Checked, checkBox2.Checked, checkBox3.Checked, checkBox4.Checked);
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

                if (jsonResponse.ContainsKey("job_id"))
                {
                    // Store the session ID in-memory
                    _jobId = jsonResponse["job_id"];
                    label4.Text = "Uploading...";
                    _statusCheckTimer.Start();
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

        public Subtitle LoadWebVttFromJson(string webVTTcontent)
        {
            // Load the WebVTT content into a Subtitle object
            var subtitle = new Subtitle();
            var webVttFormat = new WebVTT();
            var lines = webVTTcontent.SplitToLines();
            webVttFormat.LoadSubtitle(subtitle, lines, null);

            return subtitle;
        }

        private void GetSubtitlesFromResponse(string statusResponse)
        {
            var status = JsonConvert.DeserializeObject<dynamic>(statusResponse);
            var subtitles = status?.webvtt;
            if (subtitles == null)
            {
                MessageBox.Show("No subtitles found in the response", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            List<SubtitleResponse> webvtt = JsonConvert.DeserializeObject<List<SubtitleResponse>>(subtitles.ToString());

            if (webvtt == null || webvtt.Count == 0)
            {
                MessageBox.Show("No subtitle items found in the response", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var webVTTsubtitles = LoadWebVttFromJson(webvtt[0].Content);
            _subtitle.Paragraphs.Clear();
            _subtitle.Paragraphs.AddRange(webVTTsubtitles.Paragraphs);

            MessageBox.Show("Transcription completed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            _processing = false;
            DialogResult = DialogResult.OK; // Close the modal with OK result
            Close();
        }

        private async void StatusCheckTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                // Call the API to check the transcription job status
                var statusResponse = await _trueBarSubtitlerAPI.CheckJobStatus(_accessToken, _jobId);

                // Check if the response is successful
                if (!statusResponse.Contains("status"))
                {
                    // Stop the timer and show an error message
                    _statusCheckTimer.Stop();
                    _processing = false;
                    progressBar1.Visible = false;
                    label4.Text = "Failed";

                    if (string.IsNullOrWhiteSpace(statusResponse))
                    {
                        MessageBox.Show("No response from the server", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (statusResponse.Contains("error"))
                    {
                        MessageBox.Show("An error occurred while checking the status: " + statusResponse, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Unexpected response: " + statusResponse, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                    generate.Enabled = true; // Re-enable the generate button
                    return;
                }

                progressBar1.Style = ProgressBarStyle.Marquee;
                var transcriptionStatus = CheckTranscriptionStatus(statusResponse);

                switch (transcriptionStatus)
                {
                    case "started":
                        // Update the progress bar
                        label4.Text = "Transcribing...";
                        break;
                    case "finished":
                        progressBar1.Style = ProgressBarStyle.Continuous;
                        progressBar1.Value = 100;
                        _processing = false;
                        _statusCheckTimer.Stop();
                        // Retrieve the transcription
                        GetSubtitlesFromResponse(statusResponse);
                        break;
                    case "failed":
                        _statusCheckTimer.Stop(); 
                        _processing = false;
                        progressBar1.Visible = false;
                        MessageBox.Show("Transcription failed!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        generate.Enabled = true;
                        break;
                    default:
                        // Handle other statuses
                        label4.Text = transcriptionStatus;
                        break;
                }

            }
            catch (Exception ex)
            {
                _statusCheckTimer.Stop(); // Stop the timer on error
                _processing = false;
                MessageBox.Show($"An error occurred while checking the status: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string CheckTranscriptionStatus(string statusResponse)
        {
            var status = JsonConvert.DeserializeObject<dynamic>(statusResponse);
            return status.status;
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

        public static void DeleteTemporaryFiles(List<string> filesToDelete)
        {
            foreach (var fileName in filesToDelete)
            {
                try
                {
                    if (File.Exists(fileName))
                    {
                        File.Delete(fileName);
                    }
                }
                catch
                {
                    // ignore
                }
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
                DeleteTemporaryFiles(_filesToDelete);
                base.OnFormClosing(e);
            }
        }

        private void LinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            // open the True-bar website
            UiUtil.OpenUrl("https://vitasis.si/products/truebar");
        }


    }
}

public class SubtitleResponse
{
    public string LanguageCode { get; set; }
    public string Content { get; set; }
}