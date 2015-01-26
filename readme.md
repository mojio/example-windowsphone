
Toy client for Windows Phone 8 demonstrating how to log into the Mojio API.

Information about Mojio's API is found here: http://developer.moj.io
Mojio is here: http://moj.io

Checkout the code as follows:

git clone --recursive  https://github.com/mojio/Mojio-Example-WP8.git -c diff.mnemonicprefix=false -c core.quotepath=false 

That line will checkout all the submodules including RestSharp for the Mojio Client.  The RestSharp submodule takes a bit of time, on the order of 5-10 minutes, just wait for it and it will fully clone.

Once you have obtained all the submodules, the code will build.

Here's a good intro to OAuth 2: https://aaronparecki.com/articles/2012/07/29/1/oauth2-simplified

Big thank you to Vittorio Bertocci who wrote the original application on which this one is based.
Details on the orignal application are here http://www.cloudidentity.com/blog/2014/02/16/a-sample-windows-phone-8-app-getting-tokens-from-windows-azure-ad-and-adfs/.
