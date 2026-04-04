namespace AchEngine.Assets
{
    public enum DownloadStatus
    {
        NotStarted,
        Downloading,
        Complete,
        Failed
    }

    public struct DownloadProgress
    {
        public long TotalBytes { get; }
        public long DownloadedBytes { get; }
        public float Percent { get; }
        public DownloadStatus Status { get; }

        public DownloadProgress(long totalBytes, long downloadedBytes, float percent, DownloadStatus status)
        {
            TotalBytes = totalBytes;
            DownloadedBytes = downloadedBytes;
            Percent = percent;
            Status = status;
        }
    }
}
