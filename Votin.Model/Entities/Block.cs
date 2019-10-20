using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Votin.Model.Entities
{
    public class Block
    {
        public int Id { get; set; }
        public long Timestamp { get; set; }
        public byte[] Hash { get; set; }
        public byte[] PreviousHash { get; set; }
        public string Data { get; set; }

        public override bool Equals(object obj)
        {
            return Hash.SequenceEqual(((Block)obj).Hash);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public override string ToString()
        {
            return $@"Block({Id}) -
                        Timestamp     : {Timestamp}
                        Previous Hash : {PreviousHash}                      
                        Hash          : {Hash}
                        Data          : {Data}";
        }
    }
}
