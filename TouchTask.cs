public class TouchTask : BaseTask
{
    string a;
    string b;

    public TouchTask(string A, string B)
    {
        a = A;
        b = B;
    }

    public override void StartTask()
    {
        base.StartTask();

        HighlightManager.Instance.Highlight(a);
        HighlightManager.Instance.Highlight(b);

        EventBus.OnObjectsTouched += OnTouch;
    }

    void OnTouch(string objA, string objB)
    {
        if (objA == a && objB == b)
        {
            HighlightManager.Instance.Disable(a);
            HighlightManager.Instance.Disable(b);

            CompleteTask();
            EventBus.OnObjectsTouched -= OnTouch;
        }
    }

    public override void UpdateTask() { }
}