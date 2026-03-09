using UnityEngine;

[CreateAssetMenu(menuName = "VRLab/Tasks/Touch")]
public class TouchTaskData : TaskData
{
    public string objectA;
    public string objectB;

    public override BaseTask CreateRuntimeTask()
    {
        TouchTask t = new TouchTask(objectA, objectB);
        t.instruction = instruction;
        return t;
    }
}