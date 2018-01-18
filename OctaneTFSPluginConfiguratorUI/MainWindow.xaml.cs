using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

namespace OctaneTFSPluginConfiguratorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ConnectionDetails _conDetails = new ConnectionDetails();
        private readonly string _instanceId = Guid.NewGuid().ToString();
        public MainWindow()
        {
            Helper.CheckedConnection = false;
            InitializeComponent();

            if (!ConfigurationManager.ConfigurationExists())
            {                
                TfsLocation.Text = ConnectionCreator.GetTfsLocationFromHostName();                
                return;                
            }

            var connectionDetails = ConfigurationManager.Read(false);

            Location.Text = connectionDetails.ALMOctaneUrl;
            ClientId.Text = connectionDetails.ClientId;
            ClientSecret.Text = connectionDetails.ClientSecret;
            TfsLocation.Text = connectionDetails.TfsLocation;
            Pat.Text = connectionDetails.Pat;
            _instanceId= connectionDetails.InstanceId;            
        }

        private void TestConnectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            ReadFields();
            try
            {           
                
                ConnectionCreator.CheckMissingValues(_conDetails);
                ConnectionCreator.CreateTfsConnection(_conDetails);
                ConnectionCreator.CreateOctaneConnection(_conDetails);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return;                
            }

            Helper.CheckedConnection = true;

            MessageBox.Show("Connection succesfull");

        }


        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {                   
           ReadFields();
           ConfigurationManager.WriteConfig(_conDetails);

           MessageBox.Show("Settings saved!");
        }

        private void ReadFields()
        {
            var octaneServerUrl = Location.Text;
            var clientId = ClientId.Text;
            var clientSecret = ClientSecret.Text;            
            var pat = Pat.Text;
            var tfsLocation = TfsLocation.Text;

            var conDetails =
                new ConnectionDetails(octaneServerUrl, clientId, clientSecret, tfsLocation, _instanceId)
                {
                    Pat = pat
                };

            _conDetails = conDetails;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!Helper.CheckedConnection)
            {
                var res = MessageBox.Show("Connection was not checked, are you sure you want to exit?", "Warning",
                    MessageBoxButton.YesNo);
                if (res == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
