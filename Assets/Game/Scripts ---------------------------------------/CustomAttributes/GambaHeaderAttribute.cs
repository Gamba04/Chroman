using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = true)]
public class GambaHeaderAttribute : MultiPropertyAttribute
{
    public string text;
    public Color color;

    private int lines = 1;

    public GambaHeaderAttribute(string text, float a = 1)
    {
        this.text = text;
        color = new Color(0, 0, 0, a);
    }
    public GambaHeaderAttribute(string text, float r, float g, float b, float a = 1)
    {
        this.text = text;
        color = new Color(r, g, b, a);
    }

#if UNITY_EDITOR
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label, int attributeCount)
    {
        position = Reorder(position);

        GUIStyle style = new GUIStyle();
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = color;

        GUI.Label(new Rect(position.x + 2, position.y, position.width, position.height), text, style);

        if (attributeCount == 1)
        {
            EditorGUI.PropertyField(new Rect(position.x, position.y + 17, position.width, position.height - 17), property);
            lines = 2;  
        }
        else
        {
            lines = 1;
        }
    }

    public override float GetHeight()
    {
        return lines * 17;
    }
#endif
}


// OLD

/*

[CustomPropertyDrawer(typeof(SubHeaderAttribute))]
public class SubHeaderPropertyDrawer : MultiPropertyDrawer
{
public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
{
    return base.GetPropertyHeight(property, label) + 17;
}

public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
{
    SubHeaderAttribute subHeader = (SubHeaderAttribute)attribute;

    GUIStyle style = new GUIStyle();
    style.fontStyle = FontStyle.Bold;
    style.normal.textColor = subHeader.color;

    GUI.Label(new Rect(position.x + 2, position.y, position.width, position.height), subHeader.text, style);


    //EditorGUI.PropertyField(new Rect(position.x, position.y + 17, position.width, position.height - 17), property);

}
}*/



