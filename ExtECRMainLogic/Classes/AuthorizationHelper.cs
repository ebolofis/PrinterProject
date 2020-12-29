using ExtECRMainLogic.Enumerators;
using Microsoft.Extensions.Configuration;

namespace ExtECRMainLogic.Classes
{
    public class AuthorizationHelper
    {
        private readonly IConfiguration configuration;

        public AuthorizationHelper(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public AuthorizationEnum Authorize(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return AuthorizationEnum.Unknown;

            string adminPassword = configuration.GetValue<string>("AdminPassword");
            if (adminPassword == password)
                return AuthorizationEnum.Admin;

            string userPassword = configuration.GetValue<string>("UserPassword");
            if (userPassword == password)
                return AuthorizationEnum.User;

            return AuthorizationEnum.Unknown;
        }
    }
}