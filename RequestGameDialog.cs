using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SteamAppIdIdentifier
{
    public class RequestGameDialog : Form
    {
        private RadioButton radioClean;
        private RadioButton radioCracked;
        private RadioButton radioBoth;
        private Button btnRequest;
        private Button btnCancel;
        private Label lblGame;
        private Label lblType;
        private string appId;
        private string gameName;

        public RequestType SelectedType { get; private set; }

        public enum RequestType
        {
            Clean,
            Cracked,
            Both
        }

        public RequestGameDialog(string gameName, string appId)
        {
            this.gameName = gameName;
            this.appId = appId;
            InitializeDialog();
        }

        private void InitializeDialog()
        {
            this.Text = "Request Game";
            this.Size = new Size(400, 250);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(30, 30, 30);
            this.ForeColor = Color.White;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // Game name label
            lblGame = new Label
            {
                Text = $"Request: {gameName}",
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.FromArgb(100, 200, 255),
                Location = new Point(20, 20),
                Size = new Size(350, 30),
                TextAlign = ContentAlignment.MiddleCenter
            };

            // Type label
            lblType = new Label
            {
                Text = "What type of release do you need?",
                Font = new Font("Segoe UI", 10),
                Location = new Point(20, 60),
                Size = new Size(350, 25)
            };

            // Radio buttons
            radioClean = new RadioButton
            {
                Text = "Clean Files (No crack)",
                Location = new Point(50, 95),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(150, 255, 150),
                Font = new Font("Segoe UI", 10),
                Checked = true
            };

            radioCracked = new RadioButton
            {
                Text = "Cracked (Ready to play)",
                Location = new Point(50, 120),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(255, 200, 100),
                Font = new Font("Segoe UI", 10)
            };

            radioBoth = new RadioButton
            {
                Text = "Both Clean & Cracked",
                Location = new Point(50, 145),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(255, 150, 255),
                Font = new Font("Segoe UI", 10)
            };

            // Buttons
            btnRequest = new Button
            {
                Text = "Submit Request",
                Location = new Point(80, 180),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnRequest.Click += BtnRequest_Click;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(210, 180),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10)
            };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            // Add controls
            this.Controls.AddRange(new Control[] {
                lblGame, lblType,
                radioClean, radioCracked, radioBoth,
                btnRequest, btnCancel
            });
        }

        private async void BtnRequest_Click(object sender, EventArgs e)
        {
            // Determine selected type
            if (radioClean.Checked)
                SelectedType = RequestType.Clean;
            else if (radioCracked.Checked)
                SelectedType = RequestType.Cracked;
            else
                SelectedType = RequestType.Both;

            // Disable controls
            btnRequest.Enabled = false;
            btnRequest.Text = "Submitting...";

            // Submit request
            bool success = await SubmitRequest();

            if (success)
            {
                this.DialogResult = DialogResult.OK;
            }
            else
            {
                MessageBox.Show("Failed to submit request. Please try again.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnRequest.Enabled = true;
                btnRequest.Text = "Submit Request";
            }
        }

        private async Task<bool> SubmitRequest()
        {
            try
            {
                return await RequestAPI.SubmitGameRequest(appId, gameName, SelectedType.ToString());
            }
            catch
            {
                return false;
            }
        }
    }
}