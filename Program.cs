using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace han5c
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
			.ConfigureWebHostDefaults(webBuilder =>
					{
					webBuilder
					.UseKestrel(option=>
							option.Listen(System.Net.IPAddress.Any,443,(lop)=>
								lop.UseHttps("han5.cc.pfx","123456")))
					.UseStartup<Startup>();
					});
	}
}
