using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using APPID;
using Timer = System.Windows.Forms.Timer;

namespace SteamAppIdIdentifier
{
    public partial class RequestsWindow : Form
    {
        private SteamAppId mainForm;
        private Dictionary<string, RequestAPI.RequestedGame> requestedGames = new Dictionary<string, RequestAPI.RequestedGame>();
        private Dictionary<string, GameRequestStats> gameStats = new Dictionary<string, GameRequestStats>();

        public RequestsWindow(SteamAppId mainForm)
        {
            this.mainForm = mainForm;
            InitializeComponent();
            SetupSearchResultsGrid();
            SetupCommunityRequestsGrid();
            SetupMyRequestsGrid();
            _ = LoadMyRequests();
            _ = LoadCommunityRequests();
            SetupRefreshTimer();
        }

        private void SetupSearchResultsGrid()
        {
            searchResultsGrid.Columns.Clear();

            searchResultsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AppID",
                HeaderText = "AppID",
                Width = 80,
                ReadOnly = true
            });

            searchResultsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Name",
                Width = 400,
                ReadOnly = true
            });

            searchResultsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RequestCount",
                HeaderText = "Requests",
                Width = 80,
                ReadOnly = true
            });

            searchResultsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "UncrackableCount",
                HeaderText = "Uncrackable",
                Width = 100,
                ReadOnly = true
            });

            var requestColumn = new DataGridViewButtonColumn
            {
                Name = "Request",
                HeaderText = "Request",
                Text = "Request",
                UseColumnTextForButtonValue = false,
                Width = 80,
                FlatStyle = FlatStyle.Flat
            };
            searchResultsGrid.Columns.Add(requestColumn);

            var uncrackableColumn = new DataGridViewButtonColumn
            {
                Name = "Uncrackable",
                HeaderText = "Mark Uncrackable",
                Text = "🚫",
                UseColumnTextForButtonValue = false,
                Width = 120,
                FlatStyle = FlatStyle.Flat
            };
            searchResultsGrid.Columns.Add(uncrackableColumn);

            searchResultsGrid.CellContentClick += SearchResultsGrid_CellContentClick;
            searchResultsGrid.CellFormatting += SearchResultsGrid_CellFormatting;
        }

        private void SetupCommunityRequestsGrid()
        {
            communityRequestsGrid.Columns.Clear();

            communityRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AppId",
                HeaderText = "App ID",
                Width = 80,
                ReadOnly = true
            });

            communityRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Game Name",
                Width = 400,
                ReadOnly = true
            });

            communityRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RequestCount",
                HeaderText = "Total Requests",
                Width = 100,
                ReadOnly = true
            });

            communityRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "LastRequested",
                HeaderText = "Last Requested",
                Width = 150,
                ReadOnly = true
            });

            var voteColumn = new DataGridViewButtonColumn
            {
                Name = "Vote",
                HeaderText = "Vote",
                Text = "👍",
                UseColumnTextForButtonValue = true,
                Width = 60,
                FlatStyle = FlatStyle.Flat
            };
            communityRequestsGrid.Columns.Add(voteColumn);

            communityRequestsGrid.CellContentClick += CommunityRequestsGrid_CellContentClick;
        }

        private void SetupMyRequestsGrid()
        {
            myRequestsGrid.Columns.Clear();

            myRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "AppId",
                HeaderText = "App ID",
                Width = 80,
                ReadOnly = true
            });

            myRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Name",
                HeaderText = "Game Name",
                Width = 400,
                ReadOnly = true
            });

            myRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "RequestDate",
                HeaderText = "Requested",
                Width = 150,
                ReadOnly = true
            });

            myRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Status",
                HeaderText = "Status",
                Width = 100,
                ReadOnly = true
            });

            myRequestsGrid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Votes",
                HeaderText = "Votes",
                Width = 80,
                ReadOnly = true
            });

            var removeColumn = new DataGridViewButtonColumn
            {
                Name = "Remove",
                HeaderText = "Action",
                Text = "Remove",
                UseColumnTextForButtonValue = true,
                Width = 80,
                FlatStyle = FlatStyle.Flat
            };
            myRequestsGrid.Columns.Add(removeColumn);

            myRequestsGrid.CellContentClick += MyRequestsGrid_CellContentClick;
        }

        private async void SearchButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchBox.Text))
            {
                statusLabel.Text = "Enter a search term";
                return;
            }

            statusLabel.Text = "Searching...";
            searchButton.Enabled = false;

            try
            {
                var dataTable = mainForm.GetDataTable();
                if (dataTable != null)
                {
                    var searchTerm = searchBox.Text.ToLower();
                    var filteredRows = dataTable.AsEnumerable()
                        .Where(row => row.Field<string>("Name")?.ToLower().Contains(searchTerm) == true)
                        .Take(100);  // Show more results

                    searchResultsGrid.Rows.Clear();

                    foreach (var row in filteredRows)
                    {
                        var appId = row.Field<int>("AppID").ToString();
                        var name = row.Field<string>("Name");

                        // Get stats for this game
                        var stats = await GetOrFetchGameStats(appId);
                        var isRequested = requestedGames.ContainsKey(appId);

                        // Prepare button values
                        string requestButtonText = isRequested ? $"✓ ({stats?.ActiveRequests ?? 0})" : "Request";
                        string uncrackableButtonText = stats?.UncrackableVotes >= 10 ? $"🚫 {stats.UncrackableVotes}" :
                                                       stats?.UncrackableVotes > 0 ? $"🚫 ({stats.UncrackableVotes})" : "🚫";

                        var gridRowIndex = searchResultsGrid.Rows.Add(
                            appId,
                            name,
                            stats?.ActiveRequests ?? 0,
                            stats?.UncrackableVotes ?? 0,
                            requestButtonText,
                            uncrackableButtonText
                        );

                        var gridRow = searchResultsGrid.Rows[gridRowIndex];

                        // Set button styling
                        if (isRequested)
                        {
                            gridRow.Cells["Request"].Style.BackColor = Color.FromArgb(0, 50, 0);
                        }

                        // Gray out uncrackable games
                        if (stats?.UncrackableVotes >= 10)
                        {
                            gridRow.DefaultCellStyle.BackColor = Color.FromArgb(30, 0, 0);
                            gridRow.DefaultCellStyle.ForeColor = Color.Gray;
                        }
                    }

                    statusLabel.Text = $"Found {searchResultsGrid.Rows.Count} games";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Search failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statusLabel.Text = "Search failed";
            }
            finally
            {
                searchButton.Enabled = true;
            }
        }

        private async Task<GameRequestStats> GetOrFetchGameStats(string appId)
        {
            if (!gameStats.ContainsKey(appId))
            {
                gameStats[appId] = await GetGameRequestStats(appId);
            }
            return gameStats[appId];
        }

        private void SearchResultsGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var row = searchResultsGrid.Rows[e.RowIndex];
            var appId = row.Cells["AppID"].Value?.ToString();

            if (gameStats.ContainsKey(appId))
            {
                var stats = gameStats[appId];

                // Gray out uncrackable games
                if (stats.UncrackableVotes >= 10)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(20, 0, 0);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(100, 100, 100);
                }
            }
        }

        private async void SearchResultsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            var appId = searchResultsGrid.Rows[e.RowIndex].Cells["AppID"].Value?.ToString();
            var gameName = searchResultsGrid.Rows[e.RowIndex].Cells["Name"].Value?.ToString();

            if (e.ColumnIndex == searchResultsGrid.Columns["Request"].Index)
            {
                await RequestGame(appId, gameName);
                // Update the button to show it's requested
                var stats = await GetOrFetchGameStats(appId);
                searchResultsGrid.Rows[e.RowIndex].Cells["Request"].Value = $"✓ ({stats?.ActiveRequests ?? 1})";
                searchResultsGrid.Rows[e.RowIndex].Cells["RequestCount"].Value = stats?.ActiveRequests ?? 1;
            }
            else if (e.ColumnIndex == searchResultsGrid.Columns["Uncrackable"].Index)
            {
                await VoteUncrackable(appId);
                var stats = await GetOrFetchGameStats(appId);
                searchResultsGrid.Rows[e.RowIndex].Cells["Uncrackable"].Value = $"🚫 ({stats?.UncrackableVotes ?? 1})";
                searchResultsGrid.Rows[e.RowIndex].Cells["UncrackableCount"].Value = stats?.UncrackableVotes ?? 1;

                if (stats?.UncrackableVotes >= 10)
                {
                    searchResultsGrid.Rows[e.RowIndex].DefaultCellStyle.BackColor = Color.FromArgb(20, 0, 0);
                    searchResultsGrid.Rows[e.RowIndex].DefaultCellStyle.ForeColor = Color.FromArgb(100, 100, 100);
                }
            }
        }

        private async void MyRequestsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == myRequestsGrid.Columns["Remove"].Index)
            {
                var appId = myRequestsGrid.Rows[e.RowIndex].Cells["AppId"].Value?.ToString();
                await RemoveRequest(appId);
            }
        }

        private async Task RequestGame(string appId, string gameName)
        {
            try
            {
                var userId = HWIDManager.GetUserId();
                var hwid = HWIDManager.GetHWID();

                // Check if already requested - if so, remove it
                if (requestedGames.ContainsKey(appId))
                {
                    await RequestAPI.RemoveRequest(appId, userId, hwid);
                    requestedGames.Remove(appId);
                    statusLabel.Text = $"Removed request for {gameName}";
                }
                else
                {
                    await RequestAPI.SubmitRequest(appId, gameName, userId, hwid);
                    requestedGames[appId] = new RequestAPI.RequestedGame
                    {
                        AppId = appId,
                        GameName = gameName,
                        RequestDate = DateTime.Now,
                        Status = "Active"
                    };
                    statusLabel.Text = $"Requested {gameName}";
                }

                await LoadMyRequests();
                await LoadCommunityRequests(); // Refresh community list

                // Update search results if visible
                foreach (DataGridViewRow row in searchResultsGrid.Rows)
                {
                    if (row.Cells["AppID"].Value?.ToString() == appId)
                    {
                        var stats = await GetOrFetchGameStats(appId);
                        var isRequested = requestedGames.ContainsKey(appId);
                        row.Cells["Request"].Value = isRequested ? $"✓ ({stats?.ActiveRequests ?? 0})" : "Request";
                        row.Cells["RequestCount"].Value = stats?.ActiveRequests ?? 0;
                        if (isRequested)
                        {
                            row.Cells["Request"].Style.BackColor = Color.FromArgb(0, 50, 0);
                        }
                        else
                        {
                            row.Cells["Request"].Style.BackColor = searchResultsGrid.DefaultCellStyle.BackColor;
                        }
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to request game: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task VoteUncrackable(string appId)
        {
            try
            {
                var userId = HWIDManager.GetStoredHWID();
                await StartupRequestChecker.VoteUncrackable(userId, appId);

                // Refresh stats
                gameStats[appId] = await StartupRequestChecker.GetGameRequestStats(appId);

                if (gameStats[appId]?.UncrackableVotes >= 10)
                {
                    statusLabel.Text = "⚠️ This game has been marked as uncrackable by the community";
                }
                else
                {
                    statusLabel.Text = $"Marked as potentially uncrackable ({gameStats[appId]?.UncrackableVotes}/10 votes)";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to vote: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task RemoveRequest(string appId)
        {
            try
            {
                var userId = HWIDManager.GetUserId();
                var hwid = HWIDManager.GetHWID();
                await RequestAPI.RemoveRequest(appId, userId, hwid);

                requestedGames.Remove(appId);
                await LoadMyRequests();
                await LoadCommunityRequests(); // Refresh to remove if no other requests
                statusLabel.Text = "Request removed";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to remove request: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void CommunityRequestsGrid_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            if (e.ColumnIndex == communityRequestsGrid.Columns["Vote"].Index)
            {
                var appId = communityRequestsGrid.Rows[e.RowIndex].Cells["AppId"].Value?.ToString();
                var gameName = communityRequestsGrid.Rows[e.RowIndex].Cells["Name"].Value?.ToString();

                // Vote is the same as requesting
                await RequestGame(appId, gameName);

                // Refresh the grid
                await LoadCommunityRequests();
            }
        }

        private async Task LoadMyRequests()
        {
            try
            {
                var userId = HWIDManager.GetStoredHWID();
                var activity = await RequestAPI.GetUserActivity(userId);

                var myRequests = activity?.ActiveRequests ?? new List<RequestAPI.RequestedGame>();
                myRequestsGrid.Rows.Clear();

                foreach (var request in myRequests)
                {
                    requestedGames[request.AppId] = request;
                    myRequestsGrid.Rows.Add(
                        request.AppId,
                        request.GameName,
                        request.RequestDate?.ToString("yyyy-MM-dd") ?? "",
                        request.Status ?? "Active",
                        request.VoteCount,
                        "Remove"
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load requests: {ex.Message}");
            }
        }

        private async Task LoadCommunityRequests()
        {
            try
            {
                var topRequests = await RequestAPI.GetTopRequestedGames(20);
                communityRequestsGrid.Rows.Clear();

                foreach (var request in topRequests)
                {
                    communityRequestsGrid.Rows.Add(
                        request.AppId,
                        request.GameName,
                        request.RequestCount,
                        request.LastRequested?.ToString("yyyy-MM-dd") ?? ""
                    );
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load community requests: {ex.Message}");
            }
        }

        private void SetupRefreshTimer()
        {
            refreshTimer = new Timer();
            refreshTimer.Interval = 60000; // Refresh every minute
            refreshTimer.Tick += async (s, e) =>
            {
                await LoadMyRequests();
                await LoadCommunityRequests();
            };
            refreshTimer.Start();
        }

        private void searchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchButton_Click(sender, e);
            }
        }

        private void RequestsWindow_FormClosed(object sender, FormClosedEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            refreshTimer?.Stop();
            refreshTimer?.Dispose();
            base.OnFormClosed(e);
        }

        // Add this method to the RequestsWindow class to fix CS0103 and ENC0046
        private async Task<GameRequestStats> GetGameRequestStats(string appId)
        {
            // Replace this with the actual implementation or API call as appropriate for your app.
            // Example: If StartupRequestChecker is the correct class, use it as below.
            return await SteamAppIdIdentifier.StartupRequestChecker.GetGameRequestStats(appId);
        }
    }

    public class SteamStoreGame
    {
        public string AppId { get; set; }
        public string Name { get; set; }
        public string ReleaseDate { get; set; }
        public string Price { get; set; }
    }
}
