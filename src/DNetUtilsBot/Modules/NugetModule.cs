using DNetUtilsBot.AutocompleteHandlers;

using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace DNetUtilsBot.Modules;

[Group("nuget", "Nuget cmds")]
public class NugetModule(ILogger<NugetModule> logger, ILoggerFactory loggerFactory) : InteractionModuleBase<SocketInteractionContext>
{
    private readonly SourceRepository _repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

    [SlashCommand("dnet-stats", "Get statistics for DNet nuget packages")]
    public async Task DNetStatsAsync()
    {
        await DeferAsync();

        var p = await _repository.GetResourceAsync<PackageSearchResource>();

        var stats = new Dictionary<string, long?>
        {
            { "Discord.Net", null },
            { "Discord.Net.Core", null },
            { "Discord.Net.Rest", null },
            { "Discord.Net.WebSocket", null },
            { "Discord.Net.Webhook", null },
            { "Discord.Net.Interactions", null },
            { "Discord.Net.Commands", null },
            { "Discord.Net.Analyzers", null },
            { "Discord.Net.Providers.WS4Net", null },
            { "Discord.Net.BuildOverrides", null }
        };

        var filter = new SearchFilter(true);

        foreach (var stat in stats)
        {
            if (stat.Value is not null)
                return;

            var result = await p.SearchAsync(stat.Key, filter, 0, 10, NullLogger.Instance, CancellationToken.None);

            foreach (var res in result)
            {
                if (res.Identity.Id == stat.Key)
                    stats[stat.Key] = result.First(x => x.Identity.Id == stat.Key).DownloadCount;
            }

            if (stats.All(x => x.Value is not null))
                break;
        }

        var emb = new EmbedBuilder()
            .WithColor(0xff00)
            .WithTitle("`Discord.Net` Nuget Stats")
            .WithDescription($"## Total downloads: **`{stats.Sum(x => x.Value ?? 0)}`**")
            .WithCurrentTimestamp();

        foreach (var stat in stats)
        {
            emb.AddField(stat.Key, $"`{stat.Value?.ToString() ?? "Unknown"}`", true);
        }

        await FollowupAsync(embed: emb.Build());
    }

    [SlashCommand("search", "Search nuget.org")]
    public async Task SearchNugetAsync([Summary("query", "Query to search for"), Autocomplete(typeof(NugetSearchAutocomplete))] string query)
    {
        await DeferAsync();

        var filter = new SearchFilter(true);

        var resource = await _repository.GetResourceAsync<PackageSearchResource>();

        var result = await resource.SearchAsync(query, filter, 0, 3, NullLogger.Instance, CancellationToken.None);

        var emb = new EmbedBuilder()
            .WithColor(0xff33)
            .WithTitle("Search results")
            .WithCurrentTimestamp();

        var menu = new SelectMenuBuilder()
            .WithCustomId("nuget-search-select")
            .WithPlaceholder("Select a package");

        foreach (var res in result)
        {
            var packageDesc = res.Description.Length > 150 ? res.Description[..150] + "..." : res.Description;
            var desc = $"[link](https://www.nuget.org/packages/{res.Identity.Id})\nDownloads: `{res.DownloadCount}`\nTags: `{res.Tags}`\nOwners: `{res.Owners}`\nDescription: ```{packageDesc}```";
            emb.AddField(res.Identity.Id, desc);

            var packageDescShort = res.Description.Length > 100 ? res.Description[..97] + "..." : res.Description;
            menu.AddOption(res.Identity.Id, res.Identity.Id, packageDescShort);
        }

        await FollowupAsync(embed: emb.Build(), components: new ComponentBuilder().WithSelectMenu(menu).Build());
    }

    [ComponentInteraction("nuget-search-select", true)]
    public async Task NugetSearchSelect(string selection)
    {
        await DeferAsync();

        var filter = new SearchFilter(true);

        var resource = await _repository.GetResourceAsync<PackageSearchResource>();

        var result = (await resource.SearchAsync(selection, filter, 0, 1, NullLogger.Instance, CancellationToken.None)).FirstOrDefault();

        if (result is null)
        {
            await FollowupAsync("Failed to find the package");
            return;
        }

        var emb = new EmbedBuilder()
            .WithColor(0xff00)
            .WithCurrentTimestamp()
            .WithTitle($"Package `{selection}`")
            .WithDescription($"```{result.Description}```")
            .AddField("Total downloads", $"`{result.DownloadCount}`", true)
            .AddField("Is Listed", $"`{result.IsListed}`", true)
            .AddField("Link", $"[Link](https://www.nuget.org/packages/{selection})", true);

		var versions = await result.GetVersionsAsync();
		var current = versions.MaxBy(x => x.Version);

        emb.AddField("Version", $"`{current.Version}` | Downloads: `{current.DownloadCount}`")
			.AddField("Owners", $"`{result.Owners}`")
            .AddField("Tags", $"`{result.Tags}`")
			.WithThumbnailUrl(result.IconUrl?.AbsoluteUri);

        if (result.ReadmeUrl is not null)
        {
            using var http = new HttpClient();
            var res = await http.GetStringAsync(result.ReadmeUrl);

            var readme = res.Length > 500 ? res[..500] : res;
            emb.AddField("Readme", readme);
        }

		await ModifyOriginalResponseAsync(x =>
		{
			x.Components = null;
            x.Embeds = new[] { emb.Build() };
		});
	}
}
