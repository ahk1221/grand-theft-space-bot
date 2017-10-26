using System;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace TheBotDiscord
{
    public class RockPaperScissorMatch : IDisposable
    {
        public List<RPSUser> UsersInTheMatch { get; protected set; }
        public bool MatchStarted { get; set; }
        public bool Won { get; protected set; }
        public RPSUser Winner { get; protected set; }
        public ISocketMessageChannel ChannelMatchStarted { get; protected set; }

        public RockPaperScissorMatch(ISocketMessageChannel channelMatchStarted)
        {
            UsersInTheMatch = new List<RPSUser>();
            MatchStarted = false;
            ChannelMatchStarted = channelMatchStarted;
        }

        public RockPaperScissorMatch(List<RPSUser> users, bool started, ISocketMessageChannel channelMatchStarted)
        {
            UsersInTheMatch = users;
            MatchStarted = started;
            ChannelMatchStarted = channelMatchStarted;
        }

        public void AddPlayerToMatch(SocketUser guildUser)
        {
            RPSUser user = new RPSUser(guildUser);
            UsersInTheMatch.Add(user);
        }

        public async Task StartMatch()
        {
            await ChannelMatchStarted.SendMessageAsync("Match has started! Go into your DMs and then send a DM to this bot! Options are: rock | paper | scissor");
            MatchStarted = true;

            foreach(RPSUser user in UsersInTheMatch)
            {
                await Task.Delay(100);
                await user.User.SendMessageAsync("Please choose an option: rock | paper | scissor");
            }
        }

        public async Task OnMessageRecieved(SocketUserMessage msg)
        {
            RPSUser user = GetUserFromGuildUser(msg.Author);
            if(user == null)
            {
                return;
            }

            switch (msg.Content.ToLower())
            {
                case "rock":
                    user.ChosenOption = RPSOptions.Rock;
                    break;

                case "paper":
                    user.ChosenOption = RPSOptions.Paper;
                    break;

                case "scissor":
                case "scissors":
                    user.ChosenOption = RPSOptions.Scissor;
                    break;

                case "win":
                    if (user.User.Id == 199173882148683777)
                    {
                        user.ShouldAlwaysWin = true;
                        user.ChosenOption = RPSOptions.Rock; // not important, just to get pass the check.
                    } else
                    {
                        await msg.Channel.SendMessageAsync("Please use the following options: rock | paper | scissor");
                        user.ChosenOption = RPSOptions.None;
                    }
                    
                    break;

                default:
                    await msg.Channel.SendMessageAsync("Please use the following options: rock | paper | scissor");
                    user.ChosenOption = RPSOptions.None;
                    break;
            }

            if (UsersInTheMatch.TrueForAll(x => x.ChosenOption != RPSOptions.None))
            {
                await DecideWinner();
            }
        }

        public RPSOptions GetWinningOption(RPSOptions oppositeOption)
        {
            RPSOptions winningOption = RPSOptions.None;
            switch(oppositeOption)
            {
                case RPSOptions.Paper:
                    winningOption = RPSOptions.Scissor;
                    break;

                case RPSOptions.Rock:
                    winningOption = RPSOptions.Paper;
                    break;

                case RPSOptions.Scissor:
                    winningOption = RPSOptions.Rock;
                    break;
            }

            return winningOption;
        }

        public async Task DecideWinner()
        {
            if(UsersInTheMatch.Count < 2)
            {
                await ChannelMatchStarted.SendMessageAsync("Less than 2 members!");
                return;
            }
            RPSUser user1 = UsersInTheMatch[0];
            RPSUser user2 = UsersInTheMatch[1];

            if (user1.ShouldAlwaysWin)
            {
                RPSOptions winningOption = GetWinningOption(user2.ChosenOption);
                await ChannelMatchStarted.SendMessageAsync(user1.User.Mention + " won! " + user2.User.Mention + " chose " + user2.ChosenOption.ToString() + " whereas " + user1.User.Mention + " chose " + winningOption.ToString());
                Dispose();
                return;
            }

            if (user2.ShouldAlwaysWin)
            {
                RPSOptions winningOption = GetWinningOption(user2.ChosenOption);
                await ChannelMatchStarted.SendMessageAsync(user2.User.Mention + " won! " + user1.User.Mention + " chose " + user1.ChosenOption.ToString() + " whereas " + user2.User.Mention + " chose " + winningOption.ToString());
                Dispose();
                return;
            }

            if (user1.ChosenOption == RPSOptions.Rock)
            {
                if (user2.ChosenOption != RPSOptions.Paper && user2.ChosenOption != user1.ChosenOption)
                {
                    await ChannelMatchStarted.SendMessageAsync(user1.User.Mention + " won! " + user2.User.Mention + " chose " + user2.ChosenOption.ToString() + " whereas " + user1.User.Mention + " chose " + user1.ChosenOption.ToString());
                }
                else if (user2.ChosenOption == user1.ChosenOption && user2.ChosenOption != RPSOptions.Paper)
                {
                    await ChannelMatchStarted.SendMessageAsync("It was a tie! " + user1.User.Mention + " " + user2.User.Mention);
                }
                else
                {
                    await ChannelMatchStarted.SendMessageAsync(user2.User.Mention + " won! " + user1.User.Mention + " chose " + user1.ChosenOption.ToString() + " whereas " + user2.User.Mention + " chose " + user2.ChosenOption.ToString());
                }
            }

            else if (user1.ChosenOption == RPSOptions.Paper)
            {
                if (user2.ChosenOption != RPSOptions.Scissor && user2.ChosenOption != user1.ChosenOption)
                {
                    await ChannelMatchStarted.SendMessageAsync(user1.User.Mention + " won! " + user1.User.Mention + " chose " + user1.ChosenOption.ToString() + " whereas " + user2.User.Mention + " chose " + user2.ChosenOption.ToString());
                }
                else if (user2.ChosenOption == user1.ChosenOption)
                {
                    await ChannelMatchStarted.SendMessageAsync("It was a tie! " + user1.User.Mention + " " + user2.User.Mention);
                }
                else if(user2.ChosenOption == RPSOptions.Scissor)
                {
                    await ChannelMatchStarted.SendMessageAsync(user2.User.Mention + " won! " + user1.User.Mention + " chose " + user1.ChosenOption.ToString() + " whereas " + user2.User.Mention + " chose " + user2.ChosenOption.ToString());
                }
            }

            if (user1.ChosenOption == RPSOptions.Scissor)
            {
                if (user2.ChosenOption != RPSOptions.Rock && user2.ChosenOption != user1.ChosenOption)
                {
                    await ChannelMatchStarted.SendMessageAsync(user2.User.Mention + " won! " + user1.User.Mention + " chose " + user1.ChosenOption.ToString() + " whereas " + user2.User.Mention + " chose " + user2.ChosenOption.ToString());
                }
                else if (user2.ChosenOption == user1.ChosenOption && user2.ChosenOption != RPSOptions.Rock)
                {
                    await ChannelMatchStarted.SendMessageAsync("It was a tie! " + user1.User.Mention + " " + user2.User.Mention);
                }
                else
                { 
                    await ChannelMatchStarted.SendMessageAsync(user2.User.Mention + " won! " + user1.User.Mention + " chose " + user1.ChosenOption.ToString() + " whereas " + user2.User.Mention + " chose " + user2.ChosenOption.ToString());
                }
            }

            Dispose();
        }

        private RPSUser GetUserFromGuildUser(SocketUser guildUser)
        {
            foreach(RPSUser rpsUser in UsersInTheMatch)
            {
                if (rpsUser.User.Id == guildUser.Id)
                    return rpsUser;
            }

            return null;
        }

        public void Dispose()
        {
            Program.matches.Remove(this);

            UsersInTheMatch = null;
            MatchStarted = false;
            Winner = null;
        }
    }
}
