using UnityEngine;

namespace UnityJam.Player
{
    /// <summary>
    /// Find常用を避けるための、プレイヤー参照レジストリ。
    /// </summary>
    public static class PlayerRegistry
    {
        public static UnityJam.Player.PlayerDeathHandler DeathHandler { get; private set; }

        public static void Register(UnityJam.Player.PlayerDeathHandler handler)
        {
            DeathHandler = handler;
        }

        public static void Unregister(UnityJam.Player.PlayerDeathHandler handler)
        {
            if (DeathHandler == handler)
            {
                DeathHandler = null;
            }
        }
    }
}
