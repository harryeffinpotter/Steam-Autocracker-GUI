using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace SteamAppIdIdentifier
{
    public class SimpleRequestForm : Form
    {
        private TextBox searchBox;
        private ListBox resultsBox;
        private Button searchButton;
        private Button requestButton;
        private Label statusLabel;

        private string selectedAppId;
        private string selectedGameName;

        public SimpleRequestForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Request Update";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(30, 30, 30);

            var titleLabel = new Label
            {
                Text = "REQUEST A GAME UPDATE",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 100, 100),
                Location = new Point(20, 20),
                Size = new Size(460, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var instructionLabel = new Label
            {
                Text = "Search for a game that needs updating:",
                ForeColor = Color.White,
                Location = new Point(20, 60),
                Size = new Size(460, 25)
            };

            searchBox = new TextBox
            {
                Location = new Point(20, 85),
                Size = new Size(350, 25),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White,
                PlaceholderText = "Type game name..."
            };

            searchButton = new Button
            {
                Text = "Search",
                Location = new Point(380, 84),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            searchButton.Click += async (s, e) => await SearchGames();

            resultsBox = new ListBox
            {
                Location = new Point(20, 120),
                Size = new Size(440, 150),
                BackColor = Color.FromArgb(50, 50, 50),
                ForeColor = Color.White
            };
            resultsBox.SelectedIndexChanged += (s, e) =>
            {
                if (resultsBox.SelectedItem != null)
                {
                    var selected = resultsBox.SelectedItem.ToString();
                    var parts = selected.Split(new[] { " [AppID: " }, StringSplitOptions.None);
                    if (parts.Length == 2)
                    {
                        selectedGameName = parts[0];
                        selectedAppId = parts[1].TrimEnd(']');
                        requestButton.Enabled = true;
                        statusLabel.Text = $"Selected: {selectedGameName}";
                    }
                }
            };

            requestButton = new Button
            {
                Text = "🔥 REQUEST UPDATE",
                Location = new Point(150, 285),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(200, 0, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                Enabled = false
            };
            requestButton.Click += RequestUpdate;

            statusLabel = new Label
            {
                Text = "",
                ForeColor = Color.Yellow,
                Location = new Point(20, 335),
                Size = new Size(440, 25),
                TextAlign = ContentAlignment.MiddleCenter
            };

            this.Controls.AddRange(new Control[] {
                titleLabel, instructionLabel, searchBox, searchButton,
                resultsBox, requestButton, statusLabel
            });
        }

        private async System.Threading.Tasks.Task SearchGames()
        {
            searchButton.Enabled = false;
            resultsBox.Items.Clear();

            try
            {
                using (var client = new HttpClient())
                {
                    var searchTerm = Uri.EscapeDataString(searchBox.Text);
                    var response = await client.GetStringAsync($"https://steamcommunity.com/actions/SearchApps/{searchTerm}");
                    dynamic results = JsonConvert.DeserializeObject(response);

                    if (results != null)
                    {
                        foreach (var game in results)
                        {
                            resultsBox.Items.Add($"{game.name} [AppID: {game.appid}]");
                        }
                    }
                }
            }
            catch { }
            finally
            {
                searchButton.Enabled = true;
            }
        }

        private void RequestUpdate(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedAppId))
                return;

            // Save the request
            var request = new GameRequest
            {
                AppId = selectedAppId,
                GameName = selectedGameName,
                RequestedAt = DateTime.Now
            };

            SaveRequest(request);

            MessageBox.Show(
                $"✅ Request submitted!\n\n" +
                $"Anyone with {selectedGameName} will be notified it's needed.",
                "Request Sent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void SaveRequest(GameRequest request)
        {
            var requestsFile = GetRequestsFilePath();
            var requests = LoadRequests();

            // Only keep one request per game
            requests.RemoveAll(r => r.AppId == request.AppId);
            requests.Add(request);

            // Keep only last 100 requests
            if (requests.Count > 100)
                requests = requests.OrderByDescending(r => r.RequestedAt).Take(100).ToList();

            Directory.CreateDirectory(Path.GetDirectoryName(requestsFile));
            File.WriteAllText(requestsFile, JsonConvert.SerializeObject(requests, Formatting.Indented));
        }

        public static List<GameRequest> LoadRequests()
        {
            var requestsFile = GetRequestsFilePath();

            if (File.Exists(requestsFile))
            {
                var json = File.ReadAllText(requestsFile);
                return JsonConvert.DeserializeObject<List<GameRequest>>(json) ?? new List<GameRequest>();
            }

            return new List<GameRequest>();
        }

        private static string GetRequestsFilePath()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SteamAutoCracker",
                "requests.json"
            );
        }

        public class GameRequest
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public DateTime RequestedAt { get; set; }
        }
    }

    // Add this to main form - checks if user has any requested games
    public static class RequestChecker
    {
        public static List<string> CheckUserGames(List<dynamic> userGames)
        {
            var alerts = new List<string>();
            var requests = SimpleRequestForm.LoadRequests();

            foreach (var request in requests)
            {
                if (userGames.Any(g => g.AppId == request.AppId))
                {
                    alerts.Add($"🔥 {request.GameName} - YOUR FILES ARE NEEDED!");
                }
            }

            return alerts;
        }
    }
}