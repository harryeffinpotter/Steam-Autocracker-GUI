using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public class TabbedShareForm : Form
    {
        private TabControl mainTabControl;
        private Form mainForm;
        private Dictionary<string, RequestAPI.RequestedGame> allRequests;
        private Dictionary<string, SteamGame> userGames;

        public TabbedShareForm(Form parent)
        {
            mainForm = parent;
            allRequests = new Dictionary<string, RequestAPI.RequestedGame>();
            userGames = new Dictionary<string, SteamGame>();
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Steam Auto-Cracker - Community Hub";
            this.Size = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 30);
            this.ForeColor = Color.White;

            // Hide main form
            mainForm.Visible = false;
            this.FormClosed += (s, e) => mainForm.Visible = true;

            // Create tab control
            mainTabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                SizeMode = TabSizeMode.Fixed,
                ItemSize = new Size(120, 40),
                Font = new Font("Segoe UI", 11, FontStyle.Bold)
            };

            // Custom tab drawing for dark theme
            mainTabControl.DrawItem += TabControl_DrawItem;

            // Add tabs
            var yourGamesTab = new TabPage("Your Games");
            var topWantedTab = new TabPage("Top Wanted");
            var yourStatsTab = new TabPage("Your Stats");

            mainTabControl.TabPages.Add(yourGamesTab);
            mainTabControl.TabPages.Add(topWantedTab);
            mainTabControl.TabPages.Add(yourStatsTab);

            // Initialize tab contents
            InitializeYourGamesTab(yourGamesTab);
            InitializeTopWantedTab(topWantedTab);
            InitializeYourStatsTab(yourStatsTab);

            this.Controls.Add(mainTabControl);

            // Load data
            _ = LoadAllData();
        }

        private void TabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            Rectangle tabRect = tabControl.GetTabRect(e.Index);

            // Colors
            Color backColor = e.Index == tabControl.SelectedIndex ?
                Color.FromArgb(50, 50, 70) : Color.FromArgb(30, 30, 40);
            Color textColor = e.Index == tabControl.SelectedIndex ?
                Color.White : Color.FromArgb(150, 150, 150);

            // Draw background
            using (var brush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(brush, tabRect);
            }

            // Draw border
            using (var pen = new Pen(Color.FromArgb(60, 60, 80)))
            {
                e.Graphics.DrawRectangle(pen, tabRect);
            }

            // Draw text
            string tabText = tabControl.TabPages[e.Index].Text;
            using (var brush = new SolidBrush(textColor))
            {
                var textRect = new RectangleF(tabRect.X, tabRect.Y, tabRect.Width, tabRect.Height);
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };
                e.Graphics.DrawString(tabText, tabControl.Font, brush, textRect, sf);
            }

            // Add notification badges
            if (e.Index == 1) // Top Wanted tab
            {
                int urgentCount = allRequests.Values.Count(r => r.RequestCount >= 10);
                if (urgentCount > 0)
                {
                    var badgeRect = new Rectangle(tabRect.Right - 25, tabRect.Top + 5, 20, 15);
                    using (var brush = new SolidBrush(Color.Red))
                    {
                        e.Graphics.FillEllipse(brush, badgeRect);
                    }
                    using (var brush = new SolidBrush(Color.White))
                    {
                        var sf = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };
                        e.Graphics.DrawString(urgentCount.ToString(),
                            new Font("Segoe UI", 8, FontStyle.Bold), brush, badgeRect, sf);
                    }
                }
            }
        }

        private void InitializeYourGamesTab(TabPage tab)
        {
            tab.BackColor = Color.FromArgb(25, 25, 35);

            // Header panel
            var headerPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(35, 35, 45)
            };

            var lblHeader = new Label
            {
                Text = "Your Steam Library - Share with Community",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(20, 15),
                Size = new Size(500, 30)
            };

            var lblHonor = new Label
            {
                Text = $"Your Honor: {HWIDManager.GetHonorScore()} ⭐",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.Gold,
                Location = new Point(20, 35),
                Size = new Size(200, 20),
                Name = "HonorLabel"
            };

            headerPanel.Controls.Add(lblHeader);
            headerPanel.Controls.Add(lblHonor);

            // Games grid
            var gamesGrid = CreateYourGamesGrid();
            gamesGrid.Name = "YourGamesGrid";

            tab.Controls.Add(gamesGrid);
            tab.Controls.Add(headerPanel);
        }

        private void InitializeTopWantedTab(TabPage tab)
        {
            tab.BackColor = Color.FromArgb(25, 25, 35);

            // Header
            var headerPanel = new Panel
            {
                Height = 60,
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(35, 35, 45)
            };

            var lblHeader = new Label
            {
                Text = "🔥 Community's Most Wanted Games",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 100, 100),
                Location = new Point(20, 10),
                Size = new Size(600, 25),
                Name = "TopWantedHeader"
            };

            var lblSubheader = new Label
            {
                Text = "These games are in high demand - become a hero by providing them!",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.Gray,
                Location = new Point(20, 35),
                Size = new Size(600, 20)
            };

            headerPanel.Controls.Add(lblHeader);
            headerPanel.Controls.Add(lblSubheader);

            // Top wanted grid
            var topWantedGrid = CreateTopWantedGrid();
            topWantedGrid.Name = "TopWantedGrid";

            tab.Controls.Add(topWantedGrid);
            tab.Controls.Add(headerPanel);
        }

        private void InitializeYourStatsTab(TabPage tab)
        {
            tab.BackColor = Color.FromArgb(25, 25, 35);

            // Split container for stats overview and detailed lists
            var splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 280,
                BackColor = Color.FromArgb(40, 40, 50)
            };

            // Top panel - stats overview
            var statsOverviewPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };

            // Main stats card (smaller)
            var mainStatsCard = CreateMainStatsCard();
            mainStatsCard.Size = new Size(280, 250);
            mainStatsCard.Location = new Point(10, 10);

            // Quick summary card
            var summaryCard = CreateQuickSummaryCard();
            summaryCard.Location = new Point(300, 10);

            // Notifications card (FULFILLED REQUESTS!)
            var notificationsCard = CreateNotificationsCard();
            notificationsCard.Location = new Point(590, 10);

            statsOverviewPanel.Controls.AddRange(new Control[] {
                mainStatsCard, summaryCard, notificationsCard
            });

            // Bottom panel - detailed activity lists
            var detailsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding = new Padding(10)
            };

            // Tabbed view for different activity types
            var activityTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                DrawMode = TabDrawMode.OwnerDrawFixed,
                ItemSize = new Size(150, 30),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            activityTabs.DrawItem += (s, e) => {
                var tabControl = s as TabControl;
                var tabRect = tabControl.GetTabRect(e.Index);
                var backColor = e.Index == tabControl.SelectedIndex ? Color.FromArgb(50, 50, 70) : Color.FromArgb(30, 30, 40);
                var textColor = e.Index == tabControl.SelectedIndex ? Color.White : Color.Gray;

                using (var brush = new SolidBrush(backColor))
                    e.Graphics.FillRectangle(brush, tabRect);
                using (var brush = new SolidBrush(textColor))
                {
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString(tabControl.TabPages[e.Index].Text, tabControl.Font, brush, tabRect, sf);
                }
            };

            // Your Requests tab
            var requestsTab = new TabPage("📢 Your Requests");
            requestsTab.BackColor = Color.FromArgb(25, 25, 35);
            var requestsGrid = CreateYourRequestsGrid();
            requestsTab.Controls.Add(requestsGrid);

            // Fulfilled Requests tab (THE GOLDEN TAB!)
            var fulfilledTab = new TabPage("🎉 Fulfilled Requests");
            fulfilledTab.BackColor = Color.FromArgb(25, 25, 35);
            var fulfilledGrid = CreateFulfilledRequestsGrid();
            fulfilledTab.Controls.Add(fulfilledGrid);

            // Your Contributions tab
            var contributionsTab = new TabPage("🏆 Your Contributions");
            contributionsTab.BackColor = Color.FromArgb(25, 25, 35);
            var contributionsGrid = CreateContributionsGrid();
            contributionsTab.Controls.Add(contributionsGrid);

            activityTabs.TabPages.AddRange(new TabPage[] { requestsTab, fulfilledTab, contributionsTab });

            detailsPanel.Controls.Add(activityTabs);

            splitContainer.Panel1.Controls.Add(statsOverviewPanel);
            splitContainer.Panel2.Controls.Add(detailsPanel);

            tab.Controls.Add(splitContainer);
        }

        private DataGridView CreateYourGamesGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(15, 15, 20),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };

            // Style
            grid.DefaultCellStyle.BackColor = Color.FromArgb(25, 25, 30);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 70);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 50);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Columns
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GameName", HeaderText = "Game", Width = 300 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "BuildID", HeaderText = "Your Build", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestStatus", HeaderText = "Community Needs", Width = 150 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestType", HeaderText = "Type", Width = 100 });

            var shareCleanBtn = new DataGridViewButtonColumn { Name = "ShareClean", HeaderText = "Share Clean", Width = 120 };
            var shareCrackedBtn = new DataGridViewButtonColumn { Name = "ShareCracked", HeaderText = "Share Cracked", Width = 120 };
            var requestBtn = new DataGridViewButtonColumn { Name = "RequestUpdate", HeaderText = "Request Update", Width = 130 };

            grid.Columns.AddRange(new DataGridViewColumn[] { shareCleanBtn, shareCrackedBtn, requestBtn });

            // Hidden columns
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AppID", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "InstallPath", Visible = false });

            return grid;
        }

        private DataGridView CreateTopWantedGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(15, 15, 20),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowHeadersVisible = false,
                EnableHeadersVisualStyles = false
            };

            // Style
            grid.DefaultCellStyle.BackColor = Color.FromArgb(25, 25, 30);
            grid.DefaultCellStyle.ForeColor = Color.White;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(50, 50, 70);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 50);
            grid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);

            // Columns
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Rank", HeaderText = "#", Width = 40 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GameName", HeaderText = "Game", Width = 350 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Requests", HeaderText = "Requests", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TypeNeeded", HeaderText = "Type", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DaysOld", HeaderText = "Days Old", Width = 100 });

            var storeBtn = new DataGridViewButtonColumn { Name = "ViewStore", HeaderText = "Steam Store", Width = 120 };
            var voteBtn = new DataGridViewButtonColumn { Name = "VoteUncrackable", HeaderText = "Vote Uncrackable", Width = 140 };

            grid.Columns.AddRange(new DataGridViewColumn[] { storeBtn, voteBtn });

            // Hidden columns
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AppID", Visible = false });

            return grid;
        }

        private Panel CreateMainStatsCard()
        {
            var card = new Panel
            {
                Size = new Size(350, 250),
                BackColor = Color.FromArgb(35, 35, 45),
                BorderStyle = BorderStyle.FixedSingle
            };

            var title = new Label
            {
                Text = "📊 Your Community Impact",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(20, 15),
                Size = new Size(300, 25)
            };

            var honorLabel = new Label
            {
                Text = $"⭐ Honor Score: {HWIDManager.GetHonorScore()}",
                Font = new Font("Segoe UI", 12),
                ForeColor = Color.Gold,
                Location = new Point(20, 50),
                Size = new Size(300, 25),
                Name = "MainHonorScore"
            };

            var fulfilledLabel = new Label
            {
                Text = "🏆 Fulfilled: Loading...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(150, 255, 150),
                Location = new Point(20, 85),
                Size = new Size(300, 25),
                Name = "FulfilledCount"
            };

            var requestedLabel = new Label
            {
                Text = "📢 Requested: Loading...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(255, 200, 100),
                Location = new Point(20, 115),
                Size = new Size(300, 25),
                Name = "RequestedCount"
            };

            var filledLabel = new Label
            {
                Text = "🙏 Filled: Loading...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.FromArgb(150, 150, 255),
                Location = new Point(20, 145),
                Size = new Size(300, 25),
                Name = "FilledCount"
            };

            var ratioLabel = new Label
            {
                Text = "📈 Give/Take Ratio: Loading...",
                Font = new Font("Segoe UI", 11),
                ForeColor = Color.White,
                Location = new Point(20, 175),
                Size = new Size(300, 25),
                Name = "RatioLabel"
            };

            var typeLabel = new Label
            {
                Text = "🏷️ User Type: Loading...",
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 255, 100),
                Location = new Point(20, 205),
                Size = new Size(300, 25),
                Name = "UserTypeLabel"
            };

            card.Controls.AddRange(new Control[] {
                title, honorLabel, fulfilledLabel, requestedLabel, filledLabel, ratioLabel, typeLabel
            });

            return card;
        }

        private Panel CreateBreakdownCard()
        {
            var card = new Panel
            {
                Size = new Size(350, 250),
                BackColor = Color.FromArgb(35, 35, 45),
                BorderStyle = BorderStyle.FixedSingle
            };

            var title = new Label
            {
                Text = "📋 Detailed Breakdown",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(20, 15),
                Size = new Size(300, 25)
            };

            // This will be populated with detailed stats
            var detailsLabel = new Label
            {
                Text = "Loading detailed statistics...",
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.White,
                Location = new Point(20, 50),
                Size = new Size(300, 180),
                Name = "DetailedBreakdown"
            };

            card.Controls.Add(title);
            card.Controls.Add(detailsLabel);

            return card;
        }

        private Panel CreateRecentActivityCard()
        {
            var card = new Panel
            {
                Size = new Size(350, 200),
                BackColor = Color.FromArgb(35, 35, 45),
                BorderStyle = BorderStyle.FixedSingle
            };

            var title = new Label
            {
                Text = "🕒 Recent Activity",
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(20, 15),
                Size = new Size(300, 25)
            };

            var activityList = new ListBox
            {
                Location = new Point(20, 50),
                Size = new Size(300, 130),
                BackColor = Color.FromArgb(25, 25, 35),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Name = "RecentActivityList"
            };

            card.Controls.Add(title);
            card.Controls.Add(activityList);

            return card;
        }

        private Panel CreateQuickSummaryCard()
        {
            var card = new Panel
            {
                Size = new Size(280, 250),
                BackColor = Color.FromArgb(35, 35, 45),
                BorderStyle = BorderStyle.FixedSingle
            };

            var title = new Label
            {
                Text = "📋 Quick Summary",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(15, 10),
                Size = new Size(250, 25)
            };

            var summaryText = new Label
            {
                Text = "Loading summary...",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.White,
                Location = new Point(15, 40),
                Size = new Size(250, 200),
                Name = "QuickSummary"
            };

            card.Controls.Add(title);
            card.Controls.Add(summaryText);

            return card;
        }

        private Panel CreateNotificationsCard()
        {
            var card = new Panel
            {
                Size = new Size(280, 250),
                BackColor = Color.FromArgb(0, 50, 0), // Green tint for good news!
                BorderStyle = BorderStyle.FixedSingle
            };

            var title = new Label
            {
                Text = "🎉 FULFILLED REQUESTS!",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 255, 100),
                Location = new Point(15, 10),
                Size = new Size(250, 25)
            };

            var subtitle = new Label
            {
                Text = "Your wishes came true!",
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.FromArgb(150, 255, 150),
                Location = new Point(15, 35),
                Size = new Size(250, 20)
            };

            var notificationsList = new ListBox
            {
                Location = new Point(15, 60),
                Size = new Size(250, 160),
                BackColor = Color.FromArgb(20, 40, 20),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 9),
                Name = "FulfilledNotifications"
            };

            var viewAllButton = new Button
            {
                Text = "View All →",
                Location = new Point(190, 225),
                Size = new Size(75, 20),
                BackColor = Color.FromArgb(0, 100, 0),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8)
            };
            viewAllButton.Click += (s, e) => {
                // Switch to fulfilled requests tab
                var parentTab = card.FindForm().Controls.Find("TabControl", true).FirstOrDefault() as TabControl;
                if (parentTab != null) parentTab.SelectedIndex = 1; // Fulfilled tab
            };

            card.Controls.AddRange(new Control[] { title, subtitle, notificationsList, viewAllButton });

            return card;
        }

        private DataGridView CreateYourRequestsGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(15, 15, 20),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Name = "YourRequestsGrid"
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GameName", HeaderText = "Game Requested" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestedDate", HeaderText = "Requested", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "Status", Width = 120 });

            var cancelBtn = new DataGridViewButtonColumn { Name = "Cancel", HeaderText = "Action", Width = 80 };
            grid.Columns.Add(cancelBtn);

            return grid;
        }

        private DataGridView CreateFulfilledRequestsGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(15, 15, 20),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Name = "FulfilledRequestsGrid"
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GameName", HeaderText = "Game" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FulfilledDate", HeaderText = "Fulfilled", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FulfilledBy", HeaderText = "Hero", Width = 100 });

            var downloadBtn = new DataGridViewButtonColumn { Name = "Download", HeaderText = "Get It!", Width = 100 };
            downloadBtn.DefaultCellStyle.BackColor = Color.FromArgb(0, 150, 0);
            downloadBtn.DefaultCellStyle.ForeColor = Color.White;
            grid.Columns.Add(downloadBtn);

            var rinBtn = new DataGridViewButtonColumn { Name = "OpenRIN", HeaderText = "CS.RIN", Width = 80 };
            rinBtn.DefaultCellStyle.BackColor = Color.FromArgb(0, 100, 200);
            rinBtn.DefaultCellStyle.ForeColor = Color.White;
            grid.Columns.Add(rinBtn);

            // Hidden columns
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DownloadUrl", Visible = false });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "AppId", Visible = false });

            return grid;
        }

        private DataGridView CreateContributionsGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                BackgroundColor = Color.FromArgb(15, 15, 20),
                ForeColor = Color.White,
                GridColor = Color.FromArgb(40, 40, 50),
                BorderStyle = BorderStyle.None,
                AllowUserToAddRows = false,
                ReadOnly = true,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                Name = "ContributionsGrid"
            };

            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "GameName", HeaderText = "Game Shared" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", Width = 100 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SharedDate", HeaderText = "Shared", Width = 120 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "RequestsFulfilled", HeaderText = "Helped", Width = 80 });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "HonorEarned", HeaderText = "Honor", Width = 80 });

            return grid;
        }

        private async Task LoadAllData()
        {
            // Load requests
            var requests = await RequestAPI.GetActiveRequests();
            foreach (var req in requests)
            {
                allRequests[req.AppId] = req;
            }

            // Load user games
            var steamGames = ScanSteamLibraries();
            foreach (var game in steamGames)
            {
                userGames[game.AppId] = game;
            }

            // Update UI
            await UpdateYourGamesTab();
            await UpdateTopWantedTab();
            await UpdateYourStatsTab();
        }

        private async Task UpdateYourGamesTab()
        {
            var grid = this.Controls.Find("YourGamesGrid", true).FirstOrDefault() as DataGridView;
            if (grid == null) return;

            grid.Rows.Clear();

            // Add owned games with request highlighting
            var sortedGames = userGames.Values.OrderByDescending(g =>
                allRequests.ContainsKey(g.AppId) ? allRequests[g.AppId].RequestCount + 1000 : 0);

            foreach (var game in sortedGames)
            {
                var row = grid.Rows[grid.Rows.Add()];
                row.Cells["GameName"].Value = game.Name;
                row.Cells["BuildID"].Value = game.BuildId;
                row.Cells["AppID"].Value = game.AppId;
                row.Cells["InstallPath"].Value = game.InstallDir;

                if (allRequests.ContainsKey(game.AppId))
                {
                    var req = allRequests[game.AppId];
                    row.Cells["RequestStatus"].Value = $"{req.RequestCount} requests";
                    row.Cells["RequestType"].Value = req.RequestType;

                    // Highlight buttons based on request type
                    if (req.RequestType == "Clean" || req.RequestType == "Both")
                    {
                        row.Cells["ShareClean"].Value = "📦 NEEDED!";
                        row.Cells["ShareClean"].Style.BackColor = Color.FromArgb(255, 20, 147);
                        row.Cells["ShareClean"].Style.ForeColor = Color.White;
                    }
                    if (req.RequestType == "Cracked" || req.RequestType == "Both")
                    {
                        row.Cells["ShareCracked"].Value = "🎮 NEEDED!";
                        row.Cells["ShareCracked"].Style.BackColor = Color.FromArgb(255, 20, 147);
                        row.Cells["ShareCracked"].Style.ForeColor = Color.White;
                    }
                }
                else
                {
                    row.Cells["RequestStatus"].Value = "No requests";
                    row.Cells["RequestType"].Value = "-";
                    row.Cells["ShareClean"].Value = "📦 Share Clean";
                    row.Cells["ShareCracked"].Value = "🎮 Share Cracked";
                }

                row.Cells["RequestUpdate"].Value = "📢 Request Update";
            }
        }

        private async Task UpdateTopWantedTab()
        {
            var grid = this.Controls.Find("TopWantedGrid", true).FirstOrDefault() as DataGridView;
            if (grid == null) return;

            grid.Rows.Clear();

            var topRequests = allRequests.Values
                .OrderByDescending(r => r.RequestCount)
                .Take(50)
                .ToList();

            for (int i = 0; i < topRequests.Count; i++)
            {
                var req = topRequests[i];
                var row = grid.Rows[grid.Rows.Add()];

                row.Cells["Rank"].Value = $"#{i + 1}";
                row.Cells["GameName"].Value = req.GameName;
                row.Cells["Requests"].Value = req.RequestCount;
                row.Cells["TypeNeeded"].Value = req.RequestType;
                row.Cells["DaysOld"].Value = $"{(DateTime.UtcNow - req.FirstRequested).Days}d";
                row.Cells["AppID"].Value = req.AppId;

                row.Cells["ViewStore"].Value = "🛒 Steam Store";
                row.Cells["VoteUncrackable"].Value = "🚫 Vote Uncrackable";

                // Apply urgency coloring
                ApplyUrgencyColoring(row, req, i);
            }

            // Update header with urgent count
            var header = this.Controls.Find("TopWantedHeader", true).FirstOrDefault() as Label;
            if (header != null)
            {
                int urgentCount = topRequests.Count(r => r.RequestCount >= 10);
                header.Text = urgentCount > 0 ?
                    $"🚨 {urgentCount} CRITICALLY NEEDED + {topRequests.Count - urgentCount} More Wanted" :
                    $"🔥 {topRequests.Count} Community Requested Games";
            }
        }

        private async Task UpdateYourStatsTab()
        {
            try
            {
                var stats = await RequestAPI.GetDetailedUserStats(HWIDManager.GetUserId());
                var userActivity = await RequestAPI.GetUserActivity(HWIDManager.GetUserId());

                // Update main stats card
                var fulfilledLabel = this.Controls.Find("FulfilledCount", true).FirstOrDefault() as Label;
                var requestedLabel = this.Controls.Find("RequestedCount", true).FirstOrDefault() as Label;
                var filledLabel = this.Controls.Find("FilledCount", true).FirstOrDefault() as Label;
                var ratioLabel = this.Controls.Find("RatioLabel", true).FirstOrDefault() as Label;
                var typeLabel = this.Controls.Find("UserTypeLabel", true).FirstOrDefault() as Label;

                if (fulfilledLabel != null) fulfilledLabel.Text = $"🏆 Fulfilled: {stats.Fulfilled}";
                if (requestedLabel != null) requestedLabel.Text = $"📢 Requested: {stats.Requested}";
                if (filledLabel != null) filledLabel.Text = $"🙏 Filled: {stats.Filled}";
                if (ratioLabel != null) ratioLabel.Text = $"📈 Give/Take Ratio: {stats.GetGiveToTakeRatio():F1}:1";
                if (typeLabel != null) typeLabel.Text = $"🏷️ {stats.GetUserType()}";

                // Update fulfilled notifications (THE GOLDEN FEATURE!)
                var notificationsList = this.Controls.Find("FulfilledNotifications", true).FirstOrDefault() as ListBox;
                if (notificationsList != null)
                {
                    notificationsList.Items.Clear();
                    var recentFulfilled = userActivity.FulfilledRequests.Take(5);
                    foreach (var fulfilled in recentFulfilled)
                    {
                        notificationsList.Items.Add($"🎉 {fulfilled.GameName} - {fulfilled.Type}");
                    }
                    if (!recentFulfilled.Any())
                    {
                        notificationsList.Items.Add("No fulfilled requests yet");
                        notificationsList.Items.Add("Make some requests!");
                    }
                }

                // Update grids
                await UpdateRequestsGrid(userActivity.ActiveRequests);
                await UpdateFulfilledGrid(userActivity.FulfilledRequests);
                await UpdateContributionsGrid(userActivity.Contributions);
            }
            catch { }
        }

        private async Task UpdateRequestsGrid(List<UserRequestActivity> requests)
        {
            var grid = this.Controls.Find("YourRequestsGrid", true).FirstOrDefault() as DataGridView;
            if (grid == null) return;

            grid.Rows.Clear();
            foreach (var req in requests)
            {
                var row = grid.Rows[grid.Rows.Add()];
                row.Cells["GameName"].Value = req.GameName;
                row.Cells["Type"].Value = req.RequestType;
                row.Cells["RequestedDate"].Value = req.RequestedAt.ToString("MMM dd");
                row.Cells["Status"].Value = "Waiting...";
                row.Cells["Cancel"].Value = "❌ Cancel";
            }
        }

        private async Task UpdateFulfilledGrid(List<FulfilledRequestActivity> fulfilled)
        {
            var grid = this.Controls.Find("FulfilledRequestsGrid", true).FirstOrDefault() as DataGridView;
            if (grid == null) return;

            grid.Rows.Clear();
            foreach (var item in fulfilled)
            {
                var row = grid.Rows[grid.Rows.Add()];
                row.Cells["GameName"].Value = item.GameName;
                row.Cells["Type"].Value = item.Type;
                row.Cells["FulfilledDate"].Value = item.FulfilledAt.ToString("MMM dd");
                row.Cells["FulfilledBy"].Value = "Hero #" + item.FulfilledByUserId.Substring(0, 8);
                row.Cells["Download"].Value = "🎁 GET IT!";
                row.Cells["OpenRIN"].Value = "💬 RIN";
                row.Cells["DownloadUrl"].Value = item.DownloadUrl;
                row.Cells["AppId"].Value = item.AppId;

                // Make fulfilled items glow green
                row.DefaultCellStyle.BackColor = Color.FromArgb(0, 40, 0);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 255, 150);
            }
        }

        private async Task UpdateContributionsGrid(List<ContributionActivity> contributions)
        {
            var grid = this.Controls.Find("ContributionsGrid", true).FirstOrDefault() as DataGridView;
            if (grid == null) return;

            grid.Rows.Clear();
            foreach (var contrib in contributions)
            {
                var row = grid.Rows[grid.Rows.Add()];
                row.Cells["GameName"].Value = contrib.GameName;
                row.Cells["Type"].Value = contrib.ShareType;
                row.Cells["SharedDate"].Value = contrib.SharedAt.ToString("MMM dd");
                row.Cells["RequestsFulfilled"].Value = contrib.RequestsFulfilled.ToString();
                row.Cells["HonorEarned"].Value = $"+{contrib.HonorEarned}";

                // Gold tint for contributions
                if (contrib.RequestsFulfilled > 0)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(40, 30, 0);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(255, 220, 100);
                }
            }
        }

        private void ApplyUrgencyColoring(DataGridViewRow row, RequestAPI.RequestedGame req, int rank)
        {
            // Calculate red intensity
            int requestIntensity = Math.Min(req.RequestCount * 8, 200);
            float rankModifier = 1.0f - (rank * 0.05f);
            rankModifier = Math.Max(0.2f, rankModifier);

            int finalRed = (int)(requestIntensity * rankModifier);
            finalRed = Math.Max(30, Math.Min(255, finalRed));

            int green = Math.Min(finalRed / 4, 50);
            int blue = Math.Min(finalRed / 6, 30);

            row.DefaultCellStyle.BackColor = Color.FromArgb(finalRed, green, blue);
            row.DefaultCellStyle.ForeColor = finalRed > 128 ? Color.Black : Color.White;

            if (req.RequestCount >= 10)
            {
                row.Cells["Requests"].Style.ForeColor = Color.Yellow;
                row.Cells["Requests"].Style.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            }
        }

        private List<SteamGame> ScanSteamLibraries()
        {
            // Implementation would scan Steam libraries
            return new List<SteamGame>();
        }

        private class SteamGame
        {
            public string AppId { get; set; }
            public string Name { get; set; }
            public string InstallDir { get; set; }
            public string BuildId { get; set; }
        }

        public class UserRequestActivity
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public string RequestType { get; set; }
            public DateTime RequestedAt { get; set; }
            public bool IsFulfilled { get; set; }
        }

        public class FulfilledRequestActivity
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public string Type { get; set; }
            public DateTime FulfilledAt { get; set; }
            public string FulfilledByUserId { get; set; }
            public string DownloadUrl { get; set; }
        }

        public class ContributionActivity
        {
            public string AppId { get; set; }
            public string GameName { get; set; }
            public string ShareType { get; set; }
            public DateTime SharedAt { get; set; }
            public int RequestsFulfilled { get; set; }
            public int HonorEarned { get; set; }
        }

        public class UserActivityData
        {
            public List<UserRequestActivity> ActiveRequests { get; set; } = new List<UserRequestActivity>();
            public List<FulfilledRequestActivity> FulfilledRequests { get; set; } = new List<FulfilledRequestActivity>();
            public List<ContributionActivity> Contributions { get; set; } = new List<ContributionActivity>();
        }
    }
}