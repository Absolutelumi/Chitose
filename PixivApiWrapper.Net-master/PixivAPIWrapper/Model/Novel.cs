using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixivAPIWrapper.Model
{
    public class Novel : Item
    {
        public string Ext { get; protected set; }
        public string Server { get; protected set; }
        /// <summary>
        /// A Uri to the thumbnail for this Novel. 
        /// </summary>
        /// <remarks>Note that the referer should be set to the pixiv 
        /// host for the image to be loaded correctly.</remarks>
        public Uri ThumbURL { get; protected set; }

        /// <summary>
        /// A Uri to the cover image for this Novel. 
        /// </summary>
        /// <remarks>Note that the referer should be set to the pixiv 
        /// host for the image to be loaded correctly.</remarks>
        public Uri CoverURL { get; protected set; }

        /// <summary>
        /// A novel has its default cover if it's what Pixiv selected. 
        /// If the author selected the cover, this value will be false
        /// </summary>
        public bool IsDefaultCover { get; protected set; }

        public Uri TextURL { get; protected set; }

        public Novel(int id, int authorId, string title, string authorName, string date, 
            Tag[] tags, int point, int feedback, int views, string caption, 
            string ext, string server, Uri thumbUrl, Uri coverUrl, bool isDefaultCover, Uri textUrl) 
            : base(id, authorId, title, authorName, date, tags, point, feedback, views, caption)
        {
            Ext = ext;
            Server = server;
            ThumbURL = thumbUrl;
            CoverURL = coverUrl;
            IsDefaultCover = isDefaultCover;
            TextURL = textUrl;
        }

        internal Novel(PixivObjectFactory.NovelTransferObject obj)
            : base(obj)
        {
            Ext = obj.Extension;
            Server = obj.Server;
            ThumbURL = obj.ThumbUrl;
            CoverURL = obj.CoverUrl;
            IsDefaultCover = obj.IsDefaultCover;
            TextURL = obj.TextUrl;
        }
    }
}
