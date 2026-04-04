using AchEngine.Assets;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Assets.Samples.RemoteContent
{
    /// <summary>
    /// DownloadProgress 값을 간단한 uGUI 텍스트와 슬라이더에 바인딩하는 도우미 컴포넌트입니다.
    /// </summary>
    public class DownloadProgressUI : MonoBehaviour
    {
        [Header("UI")]
        public Slider progressSlider;
        public Text percentText;
        public Text bytesText;
        public Text stateText;

        public void ResetView(string stateMessage = "대기 중")
        {
            if (progressSlider != null)
                progressSlider.value = 0f;

            if (percentText != null)
                percentText.text = "0%";

            if (bytesText != null)
                bytesText.text = "0 B / 0 B";

            if (stateText != null)
                stateText.text = stateMessage;
        }

        public void Apply(DownloadProgress progress)
        {
            if (progressSlider != null)
                progressSlider.value = progress.Percent;

            if (percentText != null)
                percentText.text = $"{progress.Percent * 100f:F0}%";

            if (bytesText != null)
            {
                bytesText.text =
                    $"{FormatBytes(progress.DownloadedBytes)} / {FormatBytes(progress.TotalBytes)}";
            }

            if (stateText != null)
                stateText.text = ConvertStatus(progress.Status);
        }

        public void ShowMessage(string message)
        {
            if (stateText != null)
                stateText.text = message;
        }

        public static string FormatBytes(long bytes)
        {
            const float kilo = 1024f;
            const float mega = kilo * 1024f;
            const float giga = mega * 1024f;

            if (bytes >= giga)
                return $"{bytes / giga:F2} GB";

            if (bytes >= mega)
                return $"{bytes / mega:F2} MB";

            if (bytes >= kilo)
                return $"{bytes / kilo:F2} KB";

            return $"{bytes} B";
        }

        private static string ConvertStatus(DownloadStatus status)
        {
            return status switch
            {
                DownloadStatus.NotStarted => "준비 전",
                DownloadStatus.Downloading => "다운로드 중",
                DownloadStatus.Complete => "완료",
                DownloadStatus.Failed => "실패",
                _ => status.ToString()
            };
        }
    }
}
