using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.Game
{
    /// <summary>
    /// 카드를 드랍할 슬롯에 붙이세요.
    /// ObjectTouchManager가 씬에 있어야 OnTouched()가 호출됩니다.
    /// </summary>
    public class CardSlot : TouchableObject
    {
        [SerializeField] private Text lblExpected;

        public int  ExpectedValue { get; private set; }
        public bool IsOccupied    { get; private set; }

        public void Setup(int expectedValue)
        {
            ExpectedValue = expectedValue;
            IsOccupied    = false;

            if (lblExpected != null)
                lblExpected.text = "?";
        }

        public void Occupy()
        {
            IsOccupied = true;
            if (lblExpected != null)
                lblExpected.text = ExpectedValue.ToString();
        }

        protected override void OnTouched()
        {
            if (!IsOccupied)
                Debug.Log($"[CardSlot] 슬롯 {ExpectedValue} 선택됨");
        }
    }
}
