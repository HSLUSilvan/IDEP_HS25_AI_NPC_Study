using System.Collections.Generic;
using System.Text;

public class SseLineBuffer
{
    private readonly StringBuilder _sb = new StringBuilder();
    public IEnumerable<string> AppendAndExtractLines(byte[] data, int length)
    {
        var text = Encoding.UTF8.GetString(data, 0, length);
        _sb.Append(text);

        var lines = new List<string>();
        while (true)
        {
            var all = _sb.ToString();
            var idx = all.IndexOf('\n');
            if (idx < 0) break;

            var line = all.Substring(0, idx);
            lines.Add(line.TrimEnd('\r'));
            _sb.Remove(0, idx + 1);
        }

        return lines;
    }
}
