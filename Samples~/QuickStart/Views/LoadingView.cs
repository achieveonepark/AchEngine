using System.Collections;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples
{
    public class LoadingView : UIView
    {
        public struct Payload
        {
            public string message;
            public float duration; // 0 means manual close.
        }

        [SerializeField] private Text messageText;
        [SerializeField] private Slider progressBar;
        [SerializeField] private Text percentText;

        private Coroutine autoCloseCoroutine;

        protected override void OnOpened(object payload)
        {
            if (payload is Payload loadingPayload)
            {
                if (messageText != null)
                {
                    messageText.text = loadingPayload.message;
                }

                SetProgress(0f);

                if (loadingPayload.duration > 0f)
                {
                    if (autoCloseCoroutine != null)
                    {
                        StopCoroutine(autoCloseCoroutine);
                    }

                    autoCloseCoroutine = StartCoroutine(AutoProgress(loadingPayload.duration));
                }
            }
            else
            {
                if (messageText != null)
                {
                    messageText.text = "Loading...";
                }

                SetProgress(0f);
            }
        }

        protected override void OnClosed()
        {
            if (autoCloseCoroutine != null)
            {
                StopCoroutine(autoCloseCoroutine);
                autoCloseCoroutine = null;
            }
        }

        public void SetProgress(float value)
        {
            value = Mathf.Clamp01(value);
            if (progressBar != null) progressBar.value = value;
            if (percentText != null) percentText.text = $"{(int)(value * 100)}%";
        }

        private IEnumerator AutoProgress(float duration)
        {
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetProgress(elapsed / duration);
                yield return null;
            }

            SetProgress(1f);
            CloseSelf();
        }
    }
}
