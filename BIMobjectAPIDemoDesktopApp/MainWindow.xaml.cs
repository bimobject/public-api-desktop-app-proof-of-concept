using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using BIMobjectAPIDemoDesktopApp.Dtos;
using BIMobjectAPIDemoDesktopApp.Helpers;
using File = System.IO.File;
using WinForms = System.Windows.Forms;

namespace BIMobjectAPIDemoDesktopApp
{
    /// <summary>
    /// BimObject Auth Code with Proof Key Flow
    /// </summary>
    public partial class MainWindow
    {
        // client configuration
        private string _clientId;
        private string _clientSecret;
        private string _directoryPath;

        private const string Scopes = "search_api search_api_downloadbinary offline_access";
        private const string RevitFileType = "9";
        private ITokenStore _tokenStore;

        public ITokenStore TokenStore
        {
            get => _tokenStore ?? (_tokenStore = new MemoryTokenStore());
            set => _tokenStore = value;
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Login(object sender, RoutedEventArgs e)
        {
            _clientId = clientId.Text.Trim();
            _clientSecret = passwordBox.Password.Trim();
            _directoryPath = directory.Text;
            
            // Generates state and PKCE values.
            var state = AuthHelpers.CryptoRandomBase64Url(32);
            var codeVerifier = AuthHelpers.CryptoRandomBase64Url(32);
            var codeChallenge = AuthHelpers.Base64UrlencodeNoPadding(AuthHelpers.Sha256(codeVerifier));

            //Creates a redirect URI using an available port on the loopback address. This is needed for native applications
            var redirectUri = AuthHelpers.CreateRedirectUrl();

            // Creates an HttpListener to listen for requests on that redirect URI.
            var http = AuthHelpers.CreateListener(redirectUri);
            http.Start();

            // Creates the authentication request to the authorize enpoint.
            var authorizationRequest =
                $"{Endpoints.AuthorizationEndpoint}?response_type=code&scope={Scopes}&redirect_uri={redirectUri}&client_id={_clientId}&state={state}&code_challenge={codeChallenge}&code_challenge_method={AuthHelpers.CodeChallengeMethod}";

            // Send the user to the authorize endpoint with authenication paramters
            System.Diagnostics.Process.Start(authorizationRequest);

            // Waits for the authorization response.
            var context = await http.GetContextAsync();

            // Focus the application
            Activate();

            // Once authentication process is completed send the response to the browser and close the process.
            var response = context.Response;
            string responseString = "<html><head><meta http-equiv=\'refresh\' content=\'2;url=https://bimobject.com\'></head><body>Please return to the app.</body></html>";
            var buffer = Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            var responseOutput = response.OutputStream;
            var responseTask = responseOutput.WriteAsync(buffer, 0, buffer.Length).ContinueWith((task) =>
            {
                responseOutput.Close();
                http.Stop();
            });

            // Checks for errors.
            if (context.Request.QueryString.Get("error") != null)
            {
                Log($"Authorization error: {context.Request.QueryString.Get("error")}.");
                return;
            }

            if (context.Request.QueryString.Get("code") == null || context.Request.QueryString.Get("state") == null)
            {
                Log("Bad Authorization Request:" + context.Request.QueryString);
                return;
            }

            // extracts the code
            var code = context.Request.QueryString.Get("code");
            var incomingState = context.Request.QueryString.Get("state");

            // Compares the receieved state to the expected Result, to ensure that
            // this app made the request which resulted in authorization.
            if (incomingState != state)
            {
                Log($"Received request with invalid state: {incomingState}.");
                return;
            }

            // Starts the code exchange at the Token Endpoints and uses the token to get a list of products
            await PerformCodeExchange(code, codeVerifier, redirectUri);
            productList.IsEnabled = true;
            GetProducts(TokenStore.AccessToken);
        }

        private async Task PerformCodeExchange(string code, string codeVerifier, string redirectUri)
        {
            // Builds the request uri
            var tokenRequestBody = $"code={code}&redirect_uri={redirectUri}&client_id={_clientId}&code_verifier={codeVerifier}&client_secret={_clientSecret}&scope={Scopes}&grant_type=authorization_code";
            var response = await ApiRequestHelper.PostRequest<Dictionary<string, string>>(Endpoints.TokenEndpoint, tokenRequestBody); 

            // Get all the acces_token and refresh_token and the expiry from the response
            TokenStore.Update(response.Result);
        }

        private async void GetProducts(string accessToken)
        {           
            // Token has expired get a new access token and update the token store.      
            if (TokenStore.AccessTokenExpirationUtc <= DateTime.UtcNow)
                await RefreshToken();
                
            var request = $"{Endpoints.SearchApiBaseUrl}?filter.filetypeid={RevitFileType}";
            var response = ApiRequestHelper.GetRequest<ArrayResult<ProductItem>>(request, accessToken);
            var result = await response;

            var items = result.Result;
            productList.Items.Clear();
            ProductItem[] products = items.Data;

            foreach (var product in products)
            {
                productList.Items.Add(new ListViewItem
                {
                    Content = product.Name,
                    Tag = product.Id,
                });
            }
        }

        private async Task RefreshToken()
        {  
            var refreshDetails = await ApiRequestHelper.RefreshAccessToken<Dictionary<string, string>>(_clientId, _clientSecret, TokenStore.RefreshToken);
            TokenStore.Update(refreshDetails);
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            var item = productList.SelectedItems[0] as ListViewItem;
            var id = item?.Tag;

            if (id == null)
            {
                Log("Invalid or missing file id.");
                return;
            }

            DownloadFile(id.ToString());
        }

        private async void DownloadFile(string productId)
        {
            Log("Making API Call to Userinfo...");

            // Token has expired get a new access token and update the token store.
            if (TokenStore.AccessTokenExpirationUtc <= DateTime.UtcNow)
                await RefreshToken();

            // builds the request
            var request = $"{Endpoints.SearchApiBaseUrl}/{productId}";

            // sends the request with the token from the store.
            var productDetailsResponse = await ApiRequestHelper.GetRequest<ObjectResult<Product>>(request, _tokenStore.AccessToken);
            var fileDetails = productDetailsResponse.Result.Data.Files.First(x => x.FileType.Id == RevitFileType);
            var fileName = fileDetails.Name;
            var fileId = fileDetails.Id;

            request = $"{Endpoints.SearchApiBaseUrl}/{productId}/files/{fileId}/binary";

            var response = await ApiRequestHelper.GetBinaryRequest(request, fileName, _tokenStore.AccessToken);
            var file = response.Result;

            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }

            // Try to create the directory.
            using (var fileStream = File.Create(_directoryPath + "\\" + fileName))
            {
                await fileStream.WriteAsync(file, 0, file.Length);
            }

            MessageBox.Show($"{fileName} downloaded!");
        }


        private void Download_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new WinForms.FolderBrowserDialog();
            var result = dialog.ShowDialog();

            if (result != WinForms.DialogResult.OK)
            {
                Log("Invalid directory. Please choose another.");
            }

            directory.Text = dialog.SelectedPath;
            _directoryPath = dialog.SelectedPath;
        }

        // Log message
        private static void Log(string message)
        {
            //TODO Log error messages
        }
    }
}
