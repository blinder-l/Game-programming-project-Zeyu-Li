public class RoundManager
{
    public int targetScore;
    public int currentScore;
    public int handsRemaining;
    public int discardsRemaining;

    public bool HasPassedBlind => currentScore >= targetScore;
    public bool HasFailedBlind => handsRemaining <= 0 && !HasPassedBlind;

    public RoundManager(int targetScore = 300, int startingHands = 4, int startingDiscards = 3)
    {
        this.targetScore = targetScore;
        currentScore = 0;
        handsRemaining = startingHands;
        discardsRemaining = startingDiscards;
    }

    public void ApplyPlayedHandScore(int score)
    {
        if (HasPassedBlind || HasFailedBlind)
        {
            return;
        }

        currentScore += score;
        handsRemaining--;
    }

    public void UseDiscard()
    {
        if (HasPassedBlind || HasFailedBlind || discardsRemaining <= 0)
        {
            return;
        }

        discardsRemaining--;
    }

    public string GetDebugStatus()
    {
        return $"Score: {currentScore}/{targetScore} | Hands: {handsRemaining} | Discards: {discardsRemaining} | Passed: {HasPassedBlind} | Failed: {HasFailedBlind}";
    }
}
