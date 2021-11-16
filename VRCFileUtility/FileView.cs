using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using VRChatAPI.APIParams;
using VRChatAPI.Interfaces;
using VRChatAPI.Objects;
using VRChatAPI.Extentions;

namespace VRCFileUtility
{
	class FileView
	{
		private readonly ISession session;
		private List<VRCFile> current;

		public FileView(ISession session)
		{
			this.session = session;
			current = new List<VRCFile>();
		}

		private async IAsyncEnumerable<VRCFile> GetAllFilesAsync()
		{
			var c = 0;
			while (true)
			{
				IEnumerable<VRCFile> response = await session.Get(new VRCFileSearchParams(), 100, c);
				foreach (var i in response) yield return i;
				if (response.Count() < 100) break;
				c += response.Count();
			}
		}

		public async Task Show()
		{
			await AnsiConsole.Status().StartAsync("Gathering File Objects.", async c =>
			{
				await foreach (var i in GetAllFilesAsync())
				{
					AnsiConsole.MarkupLine($"[gray]Found: {i.Name}[/]");
					current.Add(i);
				}
			});
			AnsiConsole.MarkupLine($"Found [cyan]{current.Count}[/] files.");
			fileGet:
			var f = GetFile();

			while (true)
			{
				var c = AnsiConsole.Prompt(new SelectionPrompt<string>().Title("Choose Action")
				.AddChoices("Get Download Url", "See Detail", "Go Back"));
				switch (c)
				{
					case "Get Download Url":
						AnsiConsole.MarkupLine($"Url: [cyan]{f.GetFilePath(f.Versions.Last().Version)}[/]");
						break;
					case "See Detail":
						AnsiConsole.WriteLine(
							JsonSerializer.Serialize(f, new JsonSerializerOptions
							{
								WriteIndented = true,
							}));
						break;
					case "Go Back":
						goto fileGet;
				}
			}
		}

		private VRCFile GetFile()
		{
			var f = AnsiConsole.Prompt(new SelectionPrompt<string>()
				.Title("Choose [green]Category[/] to Show")
				.PageSize(10)
				.AddChoices(current.GroupBy(f => f.Extension).Select(v => v.Key)));
			var name = AnsiConsole.Prompt(new SelectionPrompt<string>()
				.Title("Chose [green]File[/] to see detail.")
				.PageSize(10)
				.AddChoices(current.Where(v => f.Contains(v.Extension)).Select(v => v.Name)));
			return current.First(v => v.Name == name);
		}
	}
}
