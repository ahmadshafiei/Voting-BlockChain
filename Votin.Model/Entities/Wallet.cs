using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using System.Security.Cryptography;

namespace Votin.Model.Entities
{
    public class Wallet
    {
        public EthECKey KeyPair { get; set; }
        public int Balance { get; set; }
        public string PublicKey { get; set; }

        public Wallet()
        {
            Balance = Config.INITIAL_BALANCE;
            KeyPair = EthECKey.GenerateKey();
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
