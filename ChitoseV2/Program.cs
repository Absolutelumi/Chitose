using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChitoseV2
{
    class Program
    {
        static void Main(string[] args)
        {
            client.MessageCreated += async (_, messageInfo) => {
                if (messageInfo.Message.Content == "angry")
                {
                    await client.SendMessage(messageInfo.Message.ChannelID, "online url of image");
                }
            };
        }
    }
}
