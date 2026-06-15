public class RunManager
{
    private const int StartingTargetScore = 300;
    private const int TargetScoreIncreasePerBlind = 200;
    private const int BlindsPerAnte = 3;
    private const int NormalBlindReward = 2;
    private const int BossBlindReward = 5;

    public int CurrentBlindNumber { get; private set; } = 1;

    public int GetCurrentTargetScore()
    {
        return StartingTargetScore + ((CurrentBlindNumber - 1) * TargetScoreIncreasePerBlind);
    }

    public int GetAnteNumber()
    {
        return ((CurrentBlindNumber - 1) / BlindsPerAnte) + 1;
    }

    public int GetBlindInAnte()
    {
        return GetBlindInAnte(CurrentBlindNumber);
    }

    public int GetBlindInAnte(int blindNumber)
    {
        return ((blindNumber - 1) % BlindsPerAnte) + 1;
    }

    public string GetBlindDisplayName()
    {
        switch (GetBlindInAnte())
        {
            case 1:
                return "Small Blind";
            case 2:
                return "Big Blind";
            default:
                return "Boss Blind";
        }
    }

    public bool IsBossBlind()
    {
        return IsBossBlind(CurrentBlindNumber);
    }

    public bool IsBossBlind(int blindNumber)
    {
        return GetBlindInAnte(blindNumber) == BlindsPerAnte;
    }

    public int GetFixedBlindReward()
    {
        return IsBossBlind() ? BossBlindReward : NormalBlindReward;
    }

    public void AdvanceToNextBlind()
    {
        CurrentBlindNumber++;
    }

    public string GetDebugStatus()
    {
        return $"Ante: {GetAnteNumber()} | Blind: {CurrentBlindNumber} ({GetBlindDisplayName()}) | Target Score: {GetCurrentTargetScore()} | Fixed Reward: {GetFixedBlindReward()}";
    }
}
