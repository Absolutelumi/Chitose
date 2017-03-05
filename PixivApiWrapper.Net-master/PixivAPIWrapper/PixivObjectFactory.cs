using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PixivAPIWrapper.Model;

namespace PixivAPIWrapper
{
    /// <summary>
    /// A helper class for the construction of Pixiv objects since everything gets passed around as string arrays
    /// </summary>
    public class PixivObjectFactory
    {
        internal class ItemTransferObject
        {
            public int Id { get; set; }
            public int AuthorId { get; set; }
            public string Title { get; set; }
            public string AuthorName { get; set; }
            public string Date { get; set; }
            public Tag[] Tags { get; set; }
            public int Feedback { get; set; }
            public int Point { get; set; }
            public int Views { get; set; }
            public string Caption { get; set; }
            public int Bookmarks { get; set; }
        }

        internal class IllustrationTransferObject : ItemTransferObject
        {
            public string Extension { get; set; }
            public string Server { get; set; }
            public Uri ThumbUrl { get; set; }
            public Uri MobileUrl { get; set; }
            public string Tool { get; set; }
            public Uri PageUrl { get; set; }
            public Uri ImageUrl { get; set; }
        }

        internal class MangaTransferObject : IllustrationTransferObject
        {
            public Uri[] ImageUrls { get; set; }
            public int PageCount { get; set; }
        }

        internal class NovelTransferObject : ItemTransferObject
        {
            public bool IsDefaultCover { get; set; }
            public string Extension { get; set; }
            public string Server { get; set; }
            public Uri ThumbUrl { get; set; }
            public Uri CoverUrl { get; set; }
            public Uri TextUrl { get; set; }
        }

        public static User CreateUser(string[] userData)
        {
            int id = int.Parse(userData[1]);
            string name = userData[5];
            Uri thumbnail = new Uri(userData[6]);
            string ename = userData[24];

            return new User(id, name, null, ename, thumbnail);
        }

        public static Novel CreateNovel(string[] novelData)
        {
            NovelTransferObject obj = new NovelTransferObject();
            SetNovelValues(novelData, obj);
            
            return new Novel(obj);
        }

        public static Illustration CreateIllust(string[] illustData)
        {
            IllustrationTransferObject obj = new IllustrationTransferObject();
            SetIllustrationValues(illustData, obj);

            //TODO: fix ImageURL
            return new Illustration(obj);
        }

        public static Manga CreateManga(string[] mangaData)
        {
            MangaTransferObject obj = new MangaTransferObject();
            SetMangaValues(mangaData, obj);

            return new Manga(obj);
        }

        #region DTO value setting
        private static void SetNovelValues(string[] data, NovelTransferObject obj)
        {            
            if (!data[2].Equals("0"))
            {
                obj.Extension = data[2];
                obj.Server = data[4];
                obj.IsDefaultCover = false;
            }
            else
                obj.IsDefaultCover = true;
            obj.ThumbUrl = new Uri(data[6]);
            obj.CoverUrl = new Uri(data[9]);
            obj.TextUrl = new Uri(string.Format("http://spapi.pixiv.net/iphone/novel_text.php?id={0}", obj.Id));
        }

        private static void SetMangaValues(string[] data, MangaTransferObject obj)
        {
            SetIllustrationValues(data, obj);

            int pageCount = int.Parse(data[19]);

            string ext = data[2];
            string server = data[4];
            Uri thumbURL = new Uri(data[6]);
            Uri mobileURL = new Uri(data[9]);
            string tool = data[14];

            string rawMobileURL = obj.MobileUrl.ToString();
            List<Uri> urls = new List<Uri>();
            string big = (obj.Id > 10000000) ? "_big" : "";
            for (int i = 0; i < pageCount; i++)
            {  
                Uri aPage = new Uri(String.Format("{0}{1}{2}_p{3}.{4}", rawMobileURL.Substring(0, rawMobileURL.LastIndexOf("mobile")), obj.Id, big, i, obj.Extension));
                urls.Add(aPage);
            }
            
            obj.ImageUrls = urls.ToArray();
            obj.ImageUrl = obj.ImageUrls[0];
            obj.PageCount = pageCount;
        }

        private static void SetIllustrationValues(string[] data, IllustrationTransferObject obj)
        {
            SetItemValues(data, obj);
            obj.Extension = data[2];
            obj.Server = data[4];
            obj.ThumbUrl = new Uri(data[6]);
            obj.MobileUrl = new Uri(data[9]);
            obj.Tool = data[14];
            obj.PageUrl = new Uri(String.Format("http://www.pixiv.net/member_illust.php?mode=medium&illust_id={0}", obj.Id));
        }

        private static void SetItemValues(string[] data, ItemTransferObject obj)
        {
            obj.Id = int.Parse(data[0]);
            obj.AuthorId = int.Parse(data[1]);
            obj.Title = data[3];
            obj.AuthorName = data[5];
            obj.Date = data[12];

            string tags_org = data[13];
            List<Tag> tags = new List<Tag>();
            string[] tags_tmp = tags_org.Split(' ');
            foreach (string tag_s in tags_tmp)
            {
                Tag tag = new Tag(tag_s);
                tags.Add(tag);
            }
            obj.Tags = tags.ToArray();
            obj.Feedback = (!data[15].Equals("")) ? int.Parse(data[15]) : 0;
            obj.Point = (!data[16].Equals("")) ? int.Parse(data[16]) : 0;
            obj.Views = (!data[17].Equals("")) ? int.Parse(data[17]) : 0;
            obj.Caption = data[18];
            obj.Bookmarks = (!data[22].Equals("")) ? int.Parse(data[22]) : 0;
        }
        #endregion
    }
}
