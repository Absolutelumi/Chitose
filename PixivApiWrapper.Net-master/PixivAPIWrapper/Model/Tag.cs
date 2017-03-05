using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixivAPIWrapper
{
    /// <summary>
    /// タグを表すクラスです。
    /// </summary>
    public class Tag
    {
        private const string BaseUrl = "http://dic.pixiv.net/a/";        
        private readonly string TagName;

        public Tag(string tag)
        {
            this.TagName = tag;
        }

        public override string ToString()
        {
            return this.TagName;
        }

        //TODO: Probably dont need this fuction
        /// <summary>
        /// このタグと同名の百科事典記事が存在するかどうか確かめます
        /// </summary>
        /// <remarks>
        /// 未実装
        /// </remarks>
        /// <returns>記事が存在すればtrue</returns>
        public bool ExistDictionaryPage()
        {
            return true;
        }

        /// <summary>
        /// Returns an Uri to the Pixiv encyclopedia
        /// </summary>
        /// <returns>Pixiv encyclopedia Uri</returns>
        public Uri GetDictionaryURL()
        {
            return new Uri(String.Format("{0}{1}", BaseUrl, this.TagName));
        }
    }
}
