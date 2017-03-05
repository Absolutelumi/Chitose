using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixivAPIWrapper;
using PixivAPIWrapper.Model;

namespace PixivAPITest
{
    [TestClass]
    public class IllustrationTest
    {
        PixivAPI api;

        /// <summary>
        /// Construct a single API instance to log in to.
        /// </summary>
        /// <remarks>Besides the session, no other state is stored there,
        /// so no additional testing is required for now.</remarks>
        public IllustrationTest()
        {
            api = new PixivAPI();
            api.Login("wrappertest", "APITest");
        }

        public void ImageFactoryTest()
        {
            string[] data = new string[]
            {

            };

            Illustration illust = PixivObjectFactory.CreateIllust(data);


        }
    }
}
