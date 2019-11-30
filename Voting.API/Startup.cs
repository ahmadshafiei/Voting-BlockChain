using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Voting.Infrastructure;
using Voting.Infrastructure.MiddleWares;
using Voting.Infrastructure.PeerToPeer;
using Voting.Infrastructure.Services;
using Voting.Infrastructure.Services.BlockChainServices;
using Voting.Infrastructure.Services.BlockServices;

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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton<BlockService>();
            services.AddSingleton<BlockChainService>();
            services.AddSingleton<TransactionPoolService>();
            services.AddSingleton<TransactionService>();
            services.AddSingleton<WalletService>();
            services.AddSingleton<MinerService>();

            services.AddSingleton<BlockChain>();
            services.AddSingleton<P2PNetwork>();

            services.AddSingleton<IConfiguration>(_configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.ApplicationServices.GetService<P2PNetwork>().InitialNetwrok();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}

            app.UseExceptionHandlerMiddleware();


            app.UseMvc();

        }

    }
}
