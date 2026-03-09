using UnityEngine;

public abstract class TaskData : ScriptableObject
{
    public string taskName;

    [TextArea]
    public string instruction;

    public abstract BaseTask CreateRuntimeTask();
}