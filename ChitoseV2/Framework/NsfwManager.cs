using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace ChitoseV2.Framework
{
    public class NsfwEntry
    {
        public string Channel;
        public bool Enabled;
    }

    public class NsfwManager
    {
        private Dictionary<string, NsfwEntry> nsfwServerEntries;
        private XmlSerializer serializer;

        public NsfwManager()
        {
            serializer = new XmlSerializer(typeof(List<NsfwPair>));
            List<NsfwPair> entries = null;
            try
            {
                using (FileStream stream = File.Open(Chitose.ConfigDirectory + "NSFW.txt", FileMode.OpenOrCreate, FileAccess.Read))
                {
                    entries = (List<NsfwPair>)serializer.Deserialize(stream);
                }
            }
            catch { }
            nsfwServerEntries = entries?.ToDictionary(entry => entry.Server, entry => entry.Entry) ?? new Dictionary<string, NsfwEntry>();
        }

        public NsfwEntry GetNsfwInfo(string server)
        {
            NsfwEntry existingEntry = null;
            if (!nsfwServerEntries.TryGetValue(server, out existingEntry))
            {
                existingEntry = new NsfwEntry() { Channel = null, Enabled = false };
            }
            return existingEntry;
        }

        public void UpdateServer(string server, bool? enabled = null, string channel = "")
        {
            NsfwEntry existingEntry = null;
            if (!nsfwServerEntries.TryGetValue(server, out existingEntry))
            {
                existingEntry = new NsfwEntry() { Channel = null, Enabled = false };
            }
            nsfwServerEntries[server] = new NsfwEntry() { Channel = channel != "" ? channel : existingEntry.Channel, Enabled = enabled ?? existingEntry.Enabled };
            Update();
        }

        private void Update()
        {
            using (FileStream stream = File.Open(Chitose.ConfigDirectory + "NSFW.txt", FileMode.Truncate, FileAccess.Write))
            {
                serializer.Serialize(stream, nsfwServerEntries.Select(entry => new NsfwPair() { Server = entry.Key, Entry = entry.Value }).ToList());
            }
        }
    }

    public class NsfwPair
    {
        public NsfwEntry Entry;
        public string Server;
    }
}