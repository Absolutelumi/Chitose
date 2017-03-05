using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixivAPIWrapper.Model
{
    /// <summary>
    /// 「マンガ」を表すクラスです。
    /// </summary>
    /// <remarks>
    /// Illustとは違い、imageURLsにフルサイズの画像のURLが格納されています。
    /// </remarks>
    public class Manga : Illustration
    {
        public Manga(int id, int authorId, string title, string authorName, string date, 
            Tag[] tags, int point, int feedback, int views, string caption, 
            string ext, string server, Uri thumbUrl, Uri mobileUrl, string tool, 
            Uri imageUrl, Uri pageUrl , Uri[] imageUrLs, int pageCount) 
            : base(id, authorId, title, authorName, date, tags, point, 
            feedback, views, caption, ext, server, thumbUrl, mobileUrl, tool, imageUrl, pageUrl)
        {
            ImageURLs = imageUrLs;
            PageCount = pageCount;
        }

        internal Manga(PixivObjectFactory.MangaTransferObject obj)
            :base(obj)
        {
            ImageURLs = obj.ImageUrls;
            PageCount = obj.PageCount;
        }

        public Uri[] ImageURLs { get; protected set; }
        public int PageCount { get; protected set; }
    }
}
