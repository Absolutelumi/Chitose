using Discord;
using Discord.Audio;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace ChitoseV2
{
    internal class Games : CommandSet
    {
        private bool gameStart { get; set; }
        private char[] blanks { get; set; }
        private char[] letters { get; set; }

        public void AddCommands(DiscordClient client, CommandService commands)
        {
            commands.CreateGroup("Hangman", cgb =>
            {
                cgb.CreateCommand("Start").Description("Play a game of Hangman!").Do(async (e) =>
                {
                    if (gameStart == true)
                    {
                        await e.Channel.SendMessage("Game is already in place! Please use '!hangman end' if you wish to end current game.");
                    }
                    else
                    {
                        gameStart = true;
                    }
                });
            });

            commands.CreateCommand("end").Do(async (e) => 
            {
                if(gameStart == true)
                {
                    gameStart = false;
                    await e.Channel.SendMessage("Game has been ended!"); 
                }
                else
                {
                    await e.Channel.SendMessage("Game has not been started!");
                }
            });
        }
    }
}