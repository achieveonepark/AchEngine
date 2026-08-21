using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace AchEngine.Editor.Table
{
    public static class GoogleSheetDownloader
    {
        public static async Task<string> DownloadCsvAsync(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
                throw new ArgumentException("유효한 HTTP 또는 HTTPS URL이 필요합니다.", nameof(url));

            using var request = UnityWebRequest.Get(uri);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Delay(100);

            if (request.result != UnityWebRequest.Result.Success)
                throw new InvalidOperationException(
                    $"다운로드 실패 ({request.responseCode}): {request.error}\nURL: {url}");

            return request.downloadHandler.text;
        }

        public static async Task DownloadAndSaveAsync(TableLoaderSettings settings, SheetInfo sheet)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (sheet == null) throw new ArgumentNullException(nameof(sheet));

            var className = sheet.GetClassName();
            TableCodeGenerator.ValidateClassName(className);
            var url = settings.GetCsvDownloadUrl(sheet);
            Debug.Log($"[TableLoader] 다운로드 시작: {sheet.sheetName} ({url})");

            var csv = await DownloadCsvAsync(url);

            if (!Directory.Exists(settings.csvOutputPath))
                Directory.CreateDirectory(settings.csvOutputPath);

            var filePath = Path.Combine(settings.csvOutputPath, $"{className}.csv");
            File.WriteAllText(filePath, csv);

            Debug.Log($"[TableLoader] CSV 저장 완료: {filePath}");
        }

        public static async Task DownloadAllAsync(TableLoaderSettings settings, Action<int, int, string> onProgress = null)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var enabledSheets = settings.sheets.FindAll(sheet => sheet != null && sheet.enabled);
            var failures = new System.Collections.Generic.List<string>();

            for (int i = 0; i < enabledSheets.Count; i++)
            {
                var sheet = enabledSheets[i];
                onProgress?.Invoke(i, enabledSheets.Count, sheet.sheetName);

                try
                {
                    await DownloadAndSaveAsync(settings, sheet);
                }
                catch (Exception e)
                {
                    failures.Add($"{sheet.sheetName}: {e.Message}");
                    Debug.LogError($"[TableLoader] '{sheet.sheetName}' 다운로드 실패: {e.Message}");
                }
            }

            onProgress?.Invoke(enabledSheets.Count, enabledSheets.Count, "완료");

            if (failures.Count > 0)
                throw new InvalidOperationException(
                    $"시트 {failures.Count}개를 다운로드하지 못했습니다.\n- {string.Join("\n- ", failures)}");
        }
    }
}
