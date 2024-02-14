using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace HierarchyContext
{
    public class FragmentFormatter
    {
        private static HierarchyContextSettings _settings => HierarchyContextSettings.Instance;

        private delegate string Replac0r(Match replaceMatch, GameObject gameObject);

        private static readonly RegexOptions _defaultOptions = RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private static Dictionary<Regex, Replac0r> _replacePairs = new Dictionary<Regex, Replac0r>
        {
            {
                new Regex("{N(AME)?}", _defaultOptions),
                delegate(Match replaceMatch, GameObject gameObject)
                {
                    if(_settings.IsValidFragment(gameObject.name))
                        return gameObject.name;

                    var validParts = Regex.Matches(gameObject.name, _settings.FragmentRegexPattern)
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
                new Regex("{R:(?<Pattern>(?:(?!::).)+)(?:::(?<Format>.+))?}", _defaultOptions),
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

                            return string.Format(format, match.Groups.Select(g => g.Value).ToArray());
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

        internal static string SimpleFormat(string format, string replacement)
        {
            if (!format.Contains("{"))
            {
                return format;
            }
            string result = format;
            foreach (KeyValuePair<Regex, Replac0r> p in _replacePairs)
            {
                result = p.Key.Replace(result, replacement);
                if (!result.Contains("{"))
                {
                    break;
                }
            }
            return result;
        }
    }
}
