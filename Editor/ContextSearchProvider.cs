using Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Search;
using UnityEngine;

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
            fetchPreview = (item, context, size, options) =>
            {
                var obj = item.data as GameObject;
                if (obj == null)
                    return item.thumbnail;
                return SearchUtils.GetSceneObjectPreview(obj, size, options, item.thumbnail);
            },
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

    private static IEnumerable<SearchItem> FetchItems(SearchContext context, SearchProvider provider)
    {
;        if(sceneProvider == null)
            sceneProvider = SearchService.GetProvider("scene");

        if (string.IsNullOrEmpty(context.searchQuery) || sceneProvider == null)
            yield break;

        var regex = new Regex(WildCardToRegular(context.searchQuery), RegexOptions.IgnoreCase);

        using (var innerContext = SearchService.CreateContext(sceneProvider, $"h:t:{nameof(ContextNode3)}"))
        using (var results = SearchService.Request(innerContext))//$"h:t:{nameof(ContextNode3)}"))
        {
            foreach (var r in results)
            {
                var contextNode = (EditorUtility.InstanceIDToObject(int.Parse(r.id)) as GameObject)?.GetComponent<ContextNode3>();
                if (contextNode != null)
                {
                    
                    if (regex.IsMatch(contextNode.Context))
                        yield return provider.CreateItem(context, r.id, contextNode.Context.CompareTo(context.searchQuery),
                            r.GetLabel(innerContext, true), contextNode.Context,
                            null, contextNode.gameObject);
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

        return gameObject.GetComponent<ContextNode3>();
    }

    private static bool IsFocusedWindowTypeName(string focusWindowName)
    {
        return EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.GetType().ToString().EndsWith("." + focusWindowName);
    }
}