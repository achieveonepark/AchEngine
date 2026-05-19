using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace AchEngine
{
    /// <summary>
    /// HTTP 요청을 래핑하는 클래스입니다. <see cref="Builder"/>로 생성하여 사용합니다.
    /// </summary>
    public class HttpLink
    {
        private HttpLink(UnityWebRequest request) => _request = request;

        private UnityWebRequest _request;

        /// <summary>요청 결과 상태를 반환합니다.</summary>
        public UnityWebRequest.Result Result => _request.result;

        /// <summary>요청이 성공했는지 여부를 반환합니다.</summary>
        public bool Success => Result == UnityWebRequest.Result.Success;

        /// <summary>수신된 응답 데이터를 바이트 배열로 반환합니다.</summary>
        public byte[] ReceiveData => _request.downloadHandler.data;

        /// <summary>수신된 응답 데이터를 문자열로 반환합니다.</summary>
        public string ReceiveDataString => _request.downloadHandler.text;

        /// <summary>다운로드된 바이트 수를 반환합니다.</summary>
        public ulong DownloadSize => _request.downloadedBytes;

        /// <summary>다운로드 진행률(0~1)을 반환합니다.</summary>
        public float DownloadProgress => _request.downloadProgress;

        /// <summary>
        /// HTTP 요청을 비동기로 전송합니다.
        /// </summary>
        /// <returns>요청 완료 후 자기 자신(<see cref="HttpLink"/>)을 반환합니다.</returns>
        public async Task<HttpLink> SendAsync()
        {
            await _request.SendWebRequest().ToAchTask();

            if (!Success)
                Debug.LogError($"[HttpLink] {_request.error}");

            return this;
        }

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
            public Builder SetMethod(string method)    { _method = method; return this; }

            /// <summary>요청 타임아웃을 설정합니다.</summary>
            /// <param name="seconds">타임아웃 시간(초)</param>
            /// <returns>빌더 자신</returns>
            public Builder SetTimeout(int seconds)     { _timeout = seconds; return this; }

            /// <summary>JSON 문자열을 요청 바디로 설정합니다.</summary>
            /// <param name="json">직렬화된 JSON 문자열</param>
            /// <returns>빌더 자신</returns>
            public Builder SetJsonBody(string json)    { _body = json;     return this; }

            /// <summary>요청 헤더를 추가하거나 덮어씁니다.</summary>
            /// <param name="key">헤더 이름</param>
            /// <param name="value">헤더 값</param>
            /// <returns>빌더 자신</returns>
            public Builder AddHeader(string key, string value) { _headers[key] = value; return this; }

            /// <summary>
            /// GET 요청을 비동기로 전송하고 응답을 지정한 타입으로 역직렬화하여 반환합니다.
            /// </summary>
            /// <typeparam name="T">응답 JSON을 역직렬화할 타입</typeparam>
            /// <returns>역직렬화된 결과. 실패 시 default를 반환합니다.</returns>
            public async Task<T> GetAsync<T>()
            {
                _method = UnityWebRequest.kHttpVerbGET;
                var response = await Build().SendAsync();
                return response.Success ? JsonConvert.DeserializeObject<T>(response.ReceiveDataString) : default;
            }

            /// <summary>
            /// POST 요청을 비동기로 전송합니다.
            /// </summary>
            /// <returns>요청 성공 여부</returns>
            public async Task<bool> PostAsync()
            {
                _method = UnityWebRequest.kHttpVerbPOST;
                var response = await Build().SendAsync();
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
