using UnityEngine;

public class WaitTask : BaseTask
{
    float duration;
    float timer;

    public WaitTask(float time)
    {
        duration = time;
    }

    public override void UpdateTask()
    {
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            CompleteTask();
        }
    }
}