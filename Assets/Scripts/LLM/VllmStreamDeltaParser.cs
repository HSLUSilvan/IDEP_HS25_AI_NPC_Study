using System.Text;
using System.Text.RegularExpressions;

public static class VllmStreamDeltaParser
{
    private static readonly Regex UnicodeRegex =
        new Regex(@"\\u([0-9a-fA-F]{4})", RegexOptions.Compiled);

    public static string JsonStringUnescape(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        var sb = new StringBuilder(input.Length);

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '\\' && i + 1 < input.Length)
            {
                char next = input[i + 1];
                i++;

                switch (next)
                {
                    case '\\': sb.Append('\\'); break;
                    case '"': sb.Append('"'); break;
                    case 'n': sb.Append('\n'); break;
                    case 'r': sb.Append('\r'); break;
                    case 't': sb.Append('\t'); break;
                    case 'b': sb.Append('\b'); break;
                    case 'f': sb.Append('\f'); break;

                    case 'u':
                        if (i + 4 < input.Length)
                        {
                            string hex = input.Substring(i + 1, 4);
                            if (ushort.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out ushort code))
                            {
                                sb.Append((char)code);
                                i += 4;
                            }
                            else
                            {
                                sb.Append("\\u");
                            }
                        }
                        else
                        {
                            sb.Append("\\u");
                        }
                        break;

                    default:
                        sb.Append(next);
                        break;
                }
            }
            else
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    public static bool TryExtractDelta(string line, out string deltaText)
    {
        deltaText = null;

        if (string.IsNullOrWhiteSpace(line))
            return false;

        int idx = line.IndexOf("\"content\":\"");
        if (idx < 0)
            return false;

        idx += "\"content\":\"".Length;
        int end = line.IndexOf('"', idx);
        if (end < 0)
            return false;

        string raw = line.Substring(idx, end - idx);
        deltaText = JsonStringUnescape(raw);
        return true;
    }
}
