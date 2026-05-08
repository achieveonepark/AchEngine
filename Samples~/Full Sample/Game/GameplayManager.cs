using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Samples.Full.Messages;
using AchEngine.Samples.Full.UI;
using AchEngine.UI;
using UnityEngine;

namespace AchEngine.Samples.Full.Game
{
    /// <summary>
    /// 인게임 씬의 빈 GameObject에 붙이세요.
    /// MonoSingleton이므로 씬 내에 하나만 존재해야 합니다.
    /// </summary>
    public class GameplayManager : MonoSingleton<GameplayManager>
    {
        public const string CardPoolKey = "DraggableCard";

        [Header("Card Spawning")]
        [SerializeField] private Transform cardSpawnRoot;
        [SerializeField] private CardSlot[] slots;

        private int _hp;
        private int _maxHp;
        private int _score;
        private int _timeRemaining;
        private bool _gameRunning;

        public void StartGame(int maxHp, int roundSeconds)
        {
            _hp           = maxHp;
            _maxHp        = maxHp;
            _score        = 0;
            _timeRemaining = roundSeconds;
            _gameRunning  = true;

            SpawnCards();
            PublishHp();
            PublishScore();
        }

        public void OnSecondTick()
        {
            if (!_gameRunning) return;

            _timeRemaining--;

            if (UI.TryGetOpen<IngameHUDView>(out var hud))
                hud.SetTime(_timeRemaining);

            if (_timeRemaining <= 0)
                EndGame(win: false);
        }

        public void OnCardDropped(DraggableCard card, CardSlot slot, bool correct)
        {
            if (correct)
            {
                _score += 10;
                slot.Occupy();
                card.MarkUsed();
                PublishScore();
                Debug.Log($"[GameplayManager] 정답! 점수: {_score}");

                if (AllSlotsOccupied())
                    EndGame(win: true);
            }
            else
            {
                _hp--;
                card.transform.position = card.originalPos;
                PublishHp();
                Debug.Log($"[GameplayManager] 오답. 남은 HP: {_hp}");

                if (_hp <= 0)
                    EndGame(win: false);
            }
        }

        private void SpawnCards()
        {
            if (slots == null || slots.Length == 0 || cardSpawnRoot == null) return;

            int count = slots.Length;
            for (int i = 0; i < count; i++)
                slots[i].Setup(i + 1);

            var pool = ServiceLocator.Get<PoolManager>();
            for (int i = 0; i < count; i++)
            {
                var go   = pool.Get(CardPoolKey);
                if (go == null) continue;

                go.transform.SetParent(cardSpawnRoot, worldPositionStays: false);
                go.transform.localPosition = new Vector3(i * 120f - count * 60f, 0f, 0f);

                if (go.TryGetComponent<DraggableCard>(out var card))
                    card.Setup(i + 1);
            }
        }

        private async void EndGame(bool win)
        {
            _gameRunning = false;
            Debug.Log($"[GameplayManager] 게임 종료 — {(win ? "승리" : "패배")} / 점수: {_score}");

            await ServiceLocator.Get<AchSceneManager>().LoadSceneAsync("LobbyScene");
        }

        private bool AllSlotsOccupied()
        {
            if (slots == null) return false;
            foreach (var s in slots)
                if (!s.IsOccupied) return false;
            return true;
        }

        private void PublishHp()
        {
#if ACHENGINE_R3
            UIBindingManager.Publish(new HpChangedMessage { Current = _hp, Max = _maxHp });
#else
            if (UI.TryGetOpen<IngameHUDView>(out var hud))
                hud.SetTime(_timeRemaining);
#endif
        }

        private void PublishScore()
        {
#if ACHENGINE_R3
            UIBindingManager.Publish(new ScoreChangedMessage { Score = _score });
#endif
        }
    }
}
