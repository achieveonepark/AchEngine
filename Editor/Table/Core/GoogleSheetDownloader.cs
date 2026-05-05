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
            var request = UnityWebRequest.Get(url);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Delay(100);

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"?ㅼ슫濡쒕뱶 ?ㅽ뙣: {request.error}\nURL: {url}");

            return request.downloadHandler.text;
        }

        public static async Task DownloadAndSaveAsync(TableLoaderSettings settings, SheetInfo sheet)
        {
            var url = settings.GetCsvDownloadUrl(sheet);
            Debug.Log($"[TableLoader] ?ㅼ슫濡쒕뱶 ?쒖옉: {sheet.sheetName} ({url})");

            var csv = await DownloadCsvAsync(url);

            if (!Directory.Exists(settings.csvOutputPath))
                Directory.CreateDirectory(settings.csvOutputPath);

            var filePath = Path.Combine(settings.csvOutputPath, $"{sheet.GetClassName()}.csv");
            File.WriteAllText(filePath, csv);

            Debug.Log($"[TableLoader] CSV ????꾨즺: {filePath}");
        }

        public static async Task DownloadAllAsync(TableLoaderSettings settings, Action<int, int, string> onProgress = null)
        {
            var enabledSheets = settings.sheets.FindAll(s => s.enabled);

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
                    Debug.LogError($"[TableLoader] '{sheet.sheetName}' ?ㅼ슫濡쒕뱶 ?ㅽ뙣: {e.Message}");
                }
            }

            onProgress?.Invoke(enabledSheets.Count, enabledSheets.Count, "?꾨즺");
        }
    }
}
