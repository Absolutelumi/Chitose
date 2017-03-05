using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PixivLib
{
    public class Session
    {
        public int UserID { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string DeviceToken { get; set; }
        public string Name { get; set; }
        public string Account { get; set; }
        public string ProfileImageURL { get; set; }
        public string PHPSessionID { get; set; }
    }

    public class SessionManager
    {
        Session session;
        
        static string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        static string confFolder = Path.Combine(appData, "PixivLib");
        static string sessionFile = Path.Combine(confFolder, "Session.json");

        private void LoadSession()
        {
            var serializer = new JsonSerializer();

            Directory.CreateDirectory(confFolder);

            try
            {
                using (var file = new StreamReader(sessionFile))
                using (var reader = new JsonTextReader(file))
                {
                    session = serializer.Deserialize<Session>(reader);
                }
            }
            catch(FileNotFoundException)
            {
                // No problem.
                session = null;
            }
        }

        private void SaveSession()
        {
            var serializer = new JsonSerializer();

            Directory.CreateDirectory(confFolder);

            using (var file = new StreamWriter(sessionFile))
            using (var writer = new JsonTextWriter(file))
            {
                serializer.Serialize(writer, session);
            }
        }

        public Session GetSession()
        {
            if (session == null)
            {
                LoadSession();
            }

            return session;
        }

        public void PutSession(Session s)
        {
            session = s;

            SaveSession();
        }
    }
}
