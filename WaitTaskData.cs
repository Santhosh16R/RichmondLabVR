using UnityEngine;

[CreateAssetMenu(menuName = "VRLab/Tasks/Wait")]
public class WaitTaskData : TaskData
{
    public float waitTime;

    public override BaseTask CreateRuntimeTask()
    {
        WaitTask task = new WaitTask(waitTime);
        task.instruction = instruction;
        return task;
    }
}