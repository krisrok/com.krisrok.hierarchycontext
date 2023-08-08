using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SerializableSettings;
using UnityEngine;

namespace Navigation
{
    public class FragmentFormatter
    {
        private delegate string Replac0r(Match replaceMatch, GameObject gameObject);

        private static readonly RegexOptions _defaultOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private static Dictionary<Regex, Replac0r> _replacePairs = new Dictionary<Regex, Replac0r>
        {
            //{
            //    new Regex("({(?<TakeLast>(PARENT\\.|\\^))*(STATE|S|0)(?<SkipLast>\\.PARENT|\\^)*(?<IdsSiblings>\\.SIBLINGS|~)*\\})", _defaultOptions),
            //    delegate(Match replaceMatch, IEnumerable<string> stateParts, Dictionary<string, object> userData)
            //    {
            //        if (replaceMatch.Groups["TakeLast"].Captures.Count > 0)
            //        {
            //            int count = replaceMatch.Groups["TakeLast"].Captures.Count;
            //            stateParts = stateParts.Skip(stateParts.Count() - count);
            //        }
            //        if (replaceMatch.Groups["SkipLast"].Captures.Count > 0)
            //        {
            //            int count2 = replaceMatch.Groups["SkipLast"].Captures.Count;
            //            stateParts = stateParts.Take(stateParts.Count() - count2);
            //        }
            //        if (replaceMatch.Groups["IdsSiblings"].Captures.Count > 0)
            //        {
            //            stateParts = stateParts.Take(stateParts.Count() - 1).Append(Settings<NavigationSettings>.instance.FragmentRegexPattern);
            //        }
            //        return string.Join(Settings<NavigationSettings>.instance.Separator, stateParts);
            //    }
            //},
            {
                new Regex("{N(AME)?}", _defaultOptions),
                delegate(Match replaceMatch, GameObject gameObject)
                {
                    if(NavigationSettings.instance.IsValidFragment(gameObject.name))
                        return gameObject.name;

                    var validParts = Regex.Matches(gameObject.name, NavigationSettings.instance.FragmentRegexPattern)
                        .Select(m => m.Value)
                        .ToArray();

                    if(validParts.Length == 0)
                        throw new Exception($"Unable to build valid fragment from name \"{gameObject.name}\"");

                    return string.Concat(validParts);
                }
            },
            {
                new Regex("{I(NDEX)?}", _defaultOptions),
                delegate(Match replaceMatch, GameObject gameObject)
                {
                    var nameMatch = Regex.Match(gameObject.name, @"(\((?<Index>\d+)\)|(\.|_)(?<Index>\d+))$");
                    if(nameMatch.Success)
                        return nameMatch.Groups["Index"].Value;

                    return gameObject.transform.GetSiblingIndex().ToString();
                }
            },
            {
                new Regex("{R:(?<Pattern>.+?)(?::(?<Format>.+))?}", _defaultOptions),
                delegate(Match replaceMatch, GameObject gameObject)
                {
                    var pattern = replaceMatch.Groups["Pattern"].Value;
                    Match match;
                    try
                    {
                        match = Regex.Match(gameObject.name, pattern);
                    }
                    catch(Exception re)
                    {
                        throw new Exception($"Invalid regex pattern: {pattern}\n\n{re.GetType().ToString()}: {re.Message}");
                    }

                    if(replaceMatch.Groups["Format"].Success)
                    {
                        var format = replaceMatch.Groups["Format"].Value;

                        try
                        {
                            if(match.Groups.Count == 1)
                                return string.Format(format, match.Groups[0].Value);

                            return string.Format(format, match.Groups.Skip(1).Select(g => g.Value).ToArray());
                        }
                        catch(Exception fe)
                        {
                            throw new Exception($"Error applying format: {format}\n\n{fe.GetType().Name}: {fe.Message}");
                        }
                    }

                    if(match.Groups.Count == 1)
                        return match.Groups[0].Value;

                    return string.Concat(match.Groups.Skip(1).Select(g => g.Value));
                }
            },
        };

        public static string Format(string format, GameObject gameObject)
        {
            if (!format.Contains("{"))
            {
                return format;
            }
            string result = format;
            foreach (KeyValuePair<Regex, Replac0r> p in _replacePairs)
            {
                result = p.Key.Replace(result, (Match match) => p.Value(match, gameObject));
                if (!result.Contains("{"))
                {
                    break;
                }
            }
            return result;
        }

        public static bool Format(string format, out string result, GameObject gameObject)
        {
            result = Format(format, gameObject);
            return result != format;
        }
    }
}
