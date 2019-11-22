using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using Nethereum.Signer;

namespace Voting.Infrastructure.Utility
{
    public static class ECCUtility
    {
        public static bool VerifySignature(string publicKey, byte[] signedData, byte[] dataHash)
        {
            EthECKey verifier = new EthECKey(publicKey);
            return verifier.Verify(dataHash, EthECDSASignature.FromDER(signedData));
        }
    }
}
