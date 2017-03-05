using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixivAPIWrapper
{
    /// <summary>
    /// ユーザーを表すクラスです。
    /// </summary>
    public class User
    {        
        public int Id { get; private set; }
        public string Name { get; private set; }
        public Uri MobileURL { get; private set; }
        public string Ename { get; private set; }
        public Uri Thumbnail { get; private set; }

        public User(int id, string name, Uri mobileUrl, string eName, Uri thumbnail)
        {
            Id = id;
            Name = name;
            mobileUrl = MobileURL;
            Ename = eName;
            Thumbnail = thumbnail;
        }
    }
}
