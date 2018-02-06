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
2. Run setup
3. Configure all relevant fields following the [Configure the setup](https://github.com/MicroFocus/octane-tfs-plugin#configure-the-setup) guide

### Configuring the TFS ci plugin to connect to ALM Octane
#### Before you configure the connection:
1. Ask the ALM Octane shared space admin for an API access Client ID and Client secret. The plugin uses these for authentication when
communicating with ALM Octane. The access keys must be assigned the CI/CD Integration role in all relevant workspaces. For details, see Set up API access for integration.
2. Make sure your TFS server can communicate with ALM Octane. For example, if you are working with a SaaS version of ALM Octane, make sure that your TFS server can access the Internet .If your network requires a proxy to connect to the Internet, setup the required proxy configuration.

#### Configure the setup
During the msi setup
Enter the following information in the relevant fields:

**Installation folder**: C:\Program Files\Microsoft Team Foundation Server 15.0\Application Tier\TFSJobAgent\Plugins\

      This folder is provided by default and should be changed only if TFS installation path is different from the default

**ALM Octane Location** 

http://myServer.myCompany.com:8081/ui/?p=1002

The HTTP address of the ALM Octane application. You can copy the URL from the address bar of the browser in which you opened ALM Octane.

**ALM Octane Client ID** 

A Client ID for API access to ALM Octane. Obtain a Client ID and Client secret from the ALM Octane shared space admin.

**ALM Octane Client Secret** 

The Client secret that matches your API access Client ID.

**TFS Location** 

The HTTP address of the TFS, such as http://yourhost:8080/tfs/. 

This value is used to let plugin know how to refer to itself, for example to create links to build/run/etc. 

This is necessary because plugin cannot reliably detect such a URL from within itself.

**PAT (TFS Personal access token)** 

Personal Acess Token to TFS. PAT minimal permission set should contain :
* Build (read and execute)
* Code (read)
* Project and team (read)
* Test management (read)

**After the connection is set up**

Open ALM Octane, define a CI server and create pipelines.

For details, see ['Set up CI servers'](https://admhelp.microfocus.com/octane/en/latest/Online/Content/AdminGuide/article_CI_servers_setup.htm?cshid=Install_CI_Plugin) in the ALM Octane Help

## Administer the plugin
To administer the plugin, access the console page through the following url: http://localhost:4567/

**Editing the configuration is available only when accessing the page from localhost**

Plugin logs are located in C:\Users\Public\Documents\OctaneTfsPlugin\logs

## Relevent links
-	**Download the most recent build version of the plugin** at [appveyor](https://ci.appveyor.com/project/MicroFocus/octane-tfs-plugin)


## Contribute to the TFS plugin
- Contributions of code are always welcome!
- Follow the standard GIT workflow: Fork, Code, Commit, Push and start a Pull request
- Each pull request will be tested, pass static code analysis and code review results.
- All efforts will be made to expedite this process.

#### Guidelines
- Document your code – it enables others to continue the great work you did on the code and update it.

### Feel free to contact us on any question related to contributions - octane.ci.plugins@gmail.com
