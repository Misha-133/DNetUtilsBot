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
            { "Discord.Net.Interactions", null },
            { "Discord.Net.Commands", null }
		};

        var filter = new SearchFilter(true);

		foreach (var stat in stats)
		{
			var result = await p.SearchAsync(stat.Key, filter, 0, 10, NullLogger.Instance, CancellationToken.None);

            if (result.Any(x => x.Identity.Id == stat.Key))
			{
                stats[stat.Key] = result.First(x => x.Identity.Id == stat.Key).DownloadCount;
			}
		}

		var emb = new EmbedBuilder()
			.WithColor(0xff00)
			.WithTitle("`Discord.Net` Nuget Stats")
			.WithDescription($"Total downloads: `{stats.Sum(x => x.Value ?? 0)}`")
			.WithCurrentTimestamp();

		foreach (var stat in stats)
		{
			emb.AddField(stat.Key, $"`{stat.Value.ToString() ?? "Unknown"}`", true);
		}

		await FollowupAsync(embed: emb.Build());
	}
}
