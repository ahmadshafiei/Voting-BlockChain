using System;
using System.Collections.Generic;
using System.Text;

namespace Voting.Infrastructure.Utility
{
    public static class NumberUtil
    {
        public static byte[] To4Byte(this int num)
        {
            byte[] result = new byte[4];

            result[0] = (byte)(num >> 24);
            result[1] = (byte)(num >> 16);
            result[2] = (byte)(num >> 8);
            result[3] = (byte)num;

            return result;
        }
    }
}
