###GlacierSync

####Setup

This is simple .net command line program that will archive (zip) a directory then upload it to an Amazon Glacier "Vault". It displays a progress bar as it archives your files locally and again as it uploads your archive to Glacier. 

To get it up and running, edit the App.config file to point it to a directory you want archived and specify your vault name.

You'll also need to create a file called 'localsettings.config' which will contain your AWSAccessKey and AWSSecretKey.

    <?xml version="1.0"?>
    <appSettings file="localsettings.config">
      <add key="AWSAccessKey" value="your access key"/>
      <add key="AWSSecretKey" value="your secret key"/>
    </appSettings>

The localsettings.config is excluded by the gitignore file so you don't need to worry about accidentally checking in your private credentials.