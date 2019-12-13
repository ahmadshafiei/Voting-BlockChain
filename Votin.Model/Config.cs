using System;
using System.Collections.Generic;
using System.Text;

namespace Voting.Model
{
    public class Config
    {
        /// <summary>
        /// Initial vote for every candidate in election
        /// </summary>
        public const int INITIAL_BALANCE = 0;

        public const int DIFFICULTY = 2;

        public const long MINE_RATE = TimeSpan.TicksPerSecond * 5;

        public const int MINING_REWARD = 50;
    }
}
