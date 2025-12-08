namespace Toyo_cable_UI.Helpers
{
    public class TokenManager
    {
        public static string? Token { get; set; }
        public static bool IsAuthenticated => !string.IsNullOrEmpty(Token);
    }
}
