using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace VotingSprocket
{
    internal class Poll
    {
        private List<string> _ballots;

        public Timer Timer { get; private set; }
        public Dictionary<string, int> Votes { get; private set; }

        public Poll()
        {
            _ballots = new List<string>();
            Votes = new Dictionary<string, int>();
#if DEBUG
            Timer = new Timer(250);
#else
                Timer = new Timer(2 * 60 * 1000);
#endif
        }

        public void CastBallot(string sender, string vote)
        {
            if (HasAlreadyVoted(sender)) return;

            _ballots.Add(sender);
            if (!Votes.ContainsKey(vote)) Votes.Add(vote, 0);
            Votes[vote]++;
        }

        public bool HasAlreadyVoted(string sender)
        {
            return _ballots.Contains(sender);
        }
    }
}
