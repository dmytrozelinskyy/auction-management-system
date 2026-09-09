using System.Windows;
using System.Windows.Media;

namespace MAS_Implementation;

public partial class NotificationPopup : Window
{
    public NotificationPopup(string msg, bool isError = true)
    {
        InitializeComponent();

        var parts = msg.Split('\n', 2);
        HeadlineText.Text = parts[0];
        DetailText.Text = parts.Length > 1 ? parts[1] : string.Empty;

        if (!isError)
        {
            TitleText.Text = "Success!";
            HeadlineText.Foreground = new SolidColorBrush(Colors.LawnGreen);
        }
    }
    
    private void CloseButton_Click(object sender, RoutedEventArgs e) => Close();
}