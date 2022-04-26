using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
public class ReadOnlyAttribute : MultiPropertyAttribute
{
    public bool activated;

#if UNITY_EDITOR
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, int attributeCount)
    {
        position = Reorder(position);

        Color previousColor = GUI.color;

        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Italic;

        if (GUI.Button(new Rect(position.x - 15, position.y, 13, position.height), "", GUIStyle.none))
        {
            activated = !activated;
        }
        if (activated)
        {
            GUI.color = new Color(1, 0.95f, 0.9f);
        }
        else
        {
            GUI.color = previousColor;
        }
        GUI.enabled = activated;
        GUI.Box(new Rect(position.x - 15, position.y, 15, position.height), new GUIContent("", "ReadOnly made by Gamba"));
        if (activated)
        {
            GUI.Label(new Rect(position.x - 15, position.y, position.width, position.height), new GUIContent("✎"));
        }
        else
        {
            GUI.Label(new Rect(position.x - 15, position.y, position.width, position.height), new GUIContent("✐"));
        }

        EditorGUI.PropertyField(position, property, label);
        GUI.color = previousColor;
    }
#endif
}


// OLD

/*

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Color previousColor = GUI.color;

        ReadOnlyAttribute readOnly = attribute as ReadOnlyAttribute;
        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Italic;

        if (GUI.Button(new Rect(position.x - 15, position.y, 13, position.height), "", GUIStyle.none))
        {
            readOnly.activated = !readOnly.activated;
        }
        if (readOnly.activated)
        {
            GUI.color = new Color(1, 0.95f, 0.9f);
        }
        else
        {
            GUI.color = previousColor;
        }
        GUI.enabled = readOnly.activated;
        GUI.Box(new Rect(position.x - 15, position.y, 15, position.height), new GUIContent("", "ReadOnly made by Gamba"));
        if (readOnly.activated)
        {
            GUI.Label(new Rect(position.x - 15, position.y, position.width, position.height), new GUIContent("✎"));
        }
        else
        {
            GUI.Label(new Rect(position.x - 15, position.y, position.width, position.height), new GUIContent("✐"));
        }

        EditorGUI.PropertyField(position, property, label);
        GUI.color = previousColor;
    }
}
*/
