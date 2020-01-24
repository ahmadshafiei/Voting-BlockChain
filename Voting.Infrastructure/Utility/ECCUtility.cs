using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Model;
using Nethereum.RLP;
using Nethereum.Signer;
using Nethereum.Signer.Crypto;
using Org.BouncyCastle.Asn1.Ocsp;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace Voting.Infrastructure.Utility
{
    public static class ECCUtility
    {
        public static bool VerifySignature(string publicKey, byte[] signedData, byte[] dataHash)
        {
            var eth = new EthECKey(publicKey.HexToByteArray(), false);

            var result = eth.Verify(dataHash, EthECDSASignature.FromDER(signedData));
            
            return result;
        }
    }
}