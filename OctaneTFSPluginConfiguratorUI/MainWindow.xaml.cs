/*!
* (c) 2016-2018 EntIT Software LLC, a Micro Focus company
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
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
using log4net;
using MicroFocus.Adm.Octane.Api.Core.Connector;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;

namespace OctaneTFSPluginConfiguratorUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        ConnectionDetails _conDetails = new ConnectionDetails();
        private string _instanceId = Guid.NewGuid().ToString();
        public MainWindow()
        {
            try
            {
                Helper.CheckedConnection = false;
                InitializeComponent();
                TfsLocation.Text = ConnectionCreator.GetTfsLocationFromHostName();
                if (!ConfigurationManager.ConfigurationExists())
                {                    
                    return;
                }

                var connectionDetails = ConfigurationManager.Read(false);

                Location.Text = connectionDetails.ALMOctaneUrl;
                ClientId.Text = connectionDetails.ClientId;
                ClientSecret.Password = connectionDetails.ClientSecret;
                TfsLocation.Text = connectionDetails.TfsLocation;
                Pat.Password = connectionDetails.Pat;
                _instanceId = connectionDetails.InstanceId;
            }
            catch (Exception ex)
            {
                Log.Warn("Could not parse existing configuration file",ex);
            }
            
        }

        private void TestConnectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            NetworkSettings.EnableAllSecurityProtocols();
            NetworkSettings.IgnoreServerCertificateValidation();
            RestConnector.AwaitContinueOnCapturedContext = false;

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
            try
            {
                ReadFields();
                ConfigurationManager.WriteConfig(_conDetails);

                MessageBox.Show("Settings saved!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not save configuration file!");
            }
        }

        private void ReadFields()
        {
            var octaneServerUrl = Location.Text;
            var clientId = ClientId.Text;
            var clientSecret = ClientSecret.Password;            
            var pat = Pat.Password;
            var tfsLocation = TfsLocation.Text;
            _instanceId = string.IsNullOrEmpty(_instanceId) ? Guid.NewGuid().ToString() : _instanceId;
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
