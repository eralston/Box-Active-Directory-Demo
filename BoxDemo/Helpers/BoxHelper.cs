using Box.V2;
using Box.V2.Config;
using Box.V2.JWTAuth;
using Box.V2.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace BoxDemo.Helpers
{
    public class BoxHelper
    {
        static readonly string CLIENT_ID = ConfigurationManager.AppSettings["boxClientId"];
        static readonly string CLIENT_SECRET = ConfigurationManager.AppSettings["boxClientSecret"];
        static readonly string ENTERPRISE_ID = ConfigurationManager.AppSettings["boxEnterpriseId"];
        static readonly string JWT_PRIVATE_KEY_PASSWORD = ConfigurationManager.AppSettings["boxPrivateKeyPassword"];
        static readonly string JWT_PRIVATE_KEY = File.ReadAllText(HttpContext.Current.Server.MapPath("~") + "/private_key.pem");
        static readonly string JWT_PUBLIC_KEY_ID = ConfigurationManager.AppSettings["boxPublicKeyId"];

        static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(45);

        static readonly BoxConfig BoxConfig = new BoxConfig(CLIENT_ID, CLIENT_SECRET, ENTERPRISE_ID, JWT_PRIVATE_KEY, JWT_PRIVATE_KEY_PASSWORD, JWT_PUBLIC_KEY_ID);
        public static readonly BoxJWTAuth BoxJwtAuth = new BoxJWTAuth(BoxConfig);

        static readonly string CACHE_PREFIX = ConfigurationManager.AppSettings["boxEnterpriseId"];

        public static BoxClient AdminClient()
        {
            return BoxJwtAuth.AdminClient(EnterpriseToken());
        }

        public static BoxClient UserClient(string boxUserId)
        {
            return BoxJwtAuth.UserClient(UserToken(boxUserId), boxUserId);
        }

        public static string EnterpriseToken()
        {
            return BoxJwtAuth.AdminToken();
        }

        public static string UserToken(string boxUserId)
        {
            object userTokenObject = BoxJwtAuth.UserToken(boxUserId);
            return (string)userTokenObject;
        }

        public static async Task<BoxUser> CreateBoxUser(string email)
        {
            BoxUserRequest userRequest = new BoxUserRequest() { Name = email, IsPlatformAccessOnly = true };
            BoxUser appUser = await AdminClient().UsersManager.CreateEnterpriseUserAsync(userRequest);
            return appUser;
        }

        public static async Task<BoxUser> GetBoxUser(string email)
        {
            var appUser = await AdminClient().UsersManager.GetEnterpriseUsersAsync(email);
            return appUser.Entries.FirstOrDefault();
        }

        public static async Task<BoxUser> GetOrCreateBoxUser(string email)
        {
            var boxUser = await GetBoxUser(email);
            if (boxUser == null)
                boxUser = await CreateBoxUser(email);
            return boxUser;
        }

        public static async Task Setup(BoxClient boxClient)
        {
            var folderRequest = new BoxFolderRequest() { Name = "Test Folder", Parent = new BoxRequestEntity() { Id = "0" } };
            var newFolder = await boxClient.FoldersManager.CreateAsync(folderRequest);

            var pathToFile = HttpContext.Current.Server.MapPath("~/");
            var fileName = "text.txt";
            using (FileStream fs = File.Open(pathToFile + fileName, FileMode.Open))
            {
                // Create request object with name and parent folder the file should be uploaded to
                BoxFileRequest request = new BoxFileRequest()
                {
                    Name = fileName,
                    Parent = new BoxRequestEntity() { Id = "0" }
                };
                var boxFile = await boxClient.FilesManager.UploadAsync(request, fs);
            }


        }
    }
}