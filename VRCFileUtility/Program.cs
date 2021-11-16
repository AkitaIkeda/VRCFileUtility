using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VRChatAPI.APIParams;
using VRChatAPI.Extentions.DependencyInjection;
using VRChatAPI.Interfaces;
using VRChatAPI.Objects;

namespace VRCFileUtility
{
	class Program
	{

		static int Main(string[] args)
		{
			var app = new CommandApp<App>(new DIRegistrar(c => c.AddVRCAPI()));
			return app.Run(args);
		}
	}
}
