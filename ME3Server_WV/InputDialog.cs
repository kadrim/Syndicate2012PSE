using Avalonia.Controls;

namespace ME3Server_WV
{
    public partial class InputDialog : Window
    {
        public InputDialog()
        {
            InitializeComponent();
        }

        public InputDialog(string title, string prompt, string defaultValue = "")
        {
            InitializeComponent();
            Title = title;
            promptText.Text = prompt;
            inputBox.Text = defaultValue;
        }

        private void OK_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(inputBox.Text);
        }

        private void Cancel_Click(object sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            Close(null);
        }
    }
}
