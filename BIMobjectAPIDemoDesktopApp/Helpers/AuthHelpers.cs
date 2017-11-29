using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;

namespace BIMobjectAPIDemoDesktopApp.Helpers
{
    public class AuthHelpers
    {
        public const string CodeChallengeMethod = "S256";

        public static string CryptoRandomBase64Url(uint length)
        {
            var cryptoService = new RNGCryptoServiceProvider();
            byte[] bytes = new byte[length];
            cryptoService.GetBytes(bytes);
            return Base64UrlencodeNoPadding(bytes);
        }

        public static byte[] Sha256(string value)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(value);
            var sha256 = new SHA256Managed();
            return sha256.ComputeHash(bytes);
        }

        public static string Base64UrlencodeNoPadding(byte[] buffer)
        {
            var base64 = Convert.ToBase64String(buffer);
            base64 = base64.Replace("+", "-");
            base64 = base64.Replace("/", "_");
            base64 = base64.Replace("=", "");
            return base64;
        }

        public static HttpListener CreateListener(string uri)
        {
            var http = new HttpListener();
            http.Prefixes.Add(uri);
            return http;
        }

        public static string CreateRedirectUrl()
        {
            return $"http://{IPAddress.Loopback}:{GetRandomUnusedPort()}/";
        }

        /* ref http://stackoverflow.com/a/3978040 this is enabled for 
        Callback/redirect urls for the bimobject api*/
        private static int GetRandomUnusedPort()
        {
            var listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}