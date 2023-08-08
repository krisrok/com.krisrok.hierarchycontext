using Navigation;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
class MyHierarchyIcons
{
    static Texture2D texturePanel;
    static List<int> markedObjects;

    static MyHierarchyIcons()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyItemCB;
    }

    //static Dictionary<int, bool> _hoverMap = new Dictionary<int, bool>();

    static void HierarchyItemCB(int instanceID, Rect selectionRect)
    {
        //_hoverMap[instanceID] = selectionRect.Contains(Event.current.mousePosition);

        GameObject go = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (go == null)
            return;

        var contextProvider = go.GetComponent<IContextProvider>();
        if (contextProvider == null)
            return;

        Rect r = new Rect(selectionRect);
        r.xMin += 16 + EditorStyles.largeLabel.CalcSize(new GUIContent(go.name)).x;
        
        var guiContent = new GUIContent(contextProvider.Context);
        var style = new GUIStyle(EditorStyles.miniLabel);
        if (contextProvider.IsValid)
        {
            style.normal.textColor = Color.grey;
            //if (_hoverMap.Values.Any(v => v == true))
            //    style.normal.textColor = Color.white;
        }
        else
        {
            style.normal.textColor = Color.red;
        }
        GUI.Label(r, guiContent, style);
    }
}