using System;
using System.Collections.Generic;
using System.Text;
using Voting.Infrastructure.DTO.Profile;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class ProfileService
    {
        public WalletDTO GetNewWallet()
        {
            Wallet wallet = new Wallet();

            WalletDTO result = new WalletDTO
            {
                PublicKey = wallet.PublicKey,
                PrivateKey = wallet.KeyPair.GetPrivateKey()
            };

            return result;
        }

        public WalletDTO GetPublicKey(string privateKey)
        {
            Wallet wallet = new Wallet(privateKey);

            WalletDTO result = new WalletDTO
            {
                PublicKey = wallet.PublicKey
            };

            return result;
        }
    }
}