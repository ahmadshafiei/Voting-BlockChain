using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using System.Security.Cryptography;

namespace Voting.Model.Entities
{
    public class Wallet
    {
        public EthECKey KeyPair { get; set; }

        /// <summary>
        /// Initial votes of candidate in election
        /// </summary>
        public int Balance { get; set; } = 0;
        public string PublicKey { get; set; }

        public Wallet(string privateKey = null)
        {
            if (string.IsNullOrEmpty(privateKey))
                KeyPair = EthECKey.GenerateKey();
            else
                KeyPair = new EthECKey(privateKey);

            Balance = Config.INITIAL_BALANCE;
            PublicKey = KeyPair.GetPubKey().ToHex();
        }

        public byte[] Sign(byte[] dataHash)
        {
            return KeyPair.Sign(dataHash).ToDER();
        }

        public override string ToString()
        {
            return $@"Wallet -
                         PublicKey : {PublicKey}
                         Balance   : {Balance}";
        }
    }
}
