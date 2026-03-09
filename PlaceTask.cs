public class PlaceTask : BaseTask
{
    string targetObject;

    public PlaceTask(string id)
    {
        targetObject = id;
    }

    public override void StartTask()
    {
        base.StartTask();
        EventBus.OnObjectPlaced += OnPlaced;
    }

    void OnPlaced(string id)
    {
        if (id == targetObject)
        {
            CompleteTask();
            EventBus.OnObjectPlaced -= OnPlaced;
        }
    }

    public override void UpdateTask() { }
}