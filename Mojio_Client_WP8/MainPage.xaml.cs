using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Mojio_Client_WP8.Resources;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Phone.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Phone.Storage.SharedAccess;
using Windows.Storage;
using System.IO;

namespace Mojio_Client_WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        App app;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            app = App.Current as App;
            app.Response = "";
        }

        // drives the main app UI
        protected async override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
         
            if(app.Response==string.Empty)
            {
                // if the app.Response is empty, that means that we need to show the UI for composing a request

                spTOkenParamsForm.Visibility =  Visibility.Visible;
                spResults.Visibility = Visibility.Collapsed;
            } else
            {
                // if there is stuff, we want to show it!

                spTOkenParamsForm.Visibility =  Visibility.Collapsed;
                spResults.Visibility = Visibility.Visible;

                JObject jo = JsonConvert.DeserializeObject(app.Response) as JObject;
                try
                {
                    txtAccess.Text = (string)jo["access_token"];
                    txtRefresh.Text = (string)jo["refresh_token"];
                    txtUser.Text = (string)jo["user"].ToString(Formatting.Indented);
                    txtVehicles.Text = (string)jo["vehicles"].ToString(Formatting.Indented); 
                }
                catch
                { 
                    // if we're here, most likely the request didn't succeed
                    // the response will contian details about the error

                    MessageBox.Show(app.Response);
                    spTOkenParamsForm.Visibility = Visibility.Visible;
                    spResults.Visibility = Visibility.Collapsed;
                }
            }            
        }

        // poor's man binding
        private void SyncRequestCoordinates()
        {
            app.RedirectUri = txtRedirectUri.Text;
            app.Authority = txtAuthority.Text;
            app.ClientID = txtClientID.Text;
            //app.Resource = txtResource.Text;
        }

        // trigger the request flow
        private void btnSignIn_Click(object sender, RoutedEventArgs e)
        {
            SyncRequestCoordinates();
            NavigationService.Navigate(new Uri("/SignInPage.xaml", UriKind.Relative));
        }

        // show some docs
        private void About_Click(object sender, EventArgs e)
        {
            WebBrowserTask task = new WebBrowserTask();
            task.Uri = new Uri("http://developer.moj.io");
            task.Show();
        }
       
        // put in the clipboard whatever is currently shown in the UI
        private void Copy_Click(object sender, EventArgs e)
        {
            string toBeCopied = (spTOkenParamsForm.Visibility == Visibility.Visible) ?
                //String.Format(@"{{ ""authority"" : ""{0}"",""clientid"" : ""{1}"",""redirecturi"" : ""{2}"",""resource"" : ""{3}""}}", 
                //                  txtAuthority.Text, txtClientID.Text, txtRedirectUri.Text, txtResource.Text) :
                String.Format(@"{{ ""authority"" : ""{0}"",""clientid"" : ""{1}"",""redirecturi"" : ""{2}""}}", 
                                  txtAuthority.Text, txtClientID.Text, txtRedirectUri.Text) :
                String.Format(@"{{ ""access_token"" : ""{0}"",""refresh_token"" : ""{1}"",""user"" : ""{2}"", ""vehicles"" : ""{3}"" }}", txtAccess.Text, txtRefresh.Text, txtUser.Text, txtVehicles.Text);
            Clipboard.SetText(toBeCopied);
        }       
    }
}