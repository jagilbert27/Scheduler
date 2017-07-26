namespace B2T_Scheduler.Data
{
    public class SalesForceCredential
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Username { get; set; }
        public string Securitytoken { get; set; }
        public string Password { get; set; }
        public string TokenRequestEndpointUrl { get; set; }
        public string UserInfoEndpointUrl { get; set; }
    }
}
