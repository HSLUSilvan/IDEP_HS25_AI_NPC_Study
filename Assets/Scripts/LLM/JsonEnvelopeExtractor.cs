using System;

public static class JsonEnvelopeExtractor
{
    public const string OpenTag = "<JSON>";
    public const string CloseTag = "</JSON>";

    public static bool TryExtract(string text, out string json)
    {
        json = null;
        if (string.IsNullOrEmpty(text)) return false;

        if (TryExtractBetween(text, OpenTag, CloseTag, out json))
            return SanitizeJson(json, out json);

        if (TryExtractCodeFence(text, out json))
            return SanitizeJson(json, out json);

        return false;
    }

    private static bool TryExtractBetween(string text, string open, string close, out string inner)
    {
        inner = null;

        int a = text.IndexOf(open, StringComparison.Ordinal);
        if (a < 0) return false;

        int b = text.IndexOf(close, a + open.Length, StringComparison.Ordinal);
        if (b < 0) return false;

        int start = a + open.Length;
        inner = text.Substring(start, b - start).Trim();
        return !string.IsNullOrEmpty(inner);
    }

    private static bool TryExtractCodeFence(string text, out string inner)
    {
        inner = null;

        int start = text.IndexOf("```", StringComparison.Ordinal);
        if (start < 0) return false;

        int lineEnd = text.IndexOf('\n', start + 3);
        if (lineEnd < 0) return false;

        int contentStart = lineEnd + 1;

        int end = text.IndexOf("```", contentStart, StringComparison.Ordinal);
        if (end < 0) return false;

        inner = text.Substring(contentStart, end - contentStart).Trim();
        return !string.IsNullOrEmpty(inner);
    }

    private static bool SanitizeJson(string input, out string sanitized)
    {
        sanitized = null;
        if (string.IsNullOrEmpty(input)) return false;

        var s = input.Trim();

        s = s.TrimStart('\uFEFF');

        int obj = s.IndexOf('{');
        int arr = s.IndexOf('[');

        int first = -1;
        if (obj >= 0 && arr >= 0) first = Math.Min(obj, arr);
        else if (obj >= 0) first = obj;
        else if (arr >= 0) first = arr;

        if (first > 0)
            s = s.Substring(first);

        sanitized = s.Trim();
        return !string.IsNullOrEmpty(sanitized);
    }
}
