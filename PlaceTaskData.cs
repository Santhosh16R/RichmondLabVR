using UnityEngine;

[CreateAssetMenu(menuName = "VRLab/Tasks/Place")]
public class PlaceTaskData : TaskData
{
    public string objectID;

    public override BaseTask CreateRuntimeTask()
    {
        PlaceTask task = new PlaceTask(objectID);
        task.instruction = instruction;
        return task;
    }
}