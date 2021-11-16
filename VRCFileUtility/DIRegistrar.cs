using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console.Cli;
using System;

namespace VRCFileUtility
{
	internal class DIRegistrar : ITypeRegistrar, IDisposable
	{
		private Action<IServiceCollection> sc;
		private IHost host;

		public DIRegistrar(Action<IServiceCollection> sc) => 
			this.sc = sc;

		public ITypeResolver Build() => 
			new DIResolver((host = Host.CreateDefaultBuilder().ConfigureServices(sc).Build()).Services);

		public void Dispose() => host?.Dispose();

		public void Register(Type service, Type implementation) =>
			sc = Connect(sc, c => c.AddSingleton(service, implementation));

		public void RegisterInstance(Type service, object implementation) => 
			sc = Connect(sc, c => c.AddSingleton(service, implementation));

		public void RegisterLazy(Type service, Func<object> factory) => 
			sc = Connect(sc, c => c.AddSingleton(service, _ => factory()));

		public static Action<T> Connect<T>(Action<T> a, Action<T> b) =>
			c =>
			{
				a(c);
				b(c);
			};
	}
}