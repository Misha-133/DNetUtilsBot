using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace DNetUtilsBot.AutocompleteHandlers;

public class NugetSearchAutocomplete : AutocompleteHandler
{
	private static SourceRepository _repo = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

	public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction,
    IParameterInfo parameter, IServiceProvider services)
	{
		var input = autocompleteInteraction.Data?.Current?.Value?.ToString();

		var resource = await _repo.GetResourceAsync<AutoCompleteResource>();
        var search = await resource.IdStartsWith(input ?? string.Empty, true, NullLogger.Instance, CancellationToken.None);


        IEnumerable<AutocompleteResult> results = search.Select(x => new AutocompleteResult(x, x));

		return AutocompletionResult.FromSuccess(results.Take(25));
	}
}