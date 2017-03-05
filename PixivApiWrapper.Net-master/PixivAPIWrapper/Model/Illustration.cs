using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixivAPIWrapper.Model
{
    /// <summary>
    /// Abstract parent for the various Image types on Pixiv
    /// </summary>
    public class Illustration : Item
    {
        public Illustration(int id, int authorId, string title, string authorName, string date, 
            Tag[] tags, int point, int feedback, int views, string caption,
            string ext, string server, Uri thumbUrl, Uri mobileUrl, string tool, Uri imageUrl, Uri pageUrl) 
            : base(id, authorId, title, authorName, date, tags, point, feedback, views, caption)
        {
            Extension = ext;
            Server = server;
            ThumbURL = thumbUrl;
            MobileURL = mobileUrl;
            Tool = tool;
            ImageURL = imageUrl;
            PageURL = pageUrl;
        }

        internal Illustration(PixivObjectFactory.IllustrationTransferObject obj)
            :base(obj)
        {
            Extension = obj.Extension;
            Server = obj.Server;
            ThumbURL = obj.ThumbUrl;
            MobileURL = obj.MobileUrl;
            Tool = obj.Tool;
            ImageURL = obj.ImageUrl;
            PageURL = obj.PageUrl;
        }

        public string Extension { get; protected set; }
        public string Server { get; protected set; }
        public Uri ThumbURL { get; protected set; }
        public Uri MobileURL { get; protected set; }
        public string Tool { get; protected set; }
        public Uri ImageURL { get; protected set; }
        public Uri PageURL { get; protected set; }
    }
}
