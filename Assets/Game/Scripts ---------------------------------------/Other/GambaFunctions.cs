using System;
using System.Collections.Generic;
using UnityEngine;

public class GambaFunctions : MonoBehaviour
{
    #region Math

    public static float GetAngle(Vector2 point)
    {
        float pi = Mathf.PI;

        float r = 0;
        float x = point.x;
        float y = point.y;

        if (x > 0)
        {
            if (y > 0) // Cuadrant: 1
            {
                r = Mathf.Atan(y/x);
            }
            else if (y < 0) // Cuadrant: 4
            {
                r = pi * 3/ 2f + (pi *  3/ 2f - (pi - Mathf.Atan(y / x)));
            }
            else // Right
            {
                r = 0;
            }
        }
        else if (x < 0)
        {
            if (y > 0) // Cuadrant: 2
            {
                r = pi * 1/ 2f + (pi * 1/ 2f + Mathf.Atan(y / x));

            }
            else if (y < 0) // Cuadrant: 3
            {
                r = pi + Mathf.Atan(y / x);

            }
            else // Left
            {
                r = pi; 
            }
        }
        else 
        {
            if (y > 0) // Up
            {
                r = pi * 1/ 2f; 
            }
            else if (y < 0) // Down
            {
                r = pi * 3/ 2f;
            }
            else // Zero
            {
                r = 0;
            }
        }

        return r;
    }

    public static Vector2 AngleToVector(float angle)
    {
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    public static Vector2 Bounce(Vector2 normal, Vector2 direction)
    {
        normal.Normalize();
        direction.Normalize();

        float dotNormalDirection = Vector2.Dot(direction.normalized, normal);

        Vector2 dashYComponent = normal * Mathf.Abs(dotNormalDirection);

        Vector2 dashXComponent;

        if (dotNormalDirection > 0)
        {
            dashXComponent = direction - dashYComponent;
        }
        else
        {
            dashXComponent = direction + dashYComponent;
        }

        return (dashXComponent + dashYComponent);
    }

    public static Vector2 Perpendicular(Vector2 direction)
    {
        return new Vector2(direction.y, -direction.x);
    }

    #endregion

    #region Gizmos

    public static void GizmosDrawArrow(Vector2 origin, Vector2 direction)
    {
        Vector2 head = origin + direction;
        Gizmos.DrawLine(origin, head);

        Vector2 perpendicular = new Vector2(direction.y, -direction.x);
        Gizmos.DrawLine(head, head + perpendicular * 0.1f - direction * 0.2f);
        Gizmos.DrawLine(head, head - perpendicular * 0.1f - direction * 0.2f);
    }

    public static void GizmosDrawArrow(Vector2 origin, Vector2 direction, Vector2 headSize)
    {
        Vector2 head = origin + direction;
        Gizmos.DrawLine(origin, head);

        Vector2 perpendicular = new Vector2(direction.y, -direction.x);
        Gizmos.DrawLine(head, head + perpendicular.normalized * headSize/2f - direction.normalized * headSize);
        Gizmos.DrawLine(head, head - perpendicular.normalized * headSize/2f - direction.normalized * headSize);
    }

    public static void GizmosDrawPointedLine(Vector2 from, Vector2 to, float separation, int maxIters = 500)
    {
        float distance = (to - from).magnitude;
        float distanceToA = 0;
        float distanceToB = 0;
        int iter = 0;
        while (distanceToB < distance && iter < maxIters)
        {
            Vector2 offset = Vector2.zero;
            Vector2 pointA = from + (to - from).normalized * (iter) * separation;
            Vector2 pointB = from + (to - from).normalized * (iter + 0.5f) * separation;

            distanceToA = (pointA - from).magnitude;
            distanceToB = (pointB - from).magnitude;

            if (distanceToA < distance)
            {
                if (distanceToB > distance)
                {
                    offset = to - pointB;
                }

                Gizmos.DrawLine(pointA, pointB + offset);
            }

            iter++;
        }
    }

    #endregion

    #region Lists

    public static void ResizeList<T>(ref List<T> list, int newLenght) where T : class, new()
    {
        if (newLenght >= 0)
        {
            List<T> newList = new List<T>();

            for (int i = 0; i < newLenght; i++)
            {
                if (i < list.Count)
                {
                    newList.Add(list[i]);
                }
                else
                {
                    newList.Add(new T());
                }
            }

            list = newList;
        }
    }

    public static void ResizeListEmpty<T>(ref List<T> list, int newLenght) where T : class
    {
        if (newLenght >= 0)
        {
            List<T> newList = new List<T>();

            for (int i = 0; i < newLenght; i++)
            {
                if (i < list.Count)
                {
                    newList.Add(list[i]);
                }
                else
                {
                    newList.Add(null);
                }
            }

            list = newList;
        }
    }

    public static void AddListElements<T>( ref List<T> list, List<T> addition)
    {
        for (int i = 0; i < addition.Count; i++)
        {
            if (addition[i] != null)
            {
                list.Add(addition[i]);
            }
        }
    }

    /// <summary> QuickSort a List of any type with a custom comparison method. </summary>
    /// <param name="comparison"> Custom comparison method which defines if element1 <, <=, ==, !=, >= or > element2. Returns an int which corresponds to an equivalent comparison with 0. </param>
    public static List<T> QuickSort<T>(List<T> elements, Comparison<T> comparison)
    {
        if (elements.Count <= 1) return elements;

        int pivot = elements.Count - 1;

        // Create partitions
        List<T> left = new List<T>();
        List<T> right = new List<T>();

        for (int i = 0; i < elements.Count - 1; i++)
        {
            if (comparison(elements[i], elements[pivot]) <= 0)
            {
                left.Add(elements[i]);
            }
            else
            {
                right.Add(elements[i]);
            }
        }

        // Recurse
        left = QuickSort(left, comparison);
        right = QuickSort(right, comparison);

        // Join partitions
        List<T> syntesis = left;
        syntesis.Add(elements[pivot]);
        syntesis.AddRange(right);

        return syntesis;
    }

    /// <summary> QuickSort a List of any type with default IComparable.CompareTo. </summary>
    public static List<T> QuickSort<T>(List<T> elements) where T : IComparable<T>
    {
        return QuickSort(elements, (e1, e2) => e1.CompareTo(e2));
    }

    #endregion

    #region Editor

#if UNITY_EDITOR

    public static void DestroyInEditor(UnityEngine.Object @object)
    {
        UnityEditor.EditorApplication.delayCall += () =>
        {
            DestroyImmediate(@object);
        };
    }

#endif

    #endregion

}

public struct CollisionInfo
{
    public Vector2 point;
    public Vector2 normal;
    public Vector2 inputDir;
    public Vector2 outputDir;

    public void DebugCollision()
    {
        Gizmos.color = Color.yellow;
        GambaFunctions.GizmosDrawArrow(point - inputDir, inputDir);

        Gizmos.color = Color.green;
        GambaFunctions.GizmosDrawArrow(point, outputDir);

        Gizmos.color = Color.white;
        GambaFunctions.GizmosDrawPointedLine(point, point + normal, 0.1f);
    }

    public void DebugCollision(Color inColor, Color outColor, Color normalColor)
    {
        Gizmos.color = inColor;
        GambaFunctions.GizmosDrawArrow(point - inputDir, inputDir);

        Gizmos.color = outColor;
        GambaFunctions.GizmosDrawArrow(point, outputDir);

        Gizmos.color = normalColor;
        GambaFunctions.GizmosDrawPointedLine(point, point + normal, 0.1f);
    }
}