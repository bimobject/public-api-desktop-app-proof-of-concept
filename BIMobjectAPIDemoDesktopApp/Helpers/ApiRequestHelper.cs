using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace BIMobjectAPIDemoDesktopApp.Helpers
{
    public class ApiRequestHelper
    {
        private const string ContentType = "application/x-www-form-urlencoded";
        private const string Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

        public static async Task<Response<T>> PostRequest<T>(string endpoint, string requestBody, string token = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "POST";

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Add($"Authorization: Bearer {token}");

            request.ContentType = ContentType;
            request.Accept = Accept;
            byte[] byteVersion = Encoding.ASCII.GetBytes(requestBody);
            request.ContentLength = byteVersion.Length;
            Stream stream = request.GetRequestStream();
            await stream.WriteAsync(byteVersion, 0, byteVersion.Length);
            stream.Close();

            try
            {
                // gets the response
                var response = await request.GetResponseAsync();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();

                    // converts to dictionary
                    var value = JsonConvert.DeserializeObject<T>(responseText);
                    return new Response<T> { Result = value, Status = HttpStatusCode.OK };
                }
            }
            catch (WebException ex)
            {
                var result = new Response<T> { Result = default(T) };

                if (ex.Status != WebExceptionStatus.ProtocolError)
                    return result;

                if (!(ex.Response is HttpWebResponse errorResponse))
                    return result;

                result.Status = errorResponse.StatusCode;
                return result;
            }
        }

        public static async Task<Response<T>> GetRequest<T>(string endpoint, string token = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "GET";

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Add($"Authorization: Bearer {token}");

            request.ContentType = ContentType;
            request.Accept = Accept;

            try
            {
                // gets the response
                var response = await request.GetResponseAsync();
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    // reads response body
                    string responseText = await reader.ReadToEndAsync();

                    // converts to dictionary
                    var value = JsonConvert.DeserializeObject<T>(responseText);
                    return new Response<T> { Result = value, Status = HttpStatusCode.OK };
                }
            }
            catch (WebException ex)
            {
                var result = new Response<T> { Result = default(T) };

                if (ex.Status != WebExceptionStatus.ProtocolError)
                    return result;

                if (!(ex.Response is HttpWebResponse errorResponse))
                    return result;

                result.Status = errorResponse.StatusCode;
                return result;
            }
        }


        public static async Task<Response<byte[]>> GetBinaryRequest(string endpoint, string fileName, string token = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(endpoint);
            request.Method = "GET";

            if (!string.IsNullOrWhiteSpace(token))
                request.Headers.Add($"Authorization: Bearer {token}");

            request.ContentType = ContentType;
            request.Accept = Accept;

            try
            {
                byte[] result = null;
                byte[] buffer = new byte[4097];
                using (var memoryStream = new MemoryStream())
                {
                    WebResponse binaryResponse = await request.GetResponseAsync();
                    using (var reader = new BinaryReader(binaryResponse.GetResponseStream()))
                    {
                        do
                        {
                            var count = reader.Read(buffer, 0, buffer.Length);
                            memoryStream.Write(buffer, 0, count);

                            if (count == 0)
                            {
                                break;
                            }
                        } while (true);

                        var file = memoryStream.ToArray();                   
                        return new Response<byte[]> { Result = file, Status = HttpStatusCode.OK };
                    }
                }
            }
            catch (WebException ex)
            {
                var result = new Response<byte[]> { Result = new byte[0] };

                if (ex.Status != WebExceptionStatus.ProtocolError)
                    return result;

                if (!(ex.Response is HttpWebResponse errorResponse))
                    return result;

                result.Status = errorResponse.StatusCode;
                return result;
            }
        }

        /// <summary>
        /// Uses the client Id, client id, and refresh token to aquire a new access token from the endpoint
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="tokenStore"></param>
        /// <returns></returns>
        public static async Task<T> RefreshAccessToken<T>(string clientId, string clientSecret, string refreshToken)
        {
             var refreshTokenUri = $"client_id={clientId}&refresh_token={refreshToken}&client_secret={clientSecret}&grant_type=refresh_token";
             var response = await PostRequest<T>(Endpoints.TokenEndpoint, refreshTokenUri);

            if(response.Status != HttpStatusCode.OK)
                throw new Exception("Unable to refresh token.");

            return response.Result;
        }
    }
}
