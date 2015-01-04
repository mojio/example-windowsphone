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
using System.Text.RegularExpressions;

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
                "{0}/OAuth2/authorize?response_type=token&client_id={1}&redirect_uri={2}",
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
                Regex regex = new Regex(@"access_token=([0-9a-f-]{36})");
                Match match = regex.Match(returnURL);
                if (match.Success)
                {
                    Console.WriteLine(match.Value);
                }
                string[] words = match.Value.Split('=');
                string code = words[1];
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
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, string.Format("{0}/v1/login/{1}?id={2}",
                app.Authority, app.ClientID, code));
;
            HttpResponseMessage response = await client.SendAsync(request);
            string responseString = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(responseString)["_id"].ToString();

            MojioClient mojioClient = new MojioClient();
            Guid codeguid = new Guid(token);
            await mojioClient.BeginAsync(new Guid(app.ClientID),
                                    Guid.Empty,
                                    codeguid
                                   );

            var task = mojioClient.GetCurrentUserAsync();

            responseString = "{\"access_token\": \"" + code + "\",";
            responseString += "\"refresh_token\": \"" + token + "\",";
            responseString += "\"id_token\": \"" + token + "\",";
            responseString += "\"user\": ";
            responseString += await task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    return "Error: " + t.Exception.InnerException.Message;
                else if (t.IsCanceled)
                    return "Cancelled";
                else
                    return Newtonsoft.Json.JsonConvert.SerializeObject(t.Result);
            });

            var task2 = mojioClient.UserVehiclesAsync(task.Result.Id);
            responseString += ", \"vehicles\": ";

            responseString += await task2.ContinueWith(t =>
            {
                if (t.IsFaulted)
                    return "Error: " + t.Exception.InnerException.Message;
                else if (t.IsCanceled)
                    return "Cancelled";
                else
                    return Newtonsoft.Json.JsonConvert.SerializeObject(t.Result);
            });
            responseString += " }";

            return responseString;
        }

    }
}