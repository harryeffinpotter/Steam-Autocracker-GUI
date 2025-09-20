using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SteamAppIdIdentifier
{
    public class RequestUpdateForm : Form
    {
        private TextBox searchBox;
        private ListBox resultsBox;
        private TextBox currentRinVersionBox;
        private TextBox notesBox;
        private Button searchButton;
        private Button submitRequestButton;
        private Label statusLabel;

        private string selectedAppId;
        private string selectedGameName;

        public RequestUpdateForm()
        {
            InitializeComponent();
            LoadPendingRequests();
        }

        private void InitializeComponent()
        {
            this.Text = "Request Game Update";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            // Title
            var titleLabel = new Label
            {
                Text = "REQUEST A GAME UPDATE",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 255, 255),
                Location = new Point(20, 20),
                Size = new Size(560, 40),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Search section
            var searchLabel = new Label
            {
                Text = "Step 1: Search for the game:",
                ForeColor = Color.White,
                Location = new Point(20, 70),
                Size = new Size(200, 25)
            };

            searchBox = new TextBox
            {
                Location = new Point(20, 95),
                Size = new Size(400, 25),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };

            searchButton = new Button
            {
                Text = "Search",
                Location = new Point(430, 94),
                Size = new Size(100, 27),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            searchButton.Click += SearchButton_Click;

            // Results
            var resultsLabel = new Label
            {
                Text = "Step 2: Select the game:",
                ForeColor = Color.White,
                Location = new Point(20, 130),
                Size = new Size(200, 25)
            };

            resultsBox = new ListBox
            {
                Location = new Point(20, 155),
                Size = new Size(540, 100),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            resultsBox.SelectedIndexChanged += ResultsBox_SelectedIndexChanged;

            // Current RIN version
            var rinVersionLabel = new Label
            {
                Text = "Step 3: What's the LATEST version on CS.RIN.RU? (Build ID or version number):",
                ForeColor = Color.White,
                Location = new Point(20, 265),
                Size = new Size(540, 25)
            };

            currentRinVersionBox = new TextBox
            {
                Location = new Point(20, 290),
                Size = new Size(540, 25),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                PlaceholderText = "e.g., 12345678 or 1.2.3 or 'None - no clean files exist'"
            };

            // Notes
            var notesLabel = new Label
            {
                Text = "Additional notes (optional):",
                ForeColor = Color.White,
                Location = new Point(20, 325),
                Size = new Size(200, 25)
            };

            notesBox = new TextBox
            {
                Location = new Point(20, 350),
                Size = new Size(540, 50),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                Multiline = true,
                PlaceholderText = "e.g., 'Links are dead', 'DLC needed', etc."
            };

            // Submit button
            submitRequestButton = new Button
            {
                Text = "SUBMIT REQUEST",
                Location = new Point(200, 410),
                Size = new Size(200, 35),
                BackColor = Color.FromArgb(0, 200, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                Enabled = false
            };
            submitRequestButton.Click += SubmitRequestButton_Click;

            // Status
            statusLabel = new Label
            {
                Text = "",
                ForeColor = Color.Yellow,
                Location = new Point(20, 450),
                Size = new Size(540, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.AddRange(new Control[] {
                titleLabel, searchLabel, searchBox, searchButton,
                resultsLabel, resultsBox, rinVersionLabel, currentRinVersionBox,
                notesLabel, notesBox, submitRequestButton, statusLabel
            });
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(searchBox.Text))
                return;

            searchButton.Enabled = false;
            statusLabel.Text = "Searching Steam...";
            statusLabel.ForeColor = Color.Yellow;

            try
            {
                // Search Steam API
                using (var client = new HttpClient())
                {
                    var searchTerm = Uri.EscapeDataString(searchBox.Text);
                    var url = $"https://steamcommunity.com/actions/SearchApps/{searchTerm}";

                    var response = await client.GetStringAsync(url);
                    var results = JsonConvert.DeserializeObject<List<SteamSearchResult>>(response);

                    resultsBox.Items.Clear();

                    if (results != null && results.Count > 0)
                    {
                        foreach (var result in results.Take(10))
                        {
                            resultsBox.Items.Add($"{result.name} (AppID: {result.appid})");
                        }

                        statusLabel.Text = $"Found {results.Count} results";
                        statusLabel.ForeColor = Color.Lime;
                    }
                    else
                    {
                        statusLabel.Text = "No results found";
                        statusLabel.ForeColor = Color.Orange;
                    }
                }
            }
            catch (Exception ex)
            {
                statusLabel.Text = $"Search error: {ex.Message}";
                statusLabel.ForeColor = Color.Red;
            }
            finally
            {
                searchButton.Enabled = true;
            }
        }

        private void ResultsBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (resultsBox.SelectedItem != null)
            {
                var selected = resultsBox.SelectedItem.ToString();
                var match = Regex.Match(selected, @"(.+) \(AppID: (\d+)\)");

                if (match.Success)
                {
                    selectedGameName = match.Groups[1].Value;
                    selectedAppId = match.Groups[2].Value;
                    submitRequestButton.Enabled = true;

                    statusLabel.Text = $"Selected: {selectedGameName}";
                    statusLabel.ForeColor = Color.Cyan;
                }
            }
        }

        private async void SubmitRequestButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedAppId) || string.IsNullOrEmpty(currentRinVersionBox.Text))
            {
                MessageBox.Show("Please select a game and enter the current RIN version.",
                    "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var request = new UpdateRequest
                {
                    AppId = selectedAppId,
                    GameName = selectedGameName,
                    CurrentRinVersion = currentRinVersionBox.Text,
                    Notes = notesBox.Text,
                    RequestDate = DateTime.Now,
                    RequesterId = Environment.MachineName
                };

                // Save request locally
                await SaveRequest(request);

                // Also check if any current users have newer version
                await CheckForAvailableUpdates(request);

                statusLabel.Text = "✅ Request submitted successfully!";
                statusLabel.ForeColor = Color.Lime;

                MessageBox.Show(
                    $"Your request for {selectedGameName} has been submitted!\n\n" +
                    "If anyone using this app has a newer version, they'll be notified.",
                    "Request Submitted",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error submitting request: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task SaveRequest(UpdateRequest request)
        {
            // Save to local file for now (could be cloud database later)
            var requestsFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SteamAutoCracker",
                "update_requests.json"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(requestsFile));

            List<UpdateRequest> requests = new List<UpdateRequest>();

            if (File.Exists(requestsFile))
            {
                var json = File.ReadAllText(requestsFile);
                requests = JsonConvert.DeserializeObject<List<UpdateRequest>>(json) ?? new List<UpdateRequest>();
            }

            // Remove old request for same game if exists
            requests.RemoveAll(r => r.AppId == request.AppId);
            requests.Add(request);

            File.WriteAllText(requestsFile, JsonConvert.SerializeObject(requests, Formatting.Indented));
        }

        private async Task CheckForAvailableUpdates(UpdateRequest request)
        {
            // Check if current user has this game with newer version
            // This would integrate with the main app's game scanning
            await Task.CompletedTask;
        }

        private void LoadPendingRequests()
        {
            // Load and display existing requests
            // Could show in a separate tab or window
        }

        private class SteamSearchResult
        {
            public string appid { get; set; }
            public string name { get; set; }
            public string icon { get; set; }
        }

        private class UpdateRequest
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public string CurrentRinVersion { get; set; }
            public string Notes { get; set; }
            public DateTime RequestDate { get; set; }
            public string RequesterId { get; set; }
        }
    }

    // Add to main form
    public static class UpdateRequestChecker
    {
        public static async Task<List<string>> CheckIfUserHasRequestedGames(List<SteamGame> userGames)
        {
            var notifications = new List<string>();

            var requestsFile = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SteamAutoCracker",
                "update_requests.json"
            );

            if (!File.Exists(requestsFile))
                return notifications;

            var json = File.ReadAllText(requestsFile);
            var requests = JsonConvert.DeserializeObject<List<UpdateRequest>>(json);

            foreach (var request in requests)
            {
                var userGame = userGames.FirstOrDefault(g => g.AppId == request.AppId);

                if (userGame != null)
                {
                    // User has this game! Check if newer
                    if (IsNewerVersion(userGame.BuildId, request.CurrentRinVersion))
                    {
                        notifications.Add($"🔥 YOUR {userGame.Name} IS NEEDED!\n" +
                                        $"RIN has: {request.CurrentRinVersion}\n" +
                                        $"You have: {userGame.BuildId}\n" +
                                        $"Someone requested this update!");
                    }
                }
            }

            return notifications;
        }

        private static bool IsNewerVersion(string userVersion, string rinVersion)
        {
            // Compare build IDs or version numbers
            if (long.TryParse(userVersion, out long userBuild) &&
                long.TryParse(rinVersion, out long rinBuild))
            {
                return userBuild > rinBuild;
            }

            // Could add more complex version comparison
            return false;
        }

        private class UpdateRequest
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public string CurrentRinVersion { get; set; }
            public string Notes { get; set; }
            public DateTime RequestDate { get; set; }
            public string RequesterId { get; set; }
        }

        private class SteamGame
        {
            public string AppId { get; set; }
            public string Name { get; set; }
            public string BuildId { get; set; }
        }
    }
}