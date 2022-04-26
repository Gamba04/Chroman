using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

[AttributeUsage(AttributeTargets.Field)]
public abstract class MultiPropertyAttribute : PropertyAttribute
{
    public List<object> stored = new List<object>();
    public virtual GUIContent BuildLabel(GUIContent label)
    {
        return label;
    }
#if UNITY_EDITOR
    public abstract void OnGUI(Rect position, SerializedProperty property, GUIContent label, int attributeCount);

    public virtual float GetHeight()
    {
        return 17;
    }

    /// <summary> USE: position = Reorder(position); </summary>
    protected Rect Reorder(Rect position)
    {
        float offset = GetHeight() * order;
        return new Rect(position.x, position.y + offset, position.width, position.height - offset);
    }
#endif

}


#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MultiPropertyAttribute), true)]
public class MultiPropertyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        MultiPropertyAttribute @Attribute = attribute as MultiPropertyAttribute;

        float height = 0;

        int newOrder = 0;
        for (int i = 0; i < @Attribute.stored.Count; i++)
        {
            object atr = @Attribute.stored[i];

            if (atr as MultiPropertyAttribute != null)
            {
                float atrHeight = ((MultiPropertyAttribute)atr).GetHeight();

                if (atrHeight != 0)
                {
                    height += atrHeight;

                    ((MultiPropertyAttribute)atr).order = newOrder++;
                }
            }
        }

        return height;
    }
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        MultiPropertyAttribute @Attribute = attribute as MultiPropertyAttribute;

        if (@Attribute.stored == null || @Attribute.stored.Count == 0)
        {
            @Attribute.stored = fieldInfo.GetCustomAttributes(typeof(MultiPropertyAttribute), false).OrderBy(s => ((PropertyAttribute)s).order).ToList();
        }

        var OrigColor = GUI.color;
        var Label = label;
        for (int i = 0; i < @Attribute.stored.Count; i++)
        {
            object atr = @Attribute.stored[i];

            if (atr as MultiPropertyAttribute != null)
            {
                Label = ((MultiPropertyAttribute)atr).BuildLabel(Label);

                ((MultiPropertyAttribute)atr).OnGUI(position, property, Label, @Attribute.stored.Count);
            }

        }

        GUI.color = OrigColor;
    }
}


#endif
