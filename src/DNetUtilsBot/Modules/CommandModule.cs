namespace DNetUtilsBot.Modules;

public class CommandModule(ILogger<CommandModule> logger) : InteractionModuleBase<SocketInteractionContext>
{
    [SlashCommand("test", "Just a test command")]
    public async Task TestCommand()
        => await RespondAsync("Hello There");

}