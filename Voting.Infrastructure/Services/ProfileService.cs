using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Votin.Model.Exceptions;
using Voting.Infrastructure.API.User;
using Voting.Infrastructure.DTO.Profile;
using Voting.Infrastructure.Model.Common;
using Voting.Infrastructure.Model.Profile;
using Voting.Model.Context;
using Voting.Model.Entities;
using Voting.Model.Exceptions;

namespace Voting.Infrastructure.Services
{
    public class ProfileService
    {
        private readonly BlockchainCommonContext _commonContext;

        public ProfileService(BlockchainCommonContext commonContext)
        {
            _commonContext = commonContext;
        }

        public async Task<WalletDTO> GetNewWallet(CreateWalletModel model)
        {
            Wallet wallet = new Wallet();

            bool isUserRegistered = await _commonContext.Users.AnyAsync(u => u.PhoneNumber == model.PhoneNumber);

            if (isUserRegistered)
                throw new ValidationException("شماره همراه وارد شده در حال حاضر ثبت شده.");

            WalletDTO result = new WalletDTO
            {
                PublicKey = wallet.PublicKey,
                PrivateKey = wallet.KeyPair.GetPrivateKey()
            };

            _commonContext.Users.Add(new User
            {
                Name = model.Name,
                PhoneNumber = model.PhoneNumber,
                Role = Role.Voter,
                PublicKey = result.PublicKey
            });

            await _commonContext.SaveChangesAsync();

            return result;
        }

        public async Task<WalletDTO> GetPublicKey(string privateKey)
        {
            Wallet wallet = new Wallet(privateKey);

            WalletDTO result = new WalletDTO
            {
                PublicKey = wallet.PublicKey
            };

            bool isUserRegistered = await _commonContext.Users.AnyAsync(u => u.PublicKey == result.PublicKey);

            if (!isUserRegistered)
                throw new ValidationException("شناسه ی وارد شده صحیح نمی باشد");

            return result;
        }

        public async Task<PagedResult<User>> GetUsersAsync(UserSearch filter)
        {
            PagedResult<User> result = new PagedResult<User>();

            filter.Name = filter.Name ?? "";
            filter.Address = filter.Address ?? "";

            var users = await _commonContext.Users.Where(
                u => u.Name.Contains(filter.Name) &&
                     u.PublicKey.Contains(filter.Address)
            ).ToListAsync();

            result.Items = users;
            result.TotalCount = users.Count;

            return result;
        }

        public async Task<bool> IsAdminAsync(string publicKey)
        {
            var user = await _commonContext.Users.SingleOrDefaultAsync(u => u.PublicKey == publicKey);

            if (user == null)
                throw new NotFoundException("کاربر");

            return user.Role == Role.Admin;
        }

        public async Task<User> GetUsernameAsync(string publicKey)
        {
            var user = await _commonContext.Users.SingleOrDefaultAsync(u => u.PublicKey == publicKey);

            if (user == null)
                throw new NotFoundException("کاربر");

            return new User
            {
                Name = user.Name
            };
        }
    }
}