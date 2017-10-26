using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace TheBotDiscord
{
    public class AdminRoleAttribute : Attribute {}

    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("rockpaperscissor"), Summary("Challenges another user for a rock paper scissor match.")]
        public async Task RockPaperScissor(SocketGuildUser user)
        {
            RockPaperScissorMatch match = new RockPaperScissorMatch(Context.Channel);
            match.AddPlayerToMatch(Context.User);

            Program.matches.Add(match);

            Challenge challenge = new Challenge(Context.User, user, Context.Channel);
            Program.activeChallenges.Add(challenge);

            await Context.Channel.SendMessageAsync(Context.User.Mention + " has challenged " + user.Mention + " to a Rock-Paper-Scissor match! Do \\accept to accept the challenge!");
        }

        [Command("accept"), Summary("Accepts a challenge, if there is any.")]
        public async Task Accept()
        {
            Challenge challenge = Program.GetChallengeForChallengedUser(Context.User);
            if(challenge == null)
            {
                await Context.Channel.SendMessageAsync("You do not have any pending challenges!");
                return;
            }

            RockPaperScissorMatch match = Program.GetMatchFromUser(challenge.UserChallengedBy);
            match.AddPlayerToMatch(Context.User);

            await match.StartMatch();

            challenge.Accept();
        }

        [Command("requirements"), Summary("Lists the requirements.")]
        public async Task Requirements()
        {
            await Context.Channel.SendMessageAsync("The requirements for this mod are as follows: " + Environment.NewLine + 
                "ScriptHookV: http://www.dev-c.com/gtav/scripthookv/" + Environment.NewLine +
                "ScriptHookVDotNet: https://github.com/crosire/scripthookvdotnet/releases" + Environment.NewLine +
                ".NET Framework 4.5.2: https://www.microsoft.com/en-us/download/details.aspx?id=42642" + Environment.NewLine +
                "C++ Redist 2017: https://support.microsoft.com/en-us/help/2977003/the-latest-supported-visual-c-downloads" + Environment.NewLine +
                "NativeUI: https://github.com/Guad/NativeUI/releases");
        }

        [Command("clear"), Summary("Clear command. That's it."), AdminRole]
        public async Task Clear(int messages)
        {
            if (!DoesHaveRole((SocketGuildUser)Context.User, "admin"))
            {
                await Context.Channel.SendMessageAsync("You need to have the admin rank to use this command!");
                return;
            }

            IEnumerable<IMessage> messageList = await Context.Channel.GetMessagesAsync(messages + 1).Flatten();
            await Context.Channel.DeleteMessagesAsync(messageList);

            await Context.Channel.SendMessageAsync("Successfully deleted " + messages.ToString() + " messages!");
        }

        [Command("help"), Summary("Help command. Gives you info about all the commands in the server")]
        public async Task Help()
        {
            var builder = new EmbedBuilder()
                            .WithTitle("Commands")
                            .WithDescription("All of the commands and their usages!");

            foreach(CommandInfo info in Program.commands.Commands)
            {       
                if(info.Attributes.FirstOrDefault(x => x is AdminRoleAttribute) != null)
                {
                    if(DoesHaveRole((SocketGuildUser)Context.User, "admin"))
                    {
                        builder.AddField(info.Name, "`" + info.Summary + "`");
                        continue;
                    }
                }
                else
                {
                    builder.AddField(info.Name, "`" + info.Summary + "`");
                }  
            }
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("userinfo"), Summary("Gives you info about the specified user")]
        public async Task UserInfo(IGuildUser user = null)
        {
            if (user == null) user = (IGuildUser)Context.User;

            var fieldBuilder = new EmbedFieldBuilder()
                    .WithName("Name")
                    .WithIsInline(true)
                    .WithValue(user.Username);

            var fieldBuilder2 = new EmbedFieldBuilder()
                    .WithName("Playing")
                    .WithIsInline(true)
                    .WithValue((user.Game.HasValue) ? user.Game.Value.Name : "Nothing");

            var embedBuilder = new EmbedBuilder()
                    .WithTitle("User Info")
                    .WithThumbnailUrl(user.GetAvatarUrl(ImageFormat.Png))
                    .AddField(fieldBuilder)
                    .AddField(fieldBuilder2);

            var embed = embedBuilder.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("serverinfo"), Summary("Gives you information about the server.")]
        public async Task ServerInfo()
        {
            var users = Context.Guild.Users;
        var numberOfUsers = 0;
        var numberOfBots = 0;
        foreach(IGuildUser user in users)
        {
            if (user.IsBot)
                numberOfBots++;
        }

        numberOfUsers = Context.Guild.MemberCount - numberOfBots;

            var fieldBuilder = new EmbedFieldBuilder()
                    .WithName("What is this server about?")
                    .WithIsInline(true)
                    .WithValue("This server is about the mod Grand Theft Space, or GTS for short. It is a space mod for GTA 5 developed by " + GetMentionForUser("sollaholla", Context.Guild));

            var fieldBuilder2 = new EmbedFieldBuilder()
                    .WithName("Amount of users")
                    .WithIsInline(true)
                    .WithValue(numberOfUsers.ToString());

            var fieldBuilder3 = new EmbedFieldBuilder()
                    .WithName("Amount of bots")
                    .WithIsInline(true)
                    .WithValue(numberOfBots.ToString());

            var fieldBuilder4 = new EmbedFieldBuilder()
                    .WithName("Server Ping")
                    .WithIsInline(true)
                    .WithValue(Program.Latency);

            var fieldBuilder5 = new EmbedFieldBuilder()
                    .WithName("Text Channels")
                    .WithIsInline(true)
                    .WithValue(Context.Guild.TextChannels.Count.ToString());

            var fieldBuilder6 = new EmbedFieldBuilder()
                    .WithName("Voice Channels")
                    .WithIsInline(true)
                    .WithValue(Context.Guild.VoiceChannels.Count.ToString());

            var builder = new EmbedBuilder()
                    .WithTitle("Server Info")
                    .WithDescription("Information about the server")
                    .WithThumbnailUrl(Context.Guild.IconUrl)
                    .AddField(fieldBuilder)
                    .AddField(fieldBuilder2)
                    .AddField(fieldBuilder3)
                    .AddField(fieldBuilder4)
                    .AddField(fieldBuilder5)
                    .AddField(fieldBuilder6);


            string emoteString = "";
            foreach(Emote e in Context.Guild.Emotes)
            {
                emoteString += e;
            }

            builder.AddField("Emotes", emoteString);

            var embed = builder.Build();

            await Context.Channel.SendMessageAsync("", false, embed);
        }

        [Command("say"), Summary("Says the message you have provided")]
        public async Task Say([Remainder, Summary("Text to say")] string text)
        {
            await Context.Channel.SendMessageAsync(text);
        }

        [Command("getId"), Summary("Gets the id of the user you have specified. If you have not specified a user, it gives your id instead")]
        public async Task GetId(IUser user = null)
        {
            ulong id = 0;
            if (user == null)
                id = Context.Message.Author.Id;
            else
                id = user.Id;
            await Context.Channel.SendMessageAsync(id.ToString());
        }

        [Command("installation"), Summary("Gives you a detailed video explaining how to install the mod")]
        public async Task Installation()
        {
            await Context.Channel.SendMessageAsync("If you want to know how to install the mod, please follow this tutorial! " + Environment.NewLine + "https://www.youtube.com/watch?v=SgxVtHjcehI");
        }

        [Command("ban"), Summary("Bans the user specifed. Usage: '\\ban @AHK1221 reason 3' will ban the user AHK1221 and clear the messages they have sent in the past three days"), AdminRole]
        public async Task Ban(IUser user = null, string reason = null, int pruneMessages = 7)
        {
            if (!DoesHaveRole((SocketGuildUser)Context.User, "admin"))
            {
                await Context.Channel.SendMessageAsync("You need to have the admin rank to use this command!");
                return;
            }

            IGuildUser guildUser = user as IGuildUser;
            if (guildUser == null)
            {
                await Context.Channel.SendMessageAsync(new Emoji("❎") + " User specified does not exist!");
                return;
            }

            await Context.Guild.AddBanAsync(guildUser, pruneMessages, reason);
            await Context.Channel.SendMessageAsync(new Emoji("☑") + " Banned " + guildUser.Mention);
        }

        [Command("unban"), Summary("Unbans someone."), AdminRole]
        public async Task Unban(string userName)
        {
            if (!DoesHaveRole((SocketGuildUser)Context.User, "admin"))
            {
                await Context.Channel.SendMessageAsync("You need to have the admin rank to use this command!");
                return;
            }

            IReadOnlyCollection<IBan> bans = await Context.Guild.GetBansAsync();
            foreach (IBan ban in bans.ToList())
            {
                if (ban.User.Username.ToLower() == userName.ToLower())
                {            
                    await Context.Guild.RemoveBanAsync(ban.User);
                    await Context.Channel.SendMessageAsync(new Emoji("☑") + " Unbanned " + ban.User.Mention);
                    return;
                }
            }

            await Context.Channel.SendMessageAsync(new Emoji("❎") + " User " + userName + " is not banned!");
        }

        [Command("grammarcheck"), Summary("Toggles grammar checking feature of the bot."), AdminRole]
        public async Task GrammarCheck()
        {
            if (!DoesHaveRole((SocketGuildUser)Context.User, "admin"))
            {
                await Context.Channel.SendMessageAsync("You need to have the admin rank to use this command!");
                return;
            }

            Program.IsGrammarCheckingOn = !Program.IsGrammarCheckingOn;
            await Context.Channel.SendMessageAsync(new Emoji("☑") + " Grammar checking is now " + (Program.IsGrammarCheckingOn ? "on" : "off") + ".");
        }

        private bool DoesHaveRole(SocketGuildUser user, string roleName)
        {
            foreach(SocketRole role in user.Roles)
            {
                if(role.Name.ToLower() == roleName.ToLower())
                {
                    return true;
                }
            }

            return false;
        }

        private string GetMentionForUser(string userName, SocketGuild guild)
        {
            foreach(SocketGuildUser user in guild.Users)
            {
                if(user.Username.ToLower() == userName.ToLower())
                {
                    return user.Mention;
                }
            }

            return string.Empty;
        }
    }
}
