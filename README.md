###GlacierSync

####Setup

This is simple .net command line program that will archive (zip) a directory then upload it to an Amazon Glacier "Vault". It displays a progress bar as it archives your files locally and again as it uploads your archive to Glacier. 

To get it up and running, edit the App.config file to point it to a directory you want archived and specify your vault name.

You'll also need to set two environment variables:

AWS_ACCESS_KEY_ID – AWS access key.

AWS_SECRET_ACCESS_KEY – AWS secret key. Access and secret key variables override credentials stored in credential and config files.
