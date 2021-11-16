using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using VRCFileUtility.Utillity;
using VRChatAPI.Implementations;
using VRChatAPI.Interfaces;
using VRChatAPI.Objects;

namespace VRCFileUtility
{
	public class App : AsyncCommand<App.Settings>
	{
		private readonly ISession session;

		public App(ISession session)
		{
			this.session = session;
		}

		public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] Settings settings)
		{
			AnsiConsole.Write(
				new FigletText("VRCFileUtil.cli")
					.LeftAligned()
					.Color(Color.Aquamarine1_1));

			AnsiConsole.Write(new Rule("[aqua]Login[/]").LeftAligned());
			var c = await LoginLoop(settings.TokenPath);

			if (c is null)
				return 0;
			
			TokenUtillity.SaveCredential(session.Credential, settings.TokenPath);

			AnsiConsole.MarkupLine($"Login success. Account: [cyan]{c.DisplayName}[/]");

			await Run();

			return 0;
		}

		private async Task Run()
		{
			var v = AnsiConsole.Prompt(new SelectionPrompt<string>()
				.Title("Choose [green]Object[/] to See.")
				.AddChoices("Files"));
			switch (v)
			{
				case "Files":
					await new FileView(session).Show();
					break;
				default:
					throw new NotImplementedException();
			}
		}

		#region Login
		private async Task<CurrentUser> LoginLoop(string tokenPath)
		{
			LoginInfo li;
			while (true)
			{
				var cred = GetCredential(tokenPath);
				var isToken = cred is ITokenCredential;

				try
				{
					li = await AnsiConsole.Status().StartAsync("Logging in...", x => session.Login(cred));
					break;
				}
				catch (Exception ex)
				{
					AnsiConsole.WriteException(ex,
						ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes |
						ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
					if (cred is ITokenCredential) File.Delete(tokenPath);
					if (!AnsiConsole.Confirm("[red]Login Failed.[/] Continue?")) return null;
				}
			}

			if (!li.TFARequired) return li.User;

			AnsiConsole.MarkupLine("[cyan]2FA[/] Required.");
			while (true)
			{
				var tfaToken = AnsiConsole.Ask<string>("[green]2FA Token[/]: ");

				try
				{
					var ret = await AnsiConsole.Status().StartAsync("Verifying...", x => session.VerifyTwoFactorAuth(tfaToken));
					if (ret) break;
				}
				catch (Exception ex)
				{
					AnsiConsole.WriteException(ex,
						ExceptionFormats.ShortenPaths | ExceptionFormats.ShortenTypes |
						ExceptionFormats.ShortenMethods | ExceptionFormats.ShowLinks);
				}
				if (!AnsiConsole.Confirm("[red]Verification Failed.[/] Continue?")) return null;
			}

			return await AnsiConsole.Status().StartAsync("Gathering User Info...", x => session.GetCurrentUser());
		}

		private static ICredential GetCredential(string tokenPath)
		{
			if (TokenUtillity.TryGetSavedCredential(out var credential, tokenPath))
			{
				AnsiConsole.MarkupLine("[cyan]Found saved AuthToken.[/]");
				return credential;
			}

			var id = AnsiConsole.Ask<string>("[green]Username[/]: ");
			var pass = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Password[/]: ")
					.PromptStyle("red")
					.Secret());
			return new BasicAuthCredential(id, pass);
		}
		#endregion

		public class Settings : CommandSettings
		{
			[CommandArgument(0, "[TokenPath]")]
			public string TokenPath { get; set; }
		}
	}
}