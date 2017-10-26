using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheBotDiscord
{
    public enum RPSOptions
    {
        None,
        Rock,
        Paper,
        Scissor
    }

    public class RPSUser
    {
        public SocketUser User { get; protected set; }
        public RPSOptions ChosenOption { get; set; }
        public bool ShouldAlwaysWin { get; set; }
        public bool ShouldAlwaysLose { get; set; }

        public RPSUser(SocketUser user)
        {
            User = user;
            ChosenOption = RPSOptions.None;
        }

        public RPSUser(SocketUser user, RPSOptions chosenOption)
        {
            User = user;
            ChosenOption = chosenOption;
        }
    }
}
