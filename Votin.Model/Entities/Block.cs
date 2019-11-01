using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Votin.Model.Entities
{
    public class Block
    {
        public long Timestamp { get; set; }
        public byte[] Hash { get; set; }
        public byte[] PreviousHash { get; set; }
        public string Data { get; set; }
        public int Nonce { get; set; }

        public override bool Equals(object obj)
        {
            return Hash.SequenceEqual(((Block)obj).Hash);
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
                        Nonce         : {Nonce}";
        }
    }
}
