using System.Collections.Generic;

namespace BIMobjectAPIDemoDesktopApp.Helpers
{
    using System;

    public class MemoryTokenStore : ITokenStore
    {
        public string AccessToken { get; set; }
        public DateTime? AccessTokenExpirationUtc { get; set; }
        public string RefreshToken { get; set; }

        public void Clear()
        {
            AccessToken = null;
            AccessTokenExpirationUtc = null;
            RefreshToken = null;
        }

        public void Update(Dictionary<string, string> response)
        {
            AccessToken = response["access_token"];
            RefreshToken = response["refresh_token"];
            var expiry = int.Parse(response["expires_in"]);
            AccessTokenExpirationUtc = DateTime.UtcNow.AddSeconds(expiry);
        }
    }

    public interface ITokenStore
    {
        string AccessToken { get; set; }
        DateTime? AccessTokenExpirationUtc { get; set; }
        string RefreshToken { get; set; }
        void Update(Dictionary<string, string> response);
        void Clear();
    }
}
