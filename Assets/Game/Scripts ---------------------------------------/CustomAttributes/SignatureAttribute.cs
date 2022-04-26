using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
public class SignatureAttribute : MultiPropertyAttribute
{
#if UNITY_EDITOR 
    public override float GetHeight()
    {
        return 100;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, int attributeCount)
    {

        Rect lastRect = GUILayoutUtility.GetLastRect();

        Texture2D scrollbar = Resources.Load("prince scrollbar", typeof(Texture2D)) as Texture2D;

        if (scrollbar != null)
        {
            GUILayout.MinWidth(scrollbar.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + 10, scrollbar.width / 3, scrollbar.height / 3), scrollbar, new Rect(0, 0, 1, 1));
        }

        EditorGUI.LabelField(position, new GUIContent("", "Made by Gamba04"));
    }
#endif
}
/*
[CustomPropertyDrawer(typeof(SignatureAttribute))]
public class SignatureCustomDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return 100;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Rect lastRect = GUILayoutUtility.GetLastRect();

        Texture2D scrollbar = Resources.Load("prince scrollbar", typeof(Texture2D)) as Texture2D;

        if (scrollbar != null)
        {
            GUILayout.MinWidth(scrollbar.width);
            GUI.DrawTextureWithTexCoords(new Rect(position.x, position.y + 10, scrollbar.width/3, scrollbar.height/3), scrollbar, new Rect(0, 0, 1, 1));
        }

        EditorGUI.LabelField(position, new GUIContent("", "Made by Gamba04"));

    }
}
*/

