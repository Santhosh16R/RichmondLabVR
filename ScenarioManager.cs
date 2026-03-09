using UnityEngine;

public class ScenarioManager : MonoBehaviour
{
    public ScenarioData scenario;

    int currentTaskIndex;
    BaseTask currentTask;

    void Start()
    {
        StartTask();
    }

    void StartTask()
    {
        currentTask = scenario.tasks[currentTaskIndex].CreateRuntimeTask();
        currentTask.StartTask();
    }

    void Update()
    {
        if (currentTask == null) return;

        currentTask.UpdateTask();

        if (currentTask.IsCompleted)
        {
            currentTaskIndex++;

            if (currentTaskIndex >= scenario.tasks.Count)
            {
                Debug.Log("Scenario Completed");
                return;
            }

            StartTask();
        }
    }
}