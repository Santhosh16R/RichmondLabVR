public abstract class BaseTask
{
    public bool IsCompleted { get; protected set; }

    public string instruction;

    public int scoreReward = 10;

    public virtual void StartTask()
    {
        IsCompleted = false;

        if (InstructionManager.Instance != null)
            InstructionManager.Instance.ShowInstruction(instruction);
    }

    public abstract void UpdateTask();

    public virtual void CompleteTask()
    {
        IsCompleted = true;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddScore(scoreReward);
    }
}