using UnityEngine;

[CreateAssetMenu(menuName = "VRLab/Tasks/Grab")]
public class GrabTaskData : TaskData
{
    public string objectID;

    public override BaseTask CreateRuntimeTask()
    {
        GrabTask task = new GrabTask(objectID);
        task.instruction = instruction;
        return task;
    }
}