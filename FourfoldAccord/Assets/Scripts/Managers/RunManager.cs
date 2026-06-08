public class RunManager
{
    private const int StartingTargetScore = 300;
    private const int TargetScoreIncreasePerBlind = 200;

    public int CurrentBlindNumber { get; private set; } = 1;

    public int GetCurrentTargetScore()
    {
        return StartingTargetScore + ((CurrentBlindNumber - 1) * TargetScoreIncreasePerBlind);
    }

    public void AdvanceToNextBlind()
    {
        CurrentBlindNumber++;
    }

    public string GetDebugStatus()
    {
        return $"Blind: {CurrentBlindNumber} | Target Score: {GetCurrentTargetScore()}";
    }
}
