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
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using log4net;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Configuration;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools;
using MicroFocus.Adm.Octane.CiPlugins.Tfs.Core.Tools.Connectivity;

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
        private readonly TfsVersion _tfsVersion;
        public MainWindow()
        {
            InitializeComponent();
            LogUtils.ConfigureLog4NetForPluginMode(false);

            _tfsVersion = Helpers.GetInstalledTfsVersion();
            lbl_Version.Content = $"ver {Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";
            switch (_tfsVersion)
            {
                case TfsVersion.Tfs2015:
                    Set2015FieldsVisibility(Visibility.Visible);
                    break;
                case TfsVersion.Tfs2017:
                    Set2017FieldsVisibility(Visibility.Visible);
                    break;
                case TfsVersion.NotDefined:
                    break;
                default:
                    Set2017FieldsVisibility(Visibility.Visible);
                    break;

            }

            try
            {
                Helper.CheckedConnection = false;
                
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
                PasswordInput.Password = connectionDetails.Password;
                UsernameInput.Text = connectionDetails.Pat;
            }
            catch (Exception ex)
            {
                Log.Warn("Could not parse existing configuration file",ex);
            }

          
           

        }

        private void Set2015FieldsVisibility(Visibility v)
        {
            if (v == Visibility.Visible)
            {
                Set2017FieldsVisibility(Visibility.Collapsed);
            }
            LabelCredentials.Content = "Credentials";
            TipLabel.Content="TFS Username/Password";
            PasswordLabel.Visibility = v;
            PasswordInput.Visibility = v;
            UsernameInput.Visibility = v;
            UserNameLabel.Visibility = v;            
        }
        private void Set2017FieldsVisibility(Visibility v)
        {
            if (v == Visibility.Visible)
            {
                Set2015FieldsVisibility(Visibility.Collapsed);
            }
            LabelCredentials.Content = "TFS PAT:";
            TipLabel.Content = "(Personal Access Token)";
            Pat.Visibility = v;
            PasswordInput.Password = "";            
        }

        private void TestConnectionButton_OnClick(object sender, RoutedEventArgs e)
        {
            ReadFields();
            try
            {
                ConnectionCreator.InitRestConnectorForUI();
                ConnectionCreator.CheckMissingValues(_conDetails);
                ConnectionCreator.CreateOctaneConnection(_conDetails);
                ConnectionCreator.CreateTfsConnection(_conDetails);                
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, "ALM Octane", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
                ErrorMessageWindow.Show("ALM Octane", ex.Message);
                return;                
            }

            Helper.CheckedConnection = true;

            MessageBox.Show("Connection successfull","ALM Octane",MessageBoxButton.OK,MessageBoxImage.Information,MessageBoxResult.OK);
        }


        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                ReadFields();
                ConfigurationManager.WriteConfig(_conDetails);
                
                MessageBox.Show("Settings saved!", "ALM Octane", MessageBoxButton.OK, MessageBoxImage.Information, MessageBoxResult.OK);
            }
            catch (Exception ex)
            {
                const string error = "Could not save configuration file!";
                Log.Error(error,ex);
                MessageBox.Show(error, "ALM Octane", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void ReadFields()
        {
            var octaneServerUrl = Location.Text;
            var clientId = ClientId.Text;
            var clientSecret = ClientSecret.Password;            
            var pat = _tfsVersion == TfsVersion.Tfs2015 ? UsernameInput.Text : Pat.Password;
            var password = PasswordInput.Password;
            var tfsLocation = TfsLocation.Text;
            _instanceId = string.IsNullOrEmpty(_instanceId) ? Guid.NewGuid().ToString() : _instanceId;
            var conDetails =
                new ConnectionDetails(octaneServerUrl, clientId, clientSecret, tfsLocation, _instanceId)
                {
                    Pat = pat,
                    Password = password
                };

            _conDetails = conDetails;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!Helper.CheckedConnection)
            {
                var res = MessageBox.Show("Connection was not checked, are you sure you want to exit?", "Warning",
                    MessageBoxButton.YesNo,MessageBoxImage.Warning);
                if (res == MessageBoxResult.No)
                {
                    e.Cancel = true;
                }
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MainWindow_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
            
        }

        private void TipButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TipLabel.Visibility == Visibility.Hidden)
            {
                TipLabel.Visibility = Visibility.Visible;
            }else
                TipLabel.Visibility = Visibility.Hidden;

        }
    }
}
