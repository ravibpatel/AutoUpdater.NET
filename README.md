# AutoUpdater.NET  [![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](http://paypal.me/rbsoft)
AutoUpdater.NET is a class library that allows .NET developers to easily add auto update functionality to their classic desktop application projects.

## The nuget package  [![NuGet](https://img.shields.io/nuget/v/Autoupdater.NET.Official.svg)](https://www.nuget.org/packages/Autoupdater.NET.Official/) [![NuGet](https://img.shields.io/nuget/dt/Autoupdater.NET.Official.svg)](https://www.nuget.org/packages/Autoupdater.NET.Official/)
https://www.nuget.org/packages/Autoupdater.NET.Official/

    PM> Install-Package Autoupdater.NET.Official

## How it works

AutoUpdater.NET downloads the XML file containing update information from your server. It uses this XML file to get the information about the latest version of the software. If latest version of the software is greater then current version of the software installed on User's PC then AutoUpdater.NET shows update dialog to the user. If user press the update button to update the software then It downloads the update file (Installer) from URL provided in XML file and executes the installer file it just downloaded. It is a job of installer after this point to carry out the update. If you provide zip file URL instead of installer then AutoUpdater.NET will extract the contents of zip file to application directory. 

## Using the code
### XML file
AutoUpdater.NET uses XML file located on a server to get the release information about the latest version of the software. You need to create XML file like below and then you need to upload it to your server.

````xml
<?xml version="1.0" encoding="UTF-8"?>
<item>
    <version>2.0.0.0</version>
    <url>http://rbsoft.org/downloads/AutoUpdaterTest.zip</url>
    <changelog>https://github.com/ravibpatel/AutoUpdater.NET/releases</changelog>
    <mandatory>false</mandatory>
</item>
````

There are three things you need to provide in XML file as you can see above.

* version (Required) : You need to provide latest version of the application between version tags. Version should be in X.X.X.X format.
* url (Required): You need to provide URL of the latest version installer file between url tags. AutoUpdater.NET downloads the file provided here and install it when user press the Update button.
* changelog (Optional): You need to provide URL of the change log of your application between changelog tags. If you don't provide the URL of the changelog then update dialog won't show the change log.
* mandatory (Optional): You can set this to true if you don't want user to skip this version. This will ignore Remind Later and Skip options and hide both Skip and Remind Later button on update dialog.

### Adding one line to make it work

After you done creating and uploading XML file, It is very easy to add a auto update functionality to your application. First you need to add following line at the top of your form.

````csharp
using AutoUpdaterDotNET;
````

Now you just need to add following line to your main form constructor or in Form_Load event. You can add this line anywhere you like. If you don't like to check for update when application starts then you can create a Check for update button and add this line to Button_Click event.

````csharp
AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTest.xml");
````

Start method of AutoUpdater class takes URL of the XML file you uploaded to server as a parameter.

## Configuration Options
### Change Language

You can change of language of the update dialog by adding following line with the above code. If you don't do this it will use current culture of your application.

````csharp
AutoUpdater.CurrentCulture = CultureInfo.CreateSpecificCulture("ru");
````

In above example AutoUpdater.NET will show update dialog in russian language.

### Disable Skip Button

If you don't want to show Skip button on Update form then just add following line with above code.

````csharp
AutoUpdater.ShowSkipButton = false;
````

### Disable Remind Later Button

If you don't want to show Remind Later button on Update form then just add following line with above code.

````csharp
AutoUpdater.ShowRemindLaterButton = false;
````

### Enable Error Reporting

You can turn on error reporting by adding below code. If you do this AutoUpdater.NET will show error message, if there is no update available or if it can't get to the XML file from web server.

````csharp
AutoUpdater.ReportErrors = true;
````

### Open Download Page

If you don't want to download the latest version of the application and just want to open the URL between url tags of your XML file then you need to add following line with above code.

````csharp
AutoUpdater.OpenDownloadPage = true;
````

This kind of scenario is useful if you want to show some information to users before they download the latest version of an application.

### Remind Later

If you don't want users to select Remind Later time when they press the Remind Later button of update dialog then you need to add following lines with above code.

````csharp
AutoUpdater.LetUserSelectRemindLater = false;
AutoUpdater.RemindLaterTimeSpan = RemindLaterFormat.Days;
AutoUpdater.RemindLaterAt = 2;
````

In above example when user press Remind Later button of update dialog, It will remind user for update after 2 days.

### Proxy Server

If your XML and Update file can only be used from certain Proxy Server then you can use following settings to tell AutoUpdater.NET to use that proxy. Currently, if your Changelog URL is also restricted to Proxy server then you should omit changelog tag from XML file cause it is not supported using Proxy Server.

````csharp
var proxy = new WebProxy("ProxyIP:ProxyPort", true) 
{
    Credentials = new NetworkCredential("ProxyUserName", "ProxyPassword")
};
AutoUpdater.Proxy = proxy;
````

## Check updates frequently

You can call Start method inside Timer to check for updates frequently.

### WinForms

````csharp
System.Timers.Timer timer = new System.Timers.Timer
{
    Interval = 2 * 60 * 1000,
    SynchronizingObject = this
};
timer.Elapsed += delegate
{
    AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTest.xml");
};
timer.Start();
````

### WPF

````csharp
DispatcherTimer timer = new DispatcherTimer {Interval = TimeSpan.FromMinutes(2)};
timer.Tick += delegate
{
    AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTestWPF.xml");
};
timer.Start();
````
## Handling parsing logic manually

If you want to use other format instead of XML as a AppCast file then you need to handle the parsing logic by subscribing to ParseUpdateInfoEvent. You can do it as follows.

````csharp
AutoUpdater.Start("http://rbsoft.org/updates/AutoUpdaterTest.json");
AutoUpdater.ParseUpdateInfoEvent += AutoUpdaterOnParseUpdateInfoEvent;

private void AutoUpdaterOnParseUpdateInfoEvent(ParseUpdateInfoEventArgs args)
{
    dynamic json = JsonConvert.DeserializeObject(args.RemoteData);
    args.UpdateInfo = new UpdateInfoEventArgs
    {
        CurrentVersion = json.version,
        ChangelogURL = json.changelog,
        Mandatory = json.mandatory,
        DownloadURL = json.url
    };
}
````
### JSON file used in the Example above

````json
{
    "version":"2.0.0.0", 
    "url":"http://rbsoft.org/downloads/AutoUpdaterTest.zip",
    "changelog":"https://github.com/ravibpatel/AutoUpdater.NET/releases",
    "mandatory":true 
}
````

## Handling updates manually

Sometimes as a developer you need to maintain look and feel for the entire application similarly or you just need to do something before update. In this type of scenarios you can handle the updates manually by subscribing to an event. You can do it by adding following line with above code.

````csharp
AutoUpdater.CheckForUpdateEvent += AutoUpdaterOnCheckForUpdateEvent;

private void AutoUpdaterOnCheckForUpdateEvent(UpdateInfoEventArgs args)
{
    if (args != null)
    {
        if (args.IsUpdateAvailable)
        {
            DialogResult dialogResult;
            if (args.Mandatory)
            {
                dialogResult =
                    MessageBox.Show(
                        $@"There is new version {args.CurrentVersion} available. You are using version {args.InstalledVersion}. This is required update. Press Ok to begin updating the application.", @"Update Available",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
            }
            else
            {
                dialogResult =
                    MessageBox.Show(
                        $@"There is new version {args.CurrentVersion} available. You are using version {
                                args.InstalledVersion
                            }. Do you want to update the application now?", @"Update Available",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information);
            }

            if (dialogResult.Equals(DialogResult.Yes))
            {
                try
                {
                    if (AutoUpdater.DownloadUpdate())
                    {
                        Application.Exit();
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, exception.GetType().ToString(), MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }
        else
        {
            MessageBox.Show(@"There is no update available please try again later.", @"No update available",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
    else
    {
        MessageBox.Show(
                @"There is a problem reaching update server please check your internet connection and try again later.",
                @"Update check failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
````

When you do this it will execute the code in above event when AutoUpdater.Start method is called instead of showing the update dialog. UpdateInfoEventArgs object carries all the information you need about the update. If its null then it means AutoUpdater.NET can't reach the XML file on your server. UpdateInfoEventArgs has following information about the update.

* IsUpdateAvailable (bool) :  If update is available then returns true otherwise false.
* DownloadURL (string) : Download URL of the update file..
* ChangelogURL (string) : URL of the webpage specifying changes in the new update.
* CurrentVersion (Version) : Newest version of the application available to download.
* InstalledVersion (Version) : Version of the application currently installed on the user's PC.
* Mandatory (bool) : Shows if the update is required or optional.
