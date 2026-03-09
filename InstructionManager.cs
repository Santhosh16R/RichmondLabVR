using UnityEngine;
using TMPro;

public class InstructionManager : MonoBehaviour
{
    public static InstructionManager Instance;

    public TextMeshProUGUI instructionText;

    void Awake()
    {
        Instance = this;
    }

    public void ShowInstruction(string message)
    {
        instructionText.text = message;
    }
}