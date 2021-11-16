using Microsoft.Extensions.DependencyInjection;
using Spectre.Console.Cli;
using System;

namespace VRCFileUtility
{
	internal class DIResolver : ITypeResolver
	{
		private IServiceProvider serviceProvider;

		public DIResolver(IServiceProvider serviceProvider) => 
			this.serviceProvider = serviceProvider;

		public object Resolve(Type type) => 
			serviceProvider.GetService(type);
	}
}