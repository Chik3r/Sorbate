namespace Sorbate.DiscordBot;

public static class Constants {
    // You are supposed to get this uri from the GetGateway endpoint
    // However I am lazy
    public const string DiscordGatewayUri = "wss://gateway.discord.gg/?encoding=json&v=9";
    public const string TmlGuildId = "103110554649894912";
    public const string TmlModBrowserChannelId = "284077567986761730";

    public const string SearchUri =
        $"https://discord.com/api/v9/guilds/{TmlGuildId}/messages/search?channel_id={TmlModBrowserChannelId}&has=file&sort_by=timestamp&sort_order=asc";
}