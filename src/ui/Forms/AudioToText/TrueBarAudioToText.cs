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
        private string _clientID;
        private string _clientSecret;
        private string _jobId;
        private readonly Timer _statusCheckTimer;
        private readonly TrueBarSubtitlerAPI _trueBarSubtitlerAPI;
        bool _processing = false;
        private readonly List<string> _filesToDelete;
        bool _isUserLoggedIn = false;
        private Button generateButton;
        private Label infoLabel;
        private Button LoginButton;
        private Label clientIdLabel;
        private Label clientSecretLabel;
        private TextBox clientIdTextBox;
        private TextBox clientSecretTextBox;
        private LinkLabel trueBarLinkLabel;
        private ProgressBar progressBar1;
        private Label label4;
        private Label label5;
        private Label settingsLabel;
        private CheckBox voiceActivityDetectionCheckBox;
        private CheckBox punctuationCheckBox;
        private CheckBox denormalizationCheckBox;
        private CheckBox speakerChangeCheckBox;
        private ComboBox languageComboBox;
        private CheckBox translateToEnglishCheckBox;
        private Label languageLabel;
        private CheckBox rememberMeCheckBox;
        private Button cancelButton;

        public TrueBarAudioToText(string videoFileName, Subtitle subtitle, int audioTrackNumber, Form parentForm, TrueBarSubtitlerAPI trueBarSubtitlerAPI)
        {
            UiUtil.PreInitialize(this);
            InitializeComponent();
            UiUtil.FixFonts(this);
            UiUtil.FixLargeFonts(this, generateButton);
            _videoFileName = videoFileName;
            _subtitle = subtitle;
            _audioTrackNumber = audioTrackNumber;
            _parentForm = parentForm;
            _trueBarSubtitlerAPI = trueBarSubtitlerAPI;
            _statusCheckTimer = new Timer
            {
                Interval = 5000
            };
            _statusCheckTimer.Tick += StatusCheckTimer_Tick;

            _filesToDelete = new List<string>();
            CheckIfAuthenticated();
            
            clientIdTextBox.KeyDown += TextBox_KeyDown;
            clientSecretTextBox.KeyDown += TextBox_KeyDown;
        }


        public void InitializeComponent()
        {
            this.cancelButton = new System.Windows.Forms.Button();
            this.generateButton = new System.Windows.Forms.Button();
            this.infoLabel = new System.Windows.Forms.Label();
            this.LoginButton = new System.Windows.Forms.Button();
            this.clientIdLabel = new System.Windows.Forms.Label();
            this.clientSecretLabel = new System.Windows.Forms.Label();
            this.clientIdTextBox = new System.Windows.Forms.TextBox();
            this.clientSecretTextBox = new System.Windows.Forms.TextBox();
            this.trueBarLinkLabel = new System.Windows.Forms.LinkLabel();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.settingsLabel = new System.Windows.Forms.Label();
            this.voiceActivityDetectionCheckBox = new System.Windows.Forms.CheckBox();
            this.punctuationCheckBox = new System.Windows.Forms.CheckBox();
            this.denormalizationCheckBox = new System.Windows.Forms.CheckBox();
            this.speakerChangeCheckBox = new System.Windows.Forms.CheckBox();
            this.languageComboBox = new System.Windows.Forms.ComboBox();
            this.translateToEnglishCheckBox = new System.Windows.Forms.CheckBox();
            this.languageLabel = new System.Windows.Forms.Label();
            this.rememberMeCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(381, 323);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(134, 26);
            this.cancelButton.TabIndex = 20;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // generateButton
            // 
            this.generateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.generateButton.Location = new System.Drawing.Point(241, 323);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(134, 26);
            this.generateButton.TabIndex = 3;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.Generate_Click);
            // 
            // infoLabel
            // 
            this.infoLabel.AutoSize = true;
            this.infoLabel.Location = new System.Drawing.Point(12, 40);
            this.infoLabel.Name = "infoLabel";
            this.infoLabel.Size = new System.Drawing.Size(276, 13);
            this.infoLabel.TabIndex = 3;
            this.infoLabel.Text = "Generate text from audio via True-bar speech recognition";
            // 
            // LoginButton
            // 
            this.LoginButton.Location = new System.Drawing.Point(381, 130);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(134, 26);
            this.LoginButton.TabIndex = 2;
            this.LoginButton.Text = "Login";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_Click);
            // 
            // clientIdLabel
            // 
            this.clientIdLabel.AutoSize = true;
            this.clientIdLabel.Location = new System.Drawing.Point(12, 111);
            this.clientIdLabel.Name = "clientIdLabel";
            this.clientIdLabel.Size = new System.Drawing.Size(44, 13);
            this.clientIdLabel.TabIndex = 10;
            this.clientIdLabel.Text = "ClientID";
            // 
            // clientSecretLabel
            // 
            this.clientSecretLabel.AutoSize = true;
            this.clientSecretLabel.Location = new System.Drawing.Point(185, 111);
            this.clientSecretLabel.Name = "clientSecretLabel";
            this.clientSecretLabel.Size = new System.Drawing.Size(64, 13);
            this.clientSecretLabel.TabIndex = 11;
            this.clientSecretLabel.Text = "ClientSecret";
            // 
            // clientIdTextBox
            // 
            this.clientIdTextBox.Location = new System.Drawing.Point(15, 136);
            this.clientIdTextBox.Name = "clientIdTextBox";
            this.clientIdTextBox.Size = new System.Drawing.Size(155, 20);
            this.clientIdTextBox.TabIndex = 0;
            // 
            // clientSecretTextBox
            // 
            this.clientSecretTextBox.Location = new System.Drawing.Point(188, 136);
            this.clientSecretTextBox.Name = "clientSecretTextBox";
            this.clientSecretTextBox.PasswordChar = '*';
            this.clientSecretTextBox.Size = new System.Drawing.Size(155, 20);
            this.clientSecretTextBox.TabIndex = 1;
            this.clientSecretTextBox.UseSystemPasswordChar = true;
            // 
            // trueBarLinkLabel
            // 
            this.trueBarLinkLabel.AutoSize = true;
            this.trueBarLinkLabel.Location = new System.Drawing.Point(12, 64);
            this.trueBarLinkLabel.Name = "trueBarLinkLabel";
            this.trueBarLinkLabel.Size = new System.Drawing.Size(86, 13);
            this.trueBarLinkLabel.TabIndex = 14;
            this.trueBarLinkLabel.TabStop = true;
            this.trueBarLinkLabel.Text = "True-bar website";
            this.trueBarLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.LinkLabel_LinkClicked);
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
            // settingsLabel
            // 
            this.settingsLabel.AutoSize = true;
            this.settingsLabel.Location = new System.Drawing.Point(12, 180);
            this.settingsLabel.Name = "settingsLabel";
            this.settingsLabel.Size = new System.Drawing.Size(45, 13);
            this.settingsLabel.TabIndex = 19;
            this.settingsLabel.Text = "Settings";
            // 
            // voiceActivityDetectionCheckBox
            // 
            this.voiceActivityDetectionCheckBox.AutoSize = true;
            this.voiceActivityDetectionCheckBox.Checked = true;
            this.voiceActivityDetectionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.voiceActivityDetectionCheckBox.Location = new System.Drawing.Point(15, 206);
            this.voiceActivityDetectionCheckBox.Name = "voiceActivityDetectionCheckBox";
            this.voiceActivityDetectionCheckBox.Size = new System.Drawing.Size(139, 17);
            this.voiceActivityDetectionCheckBox.TabIndex = 20;
            this.voiceActivityDetectionCheckBox.Text = "Voice Activity Detection";
            this.voiceActivityDetectionCheckBox.UseVisualStyleBackColor = true;
            // 
            // punctuationCheckBox
            // 
            this.punctuationCheckBox.AutoSize = true;
            this.punctuationCheckBox.Checked = true;
            this.punctuationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.punctuationCheckBox.Location = new System.Drawing.Point(15, 229);
            this.punctuationCheckBox.Name = "punctuationCheckBox";
            this.punctuationCheckBox.Size = new System.Drawing.Size(83, 17);
            this.punctuationCheckBox.TabIndex = 21;
            this.punctuationCheckBox.Text = "Punctuation";
            this.punctuationCheckBox.UseVisualStyleBackColor = true;
            // 
            // denormalizationCheckBox
            // 
            this.denormalizationCheckBox.AutoSize = true;
            this.denormalizationCheckBox.Checked = true;
            this.denormalizationCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.denormalizationCheckBox.Location = new System.Drawing.Point(15, 252);
            this.denormalizationCheckBox.Name = "denormalizationCheckBox";
            this.denormalizationCheckBox.Size = new System.Drawing.Size(101, 17);
            this.denormalizationCheckBox.TabIndex = 22;
            this.denormalizationCheckBox.Text = "Denormalization";
            this.denormalizationCheckBox.UseVisualStyleBackColor = true;
            // 
            // speakerChangeCheckBox
            // 
            this.speakerChangeCheckBox.AutoSize = true;
            this.speakerChangeCheckBox.Checked = true;
            this.speakerChangeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.speakerChangeCheckBox.Location = new System.Drawing.Point(15, 275);
            this.speakerChangeCheckBox.Name = "speakerChangeCheckBox";
            this.speakerChangeCheckBox.Size = new System.Drawing.Size(155, 17);
            this.speakerChangeCheckBox.TabIndex = 23;
            this.speakerChangeCheckBox.Text = "Speaker Change Detection";
            this.speakerChangeCheckBox.UseVisualStyleBackColor = true;
            // 
            // languageComboBox
            // 
            this.languageComboBox.Enabled = false;
            this.languageComboBox.FormattingEnabled = true;
            this.languageComboBox.Location = new System.Drawing.Point(209, 229);
            this.languageComboBox.Name = "languageComboBox";
            this.languageComboBox.Size = new System.Drawing.Size(134, 21);
            this.languageComboBox.TabIndex = 24;
            this.languageComboBox.Text = "Slovenščina (sl-SI)";
            // 
            // translateToEnglishCheckBox
            // 
            this.translateToEnglishCheckBox.AutoSize = true;
            this.translateToEnglishCheckBox.Enabled = false;
            this.translateToEnglishCheckBox.Location = new System.Drawing.Point(381, 231);
            this.translateToEnglishCheckBox.Name = "translateToEnglishCheckBox";
            this.translateToEnglishCheckBox.Size = new System.Drawing.Size(119, 17);
            this.translateToEnglishCheckBox.TabIndex = 25;
            this.translateToEnglishCheckBox.Text = "Translate to English";
            this.translateToEnglishCheckBox.UseVisualStyleBackColor = true;
            // 
            // languageLabel
            // 
            this.languageLabel.AutoSize = true;
            this.languageLabel.Location = new System.Drawing.Point(206, 206);
            this.languageLabel.Name = "languageLabel";
            this.languageLabel.Size = new System.Drawing.Size(55, 13);
            this.languageLabel.TabIndex = 26;
            this.languageLabel.Text = "Language";
            // 
            // rememberMeCheckBox
            // 
            this.rememberMeCheckBox.AutoSize = true;
            this.rememberMeCheckBox.Checked = true;
            this.rememberMeCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.rememberMeCheckBox.Location = new System.Drawing.Point(188, 162);
            this.rememberMeCheckBox.Name = "rememberMeCheckBox";
            this.rememberMeCheckBox.Size = new System.Drawing.Size(94, 17);
            this.rememberMeCheckBox.TabIndex = 28;
            this.rememberMeCheckBox.Text = "Remember me";
            this.rememberMeCheckBox.UseVisualStyleBackColor = true;
            // 
            // TrueBarAudioToText
            // 
            this.ClientSize = new System.Drawing.Size(527, 361);
            this.Controls.Add(this.rememberMeCheckBox);
            this.Controls.Add(this.languageLabel);
            this.Controls.Add(this.translateToEnglishCheckBox);
            this.Controls.Add(this.languageComboBox);
            this.Controls.Add(this.speakerChangeCheckBox);
            this.Controls.Add(this.denormalizationCheckBox);
            this.Controls.Add(this.punctuationCheckBox);
            this.Controls.Add(this.voiceActivityDetectionCheckBox);
            this.Controls.Add(this.settingsLabel);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.trueBarLinkLabel);
            this.Controls.Add(this.clientSecretTextBox);
            this.Controls.Add(this.clientIdTextBox);
            this.Controls.Add(this.clientSecretLabel);
            this.Controls.Add(this.clientIdLabel);
            this.Controls.Add(this.LoginButton);
            this.Controls.Add(this.infoLabel);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.cancelButton);
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

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.KeyCode == Keys.Enter)
            {
                // Trigger the Login button's click event
                LoginButton.PerformClick();
                e.SuppressKeyPress = true; // Prevents the "ding" sound
            }
        }

        private void CheckIfAuthenticated()
        {
            // Load authentication data
            var (clientID, clientSecret) = AuthHelper.LoadAuthData();
            if (!string.IsNullOrWhiteSpace(clientID) && !string.IsNullOrWhiteSpace(clientSecret))
            {
                clientIdTextBox.Text = clientID;
                clientSecretTextBox.Text = clientSecret;
                _clientID = clientID;
                _clientSecret = clientSecret;
            }

        }

        // Login to the True-bar (Subtitler) API
        private async void LoginButton_Click(object sender, EventArgs e)
        {
            // Check if the user is already logged in
            if (LoginButton.Text == "Log Out")
            {
                // Clear the access token
                _accessToken = null;
                // Clear the login fields
                clientSecretTextBox.Text = string.Empty;
                // Enable the login fields
                clientIdTextBox.Enabled = true;
                clientSecretTextBox.Enabled = true;
                // Change the button text
                LoginButton.Text = "Login";
                return;
            }

            if (string.IsNullOrWhiteSpace(clientIdTextBox.Text) || string.IsNullOrWhiteSpace(clientSecretTextBox.Text))
            {
                MessageBox.Show("Please enter username and password");
                return;
            }

            // Disable the login button while the request is in progress
            _processing = true;
            LoginButton.Enabled = false;

            // Check internet connection
            if (!await _trueBarSubtitlerAPI.CheckForInternetConnection())
            {
                MessageBox.Show("No internet connection!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoginButton.Enabled = true;
                return;
            }

            // Check if the API server is reachable
            if (!await _trueBarSubtitlerAPI.IsApiServerReachableAsync())
            {
                MessageBox.Show("API is not reachable!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                LoginButton.Enabled = true;
                return;
            }

            // TODO: handle session expiration

            await Login(clientIdTextBox.Text, clientSecretTextBox.Text);
        }

        private async Task Login(string clientID, string clientSecret, bool relogin = false)
        {
            try
            {
                // Call the new Subtitler API to login
                string response = await _trueBarSubtitlerAPI.Login(clientID, clientSecret);
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

                if (jsonResponse.ContainsKey("access_token"))
                {
                    string accessToken = jsonResponse["access_token"];

                    LoginButton.Text = "Log Out";
                    LoginButton.Enabled = true;
                    // Store the client ID and secret
                    if (string.IsNullOrWhiteSpace(_clientID) || string.IsNullOrWhiteSpace(_clientSecret))
                    {
                        _clientID = clientID;
                        _clientSecret = clientSecret;

                        // Save the authentication data if the user checked the remember me checkbox
                        if (rememberMeCheckBox.Checked)
                        {
                            AuthHelper.SaveAuthData(clientID, clientSecret);
                        }
                    }

                    if (!relogin)
                    {
                        _processing = false;
                        MessageBox.Show("Login successful!");
                        // Put focus on the generate button
                        generateButton.Focus();
                        // Disable the login fields
                        clientIdTextBox.Enabled = false;
                        clientSecretTextBox.Enabled = false;
                        rememberMeCheckBox.Enabled = false;
                    }

                    // Store the access token in-memory
                    _accessToken = accessToken;
                }
                else if (jsonResponse.ContainsKey("error"))
                {

                    if (!relogin && jsonResponse["error"].Contains("unauthorized_client"))
                    {
                        MessageBox.Show("Login failed: " + jsonResponse["error_description"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else if (!relogin)
                    {
                        MessageBox.Show("Login failed: " + jsonResponse["error"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Automatic re-login failed: " + jsonResponse["error"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
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
            // Check if the user is logged in and if access token is available
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

            if (!await _trueBarSubtitlerAPI.CheckTokenValidity(_accessToken))
            {
                // relogin
                await Login(_clientID, _clientSecret, true);
            }

            // Check if the API server is reachable 
            if (!await _trueBarSubtitlerAPI.IsApiServerReachableAsync())
            {
                MessageBox.Show("API is not reachable!", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            _processing = true;
            // Disable the generate button while the request is in progress
            generateButton.Enabled = false;
            LoginButton.Enabled = false;
            progressBar1.Visible = true;
            progressBar1.Style = ProgressBarStyle.Marquee;
            label4.Text = "Generating WAV file...";

            // Check if the the uploaded file is .wav, if not, convert it to .wav
            _audioFileName = _videoFileName != null ? GenerateWavFile(_videoFileName, _audioTrackNumber) : null;

            try
            {
                label4.Text = "Uploading...";
                string response = await _trueBarSubtitlerAPI.UploadFileAsync(_accessToken, _audioFileName, voiceActivityDetectionCheckBox.Checked, punctuationCheckBox.Checked, denormalizationCheckBox.Checked, speakerChangeCheckBox.Checked);
                var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

                if (jsonResponse.ContainsKey("job_id"))
                {
                    // Store the session ID in-memory
                    _jobId = jsonResponse["job_id"];
                    _statusCheckTimer.Start();
                }
                else if (jsonResponse.ContainsKey("error"))
                {
                    label4.Text = "Failed";
                    // Check if the token is expired and re-login
                    if (jsonResponse["error"].Contains("Unauthorized"))
                    {
                        // try to re-login
                        // await Login(_clientID, _clientSecret, true);
                        // Retry the generate button click (!possible infinite loop)
                        // Generate_Click(sender, e);
                        MessageBox.Show("Session expired. Please login again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        LoginButton.Text = "Login";
                        _processing = false;
                        generateButton.Enabled = true;
                        progressBar1.Visible = false;

                        return;
                    }

                    MessageBox.Show("Upload failed: " + jsonResponse["error"], "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _processing = false;
                    generateButton.Enabled = true;
                    LoginButton.Enabled = true;
                    progressBar1.Visible = false;
                }
                else
                {
                    MessageBox.Show("Unexpected response: " + response, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    label4.Text = "Failed";
                    _processing = false;
                    generateButton.Enabled = true;
                    progressBar1.Visible = false;
                    LoginButton.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                label4.Text = "Failed";
                MessageBox.Show("Error processing response: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _processing = false;
                generateButton.Enabled = true;
                progressBar1.Visible = false;
                LoginButton.Enabled = true;
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
                // Deserialize the response
                var statusResponseJson = JsonConvert.DeserializeObject<dynamic>(statusResponse);

                // TODO: rewrite the status check logic

                // Check if the response is successful
                if (statusResponseJson.status == null)
                {
                    // First check if the token is expired and re-login
                    if (statusResponse.Contains("Unauthorized"))
                    {
                        // try to re-login
                        await Login(_clientID, _clientSecret, true);
                        return;
                    }

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

                    generateButton.Enabled = true; // Re-enable the generate button
                    return;
                }

                progressBar1.Style = ProgressBarStyle.Marquee;
                var transcriptionStatus = statusResponseJson.status.ToString();

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
                        generateButton.Enabled = true;
                        LoginButton.Enabled = true;
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