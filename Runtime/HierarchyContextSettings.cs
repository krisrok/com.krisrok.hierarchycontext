using System.Linq;
using System.Text.RegularExpressions;
using SerializableSettings;
using UnityEngine;

namespace HierarchyContext
{
    [RuntimeProjectSettings("Hierarchy Context")]
    public class HierarchyContextSettings : SerializableSettings<HierarchyContextSettings>
    {
        [SerializeField]
        private string _separator = "/";
        public string Separator => _separator;

        [SerializeField]
        private string _fragmentRegexPattern = "[A-Za-z0-9_]+";
        public string FragmentRegexPattern => _fragmentRegexPattern;

        public Regex SingleFragmentRegex
        {
            get
            {
                return new Regex($"^{_fragmentRegexPattern}$");
            }
        }

        public bool IsValidFragment(string fragment)
        {
            return !string.IsNullOrEmpty(fragment) && SingleFragmentRegex.IsMatch(fragment);
        }

        public bool IsValidContext(string context)
        {
            if (context == null)
                return false;

            return context.Split(_separator).All((string fragment) => IsValidFragment(fragment));
        }
    }
}
