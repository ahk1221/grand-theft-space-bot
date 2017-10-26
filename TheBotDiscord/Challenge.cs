using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace TheBotDiscord
{
    public class Challenge : IDisposable
    {
        public SocketUser UserChallenged { get; protected set; }
        public SocketUser UserChallengedBy { get; protected set; }
        public ISocketMessageChannel Channel { get; protected set; }

        private Timer timer;

        public Challenge(SocketUser challengedBy, SocketUser challenged, ISocketMessageChannel channel)
        {
            UserChallenged = challenged;
            UserChallengedBy = challengedBy;
            channel = Channel;
            timer = new Timer(60000);
            timer.Enabled = true;
            timer.Start();
            timer.Elapsed += (s, e) =>
            {
                Channel.SendMessageAsync(UserChallenged.Mention + " waited too long!");
                Dispose();
            };
        }

        public void Accept()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            Program.activeChallenges.Remove(this);

            UserChallenged = null;
            UserChallengedBy = null;
            timer.Stop();
            timer.Dispose();
        }
    }
}
