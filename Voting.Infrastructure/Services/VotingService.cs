using System.Collections.Generic;
using System.Threading.Tasks;
using Voting.Infrastructure.API.Vote;
using Voting.Model.Context;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class VotingService
    {
        private readonly BlockchainContext _dbContext;
        private readonly WalletService _walletService;

        public VotingService(BlockchainContext dbContext, WalletService walletService)
        {
            _dbContext = dbContext;
            _walletService = walletService;
        }

        public async Task Vote(List<Vote> votes, string privateKey)
        {
            Wallet wallet = new Wallet(privateKey);

            votes.ForEach(v => { _walletService.CreateTransaction(wallet, v.ElectionAddress, v.Candidate); });
        }
    }
}