using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SerializableSettings;
using Sirenix.OdinInspector;

namespace Navigation
{
    [RuntimeProjectSettings("Navigation")]
    public class NavigationSettings : SerializableSettings<NavigationSettings>
    {
        [Serializable]
        public struct StringDropdownItem
        {
            public string Description;

            public string Pattern;

            public StringDropdownItem(string description, string pattern)
            {
                Description = description;
                Pattern = pattern;
            }
        }

        public string Separator = "/";

        public string FragmentRegexPattern = "[A-Za-z0-9_]+";

        [ValidateInput(nameof(IsValidFragment))]
        public string DefaultFragment = "00";

        [PropertySpace]
        public bool AutoSetGameObjectNames = true;

        [ShowIf(nameof(AutoSetGameObjectNames), true)]
        public string GameObjectNamePostfixFormat = " [{0}]";

        [ShowIf(nameof(AutoSetGameObjectNames), true)]
        public bool SetRoutePostfix = true;

        [PropertySpace]
        [OnValueChanged(nameof(UpdatePatternFormatTemplateDropdownList), false, IncludeChildren = true)]
        public List<StringDropdownItem> PatternFormatTemplates = new List<StringDropdownItem>
        {
                new StringDropdownItem("Our context only", "^{s}$"),
                new StringDropdownItem("Our context and children", "^{s}($|{d})"),
                new StringDropdownItem("Parent context only", "^{s^}$"),
                new StringDropdownItem("Parent context and children", "^{s^}($|{d})"),
                new StringDropdownItem("Our context and siblings", "^{s^}{d}{f}$"),
                new StringDropdownItem("Our context and siblings and children", "^{s^}{d}"),
                new StringDropdownItem("Our siblings only", "^{s^}{d}(?!{^s}$){f}$"),
                new StringDropdownItem("Our siblings and their children only", "^{s^}{d}(?!{^s}($|{d})){f}"),
                new StringDropdownItem("Parent context, our siblings and their children only", "^{s^}(?!{d}{^s}($|{d}))")
        };

        private List<ValueDropdownItem<string>> _patternFormatTemplatesDropdownList;

        public List<ValueDropdownItem<string>> PatternFormatTemplatesDropdownList
        {
            get
            {
                if (_patternFormatTemplatesDropdownList == null)
                {
                    UpdatePatternFormatTemplateDropdownList();
                }
                return _patternFormatTemplatesDropdownList;
            }
        }

        public Regex SingleFragmentRegex
        {
            get
            {
                return new Regex($"^{FragmentRegexPattern}$");
            }
        }

        public Regex GameObjectNamePostfixRegex
        {
            get
            {
                return new Regex(string.Format(Regex.Escape(string.Format(GameObjectNamePostfixFormat, "%%!%%")).Replace("%%!%%", "{0}"), ".*"));
            }
        }

        private void UpdatePatternFormatTemplateDropdownList()
        {
            _patternFormatTemplatesDropdownList = PatternFormatTemplates?.Select((StringDropdownItem kvp) => new ValueDropdownItem<string>(kvp.Description, kvp.Pattern)).ToList();
        }

        public bool IsValidFragment(string fragment)
        {
            return !string.IsNullOrEmpty(fragment) && SingleFragmentRegex.IsMatch(fragment);
        }

        public bool IsValidContext(string context)
        {
            if (context == null)
                return false;

            return context.Split(Separator).All((string fragment) => IsValidFragment(fragment));
        }
    }
}
