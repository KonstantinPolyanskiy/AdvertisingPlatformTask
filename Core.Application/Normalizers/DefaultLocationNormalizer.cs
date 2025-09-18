using System.Text;

namespace Core.Application.Normalizers;

/// <summary>
/// Стандартный нормализтор строк.
/// </summary>
internal static class DefaultLocationNormalizer
{
    public static bool TryNormalize(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input)) return false;

        var s = input.Trim().ToLowerInvariant();

        if (!s.StartsWith('/')) s = "/" + s;

        if (s.Contains("//"))
        {
            var sb = new StringBuilder(s.Length);

            bool prevSlash = false;

            foreach (var ch in s)
            {
                if (ch == '/')
                {
                    if (!prevSlash) sb.Append('/');
                    prevSlash = true;
                }
                else
                {
                    sb.Append(ch);
                    prevSlash = false;
                }
            }
            s = sb.ToString();
        }

        if (s.Length > 1 && s[^1] == '/') s = s.TrimEnd('/');

        foreach (var c in s.Where(c => c != '/'))
        {
            switch (c)
            {
                case >= 'a' and <= 'z':
                    continue;
                case >= '0' and <= '9':
                    continue;
                default:
                    return false;
            }
        }

        normalized = s;

        return true;
    }
}