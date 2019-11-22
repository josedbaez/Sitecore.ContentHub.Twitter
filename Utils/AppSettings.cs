using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace Sitecore.ContentHub.Twitter.Utils
{
    //original source: https://github.com/STYLELABS/cloud-dev-examples/blob/master/src/Skeleton/WebClientSkeleton/WebClientSkeleton/AppSettings.cs
    public static class AppSettings
    {
        private static IConfiguration _config;
        public static IConfiguration Configuration
        {
            get
            {
                if (_config == null)
                {
                    _config = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("local.settings.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .Build();
                }

                return _config;
            }
        }

        public static Uri Host { get { return new Uri($"{Configuration["ContentHub:Host"]}"); } }
        
        public static string ClientId { get { return $"{Configuration["ContentHub:ClientId"]}"; } }
        
        public static string ClientSecret { get { return $"{Configuration["ContentHub:ClientSecret"]}"; } }
        
        public static string Username { get { return $"{Configuration["ContentHub:Username"]}"; } }
        
        public static string Password { get { return $"{Configuration["ContentHub:Password"]}"; } }

        public static string TwitterConsumerKey { get { return $"{Configuration["Twitter:ConsumerKey"]}"; } }

        public static string TwitterConsumerSecret { get { return $"{Configuration["Twitter:ConsumerSecret"]}"; } }

        public static string TwitterUserAccessToken { get { return $"{Configuration["Twitter:UserAccessToken"]}"; } }

        public static string TwitterUserAccessSecret { get { return $"{Configuration["Twitter:UserAccessSecret"]}"; } }
    }
}
