public class GrabTask : BaseTask
{
    string targetObject;

    public GrabTask(string id)
    {
        targetObject = id;
    }

    public override void StartTask()
    {
        base.StartTask();

        HighlightManager.Instance.Highlight(targetObject);

        EventBus.OnObjectGrabbed += OnGrab;
    }

    void OnGrab(string id)
    {
        if (id == targetObject)
        {
            HighlightManager.Instance.Disable(targetObject);

            CompleteTask();
            EventBus.OnObjectGrabbed -= OnGrab;
        }
    }

    public override void UpdateTask() { }
}