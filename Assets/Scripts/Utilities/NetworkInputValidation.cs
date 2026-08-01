public static class NetworkInputValidation
{
    public const int MaximumChatMessageLength = 256;
    public const int MaximumDamagePerHit = 50;

    public static bool TryNormalizeChatMessage(
        string message,
        out string normalizedMessage)
    {
        normalizedMessage = string.IsNullOrWhiteSpace(message)
            ? string.Empty
            : message.Trim();

        if (normalizedMessage.Length == 0)
            return false;

        if (normalizedMessage.Length > MaximumChatMessageLength)
        {
            normalizedMessage = normalizedMessage.Substring(
                0,
                MaximumChatMessageLength
            );
        }

        return true;
    }

    public static bool IsValidDamage(int damage)
    {
        return damage > 0 && damage <= MaximumDamagePerHit;
    }
}
