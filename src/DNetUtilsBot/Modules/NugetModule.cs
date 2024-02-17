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
        var p = await _repository.GetResourceAsync<PackageSearchResource>();

        var filter = new SearchFilter(true);
        var meta = (await p.SearchAsync("Discord.Net", filter, 0, 1, NullLogger.Instance, CancellationToken.None)).First();
        var core = (await p.SearchAsync("Discord.Net.Core", filter, 0, 1, NullLogger.Instance, CancellationToken.None)).First();
        var rest = (await p.SearchAsync("Discord.Net.Rest", filter, 0, 1, NullLogger.Instance, CancellationToken.None)).First();
        var socket = (await p.SearchAsync("Discord.Net.WebSocket", filter, 0, 1, NullLogger.Instance, CancellationToken.None)).First();
        var interactions = (await p.SearchAsync("Discord.Net.Interactions", filter, 0, 1, NullLogger.Instance, CancellationToken.None)).First();
        var commands = (await p.SearchAsync("Discord.Net.Commands", filter, 0, 1, NullLogger.Instance, CancellationToken.None)).First();




    }
}
