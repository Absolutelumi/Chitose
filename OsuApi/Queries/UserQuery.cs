using OsuApi.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace OsuApi.Queries
{
    public interface IUserQuery
    {
        Task<User> GetUser();

        IUserQuery WithMode(Mode mode);
    }

    public interface IUserSpecificQuery
    {
        IUserQuery WithUser(string user, UserCredentialType type = UserCredentialType.Auto);
    }

    internal class UserQuery : IUserQuery, IUserSpecificQuery
    {
        private Dictionary<string, string> Parameters;

        internal UserQuery(string apiKey)
        {
            Parameters = new Dictionary<string, string>
            {
                { "k", apiKey }
            };
        }

        public async Task<User> GetUser()
        {
            var queryString = string.Join("&", Parameters.Select(pair => $"{pair.Key}={pair.Value}"));
            var userRequest = WebRequest.CreateHttp($"https://osu.ppy.sh/api/get_user?{queryString}");
            var response = await userRequest.GetResponseAsync();
            string responseJson = new StreamReader(response.GetResponseStream()).ReadToEnd();
            return Api.Json.Deserialize<User[]>(responseJson).First();
        }

        public IUserQuery WithUser(string user, UserCredentialType type = UserCredentialType.Auto)
        {
            Parameters["u"] = user;
            return this;
        }

        public IUserQuery WithMode(Mode mode)
        {
            Parameters["m"] = $"{(int)mode}"; ;
            return this; 
        }
    }
}
