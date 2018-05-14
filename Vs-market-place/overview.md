![HPE LOGO](https://upload.wikimedia.org/wikipedia/commons/4/4e/MicroFocus_logo_blue.png)

# ALM Octane plugin for Microsoft Team Foundation Server CI                        

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/fde28fd11a494839b50c2b49f2fd486a)](https://www.codacy.com/app/HPSoftware/octane-tfs-plugin?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=MicroFocus/octane-tfs-plugin&amp;utm_campaign=Badge_Grade)

Project status:
[![Build status](https://ci.appveyor.com/api/projects/status/ea529f2p7jit8m8t?svg=true)](https://ci.appveyor.com/project/m-seldin/octane-tfs-plugin-0ykgj)

Latest release branch status:
[![Build status](https://ci.appveyor.com/api/projects/status/ea529f2p7jit8m8t?svg=true)](https://ci.appveyor.com/project/m-seldin/octane-tfs-plugin-0ykgj)

##### This plugin integrates ALM Octane with TFS, enabling ALM Octane to display TFS build pipelines and track build and test run results.

## Installation instructions

1. Download latest msi release setup from : [releases](https://github.com/MicroFocus/octane-tfs-plugin/releases)
2. Run setup.exe
3. Configure all relevant fields following the [Configure the setup](https://github.com/MicroFocus/octane-tfs-plugin#configure-the-setup) guide

### Configuring the TFS ci plugin to connect to ALM Octane
#### Before you configure the connection:
1. Ask the ALM Octane shared space admin for an API access Client ID and Client secret. The plugin uses these for authentication when
communicating with ALM Octane. The access keys must be assigned the CI/CD Integration role in all relevant workspaces. For details, see Set up API access for integration.
2. To enable the TFS server to communicate with ALM Octane, make sure the server can access the Internet. If your network requires a proxy to connect to the Internet, setup the required proxy configuration.

#### Configure the setup
During the msi setup
Enter the following information in the relevant fields:

**Installation folder**: C:\Program Files\Microsoft Team Foundation Server 15.0\Application Tier\TFSJobAgent\Plugins\

      This folder is provided by default and should be changed only if TFS installation path is different from the default

**Location**: http://myServer.myCompany.com:8081/ui/?p=1002
where 1002 is your shared space id and 8081 is the ALM Octane port service port

**Client ID/Secret**: Ask the ALM Octane shared space admin for an API access Client ID and Client secret. The plugin uses these for authentication when communicating with ALM Octane

**Client ID/Secret**: Ask the ALM Octane shared space admin for an API access Client ID and Client secret. The plugin uses these for authentication when communicating with ALM Octane

**PAT (TFS Personal access token)**: The token should be configured by TFS admin (see [PAT](https://docs.microsoft.com/en-us/vsts/accounts/use-personal-access-tokens-to-authenticate) )

**After the connection is set up**, open ALM Octane, define a CI server and create pipelines.
For details, see Integrate with your CI server in the ALM Octane Help.

## Administer the plugin
There are several rest api operations that can be performed to monitor plugin health.
**The rest api is available only from the localhost address.**

| URI           | Method           | Description  |
| -------------         |:-------------:| -----:|
| http://localhost:4567/      | GET      |   Get plugin status (on/off) |
| http://localhost:4567/logs       | GET | Get plugin logs list  |
| http://localhost:4567/logs/last | GET      |    Get last plugin logs |
| http://localhost:4567/config       | GET |Configure plugin settings|
| http://localhost:4567/start       | POST |Start plugin|
| http://localhost:4567/stop       | POST |Stop plugin|
| http://localhost:4567/version       | GET |Get plugin version|

Plugin logs are located in C:\Users\Public\Documents\OctaneTfsPlugin\logs

## Relevent links
-	**Download the most recent build version of the plugin** at [appveyor](https://ci.appveyor.com/project/MicroFocus/octane-tfs-plugin)

## Limitations
- Currently the plugin does not support commits in ALM Octane in TFS 2015 version with TFVC
## Contribute to the TFS plugin
- Contributions of code are always welcome!
- Follow the standard GIT workflow: Fork, Code, Commit, Push and start a Pull request
- Each pull request will be tested, pass static code analysis and code review results.
- All efforts will be made to expedite this process.

#### Guidelines
- Document your code – it enables others to continue the great work you did on the code and update it.

### Feel free to contact us on any question related to contributions - octane.ci.plugins@gmail.com
