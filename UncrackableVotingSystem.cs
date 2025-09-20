using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public static class UncrackableVotingSystem
    {
        public const int VOTES_NEEDED_FOR_AUTO_REMOVAL = 5;

        public class UncrackableVote
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public string UserId { get; set; }
            public DateTime VotedAt { get; set; }
            public string Reason { get; set; }
            public bool IsAgree { get; set; } // true = uncrackable, false = still crackable
        }

        public class UncrackableStatus
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public int UncrackableVotes { get; set; }
            public int CrackableVotes { get; set; }
            public bool IsMarkedUncrackable { get; set; }
            public bool HasUserVoted { get; set; }
            public string UserVoteType { get; set; } // "uncrackable", "crackable", null
            public DateTime? AutoRemovedAt { get; set; }
        }

        public static async Task<bool> ShowVoteDialog(string appId, string gameName, Form parentForm)
        {
            // First check if user already voted
            var status = await RequestAPI.GetUncrackableStatus(appId, HWIDManager.GetUserId());

            if (status.HasUserVoted)
            {
                MessageBox.Show(
                    $"You already voted on {gameName}.\n\n" +
                    $"Your vote: {status.UserVoteType}\n" +
                    $"Current votes: {status.UncrackableVotes} uncrackable, {status.CrackableVotes} crackable",
                    "Already Voted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
                return false;
            }

            var voteForm = new UncrackableVoteForm(appId, gameName, status);
            var result = voteForm.ShowDialog(parentForm);

            return result == DialogResult.OK;
        }

        public static async Task<bool> SubmitVote(string appId, string gameName, bool isUncrackable, string reason)
        {
            try
            {
                var vote = new
                {
                    appId = appId,
                    gameName = gameName,
                    userId = HWIDManager.GetUserId(),
                    hwid = HWIDManager.GetHWID(),
                    isUncrackable = isUncrackable,
                    reason = reason,
                    timestamp = DateTime.UtcNow
                };

                var json = Newtonsoft.Json.JsonConvert.SerializeObject(vote);
                var content = new System.Net.Http.StringContent(json, System.Text.Encoding.UTF8, "application/json");

                using (var client = new System.Net.Http.HttpClient())
                {
                    var response = await client.PostAsync("https://pydrive.harryeffingpotter.com/sacgui/api/vote-uncrackable", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseJson = await response.Content.ReadAsStringAsync();
                        dynamic result = Newtonsoft.Json.JsonConvert.DeserializeObject(responseJson);

                        // Check if this vote caused auto-removal
                        if (result.autoRemoved == true)
                        {
                            ShowAutoRemovalNotification(gameName, (int)result.totalVotes);
                        }

                        return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static void ShowAutoRemovalNotification(string gameName, int totalVotes)
        {
            var toast = new Form
            {
                FormBorderStyle = FormBorderStyle.None,
                StartPosition = FormStartPosition.Manual,
                Size = new Size(450, 120),
                BackColor = Color.FromArgb(50, 0, 0),
                TopMost = true,
                ShowInTaskbar = false,
                Opacity = 0.95
            };

            var screen = Screen.PrimaryScreen.WorkingArea;
            toast.Location = new Point(screen.Width - 470, screen.Height - 140);

            var lblTitle = new Label
            {
                Text = "🚫 GAME AUTO-REMOVED",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 100, 100),
                Location = new Point(20, 15),
                Size = new Size(400, 25)
            };

            var lblMessage = new Label
            {
                Text = $"{gameName} marked as uncrackable\n{totalVotes} community votes • Removed from requests",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(20, 45),
                Size = new Size(400, 40)
            };

            toast.Controls.AddRange(new Control[] { lblTitle, lblMessage });
            toast.Show();

            var timer = new Timer { Interval = 8000 };
            timer.Tick += (s, e) =>
            {
                toast.Close();
                timer.Stop();
            };
            timer.Start();
        }

        public static Button CreateVoteButton(string appId, string gameName)
        {
            var button = new Button
            {
                Text = "🚫 Vote Uncrackable",
                Size = new Size(130, 25),
                BackColor = Color.FromArgb(60, 30, 30),
                ForeColor = Color.FromArgb(255, 150, 150),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };

            button.Click += async (s, e) =>
            {
                var parentForm = button.FindForm();
                await ShowVoteDialog(appId, gameName, parentForm);
            };

            return button;
        }
    }

    public class UncrackableVoteForm : Form
    {
        private RadioButton radioUncrackable;
        private RadioButton radioCrackable;
        private ComboBox reasonCombo;
        private TextBox customReasonBox;
        private Button btnSubmit;
        private Button btnCancel;
        private Label lblCurrentVotes;

        private string appId;
        private string gameName;
        private UncrackableVotingSystem.UncrackableStatus status;

        public UncrackableVoteForm(string appId, string gameName, UncrackableVotingSystem.UncrackableStatus status)
        {
            this.appId = appId;
            this.gameName = gameName;
            this.status = status;
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Vote on Crackability";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Header
            var lblHeader = new Label
            {
                Text = $"Vote on: {gameName}",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(20, 20),
                Size = new Size(450, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Current vote status
            lblCurrentVotes = new Label
            {
                Text = $"Current votes: {status.UncrackableVotes} uncrackable, {status.CrackableVotes} crackable",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, 55),
                Size = new Size(450, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Vote options
            var lblVote = new Label
            {
                Text = "What's your assessment?",
                Font = new Font("Segoe UI", 11),
                Location = new Point(20, 90),
                Size = new Size(450, 25)
            };

            radioUncrackable = new RadioButton
            {
                Text = "🚫 This game is UNCRACKABLE (for now)",
                Location = new Point(40, 120),
                Size = new Size(400, 25),
                ForeColor = Color.FromArgb(255, 150, 150),
                Font = new Font("Segoe UI", 10),
                Checked = true
            };

            radioCrackable = new RadioButton
            {
                Text = "✅ This game IS or WILL BE crackable",
                Location = new Point(40, 150),
                Size = new Size(400, 25),
                ForeColor = Color.FromArgb(150, 255, 150),
                Font = new Font("Segoe UI", 10)
            };

            // Reason selection
            var lblReason = new Label
            {
                Text = "Reason (required):",
                Font = new Font("Segoe UI", 10),
                Location = new Point(40, 185),
                Size = new Size(200, 25)
            };

            reasonCombo = new ComboBox
            {
                Location = new Point(40, 210),
                Size = new Size(400, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            // Populate reasons based on vote type
            radioUncrackable.CheckedChanged += UpdateReasonOptions;
            radioCrackable.CheckedChanged += UpdateReasonOptions;
            UpdateReasonOptions(null, null);

            // Custom reason
            var lblCustom = new Label
            {
                Text = "Additional details (optional):",
                Font = new Font("Segoe UI", 9),
                Location = new Point(40, 245),
                Size = new Size(200, 20)
            };

            customReasonBox = new TextBox
            {
                Location = new Point(40, 265),
                Size = new Size(400, 50),
                Multiline = true,
                BackColor = Color.FromArgb(40, 40, 40),
                ForeColor = Color.White,
                PlaceholderText = "Any additional info that might help..."
            };

            // Warning about 5 votes
            var lblWarning = new Label
            {
                Text = $"⚠️ If {UncrackableVotingSystem.VOTES_NEEDED_FOR_AUTO_REMOVAL} users vote uncrackable, this game will be auto-removed from all request lists.",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Orange,
                Location = new Point(20, 325),
                Size = new Size(450, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Buttons
            btnSubmit = new Button
            {
                Text = "Submit Vote",
                Location = new Point(200, 360),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnSubmit.Click += BtnSubmit_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(310, 360),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblHeader, lblCurrentVotes, lblVote,
                radioUncrackable, radioCrackable,
                lblReason, reasonCombo,
                lblCustom, customReasonBox,
                lblWarning, btnSubmit, btnCancel
            });
        }

        private void UpdateReasonOptions(object sender, EventArgs e)
        {
            reasonCombo.Items.Clear();

            if (radioUncrackable.Checked)
            {
                reasonCombo.Items.AddRange(new string[]
                {
                    "Denuvo (latest version)",
                    "Always online DRM",
                    "Subscription-based game",
                    "Server-side validation",
                    "Custom protection (unbeaten)",
                    "No scene interest",
                    "Legal issues preventing cracks",
                    "Game abandoned/delisted",
                    "Other (explain in details)"
                });
            }
            else
            {
                reasonCombo.Items.AddRange(new string[]
                {
                    "Denuvo can be cracked eventually",
                    "Protection is known/beaten",
                    "Scene groups working on it",
                    "Offline mode available",
                    "Community tools exist",
                    "Similar games were cracked",
                    "Just needs time/effort",
                    "Other (explain in details)"
                });
            }

            reasonCombo.SelectedIndex = 0;
        }

        private async void BtnSubmit_Click(object sender, EventArgs e)
        {
            if (reasonCombo.SelectedItem == null)
            {
                MessageBox.Show("Please select a reason for your vote.", "Reason Required");
                return;
            }

            btnSubmit.Enabled = false;
            btnSubmit.Text = "Submitting...";

            var reason = reasonCombo.SelectedItem.ToString();
            if (!string.IsNullOrEmpty(customReasonBox.Text))
            {
                reason += " - " + customReasonBox.Text;
            }

            bool success = await UncrackableVotingSystem.SubmitVote(
                appId, gameName, radioUncrackable.Checked, reason);

            if (success)
            {
                MessageBox.Show(
                    $"Vote submitted successfully!\n\n" +
                    $"Your vote: {(radioUncrackable.Checked ? "Uncrackable" : "Crackable")}\n" +
                    "Thank you for helping the community.",
                    "Vote Submitted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show(
                    "Failed to submit vote. Please try again.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                btnSubmit.Enabled = true;
                btnSubmit.Text = "Submit Vote";
            }
        }
    }

    // Extension to RequestAPI for voting
    public static partial class RequestAPI
    {
        public static async Task<UncrackableVotingSystem.UncrackableStatus> GetUncrackableStatus(string appId, string userId)
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/uncrackable-status/{appId}?userId={userId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<UncrackableVotingSystem.UncrackableStatus>(json);
                }
            }
            catch { }

            return new UncrackableVotingSystem.UncrackableStatus
            {
                AppId = appId,
                UncrackableVotes = 0,
                CrackableVotes = 0,
                HasUserVoted = false
            };
        }

        public static async Task<bool> IsGameUncrackable(string appId)
        {
            try
            {
                var response = await client.GetAsync($"{API_BASE}/is-uncrackable/{appId}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(json);
                    return result.isUncrackable == true;
                }
            }
            catch { }
            return false;
        }
    }
}