using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Voting.Model.Entities;

namespace Voting.Model.Context
{
    public class BlockchainContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<TransactionInput> TransactionInputs { get; set; }
        public DbSet<TransactionOutput> TransactionOutputs { get; set; }
        public DbSet<Election> Elections { get; set; }
        public DbSet<ElectionCandidate> ElectionCandidates { get; set; }
        public DbSet<Block> Blocks { get; set; }

        public BlockchainContext(DbContextOptions opt) : base(opt)
        {
            Database.EnsureCreated();
        }
    }

    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<BlockchainContext>
    {
        public BlockchainContext CreateDbContext(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Voting.API"))
                .AddJsonFile("appsettings.json")
                .Build();

            // Get connection string
            var optionsBuilder = new DbContextOptionsBuilder<BlockchainContext>();
            var connectionString = config.GetConnectionString("BlockchainContext");

            string p2p_port = Environment.GetEnvironmentVariable("P2P_PORT") != null
                ? Environment.GetEnvironmentVariable("P2P_PORT")
                : config.GetSection("P2P").GetSection("DEFAULT_PORT").Value;

            string connection = string.Format(config.GetConnectionString("BlockchainContext"), p2p_port);

            optionsBuilder.UseSqlServer(string.Format(connectionString, p2p_port));

            var context = new BlockchainContext(optionsBuilder.Options);

            return context;
        }
    }
}