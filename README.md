DiscordClient client;
        CommandService commands; 

        public Chitose()
        {
            string[] Happy = { };

            string[] Angry = {"http://i.imgur.com/b6uQ2h3.jpg" };

            string[] Annoyed = { };

            string[] Sad = { }; 

            Random random = new Random();
            int rand; 

            client = new DiscordClient(input =>
            {
                input.LogLevel = LogSeverity.Info;
                input.LogHandler = Log;
            });

            client.UsingCommands(input =>
            {
                input.PrefixChar = '>';
                input.AllowMentionPrefix = true; 
            });

            commands = client.GetService<CommandService>();

            commands.CreateCommand("angry").Do(async (e) =>
            {
                rand = random.Next(0, Angry.Length);
                await e.Channel.SendMessage(Angry[rand]);
            });

            commands.CreateCommand("happy").Do(async (e) =>
            {
                rand = random.Next(0, Happy.Length);
                await e.Channel.SendMessage(Happy[rand]); 
            });

            commands.CreateCommand("annoyed").Do(async (e) =>
            {
                rand = random.Next(0, Annoyed.Length);
                await e.Channel.SendMessage(Annoyed[rand]); 
            });

            commands.CreateCommand("sad").Do(async (e) =>
            {
                rand = random.Next(0, Sad.Length);
                await e.Channel.SendMessage(Sad[rand]);
            });

            client.ExecuteAndWait(async () =>
            {
                await client.Connect("MjY1MzU3OTQwNDU2Njg1NTc5.C0yg1w.f3gHb1eCeuMpAMqGGuc-16Cm1wQ", TokenType.Bot);
            });
        }

        private void Log(object sender, LogMessageEventArgs e)
        {
            Console.WriteLine(e.Message); 
        }
