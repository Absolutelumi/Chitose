using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PixivAPIWrapper
{
    /// <summary>
    /// アクセス権限が無い作品にアクセスしようとしたときに発生する例外です。
    /// </summary>
    class AccessAuthorizationException : Exception
    {
        public AccessAuthorizationException() { }
        public AccessAuthorizationException(string message) { }
        public AccessAuthorizationException(string message, System.Exception inner) { }
    }
}
