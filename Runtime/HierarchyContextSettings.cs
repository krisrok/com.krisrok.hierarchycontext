using System.Linq;
using System.Text.RegularExpressions;
using OdinAddons;
using SerializableSettings;
using Sirenix.OdinInspector;
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

        [PropertySpace]
        [SerializeField]
        [ValidateInput(nameof(IsValidFragment))]
        [Resettable]
        private string _defaultFragment = "00";
        public string DefaultFragment => _defaultFragment;

        [SerializeField]
        [Resettable]
        private bool _useDefaultFragmentFormat = false;
        public bool UseDefaultFragmentFormat => _useDefaultFragmentFormat;

        [SerializeField]
        [ShowIf(nameof(_useDefaultFragmentFormat))]
        [ValidateInput(nameof(IsValidFragmentFormat))]
        [Resettable]
        private string _defaultFragmentFormat = "{name}";
        public string DefaultFragmentFormat => _defaultFragmentFormat;

        public Regex SingleFragmentRegex
        {
            get
            {
                return new Regex($"^{_fragmentRegexPattern}$");
            }
        }

        public bool IsValidFragmentFormat(string fragmentFormat)
        {
            var b = FragmentFormatter.SimpleFormat(fragmentFormat, _defaultFragment);
            return IsValidFragment(b); 
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
