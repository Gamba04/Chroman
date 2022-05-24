using System;
using System.Collections.Generic;
using UnityEngine;

public class ChildsReference : MonoBehaviour
{
    [Serializable]
    private class Child
    {
        [SerializeField, HideInInspector] private string ogName;
        public string name;
        public Transform child;

        public void SetName(int index)
        {
            ogName = name != "" ? name : $"Child {index}";
        }
    }

    [SerializeField]
    private List<Child> childs;

    public Transform GetChild(string name)
    {
        Child parent = childs.Find(p => p.name == name);

        return parent != null ? parent.child : null;
    }

    public Transform GetChild(int index)
    {
        if (index >= childs.Count) return null;

        return childs[index].child;
    }

    #region Editor

#if UNITY_EDITOR

    private void OnValidate()
    {
        for (int i = 0; i < childs.Count; i++)
        {
            childs[i].SetName(i);
        }
    }

#endif

    #endregion

}