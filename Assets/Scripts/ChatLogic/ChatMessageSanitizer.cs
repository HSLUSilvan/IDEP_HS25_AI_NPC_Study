using System.Collections.Generic;

public static class ChatMessageSanitizer
{
    public static List<ChatMessage> Sanitize(IEnumerable<ChatMessage> input)
    {
        var output = new List<ChatMessage>();

        foreach (var msg in input)
        {
            if (msg == null || string.IsNullOrWhiteSpace(msg.content))
                continue;

            var role = NormalizeRole(msg.role);
            output.Add(new ChatMessage(role, msg.content));
        }

        return output;
    }

    private static string NormalizeRole(string role)
    {
        switch (role)
        {
            case "system":
            case "user":
            case "assistant":
                return role;

            default:
                return "system";
        }
    }
}
