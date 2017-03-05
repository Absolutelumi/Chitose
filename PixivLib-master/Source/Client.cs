using System;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Specialized;
using Mono.Web;
using PixivLib.Models;
using System.Net;
using System.Linq;

namespace PixivLib
{
    public class PixivClient
    {
        // State.
        private CookieContainer cookieJar = new CookieContainer();
        private HttpClientHandler handler = new HttpClientHandler();
        private SessionManager sessionMan = new SessionManager();
        private HttpClient client;

        // Settings.
        public bool UseSSL = true;

        // Magic values.
        const string UserAgent = "PixivAndroidApp/4.9.6";
        const string AndroidToken = "8mMXXWT9iuwdJvsVIvQsFYDwuZpRCMePeyagSh30ZdU";

        const string OAuthClientID = "BVO2E8vAAikgUBW8FYpi6amXOjQj";
        const string OAuthClientSecret = "LI1WsFUDrrquaINOdarrJclCrkTtc3eojCOswlog";

        const string ImageSizes = "px_128x128,px_480mw,large";

        // API structure.
        const string URLAppSettings = "https://app-setting.secure.pixiv.net/v1/setting/pixiv_android.json";
        const string URLOAuthLogin = "https://oauth.secure.pixiv.net/auth/token";
        const string URLAPIv1Root = "https://public-api.secure.pixiv.net/v1";
        const string PathEmojis = "/emojis.json";
        const string PathRankingIllust = "/ranking/illust.json";
        const string PathSearchWorks = "/search/works.json";
        const string PathUserGet = "/users/{0}.json";

        public PixivClient()
        {
            handler.CookieContainer = cookieJar;
            client = new HttpClient(handler);

            client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        }

        #region Utility
        /// <summary>
        /// Gets a URL-safe NameValueCollection.
        /// </summary>
        /// <returns></returns>
        private NameValueCollection NewQuery()
        {
            return HttpUtility.ParseQueryString("");
        }

        HttpRequestMessage RequestWithAuth(HttpMethod method, string root, string path, object query, string token)
        {
            var url = new Uri(root + path + "?" + query);

            // Build request.
            var request = new HttpRequestMessage()
            {
                RequestUri = url,
                Method = method,
            };

            request.Headers.Add("Authorization", "Bearer " + token);

            return request;
        }

        HttpRequestMessage AnonRequest(HttpMethod method, string root, string path, object query)
        {
            if (!IsAuthenticated())
                throw new Exception("Authentication required.");

            return RequestWithAuth(method, root, path, query, AndroidToken);
        }

        HttpRequestMessage UserRequest(HttpMethod method, string root, string path, object query)
        {
            if (!IsAuthenticated())
                throw new Exception("Authentication required.");

            return RequestWithAuth(method, root, path, query, sessionMan.GetSession().AccessToken);
        }
        #endregion

        #region Requests
        async Task<AppSettingsResponse> AppSettingsRequest()
        {
            HttpResponseMessage resp = await client.GetAsync(URLAppSettings);

            return await resp.Content.ReadAsAsync<AppSettingsResponse>();
        }

        async Task<RankingResponse> RankedIllustsRequest()
        {
            // Build query string.
            var query = NewQuery();
            query["mode"] = "daily";
            query["page"] = "1";
            query["image_sizes"] = ImageSizes;

            if (UseSSL)
                query["get_secure_url"] = "1";
            
            // Build request.
            var request = AnonRequest(HttpMethod.Get, URLAPIv1Root, PathRankingIllust, query);

            // Send request.
            HttpResponseMessage resp = await client.SendAsync(request);

            // Deserialize results.
            return await resp.Content.ReadAsAsync<RankingResponse>();
        }

        async Task<AuthTokenResponse> AuthRequest(string username, string password)
        {
            // Build form data.
            var form = new MultipartFormDataContent();
            form.Add(new StringContent(OAuthClientID), "client_id");
            form.Add(new StringContent(OAuthClientSecret), "client_secret");
            form.Add(new StringContent("password"), "grant_type");
            form.Add(new StringContent(username), "username");
            form.Add(new StringContent(password), "password");
            form.Add(new StringContent("pixiv"), "device_token");

            // Build request.
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(URLOAuthLogin),
                Method = HttpMethod.Post,
                Content = form,
            };

            // Send request.
            HttpResponseMessage resp = await client.SendAsync(request);

            // Deserialize results.
            return await resp.Content.ReadAsAsync<AuthTokenResponse>();
        }

        async Task<EmojiResponse> GetEmojisRequest()
        {
            // Build query string.
            var query = NewQuery();

            if (UseSSL)
                query["get_secure_url"] = "1";

            // Build request.
            var request = UserRequest(HttpMethod.Get, URLAPIv1Root, PathEmojis, query);
            
            // Perform the request.
            var resp = await client.SendAsync(request);

            // Deserialize results.
            return await resp.Content.ReadAsAsync<EmojiResponse>();
        }

        async Task<UserResponse> GetUserRequest(int userID)
        {
            // Build query string.
            var query = NewQuery();
            query["profile_image_sizes"] = "px_170x170";
            query["include_stats"] = "1";
            query["include_profile"] = "1";
            query["include_contacts"] = "1";
            query["include_workspace"] = "1";

            if (UseSSL)
                query["get_secure_url"] = "1";

            // Build request.
            var path = String.Format(PathUserGet, userID);
            var request = UserRequest(HttpMethod.Get, URLAPIv1Root, path, query);

            // Perform the request.
            var resp = await client.SendAsync(request);

            // Deserialize results.
            return await resp.Content.ReadAsAsync<UserResponse>();
        }

        async Task<WorksResponse> SearchWorksRequest(string search, int page)
        {
            // Build query string.
            var query = NewQuery();
            query["q"] = search;
            query["mode"] = "tag";
            query["types"] = "illustration,manga,ugoira";
            query["order"] = "desc";
            query["sort"] = "date";
            query["period"] = "all";
            query["image_sizes"] = ImageSizes;
            query["page"] = "" + page;
            query["per_page"] = "50";

            if (UseSSL)
                query["get_secure_url"] = "1";

            // Build request.
            var request = UserRequest(HttpMethod.Get, URLAPIv1Root, PathSearchWorks, query);

            // Perform the request.
            var resp = await client.SendAsync(request);

            // Deserialize results.
            return await resp.Content.ReadAsAsync<WorksResponse>();
        }
        #endregion

        #region Authentication
        public bool IsAuthenticated()
        {
            return sessionMan.GetSession() != null;
        }

        public async Task Authenticate(string username, string password)
        {
            AuthToken auth;

            auth = (await AuthRequest(username, password)).Token;

            // Extract the magic PHP_SESSID from the cookies.
            var cookieUri = new Uri(URLOAuthLogin);
            var cookies = cookieJar.GetCookies(cookieUri).Cast<Cookie>();
            var cookie = cookies.FirstOrDefault(c => c.Name == "PHPSESSID");

            if (cookie != null)
                cookie.Expired = true;

            sessionMan.PutSession(new Session()
            {
                AccessToken = auth.AccessToken,
                RefreshToken = auth.RefreshToken,
                DeviceToken = auth.DeviceToken,
                ProfileImageURL = auth.User.ImageURLs.LargeURL,
                UserID = auth.User.ID,
                Name = auth.User.Name,
                Account = auth.User.Account,
                PHPSessionID = cookie?.Value,
            });
        }
        #endregion

        #region User
        public async Task<User> GetUser(int userID)
        {
            return (await GetUserRequest(userID)).Users[0];
        }
        #endregion

        #region Emoji
        public async Task<Emoji[]> GetEmoji()
        {
            return (await GetEmojisRequest()).Emojis;
        }
        #endregion

        #region Works
        public struct SearchWorksQuery
        {
            public string search;
        }

        public class SearchWorksPaginator : IPaginator<Work>
        {
            PixivClient client;
            SearchWorksQuery query;
            WorksResponse response;

            public SearchWorksPaginator(PixivClient c, SearchWorksQuery q)
            {
                client = c;
                query = q;
            }

            public async Task SetPage(int page)
            {
                response = await client.SearchWorksRequest(query.search, page);
            }

            public Work[] Items()
            {
                return response.Results;
            }

            public Pagination GetPagination()
            {
                return response.Pagination;
            }
        }

        public async Task<Pager<Work>> SearchWorks(string search)
        {
            var paginator = new SearchWorksPaginator(this, new SearchWorksQuery()
            {
                search = search,
            });

            await paginator.SetPage(1);

            return new Pager<Work>(paginator);
        }
        #endregion
    }
}
