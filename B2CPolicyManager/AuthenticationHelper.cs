using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace B2CPolicyManager
{
    public class AuthenticationHelper
    {
        public static string[] Scopes = { "User.Read" };

        private static PublicClientApplication identityClientApp;
        private static readonly object padlock = new object();

        public static PublicClientApplication IdentityClientApp {
            get
            {
                if (identityClientApp == null)
                {
                    lock (padlock)
                    {
                        if (identityClientApp == null)
                        {
                            identityClientApp = new PublicClientApplication(Properties.Settings.Default.V2AppId);
                        }
                    }
                }
                return identityClientApp;
            }
        }
        
        public static DateTimeOffset Expiration;

        /// <summary>
        /// Get Token for User.
        /// </summary>
        /// <returns>Token for user.</returns>
        public static async Task<string> GetTokenForUserAsync()
        {
            AuthenticationResult authResult;
            string tokenForUser = null;
            try
            {
                authResult = await IdentityClientApp.AcquireTokenSilentAsync(Scopes, IdentityClientApp.Users.First());
                tokenForUser = authResult.AccessToken;
            }

            catch (Exception)
            {
                if (tokenForUser == null || Expiration <= DateTimeOffset.UtcNow.AddMinutes(5))
                {
                    try
                    {
                        authResult = await IdentityClientApp.AcquireTokenAsync(Scopes);

                        tokenForUser = authResult.AccessToken;
                        Expiration = authResult.ExpiresOn;
                    }
                    catch
                    {
                        return tokenForUser;
                    }
                }
            }

            return tokenForUser;
        }


        public static async Task AddHeadersAsync(HttpRequestMessage requestMessage)
        {
            string tokenForUser = await GetTokenForUserAsync();
            try
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", tokenForUser);
                requestMessage.Headers.Add("SampleID", "policy-manager-jasuri");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Could not add headers to HttpRequestMessage: " + ex.Message);
            }
        }

    }
}
