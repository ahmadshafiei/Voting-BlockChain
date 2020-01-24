using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using Voting.Model;

namespace Voting.Model.Entities
{
    public class Block
    {
        public int Id { get; set; }
        public long Timestamp { get; set; }
        public byte[] Hash { get; set; }
        public byte[] PreviousHash { get; set; }
        public string Data { get; set; }
        public int Nonce { get; set; }
        public int Difficulty { get; set; } = Config.DIFFICULTY;

        public override bool Equals(object obj)
        {
            if (obj.GetType() == typeof(Block))
                return Hash.SequenceEqual(((Block) obj).Hash);

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return $@"Block -
                        Timestamp     : {Timestamp}
                        Previous Hash : {PreviousHash}                      
                        Hash          : {Hash}
                        Data          : {Data}
                        Nonce         : {Nonce}
                        Difficulty    : {Difficulty}";
        }
    }
}