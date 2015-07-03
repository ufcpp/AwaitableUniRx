using SampleModels;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class SampleBehaviour : MonoBehaviour
{
    [SerializeField]
    private Text textOutput;

    IDisposable _disposable;

    // Use this for initialization
    void Start()
    {
        var scheduler = SchedulerUnity.MainThread;

        textOutput.text = "start...";

        var c = new Class1();
        var s = new Subject<string>();

        _disposable = s.ObserveOn(scheduler).Subscribe(
            res => textOutput.text = res.Substring(0, 100),
            ex => textOutput.text = ex.Message
            );

        c.RunAsync(s);
    }

    void OnDestroy()
    {
        _disposable.Dispose();
    }
}
