using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer : MonoBehaviour
{
    [Serializable]
    private class Request
    {
        [SerializeField, HideInInspector] private string name;

        public float timer;
        public Action action;
        public bool unscaled;

        public bool abort;
        public Action cancelAction;

        public Request(float timer, Action action, bool unscaled)
        {
            this.timer = timer;
            this.action = action;
            this.unscaled = unscaled;
        }

        public void SetTimer(float value)
        {
            timer = value;
        }

        public void Cancel()
        {
            abort = true;
            cancelAction -= Cancel;
        }

        public void SetName(string name)
        {
            this.name = name;
        }
    }

    [SerializeField]
    private List<Request> requests = new List<Request>();

    #region Singleton

    private static Timer instance = null;
    public static Timer Instance
    {
        get
        {
            if (instance == null)
            {
                Timer sceneResult = FindObjectOfType<Timer>();
                if (sceneResult != null)
                {
                    instance = sceneResult;
                }
                else
                {
                    instance = new GameObject($"{instance.GetType().ToString()}_Instance", typeof(Timer)).GetComponent<Timer>();
                }
            }

            return instance;
        }
    }

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }

    #endregion

    private void Update()
    {
        TimerManagement();
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    private void TimerManagement()
    {
        for (int i = 0; i < requests.Count; i++)
        {
            if (!requests[i].abort)
            {
                float timer = requests[i].timer;

                if (requests[i].unscaled)
                {
                    ReduceCooldownUnscaled(ref timer, requests[i].action);
                }
                else
                {
                    ReduceCooldown(ref timer, requests[i].action);
                }

                requests[i].SetTimer(timer);
            }
        }

        for (int i = requests.Count - 1; i >= 0; i--)
        {
            if (requests[i].timer <= 0 || requests[i].abort)
            {
                requests.RemoveAt(i);
            }
        }
    }

    // ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------

    #region Static Methods

    /// <summary> Call a Method after a period of time. </summary>
    public static void CallOnDelay(Action action, float delay, string optionalName = "")
    {
        if (Instance.requests == null)
        {
            Instance.requests = new List<Request>();
        }

        Request request = new Request(delay, action, false);
        request.SetName(optionalName);

        Instance.requests.Add(request);
    }

    /// <summary> Call a Method after a period of time, with a cancellation Action. </summary>
    public static void CallOnDelay(Action action, float delay, ref Action cancelAction, string optionalName = "")
    {
        if (Instance.requests == null)
        {
            Instance.requests = new List<Request>();
        }

        Request request = new Request(delay, action, false);
        request.SetName(optionalName);
        cancelAction += request.Cancel;
        request.cancelAction = cancelAction;

        Instance.requests.Add(request);
    }

    /// <summary> Call a Method after a period of unscaled time. </summary>
    public static void CallOnDelayUnscaled(Action action, float delay, string optionalName = "")
    {
        if (Instance.requests == null)
        {
            Instance.requests = new List<Request>();
        }

        Request request = new Request(delay, action, true);
        request.SetName(optionalName);

        Instance.requests.Add(request);
    }

    /// <summary> Call a Method after a period of unscaled time, with a cancellation Action. </summary>
    public static void CallOnDelayUnscaled(Action action, float delay, Action cancelAction, string optionalName = "")
    {
        if (Instance.requests == null)
        {
            Instance.requests = new List<Request>();
        }

        Request request = new Request(delay, action, true);
        request.SetName(optionalName);
        cancelAction += request.Cancel;
        request.cancelAction = cancelAction;

        Instance.requests.Add(request);
    }

    /// <summary> Reduce a variable over time and call a Method or piece of code if reaches 0. </summary>
    public static void ReduceCooldown(ref float value, Action endingAction = null)
    {
        if (value > 0)
        {
            value -= Time.deltaTime;
            if (value <= 0)
            {
                value = 0;

                if (endingAction != null)
                {
                    // Check if action caller is a non-existing Unity Object (anything else or static gets called)
                    UnityEngine.Object unityObj = endingAction.Target as UnityEngine.Object;
                    bool targetExists = (!endingAction.Method.IsStatic && endingAction.Target.GetType().IsSubclassOf(typeof(UnityEngine.Object)))? unityObj: true;

                    //bool objExists = unityObj;

                    //if (!endingAction.Method.IsStatic)
                    //{
                    //    print($"Target: {endingAction.Target}, Target exists: {targetExists}, UnityObj: {objExists}");
                    //}
                    //else
                    //{
                    //    print("is static");
                    //}

                    if (targetExists)
                    {
                        endingAction();
                    }
                }
            }
        }
        else if (value < 0)
        {
            value = 0;
        }
    }

    /// <summary> Reduce a variable over unscaled time and call a Method or piece of code if reaches 0. </summary>
    public static void ReduceCooldownUnscaled(ref float value, Action endingAction = null)
    {
        if (value > 0)
        {
            value -= Time.unscaledDeltaTime;
            if (value <= 0)
            {
                value = 0;

                if (endingAction != null)
                {
                    // Check if action caller is a non-existing Unity Object (anything else or static gets called)
                    UnityEngine.Object unityObj = endingAction.Target as UnityEngine.Object;
                    bool targetExists = (!endingAction.Method.IsStatic && endingAction.Target.GetType().IsSubclassOf(typeof(UnityEngine.Object))) ? unityObj : true;

                    if (targetExists)
                    {
                        endingAction();
                    }
                }
            }
        }
        else if (value < 0)
        {
            value = 0;
        }
    }

    #endregion

}
