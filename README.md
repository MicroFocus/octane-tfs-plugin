![HPE LOGO](https://www.microfocus.com/brandcentral/microfocus/img/mf-logo-download.png)

# ALM Octane plugin for Microsoft Team Foundation Server CI                        

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/8ec4415bffe94fda8ae40415388c063e)](https://www.codacy.com/app/HPEbot/hp-application-automation-tools-plugin?utm_source=github.com&utm_medium=referral&utm_content=hpsa/hp-application-automation-tools-plugin&utm_campaign=badger)

Project status:
[![Build status](https://ci.appveyor.com/api/projects/status/gqd0x8ov1ebqjjcu?svg=true)](https://ci.appveyor.com/project/HPEbot/hp-application-automation-tools-plugin)

Latest release branch status:
[![Build status](https://ci.appveyor.com/api/projects/status/gqd0x8ov1ebqjjcu/branch/latest?svg=true)](https://ci.appveyor.com/project/HPEbot/hp-application-automation-tools-plugin/branch/latest)

##### This plugin integrates ALM Octane with TFS, enabling ALM Octane to display TFS build pipelines and track build and test run results.

## Installation instructions

1. Downoad msi setup from :
2. Run setup
3. Configure all relevant fields following the [Configure the setup](#Configure the connection)

### Configuring the TFS ci plugin to connect to ALM Octane
#### Before you configure the connection:
1. Ask the ALM Octane shared space admin for an API access Client ID and Client secret. The plugin uses these for authentication when
communicating with ALM Octane. The access keys must be assigned the CI/CD Integration role in all relevant workspaces. For details, see Set up API access for integration.
2. To enable the TFS server to communicate with ALM Octane, make sure the server can access the Internet. If your network requires a proxy to connect to the Internet, setup the required proxy configuration.
3. Decide which Bamboo user ALM Octane will use to execute jobs on the server.

#### Configure the setup
During the msi setup
Enter the following information in the relevant fields:

**Installation folder**: C:\Program Files\Microsoft Team Foundation Server 15.0\Application Tier\TFSJobAgent\Plugins\

      This folder is provided by default and should be changed only if TFS installation path is diffrent from the default

**Location**: http:// Octane fully qualified domain name / IP address> {:}/ui/?p=
For example, in this URL, the shared space ID is 1002:  http://myServer.myCompany.com:8081/ui/?p=1002

**Client ID/Secret**: Ask the ALM Octane shared space admin for an API access Client ID and Client secret. The plugin uses these for authentication when communicating with ALM Octane

**After the connection is set up**, open ALM Octane, define a CI server and create pipelines.
For details, see Integrate with your CI server in the ALM Octane Help.

## Relevent links
-	**Download the most recent build version of the plugin** at [appveyor](https://ci.appveyor.com/project/m-seldin/octane-tfs-plugin)


## Contribute to the TFS plugin
- Contributions of code are always welcome!
- Follow the standard GIT workflow: Fork, Code, Commit, Push and start a Pull request
- Each pull request will be tested, pass static code analysis and code review results.
- All efforts will be made to expedite this process.

#### Guidelines
- Document your code – it enables others to continue the great work you did on the code and update it.
- SonarLint your code – we use sonarQube with its basic built-in rule set. In the future, we will provide direct online access to test with a custom rule set.

### Feel free to contact us on any question related to contributions - octane[dot]ci[dot]plugins-[at]-gmail-dot-com
