using System.Collections.Generic;

namespace AchEngine.UI
{
    /// <summary>
    /// UI 서비스의 공개 계약을 정의하는 인터페이스.
    /// VContainer 환경과 독립 환경 모두에서 동일한 API로 UI를 제어할 수 있도록 한다.
    /// </summary>
    public interface IUIService
    {
        /// <summary>서비스가 초기화 완료 상태인지 여부.</summary>
        bool IsInitialized { get; }

        /// <summary>이 서비스에 등록된 뷰 카탈로그.</summary>
        UIViewCatalog Catalog { get; }

        /// <summary>UI 계층 구조의 루트 컴포넌트.</summary>
        UIRoot Root { get; }

        /// <summary>
        /// 지정한 ID의 UI 뷰를 열고 반환한다.
        /// </summary>
        /// <param name="id">카탈로그에 등록된 뷰 ID.</param>
        /// <param name="payload">뷰에 전달할 임의의 페이로드 객체.</param>
        /// <returns>열린 <see cref="UIView"/> 인스턴스.</returns>
        UIView Show(string id, object payload = null);

        /// <summary>
        /// 지정한 ID의 UI 뷰를 열고 지정 타입으로 반환한다.
        /// </summary>
        /// <typeparam name="T">반환받을 <see cref="UIView"/> 하위 타입.</typeparam>
        /// <param name="id">카탈로그에 등록된 뷰 ID.</param>
        /// <param name="payload">뷰에 전달할 임의의 페이로드 객체.</param>
        /// <returns>열린 <typeparamref name="T"/> 인스턴스.</returns>
        T Show<T>(string id, object payload = null) where T : UIView;

        /// <summary>
        /// 지정한 ID의 뷰를 닫는다.
        /// </summary>
        /// <param name="id">닫을 뷰의 ID.</param>
        /// <param name="closeAll">동일 ID의 뷰를 모두 닫을지 여부.</param>
        /// <returns>닫힌 뷰가 하나 이상이면 <c>true</c>.</returns>
        bool Close(string id, bool closeAll = false);

        /// <summary>
        /// 지정한 뷰 인스턴스를 닫는다.
        /// </summary>
        /// <param name="view">닫을 <see cref="UIView"/> 인스턴스.</param>
        /// <returns>뷰를 성공적으로 닫으면 <c>true</c>.</returns>
        bool Close(UIView view);

        /// <summary>
        /// 현재 열려 있는 뷰 중 가장 위에 있는 뷰를 닫는다.
        /// </summary>
        /// <returns>닫힌 뷰가 있으면 <c>true</c>.</returns>
        bool CloseTopmost();

        /// <summary>열려 있는 모든 뷰를 닫는다.</summary>
        void CloseAll();

        /// <summary>
        /// 지정한 ID의 열린 뷰를 가져온다.
        /// </summary>
        /// <param name="id">찾을 뷰 ID.</param>
        /// <param name="view">찾은 뷰 인스턴스. 없으면 <c>null</c>.</param>
        /// <returns>뷰가 열려 있으면 <c>true</c>.</returns>
        bool TryGetOpen(string id, out UIView view);

        /// <summary>
        /// 지정 타입으로 열린 뷰를 가져온다.
        /// </summary>
        /// <typeparam name="T">찾을 <see cref="UIView"/> 하위 타입.</typeparam>
        /// <param name="view">찾은 뷰 인스턴스. 없으면 <c>null</c>.</param>
        /// <returns>뷰가 열려 있으면 <c>true</c>.</returns>
        bool TryGetOpen<T>(out T view) where T : UIView;

        /// <summary>
        /// 지정한 ID의 뷰가 현재 열려 있는지 확인한다.
        /// </summary>
        /// <param name="id">확인할 뷰 ID.</param>
        /// <returns>열려 있으면 <c>true</c>.</returns>
        bool IsOpen(string id);

        /// <summary>카탈로그에 등록된 모든 뷰를 미리 인스턴스화하여 풀에 준비한다.</summary>
        void Prewarm();
    }
}
