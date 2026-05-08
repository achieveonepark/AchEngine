using System;
using System.Collections.Generic;
using System.Text;
using Unity.Serialization.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace AchEngine
{
    public class HttpLink
    {
        private HttpLink(UnityWebRequest request) => _request = request;

        private UnityWebRequest _request;

        public UnityWebRequest.Result Result => _request.result;
        public bool Success => Result == UnityWebRequest.Result.Success;
        public byte[] ReceiveData => _request.downloadHandler.data;
        public string ReceiveDataString => _request.downloadHandler.text;
        public ulong DownloadSize => _request.downloadedBytes;
        public float DownloadProgress => _request.downloadProgress;

        public async AchTask<HttpLink> SendAsync()
        {
            await _request.SendWebRequest().ToAchTask();

            if (!Success)
                Debug.LogError($"[HttpLink] {_request.error}");

            return this;
        }

        public class Builder
        {
            private string _url;
            private string _method;
            private readonly Dictionary<string, string> _headers = new();
            private string _body;
            private int _timeout;

            public Builder SetUrl(string url)          { _url = url;         return this; }
            public Builder SetMethod(string method)    { _method = method;   return this; }
            public Builder SetTimeout(int seconds)     { _timeout = seconds; return this; }
            public Builder SetJsonBody(string json)    { _body = json;       return this; }
            public Builder AddHeader(string key, string value) { _headers[key] = value; return this; }

            public async AchTask<T> GetAsync<T>()
            {
                _method = UnityWebRequest.kHttpVerbGET;
                var response = await Build().SendAsync();
                return response.Success ? JsonSerialization.FromJson<T>(response.ReceiveDataString) : default;
            }

            public async AchTask<bool> PostAsync()
            {
                _method = UnityWebRequest.kHttpVerbPOST;
                var response = await Build().SendAsync();
                return response.Success;
            }

            public HttpLink Build()
            {
                if (string.IsNullOrEmpty(_url))
                    throw new ArgumentException("[HttpLink] URL is required.");

                UnityWebRequest request;

                if (_method == UnityWebRequest.kHttpVerbPOST)
                {
                    request = UnityWebRequest.PostWwwForm(_url, _body);
                    if (!string.IsNullOrEmpty(_body))
                    {
                        request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(_body));
                        request.SetRequestHeader("Content-Type", "application/json");
                    }
                }
                else if (_method == UnityWebRequest.kHttpVerbGET)
                {
                    request = UnityWebRequest.Get(_url);
                }
                else
                {
                    throw new NotSupportedException($"[HttpLink] Unsupported HTTP method: {_method}");
                }

                if (_timeout > 0)
                    request.timeout = _timeout;

                foreach (var (key, value) in _headers)
                    request.SetRequestHeader(key, value);

                return new HttpLink(request);
            }
        }
    }
}
