using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "VRLab/Scenario")]
public class ScenarioData : ScriptableObject
{
    public string scenarioName;

    public List<TaskData> tasks = new List<TaskData>();
}