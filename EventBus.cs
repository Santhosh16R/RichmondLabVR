using System;

public static class EventBus
{
    public static Action<string> OnObjectGrabbed;
    public static Action<string> OnObjectPlaced;

    public static Action<string, string> OnObjectsTouched;
}