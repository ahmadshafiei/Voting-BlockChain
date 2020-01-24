using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voting.Model.Context;
using Voting.Infrastructure;
using Voting.Infrastructure.AutoMapper;
using Voting.Infrastructure.MiddleWares;
using Voting.Infrastructure.PeerToPeer;
using Voting.Infrastructure.Services;
using Voting.Infrastructure.Services.BlockChainServices;
using Voting.Infrastructure.Services.BlockServices;
using Voting.Model.Entities;

namespace Voting.API
{
    public class Startup
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;

        public Startup(IConfiguration configuration, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            string p2p_port = Environment.GetEnvironmentVariable("P2P_PORT") != null
                ? Environment.GetEnvironmentVariable("P2P_PORT")
                : _configuration.GetSection("P2P").GetSection("DEFAULT_PORT").Value;

            string connection = string.Format(_configuration.GetConnectionString("BlockchainContext"), p2p_port);

            services.AddDbContext<BlockchainContext>(opt =>
                opt.UseSqlServer(connection));

            services.AddDbContext<BlockchainCommonContext>(opt =>
                opt.UseSqlServer(_configuration.GetConnectionString("BlockchainCommonContext")));

            services.AddCors(opt =>
            {
                opt.AddPolicy("BlockChain Policy", builder =>
                {
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowAnyOrigin();
                });
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHttpContextAccessor();

            services.AddScoped<BlockService>();
            services.AddScoped<BlockChainService>();
            services.AddScoped<TransactionPoolService>();
            services.AddScoped<TransactionService>();
            services.AddScoped<WalletService>();
            services.AddScoped<MinerService>();
            services.AddScoped<ProfileService>();
            services.AddScoped<ElectionService>();
            services.AddScoped<VotingService>();

            services.AddSingleton<BlockChain>();
            services.AddSingleton<P2PNetwork>();

            services.AddSingleton(_configuration);

            services.AddAutoMapper(typeof(ElectionProfile).Assembly);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors("BlockChain Policy");

            app.UseExceptionHandlerMiddleware();

            app.UseMvc();
        }
    }
}