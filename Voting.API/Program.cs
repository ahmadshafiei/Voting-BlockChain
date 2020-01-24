using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Voting.Infrastructure.PeerToPeer;
using Voting.Model.Context;

namespace Voting.API
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var webhost = CreateWebHostBuilder(args).Build();

            webhost.Run();
        }

        private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost
                .CreateDefaultBuilder(args)
                .UseUrls(args)
                .UseKestrel()
                .UseStartup<Startup>();
    }
}