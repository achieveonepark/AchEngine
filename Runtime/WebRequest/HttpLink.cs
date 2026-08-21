using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace AchEngine
{
    /// <summary>
    /// HTTP 요청을 래핑하는 클래스입니다. <see cref="Builder"/>로 생성하여 사용합니다.
    /// </summary>
    public class HttpLink : IDisposable
    {
        private HttpLink(UnityWebRequest request) => _request = request;

        private UnityWebRequest _request;
        private bool _sendStarted;

        /// <summary>요청 결과 상태를 반환합니다.</summary>
        public UnityWebRequest.Result Result => GetRequest().result;

        /// <summary>요청이 성공했는지 여부를 반환합니다.</summary>
        public bool Success => Result == UnityWebRequest.Result.Success;

        /// <summary>수신된 응답 데이터를 바이트 배열로 반환합니다.</summary>
        public byte[] ReceiveData => GetRequest().downloadHandler?.data ?? Array.Empty<byte>();

        /// <summary>수신된 응답 데이터를 문자열로 반환합니다.</summary>
        public string ReceiveDataString => GetRequest().downloadHandler?.text ?? string.Empty;

        /// <summary>다운로드된 바이트 수를 반환합니다.</summary>
        public ulong DownloadSize => GetRequest().downloadedBytes;

        /// <summary>다운로드 진행률(0~1)을 반환합니다.</summary>
        public float DownloadProgress => GetRequest().downloadProgress;

        /// <summary>
        /// HTTP 요청을 비동기로 전송합니다.
        /// </summary>
        /// <returns>요청 완료 후 자기 자신(<see cref="HttpLink"/>)을 반환합니다.</returns>
        public async Task<HttpLink> SendAsync()
            => await SendAsync(CancellationToken.None);

        /// <summary>취소 토큰을 사용하여 HTTP 요청을 비동기로 전송합니다.</summary>
        public async Task<HttpLink> SendAsync(CancellationToken cancellationToken)
        {
            var request = GetRequest();
            if (_sendStarted)
                throw new InvalidOperationException("같은 HttpLink 요청은 한 번만 전송할 수 있습니다.");
            _sendStarted = true;

            cancellationToken.ThrowIfCancellationRequested();
            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    request.Abort();
                    cancellationToken.ThrowIfCancellationRequested();
                }
                await Task.Yield();
            }

            if (!Success)
                Debug.LogError($"[HttpLink] HTTP {request.responseCode}: {request.error}");

            return this;
        }

        public void Dispose()
        {
            _request?.Dispose();
            _request = null;
        }

        private UnityWebRequest GetRequest()
            => _request ?? throw new ObjectDisposedException(nameof(HttpLink));

        /// <summary>
        /// <see cref="HttpLink"/> 인스턴스를 구성하는 빌더 클래스입니다.
        /// 메서드 체이닝 방식으로 URL, 헤더, 바디 등을 설정한 뒤 <see cref="Build"/>로 완성합니다.
        /// </summary>
        public class Builder
        {
            private string _url;
            private string _method;
            private readonly Dictionary<string, string> _headers = new();
            private string _body;
            private int _timeout;

            /// <summary>요청 URL을 설정합니다.</summary>
            /// <param name="url">요청 대상 URL</param>
            /// <returns>빌더 자신</returns>
            public Builder SetUrl(string url)          { _url = url;       return this; }

            /// <summary>HTTP 메서드를 설정합니다.</summary>
            /// <param name="method">HTTP 메서드 문자열 (예: GET, POST)</param>
            /// <returns>빌더 자신</returns>
            public Builder SetMethod(string method)
            {
                if (string.IsNullOrWhiteSpace(method))
                    throw new ArgumentException("HTTP 메서드는 비어 있을 수 없습니다.", nameof(method));
                _method = method.Trim().ToUpperInvariant();
                return this;
            }

            /// <summary>요청 타임아웃을 설정합니다.</summary>
            /// <param name="seconds">타임아웃 시간(초)</param>
            /// <returns>빌더 자신</returns>
            public Builder SetTimeout(int seconds)
            {
                if (seconds < 0)
                    throw new ArgumentOutOfRangeException(nameof(seconds), "타임아웃은 0 이상이어야 합니다.");
                _timeout = seconds;
                return this;
            }

            /// <summary>JSON 문자열을 요청 바디로 설정합니다.</summary>
            /// <param name="json">직렬화된 JSON 문자열</param>
            /// <returns>빌더 자신</returns>
            public Builder SetJsonBody(string json)    { _body = json;     return this; }

            /// <summary>요청 헤더를 추가하거나 덮어씁니다.</summary>
            /// <param name="key">헤더 이름</param>
            /// <param name="value">헤더 값</param>
            /// <returns>빌더 자신</returns>
            public Builder AddHeader(string key, string value)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentException("헤더 이름은 비어 있을 수 없습니다.", nameof(key));
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                if (key.Contains('\r') || key.Contains('\n') || value.Contains('\r') || value.Contains('\n'))
                    throw new ArgumentException("헤더에는 줄바꿈 문자를 사용할 수 없습니다.");
                _headers[key] = value;
                return this;
            }

            /// <summary>
            /// GET 요청을 비동기로 전송하고 응답을 지정한 타입으로 역직렬화하여 반환합니다.
            /// </summary>
            /// <typeparam name="T">응답 JSON을 역직렬화할 타입</typeparam>
            /// <returns>역직렬화된 결과. 실패 시 default를 반환합니다.</returns>
            public async Task<T> GetAsync<T>()
            {
                _method = UnityWebRequest.kHttpVerbGET;
                using var response = await Build().SendAsync();
                if (!response.Success) return default;

                try
                {
                    return JsonConvert.DeserializeObject<T>(response.ReceiveDataString);
                }
                catch (JsonException e)
                {
                    Debug.LogError($"[HttpLink] JSON 응답을 역직렬화할 수 없습니다: {e.Message}");
                    return default;
                }
            }

            /// <summary>
            /// POST 요청을 비동기로 전송합니다.
            /// </summary>
            /// <returns>요청 성공 여부</returns>
            public async Task<bool> PostAsync()
            {
                _method = UnityWebRequest.kHttpVerbPOST;
                using var response = await Build().SendAsync();
                return response.Success;
            }

            /// <summary>
            /// 현재 설정으로 <see cref="HttpLink"/> 인스턴스를 생성합니다.
            /// </summary>
            /// <returns>생성된 <see cref="HttpLink"/> 인스턴스</returns>
            /// <exception cref="ArgumentException">URL이 지정되지 않은 경우</exception>
            /// <exception cref="NotSupportedException">지원하지 않는 HTTP 메서드인 경우</exception>
            public HttpLink Build()
            {
                if (!Uri.TryCreate(_url, UriKind.Absolute, out var uri) ||
                    (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
                    throw new ArgumentException("[HttpLink] 유효한 HTTP 또는 HTTPS URL이 필요합니다.", nameof(_url));

                var method = _method ?? (_body != null
                    ? UnityWebRequest.kHttpVerbPOST
                    : UnityWebRequest.kHttpVerbGET);

                UnityWebRequest request;

                if (method == UnityWebRequest.kHttpVerbPOST)
                {
                    request = new UnityWebRequest(uri.AbsoluteUri, UnityWebRequest.kHttpVerbPOST)
                    {
                        downloadHandler = new DownloadHandlerBuffer(),
                        uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(_body ?? string.Empty))
                    };
                    if (_body != null)
                        request.SetRequestHeader("Content-Type", "application/json");
                }
                else if (method == UnityWebRequest.kHttpVerbGET)
                {
                    request = UnityWebRequest.Get(uri.AbsoluteUri);
                }
                else
                {
                    throw new NotSupportedException($"[HttpLink] 지원하지 않는 HTTP 메서드입니다: {method}");
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
