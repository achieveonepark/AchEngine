using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.Game
{
    /// <summary>
    /// 카드 프리팹 루트에 붙이세요. Draggable을 상속합니다.
    /// CardSlot 위에 드랍하면 값 매칭 여부를 GameplayManager에 전달합니다.
    /// </summary>
    public class DraggableCard : Draggable
    {
        [SerializeField] private Text lblValue;

        public int CardValue { get; private set; }

        private bool _isUsed;

        protected override void Start()
        {
            base.Start();

            OnTouchDown += () => transform.SetAsLastSibling();

            OnTouchUp += hits =>
            {
                if (_isUsed) return;

                CardSlot matched = null;
                foreach (var hit in hits)
                {
                    if (hit.TryGetComponent<CardSlot>(out var slot) && !slot.IsOccupied)
                    {
                        matched = slot;
                        break;
                    }
                }

                if (matched != null)
                {
                    bool correct = matched.ExpectedValue == CardValue;
                    GameplayManager.Instance.OnCardDropped(this, matched, correct);
                }
                else
                {
                    transform.position = originalPos;
                }
            };
        }

        public void Setup(int value)
        {
            CardValue = value;
            _isUsed   = false;
            if (lblValue != null)
                lblValue.text = value.ToString();
        }

        public void MarkUsed()
        {
            _isUsed = true;
            ServiceLocator.Get<PoolManager>().Release(GameplayManager.CardPoolKey, gameObject);
        }
    }
}
