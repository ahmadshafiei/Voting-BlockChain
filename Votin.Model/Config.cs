using System;
using System.Collections.Generic;
using System.Text;

namespace Votin.Model
{
    public class Config
    {
        /// <summary>
        /// TODO: Should change to initial vote foreach person
        /// </summary>
        public const int INITIAL_BALANCE = 50;

        public const int DIFFICULTY = 2;

        public const long MINE_RATE = TimeSpan.TicksPerSecond * 5;
    }
}
