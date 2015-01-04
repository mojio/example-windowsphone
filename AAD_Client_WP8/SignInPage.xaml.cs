using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Mojio.Client;

namespace AAD_Client_WP8
{
    public partial class SignInPage : PhoneApplicationPage
    {
        App app = (App.Current as App);

        public SignInPage()
        {
            InitializeComponent();
            myBrowser.IsScriptEnabled = true;
        }

        // build the URL of the Authorization endpoint from the app state
        // navigate to it via browser control
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string authURL = string.Format(
                "{0}/OAuth2/authorize?response_type=code&client_id={1}&redirect_uri={2}",
                app.Authority,
               
                app.ClientID,
                app.RedirectUri);
            //navigate to it
            myBrowser.Navigate(new Uri(authURL));
        }

        // watch for the redirect URI, once you see it 
        //    - stop navigation
        //    - retrieve the authorization code
        //    - hit the Token endpoint
        //    - put the result back in the app state and go back to main
        private async void Navigating(object sender, NavigatingEventArgs e)
        {
            string returnURL = e.Uri.ToString();
            if (returnURL.StartsWith(app.RedirectUri))
            {
                string code = e.Uri.Query.Remove(0, 6);
                e.Cancel = true;
                myBrowser.Visibility = System.Windows.Visibility.Collapsed;

                app.Response = await RequestToken(code);
                
                Dispatcher.BeginInvoke(() =>
                {
                    NavigationService.GoBack();
                });
            }
        }

        // construct the request for the Token endpoint
        // hit the endpoint and return the results
        private async Task<string> RequestToken(string code)
        {
            //MojioClient client = new MojioClient(new Guid("f201b929-d28c-415d-9b71-8112532301cb"),
            //                             new Guid("2ef80a7a-780d-41c1-8a02-13a286f11a23"),
            //                             new Guid(code),
            //                             MojioClient.Live // or MojioClient.Live
            //                         );
            //EventWaitHandle Wait = new AutoResetEvent(false);

            MojioClient client = new MojioClient();
            string secretKey = "f0927a0a-386b-4148-be8d-5ffd7468ea6b";
            await client.BeginAsync(new Guid("f201b929-d28c-415d-9b71-8112532301cb"),
                                    new Guid(secretKey),
                                    new Guid(code)
                                   );

            await client.SetUserAsync("anonymous", "Password007");
    
            var task = client.GetCurrentUserAsync();
            string responseString = await task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    return "Error: " + t.Exception.InnerException.Message;
                else if (t.IsCanceled)
                    return "Cancelled";
                else
                {
                    return t.Result.Email;
                }
            });
            //HttpClient client = new HttpClient();
            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, string.Format("{0}/oauth2/token", app.Authority));
            //string tokenreq = string.Format(
            //        "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}",
            //        code,
            //        app.ClientID,
            //        HttpUtility.UrlEncode(app.RedirectUri));
            //request.Content = new StringContent(tokenreq, Encoding.UTF8, "application/x-www-form-urlencoded");
            //HttpResponseMessage response = await client.SendAsync(request);
            //string responseString = await response.Content.ReadAsStringAsync();
            return responseString;
        }

    }
}