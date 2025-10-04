using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

public class RinLoginForm : Form
{
    private TextBox usernameBox;
    private TextBox passwordBox;
    private Button loginButton;
    private Label statusLabel;
    private CheckBox saveCredsBox;

    public CookieContainer SessionCookies { get; private set; }

    public RinLoginForm()
    {
        InitializeComponent();

        // Load saved credentials if they exist (encrypted)
        LoadSavedCredentials();
    }

    private void InitializeComponent()
    {
        this.Text = "CS.RIN.RU Login";
        this.Size = new Size(400, 250);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Color.FromArgb(30, 30, 30);

        var titleLabel = new Label
        {
            Text = "Login to CS.RIN.RU",
            Font = new Font("Segoe UI", 14, FontStyle.Bold),
            ForeColor = Color.White,
            Location = new Point(20, 20),
            Size = new Size(360, 30),
            TextAlign = ContentAlignment.MiddleCenter
        };

        var userLabel = new Label
        {
            Text = "Username:",
            ForeColor = Color.White,
            Location = new Point(20, 60),
            Size = new Size(80, 25)
        };

        usernameBox = new TextBox
        {
            Location = new Point(100, 60),
            Size = new Size(260, 25),
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White
        };

        var passLabel = new Label
        {
            Text = "Password:",
            ForeColor = Color.White,
            Location = new Point(20, 90),
            Size = new Size(80, 25)
        };

        passwordBox = new TextBox
        {
            Location = new Point(100, 90),
            Size = new Size(260, 25),
            BackColor = Color.FromArgb(50, 50, 50),
            ForeColor = Color.White,
            PasswordChar = '•'
        };

        saveCredsBox = new CheckBox
        {
            Text = "Save credentials (encrypted)",
            ForeColor = Color.White,
            Location = new Point(100, 120),
            Size = new Size(200, 25),
            Checked = true
        };

        loginButton = new Button
        {
            Text = "Login",
            Location = new Point(150, 150),
            Size = new Size(100, 30),
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        loginButton.Click += LoginButton_Click;

        statusLabel = new Label
        {
            Text = "",
            ForeColor = Color.Yellow,
            Location = new Point(20, 190),
            Size = new Size(360, 25),
            TextAlign = ContentAlignment.MiddleCenter
        };

        this.Controls.AddRange(new Control[] {
            titleLabel, userLabel, usernameBox,
            passLabel, passwordBox, saveCredsBox,
            loginButton, statusLabel
        });
    }

    private async void LoginButton_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrEmpty(usernameBox.Text) || string.IsNullOrEmpty(passwordBox.Text))
        {
            statusLabel.Text = "Enter username and password";
            statusLabel.ForeColor = Color.Red;
            return;
        }

        loginButton.Enabled = false;
        statusLabel.Text = "Logging in...";
        statusLabel.ForeColor = Color.Yellow;

        bool success = await LoginToRin(usernameBox.Text, passwordBox.Text);

        if (success)
        {
            statusLabel.Text = "Login successful!";
            statusLabel.ForeColor = Color.Lime;

            if (saveCredsBox.Checked)
            {
                SaveCredentials(usernameBox.Text, passwordBox.Text);
            }

            this.DialogResult = DialogResult.OK;
            await Task.Delay(1000);
            this.Close();
        }
        else
        {
            statusLabel.Text = "Login failed - check credentials";
            statusLabel.ForeColor = Color.Red;
            loginButton.Enabled = true;
        }
    }

    private async Task<bool> LoginToRin(string username, string password)
    {
        try
        {
            var handler = new HttpClientHandler();
            handler.CookieContainer = new CookieContainer();
            handler.AllowAutoRedirect = false;

            using (var client = new HttpClient(handler))
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");

                // Step 1: Get login page to get session ID
                var loginPageUrl = "https://cs.rin.ru/forum/ucp.php?mode=login";
                var loginPage = await client.GetAsync(loginPageUrl);
                var loginPageHtml = await loginPage.Content.ReadAsStringAsync();

                // Extract SID from login form
                var sidMatch = System.Text.RegularExpressions.Regex.Match(loginPageHtml, @"sid=([a-f0-9]+)");
                string sid = sidMatch.Success ? sidMatch.Groups[1].Value : "";

                System.Diagnostics.Debug.WriteLine($"[RIN LOGIN] Got SID: {sid}");

                // Step 2: Submit login form
                var loginData = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("username", username),
                    new KeyValuePair<string, string>("password", password),
                    new KeyValuePair<string, string>("sid", sid),
                    new KeyValuePair<string, string>("redirect", "./ucp.php?mode=login"),
                    new KeyValuePair<string, string>("login", "Login"),
                    new KeyValuePair<string, string>("autologin", "on")
                });

                var loginResponse = await client.PostAsync(loginPageUrl, loginData);

                System.Diagnostics.Debug.WriteLine($"[RIN LOGIN] Response: {loginResponse.StatusCode}");

                // Check if login successful (usually redirects on success)
                if (loginResponse.StatusCode == HttpStatusCode.Found ||
                    loginResponse.StatusCode == HttpStatusCode.Redirect)
                {
                    // Save the cookies
                    this.SessionCookies = handler.CookieContainer;

                    // Test by checking if we can search
                    var testUrl = "https://cs.rin.ru/forum/search.php?keywords=test";
                    var testResponse = await client.GetAsync(testUrl);
                    var testHtml = await testResponse.Content.ReadAsStringAsync();

                    // If we're logged in, we won't see "not authorised"
                    return !testHtml.Contains("not authorised") && !testHtml.Contains("Login");
                }

                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[RIN LOGIN ERROR] {ex.Message}");
            return false;
        }
    }

    private void SaveCredentials(string username, string password)
    {
        // Encrypt and save to user settings
        // Implementation depends on your app's settings system
        try
        {
            var encryptedUser = Convert.ToBase64String(Encoding.UTF8.GetBytes(username));
            var encryptedPass = Convert.ToBase64String(Encoding.UTF8.GetBytes(password));

            // Save to registry or settings file
            Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\RinScraper")
                .SetValue("u", encryptedUser);
            Microsoft.Win32.Registry.CurrentUser.CreateSubKey(@"Software\RinScraper")
                .SetValue("p", encryptedPass);
        }
        catch { }
    }

    private void LoadSavedCredentials()
    {
        try
        {
            var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\RinScraper");
            if (key != null)
            {
                var encUser = key.GetValue("u") as string;
                var encPass = key.GetValue("p") as string;

                if (!string.IsNullOrEmpty(encUser))
                    usernameBox.Text = Encoding.UTF8.GetString(Convert.FromBase64String(encUser));
                if (!string.IsNullOrEmpty(encPass))
                    passwordBox.Text = Encoding.UTF8.GetString(Convert.FromBase64String(encPass));
            }
        }
        catch { }
    }
}