using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

namespace HierarchyContext.Editor
{
    static class ContextSearchProvider
    {
        [SearchItemProvider]
        internal static SearchProvider CreateProvider()
        {
            return new SearchProvider("contextsearch", "contextsearch")
            {
                filterId = "c:",
                priority = 99999, // put example provider at a low priority
                showDetailsOptions = ShowDetailsOptions.Description | ShowDetailsOptions.Preview,
                onDisable = OnDisable,
                trackSelection = (item, context) => PingItem(item),
                toObject = ToObject,
#if UNITY_2022_2_OR_NEWER
                fetchPreview = (item, context, size, options) =>
                {
                    var obj = item.data as GameObject;
                    if (obj == null)
                        return item.thumbnail;
                    return SearchUtils.GetSceneObjectPreview(obj, size, options, item.thumbnail);
                },
#endif
                fetchItems = (context, items, provider) => FetchItems(context, provider)
            };
        }

        private static UnityEngine.Object PingItem(SearchItem item)
        {
            var obj = item.data as GameObject;
            if (obj == null)
                return null;
            EditorGUIUtility.PingObject(obj);
            return obj;
        }

        static SearchProvider sceneProvider = null;
        static Texture2D lightTexture = EditorGUIUtility.FindTexture("Lighting");

        private static IEnumerable<SearchItem> FetchItems(SearchContext searchContext, SearchProvider provider)
        {
            if (sceneProvider == null)
                sceneProvider = SearchService.GetProvider("scene");

            if (string.IsNullOrEmpty(searchContext.searchQuery) || searchContext.searchText.StartsWith("c:") == false || sceneProvider == null)
                yield break;

            var regex = new Regex(searchContext.searchQuery, RegexOptions.IgnoreCase);

            using (var innerContext = SearchService.CreateContext(sceneProvider, $"h:t:{nameof(ContextNode)}"))
            using (var results = SearchService.Request(innerContext))//$"h:t:{nameof(ContextNode3)}"))
            {
                foreach (var r in results)
                {
                    if (r == null)
                    {
                        yield return null;
                        continue;
                    }

                    var go = EditorUtility.InstanceIDToObject(int.Parse(r.id)) as GameObject;

                    if (go == null)
                        yield return null;

                    var contextNode = go.GetComponent<ContextNode>();
                    if (contextNode != null)
                    {

                        var match = regex.Match(contextNode.Context);
                        if (match.Success)
                        {

                            var item = provider.CreateItem(searchContext, r.id, contextNode.Context.CompareTo(searchContext.searchQuery),
                                r.GetLabel(innerContext, true),
                                HighlightDescription(contextNode.Context, match),
                                null, contextNode.gameObject);
                            //item.options = SearchItemOptions.
                            //item.options ^= SearchItemOptions.
                            yield return item;
                        }
                        //yield return provider.CreateItem(context, r.id,
                        //    r.GetLabel(innerContext, true), r.GetDescription(innerContext, true),
                        //    lightTexture, null);
                    }
                    else
                    {
                        // ***IMPORTANT***: Make sure to yield so you do not block the main thread waiting for results.
                        yield return null;
                    }
                }
            }
        }

        private static string HighlightDescription(string context, Match match)
        {
            Group group = match.Groups[0];

            var result = "<color=lime>" + group.Value + "</color>";
            if (group.Index > 0)
                result = context.Substring(0, match.Index) + result;

            if (group.Index + group.Length < context.Length)
                result = result + context.Substring(group.Index + group.Length);

            return result;
        }


        private static String WildCardToRegular(String value)
        {
            return Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*");
        }

        private static void OnDisable()
        {
            sceneProvider = null;
        }

        private static UnityEngine.Object ToObject(SearchItem item, Type type)
        {
            if (!(EditorUtility.InstanceIDToObject(int.Parse(item.id)) is GameObject gameObject))
                return null;

            return gameObject.GetComponent<ContextNode>();
        }

        private static bool IsFocusedWindowTypeName(string focusWindowName)
        {
            return EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType().ToString().EndsWith("." + focusWindowName);
        }
    }
}