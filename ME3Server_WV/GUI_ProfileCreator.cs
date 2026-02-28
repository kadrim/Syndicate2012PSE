using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace ME3Server_WV
{
    public partial class GUI_ProfileCreator : Window
    {
        private long currentID;

        public GUI_ProfileCreator()
        {
            InitializeComponent();
        }

        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (IsValidPlayerName(textBox2.Text))
            {
                currentID = MakeID(textBox2.Text);
                textBox1.Text = currentID.ToString("X8");
                if (!IsValidPassword(textBox3.Text))
                    textBox3.Text = GetRandomPassword();
            }
            else
                textBox1.Text = "";
        }

        public static long MakeID(string name)
        {
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(name);
            byte[] hash = md5.ComputeHash(inputBytes);
            string res = "";
            for (int i = 0; i < 4; i++)
                res += hash[i].ToString("X2");
            return Convert.ToInt64(res, 16);
        }

        private async void button1_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (textBox1.Text == "")
            {
                textBox2.Focus();
                return;
            }
            if (!IsValidPassword(textBox3.Text))
            {
                textBox3.Focus();
                return;
            }

            string playertextfile = Frontend.loc + "player" + Path.DirectorySeparatorChar + textBox2.Text + ".txt";
            if (File.Exists(playertextfile))
            {
                string msg = "A file named '" + textBox2.Text + ".txt' already exists inside the player folder and is about to be overwritten.\n\n";
                msg += "If you proceed, server-side data from that file's respective profile will be lost.\n\nContinue?";
                var confirmDialog = new InputDialog("Name conflict", msg, "yes");
                var confirmResult = await confirmDialog.ShowDialog<string>(this);
                if (confirmResult == null || confirmResult.ToLower() != "yes")
                    return;
            }

            // Ask about creating Local_Profile.sav
            var savDialog = new InputDialog("Local_Profile.sav", "Create a Local_Profile.sav for this profile? (yes/no)", "yes");
            var savResult = await savDialog.ShowDialog<string>(this);

            if (savResult != null && savResult.ToLower() == "yes")
            {
                var topLevel = TopLevel.GetTopLevel(this);
                var file = await topLevel.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Local_Profile.sav",
                    SuggestedFileName = "Local_Profile.sav",
                    DefaultExtension = "sav",
                    FileTypeChoices = new[] { new FilePickerFileType("SAV files") { Patterns = new[] { "*.sav" } } }
                });
                if (file != null)
                {
                    string AUTH = currentID.ToString("X8") + "UoE4gBscrqJNM7j6nR84thRQrPmaqc1TgbPCXc3vTmOf-1jnUBttCGvO-j2M2RG54CP48eNSZHqbHLnGeP8PL4YsPVsqKU9s9CmyKohn9ezWeQ5HhX9u9wVY";
                    var filePath = file.TryGetLocalPath();
                    if (filePath != null)
                        File.WriteAllBytes(filePath, Local_Profile.CreateProfile((int)currentID, AUTH));
                }
            }

            if (!CreateProfile(currentID, textBox2.Text, textBox3.Text))
            {
                Logger.Log("Error on creating player profile text file.", LogColor.Red);
                return;
            }

            string doneMsg = "File 'player" + Path.DirectorySeparatorChar + textBox2.Text + ".txt' has been created.";
            doneMsg += " Login: " + textBox2.Text + " / Password: " + textBox3.Text;
            Logger.Log(doneMsg, LogColor.DarkGreen);
            this.Close();
        }

        public static bool IsValidPlayerName(string name)
        {
            if (name.Length == 0)
                return false;
            bool hasInvalidChar = false;
            char[] invalidchars = Path.GetInvalidFileNameChars();
            foreach (char c in invalidchars)
                hasInvalidChar |= name.Contains(c);
            if (hasInvalidChar)
                return false;
            if (name[0] == ' ' || name[0] == '.')
                return false;
            if (name[name.Length - 1] == ' ' || name[name.Length - 1] == '.')
                return false;
            return true;
        }

        public static bool IsValidPassword(string pw)
        {
            if (String.IsNullOrEmpty(pw))
                return false;
            if (pw.Length > 10)
                return false;
            if (pw.Contains(' '))
                return false;
            return true;
        }

        public static string GetRandomPassword()
        {
            const string availablechars = "abcdefghijklmnopqrstuvwxyz0123456789";
            Random r = new Random();
            string finalpassword = "";
            int desiredlength = r.Next(3, 6);
            for (int i = 0; i < desiredlength; i++)
                finalpassword += availablechars[r.Next(0, availablechars.Length - 1)];
            return finalpassword;
        }

        public static bool CreateProfile(long PlayerID, string PlayerName, string Password)
        {
            try
            {
                string res = "";
                res += "PID=0x" + PlayerID.ToString("X8") + "\r\n";
                res += "UID=0x" + PlayerID.ToString("X8") + "\r\n";
                res += "AUTH=" + PlayerID.ToString("X8") + "UoE4gBscrqJNM7j6nR84thRQrPmaqc1TgbPCXc3vTmOf-1jnUBttCGvO-j2M2RG54CP48eNSZHqbHLnGeP8PL4YsPVsqKU9s9CmyKohn9ezWeQ5HhX9u9wVY\r\n";
                res += "AUTH2=" + Password + "\r\n";
                res += "DSNM=" + PlayerName;
                File.WriteAllText(Frontend.loc + "player" + Path.DirectorySeparatorChar + PlayerName + ".txt", res);
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print("CreateProfile | " + ex.GetType().Name + ": " + ex.Message);
                return false;
            }
        }
    }
}
