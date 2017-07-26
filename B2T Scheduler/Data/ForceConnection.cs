using Salesforce.Common;
using Salesforce.Common.Models;
using Salesforce.Force;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace B2T_Scheduler.Data
{
    public class ForceConnection
    {
        public Dictionary<string, SalesForceCredential> SalesForceCredentials = new Dictionary<string, SalesForceCredential>();
        public string ActiveEnvironment = "PROD";
        private ForceClient _forceClient = null;

        public AuthenticationClient AuthClient {get; private set;}

        public ForceConnection()
        {
            SalesForceCredentials.Add("QA", new SalesForceCredential
            {
                ClientId = "3MVG9snqYUvtJB1OjjKeISDtvXiPVZpp2zmsjjbltCws7Pa5PV9jJapRREsN4Ez9Bg038chFJLHlom_AN_R2y",
                ClientSecret = "458262255620564311",
                Securitytoken = "5lrBa4mDRdlT1Qmksl3sEhBGE",
                Username = "jeff.gilbert@b2t.com.qa",
                Password = "DeerFoot27",
                TokenRequestEndpointUrl = "https://test.salesforce.com/services/oauth2/token",
                UserInfoEndpointUrl = "https://test.salesforce.com/services/oauth2/userinfo"
            });
            SalesForceCredentials.Add("DEV", new SalesForceCredential
            {
                ClientId = "3MVG9hq7jmfCuKfe8hUZHJ9MyeQ.kw55IUHqkWm5Ej98y2ngLsUYRzMM3xtP5zdcPfGbNWr1MkgwAni_3qiVs",
                ClientSecret = "3499427454799364352",
                Securitytoken = "7WjZzUOSFivrkmnGhKHIFJY7e",
                Username = "jeff@b2ttraining.com.dev",
                Password = "DeerFoot27",
                TokenRequestEndpointUrl = "https://test.salesforce.com/services/oauth2/token",
                UserInfoEndpointUrl = "https://test.salesforce.com/services/oauth2/userinfo"
            });
            SalesForceCredentials.Add("PROD", new SalesForceCredential
            {
                ClientId = "3MVG9KI2HHAq33RzEfF5JLxL4LBJp7d0tKQ7dSTOQzB7rvUWlDXKe6iDqluna_6UI1VTcMb9bBPZadEie9k7u",
                ClientSecret = "8129887986087408605",
                Securitytoken = "nnFCRXClZ48dWx7L3ROeI2KB",
                Username = "salesforce-tech@b2ttraining.com",
                Password = "B2T17admin",
                //Username = "projects@b2ttraining.com",
                //Password = "ACC360admin",
                //Securitytoken = "hhWZiiiBx3SLunZhoDK5oGiP4",
                TokenRequestEndpointUrl = "https://login.salesforce.com/services/oauth2/token",
                UserInfoEndpointUrl = "https://login.salesforce.com/services/oauth2/userinfo"
            });
        }

        public bool IsAuthenticated()
        {
            return Authenticate();
        }

        public bool Authenticate(string username = null, string password = null, string environment = null)
        {
            ActiveEnvironment = environment ?? ActiveEnvironment;
            var cred = SalesForceCredentials[ActiveEnvironment];
            cred.Username = username ?? cred.Username;
            cred.Password = password ?? cred.Password;
            return (Connect() != null);
        }

        public ForceClient Connect()
        {
            if (_forceClient == null)
            {
                var cred = SalesForceCredentials[ActiveEnvironment];
                AuthClient = new AuthenticationClient();
                AuthClient.UsernamePasswordAsync(cred.ClientId, cred.ClientSecret, cred.Username, cred.Password + cred.Securitytoken, cred.TokenRequestEndpointUrl).Wait();
                _forceClient = new ForceClient(AuthClient.InstanceUrl, AuthClient.AccessToken, AuthClient.ApiVersion);
            }
            return _forceClient;
        }
    }
}
