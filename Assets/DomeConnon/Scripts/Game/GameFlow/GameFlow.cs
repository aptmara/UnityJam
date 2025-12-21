using UnityEngine;
using UnityEngine.InputSystem;

namespace DomeCannon
{
    /// <summary>
    /// ゲーム全体の状態遷移を管理するクラス
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        public enum State
        {
            Ready,   // Spaceで開始待ち
            Playing, // ゲーム中
            Failed,  // 内部状態（当たった）
            Result   // GameOver表示（Spaceで再開）
        }

        [SerializeField]
        private State currentState = State.Ready;

        /// <summary>
        /// 現在のゲーム状態
        /// </summary>
        public State CurrentState => currentState;

        private void Start()
        {
            // Step 1: 起動直後は待機
            ChangeState(State.Ready);
        }

        private void Update()
        {
            // Step 1: Space入力（最小実装。後でInputActions化する）
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (currentState == State.Ready || currentState == State.Result)
                {
                    BeginGame();
                }
            }
        }

        /// <summary>
        /// ゲーム状態を変更する
        /// </summary>
        public void ChangeState(State nextState)
        {
            if (currentState == nextState)
            {
                return;
            }

            currentState = nextState;

            switch (currentState)
            {
                case State.Ready:
                    OnEnterReady();
                    break;
                case State.Playing:
                    OnEnterPlaying();
                    break;
                case State.Failed:
                    OnEnterFailed();
                    break;
                case State.Result:
                    OnEnterResult();
                    break;
            }
        }

        /// <summary>
        /// Space開始用：必要な初期化を行ってからPlayingへ遷移する
        /// </summary>
        private void BeginGame()
        {
            // Step 1: 将来ここで初期化を集約
            // ・タイマー初期化
            // ・Player位置/向きリセット
            // ・弾の全回収（プールへ戻す）
            // ・UI初期化

            ChangeState(State.Playing);
        }

        private void OnEnterReady()
        {
            // Step 1: UIで "Press Space to Start" を出す（次ステップで実装）
            Debug.Log("[GameFlow] Ready: Press Space to Start");
        }

        private void OnEnterPlaying()
        {
            Debug.Log("[GameFlow] Playing");
        }

        private void OnEnterFailed()
        {
            // Step 1: GameOverへ
            ChangeState(State.Result);
        }

        private void OnEnterResult()
        {
            Debug.Log("[GameFlow] Result: Game Over. Press Space to Retry");
        }
    }
}
