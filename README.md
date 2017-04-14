# AutoUpdater.NET
AutoUpdater.NET is a class library that allows .NET developers to easily add auto update functionality to their classic desktop application projects.

## The nuget package  [![NuGet](https://img.shields.io/nuget/v/Autoupdater.NET.Official.svg)](https://www.nuget.org/packages/Autoupdater.NET.Official/)
https://www.nuget.org/packages/Autoupdater.NET.Official/

    PM> Install-Package Autoupdater.NET.Official

## How it works

AutoUpdater.NET downloads the XML file containing update information from your server. It uses this XML file to get the information about the latest version of the software. If latest version of the software is greater then current version of the software installed on User's PC then AutoUpdater.NET shows update dialog to the user. If user press the update button to update the software then It downloads the update file (Installer) from URL provided in XML file and executes the installer file it just downloaded. It is a job of installer after this point to carry out the update.

## Using the code
### XML file
AutoUpdater.NET uses XML file located on a server to get the release information about the latest version of the software. You need to create XML file like below and then you need to upload it to your server.

````xml
<item>
    <version>4.5.0.0</version>
    <url>
    http://rbsoft.org/phocadownload/Right%20Click%20Enhancer%20Setup.exe
    </url>
    <changelog>
    http://rbsoft.org/downloads/file/1-right-click-enhancer?tmpl=component
    </changelog>
</item>
````

There are three things you need to provide in XML file as you can see above.

* version (Required) : You need to provide latest version of the application between version tags. Version should be in X.X.X.X format.
* url (Required): You need to provide URL of the latest version installer file between url tags. AutoUpdater.NET downloads the file provided here and install it when user press the Update button.
* changelog (Optional): You need to provide URL of the change log of your application between changelog tags. If you don't provide the URL of the changelog then update dialog won't show the change log.

### Adding one line to make it work

After you done creating and uploading XML file, It is very easy to add a auto update functionality to your application. First you need to add following line at the top of your form.

````csharp
using AutoUpdaterDotNET;
````

Now you just need to add following line to your main form constructor or in Form_Load event. You can add this line anywhere you like. If you don't like to check for update when application starts then you can create a Check for update button and add this line to Button_Click event.

````csharp
AutoUpdater.Start("http://rbsoft.org/updates/right-click-enhancer.xml");
````

Start method of AutoUpdater class takes URL of the XML file you uploaded to server as a parameter.

## Configuration Options
### Change Language

You can change of language of the update dialog by adding following line with the above code.

````csharp
AutoUpdater.CurrentCulture = CultureInfo.CreateSpecificCulture("ru");
````

In above example AutoUpdater.NET will show update dialog in russian language.

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
            var dialogResult =
                MessageBox.Show(
                    string.Format(
                        "There is new version {0} available. You are using version {1}. Do you want to update the application now?",
                        args.CurrentVersion, args.InstalledVersion), @"Update Available",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

            if (dialogResult.Equals(DialogResult.Yes))
            {
                try
                {
                    //You can use Download Update dialog used by AutoUpdater.NET to download the update.

                    AutoUpdater.DownloadUpdate();
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
