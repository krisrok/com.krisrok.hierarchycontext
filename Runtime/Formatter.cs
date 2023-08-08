using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hextant;
using UnityEngine;

namespace Navigation
{
    public class Formatter
    {
        private delegate string Replac0r(Match replaceMatch, IEnumerable<string> stateParts, Dictionary<string, object> userData);

        private static readonly RegexOptions _defaultOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

        private static Dictionary<Regex, Replac0r> _replacePairs = new Dictionary<Regex, Replac0r>
        {
            {
                new Regex("({(?<TakeLast>(PARENT\\.|\\^))*(STATE|S|0)(?<SkipLast>\\.PARENT|\\^)*(?<IdsSiblings>\\.SIBLINGS|~)*\\})", _defaultOptions),
                delegate(Match replaceMatch, IEnumerable<string> stateParts, Dictionary<string, object> userData)
                {
                    if (replaceMatch.Groups["TakeLast"].Captures.Count > 0)
                    {
                        int count = replaceMatch.Groups["TakeLast"].Captures.Count;
                        stateParts = stateParts.Skip(stateParts.Count() - count);
                    }
                    if (replaceMatch.Groups["SkipLast"].Captures.Count > 0)
                    {
                        int count2 = replaceMatch.Groups["SkipLast"].Captures.Count;
                        stateParts = stateParts.Take(stateParts.Count() - count2);
                    }
                    if (replaceMatch.Groups["IdsSiblings"].Captures.Count > 0)
                    {
                        stateParts = stateParts.Take(stateParts.Count() - 1).Append(Settings<NavigationSettings>.instance.FragmentRegexPattern);
                    }
                    return string.Join(Settings<NavigationSettings>.instance.Separator, stateParts);
                }
            },
            {
                new Regex("({d})", _defaultOptions),
                (Match replaceMatch, IEnumerable<string> stateParts, Dictionary<string, object> userData) => Settings<NavigationSettings>.instance.Separator
            },
            {
                new Regex("({f})", _defaultOptions),
                (Match replaceMatch, IEnumerable<string> stateParts, Dictionary<string, object> userData) => Settings<NavigationSettings>.instance.FragmentRegexPattern
            },
            {
                new Regex("({(?<UserDataKey>\\w+)})", _defaultOptions),
                delegate(Match replaceMatch, IEnumerable<string> stateParts, Dictionary<string, object> userData)
                {
                    if (userData == null)
                    {
                        return string.Empty;
                    }
                    string value = replaceMatch.Groups["UserDataKey"].Value;
                    if (!userData.ContainsKey(value))
                    {
                        Debug.LogWarning("UserData does not contains key: " + value);
                        return string.Empty;
                    }
                    return userData[value].ToString();
                }
            }
        };

        public static string Format(string format, string state, Dictionary<string, object> userData = null)
        {
            if (!format.Contains("{"))
            {
                return format;
            }
            string[] stateParts = state.Split(new string[1] { Settings<NavigationSettings>.instance.Separator }, StringSplitOptions.None).ToArray();
            string result = format;
            foreach (KeyValuePair<Regex, Replac0r> p in _replacePairs)
            {
                result = p.Key.Replace(result, (Match match) => p.Value(match, stateParts.ToArray(), userData));
                if (!result.Contains("{"))
                {
                    break;
                }
            }
            return result;
        }
    }
}
