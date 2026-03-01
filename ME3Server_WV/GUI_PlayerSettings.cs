using System;
using System.Collections.Generic;
using Avalonia.Controls;

namespace ME3Server_WV
{
    public partial class GUI_PlayerSettings : Window
    {
        public Player.PlayerInfo player;

        public GUI_PlayerSettings()
        {
            InitializeComponent();
        }

        public void FreshList()
        {
            if (player == null)
                return;
            var items = new List<string>();
            foreach (Player.PlayerInfo.SettingEntry set in player.Settings)
                items.Add(set.Key);
            listBox1.ItemsSource = items;
        }

        private void listBox1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            rtb1.Text = player.Settings[n].Data;
        }

        private void saveButton_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            int n = listBox1.SelectedIndex;
            if (n == -1)
                return;
            player.UpdateSettings(listBox1.SelectedItem.ToString(), rtb1.Text);
        }
    }
}
