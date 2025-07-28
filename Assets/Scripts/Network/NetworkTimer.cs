public class NetworkTimer
{
    float timer; // 
    public float MinTimeBetweenTicks { get; }
    public int CurrentTick { get; private set; }

    public NetworkTimer(float serverTimeRate)
    {
        MinTimeBetweenTicks = 1f / serverTimeRate;
    }

    /// <summary>
    /// Incriment the timer by deltaTime
    /// </summary>
    /// <param name="deltaTime"></param>
    public void Update(float deltaTime)
    {
        timer += deltaTime;
    }

    /// <summary>
    /// Calculates whether the timer has suppassed the required delay between ticks
    /// </summary>
    /// <returns>Bool depending on whether it should or should not tick</returns>
    public bool ShouldTick()
    {
        if (timer >= MinTimeBetweenTicks)
        {
            timer -= MinTimeBetweenTicks;
            CurrentTick++;
            return true;
        }

        return false;
    }
}
