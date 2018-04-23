using System.Windows;

namespace OctaneTFSPluginConfiguratorUI
{
    /// <summary>
    /// Interaction logic for ErrorMessageWindow.xaml
    /// </summary>
    public partial class ErrorMessageWindow : Window
    {
        public ErrorMessageWindow(string title,string errorMessage)
        {
            InitializeComponent();

            Title = title;
            TxtError.Text = errorMessage;

        }       

        private void Cmd_Ok_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }


        private void Cmd_Copy_OnClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetDataObject(TxtError.Text);
        }

        public static ErrorMessageWindow Show(string title, string errorMessage)
        {
            var errorWindow = new ErrorMessageWindow(title, errorMessage) {Owner = Application.Current.MainWindow};
            errorWindow.ShowDialog();

            return errorWindow;
        }
    }
}
