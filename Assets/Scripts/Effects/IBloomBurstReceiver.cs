namespace UnityJam.Effects
{
    /// <summary>
    /// BloomBurstController を外部から注入してもらう対象のインターフェース。
    /// TreasureSpawner などからの自動注入に使用する。
    /// </summary>
    public interface IBloomBurstReceiver
    {
        void SetBloomBurst(BloomBurstController bloomBurst);
    }
}
