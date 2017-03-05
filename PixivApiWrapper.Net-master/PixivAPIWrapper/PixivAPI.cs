using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.VisualBasic.FileIO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using PixivAPIWrapper.Model;

namespace PixivAPIWrapper
{
    /// <summary>
    /// PixivのAPIの根幹をなす機能をまとめたクラスです。
    /// 初めにLogin()を使ってログインをしておく必要があります。
    /// <seealso cref="Login"/>
    /// </summary>
    public class PixivAPI
    {
        /*
         * 定数群
         */
        private readonly string BasePixivUrl = "https://www.secure.pixiv.net/";
        private readonly string BaseURL = "http://spapi.pixiv.net/iphone/";        
        private readonly string SessionID = "PHPSESSID";
        private readonly string DummyParameter = "dummy=0";
        private readonly string Daily = "mode=day";
        private readonly string Weekly = "mode=week";
        private readonly string Monthly = "mode=month";
        private readonly string Hide = "rest=hide";
        private readonly string IDParamTemplate = "id={0}";
        private readonly Encoding UTF8 = Encoding.GetEncoding("UTF-8");
        private enum Type
        {
            new_illust, mypixiv_new_illust, bookmark_user_new_illust, ranking, search, search_user, bookmark, bookmark_user_all, mypixiv_all, member_illust, new_novel, new_novel_r18, novel_ranking, bookmark_user_new_novel, mypixiv_new_novel, bookmark_novel, member_novel, novel_bookmarks, novel_comments, illust_comments, rating_novel
        }
        /// <summary>
        /// Current session ID
        /// </summary>
        public string Session { get; private set; }

        /// <summary>
        /// Constructs a new API object. Session still needs to be instantiated through <see cref="Login"/>
        /// </summary>
        public PixivAPI(){
            this.Session = null;          
        }

        /// <summary>
        /// Logs the user in. This is required for many of the API operations since they rely on the session ID.
        /// </summary>
        /// <param name="id">User ID. Can be either their username or e-mail.</param>
        /// <param name="pass">User password. Is handled over a HTTPS connection and not stored locally.</param>
        /// <remarks>sorry for the semi-fugly construction, apparently the URL form way of old fails nowadays. 
        /// At least this is safer...</remarks>
        /// <returns>Whether the user has been logged in succesfully</returns>
        public bool Login(String id, String pass){
            //Use empty cookiejar (heh) for retrieval of PHPSESSID
            CookieContainer cookieJar = new CookieContainer();
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}login.php", BasePixivUrl));
            req.CookieContainer = cookieJar;

            //use post method with form
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            //construct post request form
            StringBuilder postData = new StringBuilder();
            postData.Append("mode="+ HttpUtility.UrlEncode("login") + "&");
            postData.Append("pixiv_id=" + HttpUtility.UrlEncode(id) + "&");
            postData.Append("pass=" + HttpUtility.UrlEncode(pass) + "&");
            postData.Append("skip=" + HttpUtility.UrlEncode("0")); 

            ASCIIEncoding ascii = new ASCIIEncoding();
            byte[] postBytes = ascii.GetBytes(postData.ToString());

            req.ContentLength = postBytes.Length;

            Stream postStream = req.GetRequestStream();
            postStream.Write(postBytes, 0, postBytes.Length);
            postStream.Flush();
            postStream.Close();

            //request login response
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();

            //retrieve PHPSESSID from cookies
            Cookie result = null;
            foreach (Cookie cookie in cookieJar.GetCookies(new Uri(BasePixivUrl)))
            {
                if (cookie.Name == SessionID && cookie.Value.Contains('_'))
                {
                    result = cookie;
                    break;
                }
            }

            if(result != null)
                this.Session = result.Value;
            
            return this.LoggedIn;
        }

        /// <summary>
        /// Returns whether the user is currently logged in based on the session ID
        /// </summary>
        /// <returns></returns>
        public bool LoggedIn {
            get { return this.Session != null; }
        }

        /// <summary>
        /// Check the API status. Useful for seeing whether Pixiv.net is availabe.
        /// </summary>
        /// <returns>true if all is good</returns>
        public bool Status()
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}maintenance.php?software-version=1.0", BaseURL));
            req.Host = "www.pixiv.net";
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            return res.StatusCode == HttpStatusCode.OK;
        }

        /// <summary>
        /// Returns an HTML page of the user's profile with general information about them. 
        /// Returns an empty string if the user is not logged in.
        /// </summary> 
        /// <returns>An HTML page</returns>
        public string Profile()
        {
            if (!this.LoggedIn) 
                return "";

            StreamReader strr;
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}profile.php?dummy=0&{1}={2}", BaseURL, this.SessionID, this.Session));
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream str = res.GetResponseStream();
            strr = new StreamReader(str, UTF8);
            string profileStr = strr.ReadToEnd();
            strr.Close();
            str.Close();
            res.Close();
            return profileStr;
        }

        /// <summary>
        /// Returns the amount of new images on Pixiv. 
        /// </summary>
        /// <returns></returns>
        public int GetNewImageSize(){
            return this.GetSize(Type.new_illust, DummyParameter);
        }

        /// <summary>
        /// Get new images by page
        /// </summary>
        /// <param name="page">The page from which to retrieve the new images</param>
        /// <returns>An array of the new images</returns>
        public Illustration[] GetNewImages(int page)
        {
            return this.GetImages(Type.new_illust, DummyParameter, page);
        }

        /// <summary>
        /// 新着小説数を取得する
        /// </summary>
        /// <returns>新着小説数</returns>
        public int GetNewNovelSize()
        {
            return this.GetSize(Type.new_novel, DummyParameter);
        }

        /// <summary>
        /// 新着小説を取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得した小説のリスト</returns>
        public Novel[] GetNewNovels(int page)
        {
            return this.GetNovels(Type.new_novel, DummyParameter, page);
        }

        /// <summary>
        /// 新着 MyPixiv イラストの数を取得する
        /// </summary>
        /// <returns>新着 MyPixiv イラスト数</returns>
        public int GetMyPixivNewImageSize()
        {
            return this.GetSize(Type.mypixiv_new_illust, DummyParameter);
        }

        /// <summary>
        /// 新着 MyPixiv イラストを取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] GetMyPixivNewImages(int page)
        {
            return this.GetImages(Type.mypixiv_new_illust, DummyParameter, page);
        }

        /// <summary>
        /// 新着 MyPixiv 小説の数を取得する
        /// </summary>
        /// <returns>新着 MyPixiv 小説数</returns>
        public int GetMyPixivNewNovelSize()
        {
            return this.GetSize(Type.mypixiv_new_novel, DummyParameter);
        }

        /// <summary>
        /// 新着 MyPixiv 小説を取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得した小説のリスト</returns>
        public Novel[] GetMyPixivNewNovels(int page)
        {
            return this.GetNovels(Type.mypixiv_new_novel, DummyParameter, page);
        }

        /// <summary>
        /// お気に入りユーザの新着イラスト数を取得する
        /// </summary>
        /// <returns>お気に入りユーザの新着イラスト数</returns>
        public int GetBookmarkedUserNewImageSize()
        {
            return this.GetSize(Type.bookmark_user_new_illust, DummyParameter);
        }

        /// <summary>
        /// お気に入りユーザの新着イラストを取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] GetBookmarkedUserNewImages(int page)
        {
            return this.GetImages(Type.bookmark_user_new_illust, DummyParameter, page);
        }

        /// <summary>
        /// お気に入りユーザの新着小説数を取得する
        /// </summary>
        /// <returns>お気に入りユーザの新着小説数</returns>
        public int GetBookmarkedUserNewNovelSize()
        {
            return this.GetSize(Type.bookmark_user_new_novel, DummyParameter);
        }

        /// <summary>
        /// お気に入りユーザの新着小説を取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得した小説のリスト</returns>
        public Novel[] GetBookmarkedUserNewNovels(int page)
        {
            return this.GetNovels(Type.bookmark_user_new_novel, DummyParameter, page);
        }

        /// <summary>
        /// デイリーランキングのイラスト数を取得する
        /// </summary>
        /// <returns>デイリーランキングのイラスト数</returns>
        public int GetDailyRankingImageSize()
        {
            return this.GetSize(Type.ranking, Daily);
        }

        /// <summary>
        /// デイリーランキングのイラストを取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] GetDailyRankingImages(int page)
        {
            return this.GetImages(Type.ranking, Daily, page);
        }

        /// <summary>
        /// デイリーランキングの小説数を取得する
        /// </summary>
        /// <returns>デイリーランキングの小説数</returns>
        public int GetDailyRankingNovelSize()
        {
            return this.GetSize(Type.novel_ranking, Daily);
        }

        /// <summary>
        /// デイリーランキングの小説を取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得した小説のリスト</returns>
        public Novel[] GetDailyRankingNovels(int page)
        {
            return this.GetNovels(Type.novel_ranking, Daily, page);
        }

        /// <summary>
        /// ウィークリーランキングのイラスト数を取得する
        /// </summary>
        /// <returns>ウィークリーランキングのイラスト数</returns>
        public int GetWeeklyRankingImageSize()
        {
            return this.GetSize(Type.ranking, Weekly);
        }

        /// <summary>
        /// ウィークリーランキングのイラストを取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] GetWeeklyRankingImages(int page)
        {
            return this.GetImages(Type.ranking, Weekly, page);
        }

        /// <summary>
        /// ウィークリーランキングの小説数を取得する
        /// </summary>
        /// <returns>ウィークリーランキングの小説数</returns>
        public int GetWeeklyRankingNovelSize()
        {
            return this.GetSize(Type.novel_ranking, Weekly);
        }

        /// <summary>
        /// ウィークリーランキングの小説を取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得した小説のリスト</returns>
        public Novel[] GetWeeklyRankingNovels(int page)
        {
            return this.GetNovels(Type.novel_ranking, Weekly, page);
        }

        /// <summary>
        /// マンスリーランキングのイラスト数を取得する
        /// </summary>
        /// <returns>マンスリーランキングのイラスト数</returns>
        public int GetMonthlyRankingImageSize()
        {
            return this.GetSize(Type.ranking, Monthly);
        }

        /// <summary>
        /// マンスリーランキングのイラストを取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] GetMonthlyRankingImages(int page)
        {
            return this.GetImages(Type.ranking, Monthly, page);
        }

        /// <summary>
        /// 自ユーザーが公開ブックマークしたイラスト数を取得する
        /// </summary>
        /// <returns>ブックマークしたイラストの数</returns>
        public int GetMyBookmarkImageSize()
        {
            return this.GetSize(Type.bookmark, DummyParameter);
        }

        /// <summary>
        /// 自ユーザーが公開ブックマークしたイラストを取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] GetMyBookmarkImages(int page)
        {
            return this.GetImages(Type.bookmark, DummyParameter, page);
        }

        /// <summary>
        /// 自ユーザーが公開ブックマークした小説数を取得する
        /// </summary>
        /// <returns>ブックマークした小説の数</returns>
        public int GetMyBookmarkNovelSize()
        {
            return this.GetSize(Type.bookmark_novel, DummyParameter);
        }

        /// <summary>
        /// 自ユーザーが公開ブックマークした小説を取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得した小説のリスト</returns>
        public Novel[] GetMyBookmarkNovels(int page)
        {
            return this.GetNovels(Type.bookmark_novel, DummyParameter, page);
        }

        /// <summary>
        /// 自ユーザーが非公開ブックマークしたイラスト数を取得する
        /// </summary>
        /// <returns>ブックマークしたイラストの数</returns>
        public int GetMyHideBookmarkImageSize()
        {
            return this.GetSize(Type.bookmark, Hide);
        }

        /// <summary>
        /// 自ユーザーが非公開ブックマークしたイラストを取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] GetMyHideBookmarkImages(int page)
        {
            return this.GetImages(Type.bookmark, Hide, page);
        }

        /// <summary>
        /// 自ユーザーが非公開ブックマークした小説数を取得する
        /// </summary>
        /// <returns>ブックマークした小説の数</returns>
        public int GetMyHideBookmarkNovelssSize()
        {
            return this.GetSize(Type.bookmark_novel, Hide);
        }

        /// <summary>
        /// 自ユーザーが非公開ブックマークした小説を取得する
        /// </summary>
        /// <param name="page">取得するページ番号</param>
        /// <returns>取得した小説のリスト</returns>
        public Novel[] GetMyHideBookmarkNovels(int page)
        {
            return this.GetNovels(Type.bookmark_novel, Hide, page);
        }

        /// <summary>
        /// 与えられたキーワードに関連するタグを付加されたイラストを取得する
        /// </summary>
        /// <param name="keyword">問い合わせるキーワード</param>
        /// <param name="size">取得するイラスト数</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] FindImagesByTag(string keyword, int size)
        {
            string param = String.Format("s_mode=s_tag&word={0}", Uri.EscapeUriString(keyword));
            return this.FindImages(param, size);
        }

        /// <summary>
        /// 与えられたキーワードに関連するタイトルを持つイラストを取得する
        /// </summary>
        /// <param name="keyword">問い合わせるキーワード</param>
        /// <param name="size">取得するイラスト数</param>
        /// <returns>取得したイラストのリスト</returns>
        public Illustration[] FindImagesByTitle(string keyword, int size)
        {
            string param = String.Format("s_mode=s_tc&word=%{0}", Uri.EscapeUriString(keyword));
            return this.FindImages(param, size);
        }

        /// <summary>
        /// Finds all Pixiv users with a given <paramref name="name"/>. Note: The API fails to retrieve users that
        /// have not yet submitted any works.
        /// </summary>
        /// <param name="name">Name of the users to search for</param>
        /// <param name="size">maximum amount of users returned</param>
        /// <param name="exactMatch">Whether the name of the user should be an exact match</param>
        /// <returns>An array of found <see cref="User"/>, at maximum <paramref name="size"/> large.</returns>
        public User[] FindUsers(string name, int size = int.MaxValue, bool exactMatch = false)
        {
            List<User> ret = new List<User>();
            string param = String.Format("s_mode=s_usr&i=0&nick={0}", Uri.EscapeUriString(name));
            if (exactMatch)
                param += "&nick_mf=1";

            for (int i = 0; true; i++)
            {
                User[] sub = this.GetUsers(Type.search_user, param, i);
                if (sub.Length == 0)
                    break;

                ret.AddRange(sub);

                if (ret.Count > size)
                    break;
            }
            return ret.Take(size).ToArray();
        }

        /// <summary>
        /// ID と名前を指定してユーザを取得する
        /// </summary>
        /// <param name="id">取得するユーザの ID</param>
        /// <param name="name">取得するユーザの名前</param>
        /// <returns>取得したユーザの User オブジェクト，ユーザが見つからなかった場合は null</returns>
        /// <remarks>未実装です。使用しないでください。</remarks>
        public User FindUser(int id, string name)
        {
            return null;
        }

        /// <summary>
        /// 指定したユーザの投稿イラスト数を取得する
        /// </summary>
        /// <param name="userId">ユーザ ID</param>
        /// <returns>指定したユーザ ID を持つユーザの投稿イラスト数</returns>
        public int GetImageSize(int userId)
        {
            return this.GetSizeById(Type.member_illust, userId);
        }
        
        /// <summary>
        /// Returns all images of the specified user's page.
        /// </summary>
        /// <param name="userId">The user from which to select the images</param>
        /// <param name="page">The page from which to select the images</param>
        /// <returns>A </returns>
        public Illustration[] GetImages(int userId, int page)
        {
            return this.GetImagesByUserId(Type.member_illust, userId, page);
        }

        /// <summary>
        /// 指定したユーザーの投稿小説数を取得する
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>指定したユーザーIDを持つユーザーの投稿小説数</returns>
        public int GetNovelSize(int userId)
        {
            return this.GetSizeById(Type.member_novel, userId);
        }

        /// <summary>
        /// 指定したユーザーの投稿小説を取得する
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="page">ページ数</param>
        /// <returns>指定したユーザーIDを持つユーザーの投稿小説</returns>
        public Novel[] GetNovels(int userId, int page)
        {
            return this.GetNovelsByUserId(Type.member_novel, userId, page);
        }

        /// <summary>
        /// 指定したユーザの MyPixiv ユーザ数を取得する
        /// </summary>
        /// <param name="userId">ユーザ ID</param>
        /// <returns>指定したユーザ ID を持つユーザの MyPixiv ユーザ数</returns>
        public int GetMyPixivSize(int userId)
        {
            return this.GetSizeById(Type.mypixiv_all, userId);
        }

        /// <summary>
        /// 指定したユーザの MyPixiv ユーザを取得する
        /// </summary>
        /// <param name="userId">ユーザ ID</param>
        /// <param name="page">ページ数</param>
        /// <returns>指定したユーザ ID を持つユーザの MyPixiv ユーザ</returns>
        public User[] GetMyPixivUsers(int userId, int page)
        {
            return this.GetUsersById(Type.mypixiv_all, userId, page);
        }

        /// <summary>
        /// 指定したユーザのお気に入りユーザ数を取得する
        /// </summary>
        /// <param name="id">ユーザ ID</param>
        /// <returns>指定したユーザ ID を持つユーザのお気に入りユーザ数</returns>
        public int GetFavoriteUserSize(int id)
        {
            return this.GetSizeById(Type.bookmark_user_all, id);
        }

        /// <summary>
        /// 指定したユーザのお気に入りユーザを取得する
        /// </summary>
        /// <param name="id">ユーザ ID</param>
        /// <param name="page">ページ数</param>
        /// <returns>指定したユーザ ID を持つユーザのお気に入りユーザ</returns>
        public User[] GetFavoriteUsers(int id, int page)
        {
            return this.GetUsersById(Type.bookmark_user_all, id, page);
        }

        /// <summary>
        /// 指定したユーザのイラストブックマーク数を取得する
        /// </summary>
        /// <param name="userId">ユーザ ID</param>
        /// <returns>指定したユーザ ID を持つユーザのブックマーク数</returns>
        public int GetImageBookmarkSize(int userId)
        {
            return this.GetSizeById(Type.bookmark, userId);
        }

        /// <summary>
        /// 指定したユーザのイラストブックマークを取得する
        /// </summary>
        /// <param name="userId">ユーザ ID</param>
        /// <param name="page">ページ数</param>
        /// <returns>指定したユーザ ID を持つユーザのブックマーク</returns>
        public Illustration[] GetImageBookmarks(int userId, int page)
        {
            return this.GetImagesByUserId(Type.bookmark, userId, page);
        }

        /// <summary>
        /// 指定したユーザの小説ブックマーク数を取得する
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <returns>指定したユーザ ID を持つユーザの小説ブックマーク数</returns>
        public int GetNovelBookmarkSize(int userId)
        {
            return this.GetSizeById(Type.bookmark_novel, userId);
        }

        /// <summary>
        /// 指定したユーザの小説ブックマークを取得する
        /// </summary>
        /// <param name="userId">ユーザ ID</param>
        /// <param name="page">ページ数</param>
        /// <returns>指定したユーザ ID を持つユーザの小説ブックマーク</returns>
        public Novel[] GetNovelBookmarks(int userId, int page)
        {
            return this.GetNovelsByUserId(Type.bookmark_novel, userId, userId);
        }

        /// <summary>
        /// ユーザ、画像または小説の総数を取得します
        /// </summary>
        /// <param name="type">取得する総数の種類</param>
        /// <param name="param">問合せ用パラメータ</param>
        /// <returns>取得したユーザ、画像または小説の総数</returns>
        private int GetSize(Type type, string param)
        {
            int ret = -1;

            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}{1}.php?{2}&{3}={4}&c_mode=count", BaseURL, type.ToString(), param, this.SessionID, this.Session));
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream str = res.GetResponseStream();
            StreamReader strr = new StreamReader(str, UTF8);
            string size_tmp = strr.ReadToEnd();
            System.Diagnostics.Debug.WriteLine(size_tmp);
            ret = int.Parse(size_tmp);
            return ret;
        }

        /// <summary>
        /// ID を指定してユーザ、画像または小説の総数を取得する
        /// </summary>
        /// <param name="type">取得する総数の種類</param>
        /// <param name="id">問合せに使用する ID</param>
        /// <returns>取得したユーザ、画像または小説の総数</returns>
        private int GetSizeById(Type type, int id)
        {
            return this.GetSize(type, String.Format(this.IDParamTemplate, id));
        }

        /// <summary>
        /// Internal function to retrieve an array of <see cref="Illustration"/> based on the passed <paramref name="param"/> 
        /// and the search <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The search type to look for</param>
        /// <param name="param">Specific Pixiv API parameters to include in the search</param>
        /// <param name="page">The requested page number</param>
        /// <returns>An array of <see cref="Illustration"/> that match this description</returns>
        private Illustration[] GetImages(Type type, string param, int page)
        {
            List<Illustration> ret = new List<Illustration>();
            Regex reg = new Regex("\" \"");
            string reqUrl = String.Format("{0}{1}.php?{2}&{3}={4}&p={5}", BaseURL, type.ToString(), param, SessionID, this.Session, page);
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(reqUrl);            
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream str = res.GetResponseStream();
            StreamReader strr = new StreamReader(str, UTF8);
            string allCsv = Regex.Replace(strr.ReadToEnd(), "\" \"", ",");
            //System.Diagnostics.Debug.WriteLine(allCsv);
            Stream csvStr = new MemoryStream(Encoding.UTF8.GetBytes(allCsv));
            try
            {
                TextFieldParser parser = new TextFieldParser(csvStr, UTF8);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    string[] row = parser.ReadFields();
                    Illustration image;
                    if (!row[19].Equals(""))
                    {
                        image = PixivObjectFactory.CreateManga(row);
                    }
                    else
                    {
                        image = PixivObjectFactory.CreateIllust(row);
                    }
                    ret.Add(image);
                }
            }
            catch (MalformedLineException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            catch (AccessAuthorizationException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            str.Close();
            res.Close();
            return ret.ToArray();
        }

        /// <summary>
        /// 小説に関する情報を取得する
        /// </summary>
        /// <param name="type">取得する小説の種類</param>
        /// <param name="param">問合せ用パラメータ</param>
        /// <param name="page">取得するページ</param>
        /// <returns>取得した小説のリスト</returns>
        private Novel[] GetNovels(Type type, string param, int page)
        {
            List<Novel> ret = new List<Novel>();
            Regex reg = new Regex("\" \"");
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(String.Format("{0}{1}.php?{2}&{3}={4}&p={5}", BaseURL, type.ToString(), param, SessionID, this.Session, page));
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream str = res.GetResponseStream();
            StreamReader strr = new StreamReader(str, UTF8);
            string allCsv = Regex.Replace(strr.ReadToEnd(), "\" \"", ",");
            //System.Diagnostics.Debug.WriteLine(allCsv);
            Stream csvStr = new MemoryStream(Encoding.UTF8.GetBytes(allCsv));
            try
            {
                TextFieldParser parser = new TextFieldParser(csvStr, UTF8);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    string[] row = parser.ReadFields();
                    Novel novel = PixivObjectFactory.CreateNovel(row);
                    ret.Add(novel);
                }
            }
            catch (MalformedLineException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            catch (AccessAuthorizationException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            str.Close();
            res.Close();
            return ret.ToArray();
        }

        private Illustration[] GetImagesByUserId(Type type, int id, int page)
        {
            return this.GetImages(type, String.Format(IDParamTemplate, id), page);
        }

        private Novel[] GetNovelsByUserId(Type type, int id, int page)
        {
            return this.GetNovels(type, String.Format(IDParamTemplate, id), page);
        }

        private Illustration[] FindImages(string param, int size)
        {
            List<Illustration> ret = new List<Illustration>();
            for (int i = 0; ret.Count < size; i++)
            {
                List<Illustration> sub = this.GetImages(Type.search, param, i).ToList<Illustration>();
                if (sub.Count == 0)
                    break;
                ret.AddRange(sub);
            }
            return ret.ToArray();
        }

        private User[] GetUsers(Type type, string param, int page)
        {
            List<User> ret = new List<User>();
            String url = String.Format("{0}{1}.php?{2}&{3}={4}&p={5}", BaseURL, type.ToString(), param, SessionID, this.Session, page);
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse res = (HttpWebResponse)req.GetResponse();
            Stream str = res.GetResponseStream();
            StreamReader strr = new StreamReader(str, UTF8);
            string allCsv = Regex.Replace(strr.ReadToEnd(), "\" \"", ",");
            Stream csvStr = new MemoryStream(Encoding.UTF8.GetBytes(allCsv));
            try
            {
                TextFieldParser parser = new TextFieldParser(csvStr, UTF8);
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    string[] row = parser.ReadFields();
                    User user = PixivObjectFactory.CreateUser(row);
                    ret.Add(user);
                }
            }
            catch (MalformedLineException e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
            }
            str.Close();
            res.Close();
            return ret.ToArray();
        }

        private User[] GetUsersById(Type type, int id, int page)
        {
            return this.GetUsers(type, String.Format(IDParamTemplate, id), page);
        }        
    }

}
