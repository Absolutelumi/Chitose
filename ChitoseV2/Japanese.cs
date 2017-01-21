using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ChitoseV2
{
    internal class Japanese : CommandSet
    {
        private static readonly JavaScriptSerializer json = new JavaScriptSerializer();

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateCommand("jisho").Parameter("search", ParameterType.Multiple).Do(async (e) =>
            {
                HttpWebRequest getJapanese = WebRequest.CreateHttp(@"http://jisho.org/api/v1/search/words?keyword=" + string.Join("+", e.Args));
                using (Stream response = (await getJapanese.GetResponseAsync()).GetResponseStream())
                {
                    string jsonResult = new StreamReader(response).ReadToEnd();
                    JishoResponse result = json.Deserialize<JishoResponse>(jsonResult);
                    StringBuilder message = new StringBuilder();
                    for (int i = 0; i < Math.Min(5, result.data.Length); i++)
                    {
                        JishoResponse.Result word = result.data[i];
                        message.AppendLine("```");
                        message.AppendLine(word.is_common ? "Common" : "Uncommon");
                        message.AppendLine("Japanese Translations:");
                        message.AppendLine("\t" + string.Join(", ", word.japanese.Select(o => o.word == null ? o.reading : o.word + " (" + o.reading + ")")));
                        message.AppendLine("English Translations:");
                        message.AppendLine("\t" + string.Join("\n\t", word.senses.Select(o => string.Join(", ", o.english_definitions) + " (" + string.Join(", ", o.parts_of_speech) + ")")));
                        message.AppendLine("```");
                    }
                    await e.Channel.SendMessage(message.ToString());
                }
            });
        }

#pragma warning disable 0649

        public class JishoResponse
        {
            public Result[] data;

            public class Result
            {
                public bool is_common;

                public Japanese[] japanese;

                public Details[] senses;

                public string[] tags;

                public class Details
                {
                    public string[] english_definitions;
                    public string[] parts_of_speech;
                }

                public class Japanese
                {
                    public string reading;
                    public string word;
                }
            }
        }

#pragma warning restore 0649
    }
}
