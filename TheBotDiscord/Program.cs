using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using System.Text.RegularExpressions;

namespace TheBotDiscord
{
    public class Program
    {
        private DiscordSocketClient _client;
        public static CommandService commands;
        private IServiceProvider services;

        public static int Latency;

        public static bool IsGrammarCheckingOn = true;

        public static List<RockPaperScissorMatch> matches = new List<RockPaperScissorMatch>();
        public static List<Challenge> activeChallenges = new List<Challenge>();

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private bool Contains(string msgToCheckFrom, string msgToCheck)
        {
            return msgToCheckFrom.ToLower().Contains(msgToCheck);
        }

        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            commands = new CommandService();

            _client.Log += Log;
            _client.LatencyUpdated += _client_LatencyUpdated;

            services = new ServiceCollection().BuildServiceProvider();

            await InstallCommands();

            _client.MessageReceived += _client_MessageReceived;
            await _client.SetGameAsync("Grand Theft Space v1.3.3.7");

            string token = "nosorry";

            await _client.LoginAsync(TokenType.Bot, token);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        public static Challenge GetChallengeForChallengedUser(SocketUser challengedUser)
        {
            foreach(Challenge challenge in activeChallenges)
            {
                if (challenge.UserChallenged.Id == challengedUser.Id)
                    return challenge;
            }

            return null;
        }

        public static RockPaperScissorMatch GetMatchFromUser(SocketUser user)
        {
            foreach(RockPaperScissorMatch match in matches)
            {
                foreach(RPSUser rpsUser in match.UsersInTheMatch)
                {
                    if(rpsUser.User.Id == user.Id)
                    {
                        return match;
                    }
                }
            }

            return null;
        }

        private Task _client_LatencyUpdated(int arg1, int arg2)
        {
            Latency = arg2;
            return Task.CompletedTask;
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            var message = arg as SocketUserMessage;
            if (message == null) return;

            if (message.Author.IsBot) return;

            if (message.Channel is SocketDMChannel)
            {
                foreach(RockPaperScissorMatch match in matches)
                {
                    try
                    {
                        await match.OnMessageRecieved(message);
                    }
                    catch (Exception e)
                    {
                        await Log(new LogMessage(LogSeverity.Error, "RPS", e.Message, e));
                    }
                }
            }

            if(IsGrammarCheckingOn)
            {
                string misspelledWords = GetMisspledWords(message);
                if (!String.IsNullOrEmpty(misspelledWords)) await message.Channel.SendMessageAsync(misspelledWords);
            }

            if(DoesMessageContainWord(message, "noice") || DoesMessageContainWord(message, "nice") || DoesMessageContainWord(message, "great"))
            {
                await message.AddReactionAsync(new Emoji("👌"));
            }

            //if (DoesMessageContainWord(message, "lol") || DoesMessageContainWord(message, "lmao") || DoesMessageContainWord(message, "lmfao") || message.Content.ToLower().Contains("haha") || DoesMessageContainWord(message, "lul") || DoesMessageContainWord(message, "lel"))
            //{
            //    await message.AddReactionAsync(new Emoji("😂"));
            //}

            //if(DoesMessageContainWord(message, "ty") || DoesMessageContainWord(message, "thanks") || (DoesMessageContainWord(message, "thanks") && (DoesMessageContainWord(message, "you") || DoesMessageContainWord(message, "u")))) {
            //    await message.Channel.SendMessageAsync("You're welcome! " + new Emoji("😃"));
            //}

            //if(DoesMessageContainWord(message, "hi") || DoesMessageContainWord(message, "hello"))
            //{
            //    await message.Channel.SendMessageAsync("Hello!");
            //}

            if(DoesMessageContainWord(message, "kys"))
            {
                await message.Channel.SendMessageAsync("Please do not encourage suicide!");
                await message.DeleteAsync();
            }

            if(DoesMessageContainWord(message, "install") && DoesMessageContainWord(message, "how") && (DoesMessageContainWord(message, "gts") || (DoesMessageContainWord(message, "space") && DoesMessageContainWord(message, "mod"))))
            {
                await message.Channel.SendMessageAsync("If you want to know how to install the mod, please follow this tutorial! " + Environment.NewLine + "https://www.youtube.com/watch?v=SgxVtHjcehI");
            }
        }

        public async Task InstallCommands()
        {
            _client.MessageReceived += HandleCommand;
            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        public async Task HandleCommand(SocketMessage msg)
        {
            var message = msg as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;
            if (!(message.HasCharPrefix('\\', ref argPos))) return;

            var context = new SocketCommandContext(_client, message);

            var result = commands.ExecuteAsync(context, argPos, services);
            if (!result.Result.IsSuccess)
                await message.Channel.SendMessageAsync(new Emoji("❎") + " " + result.Result.ErrorReason);
        }

        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private string GetMisspledWords(SocketUserMessage message)
        {
            string misspelledWords = "";
            string[] words = GetWords(message.Content);

            if (DoesUserHaveRole((SocketGuildUser)message.Author, "admin") || DoesUserHaveRole((SocketGuildUser)message.Author, "dev"))
                return null;

            foreach (string word in words)
            {
                string lowerWord = word.ToLower();

                if (lowerWord == "doesnt") misspelledWords += " Doesn't*";
                if (lowerWord == "cant") misspelledWords += " Can't*";
                if (lowerWord == "dont") misspelledWords += " Don't*";
                if (lowerWord == "im") misspelledWords += " I'm*";
                if (lowerWord == "thats") misspelledWords += " That's*";
                if (lowerWord == "pls" || lowerWord == "plese") misspelledWords += " Please*";
                if (lowerWord == "tnx" || lowerWord == "thnx" || lowerWord == "tanks") misspelledWords += " Thanks*";
                if (lowerWord == "lets") misspelledWords += " Let's*";
                if (lowerWord == "u") misspelledWords += " You*";
                if (lowerWord == "grammer" || lowerWord == "gremmer" || lowerWord == "gremmar" || lowerWord == "gremer") misspelledWords += " Grammar*";
                if (lowerWord == "srsly") misspelledWords += " Seriously*";
                if (lowerWord == "srs") misspelledWords += " Serious*";
                if (lowerWord == "wat" || lowerWord == "wut" || lowerWord == "whut" || lowerWord == "w0t" || lowerWord == "wot") misspelledWords += " What*";
                if (lowerWord == "m8") misspelledWords += " Mate*";
                if (lowerWord == "wassup" || lowerWord == "wussah" || lowerWord == "sup" || lowerWord == "sah") misspelledWords += " What's up*";
                if (lowerWord == "wont") misspelledWords += " Won't*";
            }

            return misspelledWords;
        }

        private bool DoesMessageContainWord(SocketUserMessage message, string wordToCheck)
        {
            string[] words = GetWords(message.Content);
            foreach(string word in words)
            {
                if(word.ToLower() == wordToCheck.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        private bool DoesUserHaveRole(SocketGuildUser user, string role)
        {
            foreach(IRole roleInfo in user.Guild.Roles)
            {
                if(roleInfo.Name.ToLower() == role.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        static string[] GetWords(string input)
        {
            MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

            var words = from m in matches.Cast<Match>()
                        where !string.IsNullOrEmpty(m.Value)
                        select TrimSuffix(m.Value);

            return words.ToArray();
        }

        static string TrimSuffix(string word)
        {
            int apostropheLocation = word.IndexOf('\'');
            if (apostropheLocation != -1)
            {
                word = word.Substring(0, apostropheLocation);
            }

            return word;
        }
    }
}
