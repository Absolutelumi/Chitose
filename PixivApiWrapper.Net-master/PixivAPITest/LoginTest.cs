using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PixivAPIWrapper;

namespace PixivAPITest
{
    /// <summary>
    /// Tests the log in of the Pixiv API
    /// </summary>
    /// <remarks>Since Pixiv restricts the amount of times you can login before your account is temporarily locked, please avoid running these tests too much</remarks>
    [TestClass]
    public class LoginTest
    {
        [TestInitialize]
        public void Setup()
        {
            api = new PixivAPI();
        }

        PixivAPI api;

        [TestMethod]
        public void LoginNickSucces()
        {
            bool loggedIn = api.Login("wrappertest", "APITest");

            Assert.IsTrue(loggedIn, "Logging in using nick and password failed!");
        }

        [TestMethod]
        public void LoginEmailSucces()
        {
            bool loggedIn = api.Login("pixiv.wrapper@gmail.com", "APITest");

            Assert.IsTrue(loggedIn, "Logging in using nick and password failed!");
        }

        [TestMethod]
        public void LoginFailed()
        {
            bool loggedIn = api.Login("wrappertest", "nonsensepass");

            Assert.IsFalse(loggedIn, "How did we manage to log in with a nonsense password?");
        }

        [TestMethod]
        public void SessionSucces()
        {
            bool loggedIn = api.Login("wrappertest", "APITest");

            Assert.AreEqual(loggedIn, api.LoggedIn, "the result of logging in and the API Session property are not the same!");
        }

        [TestMethod]
        public void SessionFailure()
        {
            bool loggedIn = api.Login("wrappertest", "nonsensepass");

            Assert.AreEqual(loggedIn, api.LoggedIn, "the result of logging in and the API Session property are not the same!");
        }
    }
}
