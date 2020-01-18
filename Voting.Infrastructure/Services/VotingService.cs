using System.Collections.Generic;
using System.Threading.Tasks;
using Voting.Infrastructure.API.Vote;
using Voting.Infrastructure.PeerToPeer;
using Voting.Model.Context;
using Voting.Model.Entities;

namespace Voting.Infrastructure.Services
{
    public class VotingService
    {
        private readonly BlockchainContext _dbContext;
        private readonly WalletService _walletService;
        private readonly MinerService _minerService;
        private readonly P2PNetwork _p2PNetwork;

        public VotingService(BlockchainContext dbContext, WalletService walletService, MinerService minerService,
            P2PNetwork p2PNetwork)
        {
            _dbContext = dbContext;
            _walletService = walletService;
            _minerService = minerService;
            _p2PNetwork = p2PNetwork;
        }

        public async Task Vote(List<Vote> votes, string privateKey)
        {
            Wallet wallet = new Wallet(privateKey);

            foreach (var vote in votes)
            {
                Transaction transaction =
                    await _walletService.CreateTransaction(wallet, vote.ElectionAddress, vote.Candidate);
                _p2PNetwork.BroadcastTransaction(transaction);
            }
        }
    }
}