#pragma warning disable CS0067, CS0169, CS0414, CS1591
using System;
using System.Collections;
using System.Collections.Generic;

// ──────────────────────────────────────────────────────────────
// UnityEngine 최소 스텁 — DocFX 메타데이터 생성 전용
// ──────────────────────────────────────────────────────────────
namespace UnityEngine
{
    public class Object { public string name; public static void Destroy(Object o) {} public static void DontDestroyOnLoad(Object o) {} public static T FindObjectOfType<T>() where T : Object => default; public static T Instantiate<T>(T prefab) where T : Object => default; public static T Instantiate<T>(T prefab, Transform parent, bool worldPositionStays) where T : Object => default; }
    public class Component : Object { public GameObject gameObject; public Transform transform; public T GetComponent<T>() => default; public T GetComponentInParent<T>() => default; public T GetComponentInChildren<T>() => default; public T[] GetComponents<T>() => default; public bool TryGetComponent<T>(out T component) { component = default; return false; } }
    public class Behaviour : Component { public bool enabled; public bool isActiveAndEnabled; }
    public class MonoBehaviour : Behaviour { protected Coroutine StartCoroutine(IEnumerator routine) => default; protected void StopCoroutine(Coroutine routine) {} protected void StopAllCoroutines() {} }
    public class ScriptableObject : Object { public static T CreateInstance<T>() where T : ScriptableObject => default; }
    public class GameObject : Object { public Transform transform; public bool activeSelf; public void SetActive(bool value) {} public T AddComponent<T>() where T : Component => default; public T GetComponent<T>() => default; public bool TryGetComponent<T>(out T component) { component = default; return false; } public static GameObject Find(string name) => default; public GameObject(string name, params Type[] components) {} }
    public class Transform : Component { public Vector3 position; public Vector3 localPosition; public Quaternion rotation; public Quaternion localRotation; public Vector3 localScale; public int childCount; public Transform parent; public Transform GetChild(int i) => default; public void SetParent(Transform p, bool worldPositionStays = true) {} public void SetAsLastSibling() {} public void SetSiblingIndex(int i) {} public int GetSiblingIndex() => 0; }
    public class RectTransform : Transform { public Vector2 anchorMin; public Vector2 anchorMax; public Vector2 anchoredPosition; public Vector2 sizeDelta; public Vector2 offsetMin; public Vector2 offsetMax; public Rect rect; }
    public struct Vector2 { public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; } public static readonly Vector2 zero = default; public static readonly Vector2 one = new Vector2(1,1); }
    public struct Vector2Int { public int x, y; public Vector2Int(int x, int y) { this.x = x; this.y = y; } }
    public struct Vector3 { public float x, y, z; public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; } public static readonly Vector3 zero = default; public static readonly Vector3 one = new Vector3(1,1,1); public static readonly Vector3 up = new Vector3(0,1,0); public static readonly Vector3 forward = new Vector3(0,0,1); }
    public struct Quaternion { public float x, y, z, w; public static readonly Quaternion identity = default; public static Quaternion Euler(float x, float y, float z) => default; }
    public struct Color { public float r, g, b, a; public Color(float r, float g, float b, float a = 1f) { this.r=r; this.g=g; this.b=b; this.a=a; } public static readonly Color white = new Color(1,1,1); public static readonly Color black = new Color(0,0,0); public static readonly Color red = new Color(1,0,0); public static readonly Color clear = new Color(0,0,0,0); }
    public struct Rect { public float x, y, width, height; public Vector2 position; public Vector2 size; }
    public class Coroutine {}
    public class YieldInstruction {}
    public class WaitForSeconds : YieldInstruction { public WaitForSeconds(float t) {} }
    public class WaitForSecondsRealtime : YieldInstruction { public WaitForSecondsRealtime(float t) {} }
    public class WaitUntil : YieldInstruction { public WaitUntil(Func<bool> predicate) {} }
    public class AsyncOperation : YieldInstruction { public float progress; public bool isDone; public event Action<AsyncOperation> completed; }
    public class AudioClip : Object {}
    public class AudioSource : Component { public AudioClip clip; public bool loop; public float volume; public bool isPlaying; public bool mute; public void Play() {} public void Stop() {} public void PlayOneShot(AudioClip c, float vol = 1f) {} }
    public class Sprite : Object {}
    public class SpriteRenderer : Component { public Sprite sprite; public Color color; }
    public class Camera : Component { public static Camera main; public Vector3 WorldToScreenPoint(Vector3 p) => default; public Vector3 ScreenToWorldPoint(Vector3 p) => default; public float nearClipPlane; }
    public class Collider2D : Component {}
    public class Texture : Object {}
    public class Texture2D : Texture {}
    public class Font : Object {}
    public class TextAsset : Object { public string text; public byte[] bytes; }
    public class Canvas : Component { public RenderMode renderMode; }
    public class CanvasScaler : Component { public ScaleMode uiScaleMode; public Vector2 referenceResolution; public ScreenMatchMode screenMatchMode; public float matchWidthOrHeight; public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize } public enum ScreenMatchMode { MatchWidthOrHeight, Expand, Shrink } }
    public class GraphicRaycaster : Component {}
    public class CanvasGroup : Component { public float alpha; public bool interactable; public bool blocksRaycasts; }
    public class StandaloneInputModule : Component {}
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public enum KeyCode { None = 0, Space = 32, Return = 13, Escape = 27, A = 97, D = 100, S = 115, W = 119 }
    public static class Debug { public static void Log(object m) {} public static void LogWarning(object m, Object ctx = null) {} public static void LogError(object m, Object ctx = null) {} }
    public static class Application { public static string persistentDataPath; public static string dataPath; public static bool isEditor; public static RuntimePlatform platform; public static event Action<string, string, LogType> logMessageReceivedThreaded; }
    public static class Time { public static float deltaTime; public static float unscaledDeltaTime; public static float timeScale; public static float time; public static float realtimeSinceStartup; }
    public static class Mathf { public static float Clamp01(float v) => v; public static float Clamp(float v, float a, float b) => v; public static float Max(float a, float b) => a; public static float Min(float a, float b) => a; public static float Lerp(float a, float b, float t) => a; public static float Abs(float v) => v; public static float Sin(float v) => 0; public static float Cos(float v) => 0; public const float PI = 3.14159265f; }
    public static class PlayerPrefs { public static void SetString(string k, string v) {} public static string GetString(string k, string d = "") => d; public static void SetInt(string k, int v) {} public static int GetInt(string k, int d = 0) => d; public static void SetFloat(string k, float v) {} public static float GetFloat(string k, float d = 0) => d; public static bool HasKey(string k) => false; public static void DeleteKey(string k) {} public static void DeleteAll() {} public static void Save() {} }
    public static class Screen { public static int width; public static int height; public static float dpi; public static Rect safeArea; public static ScreenOrientation orientation; }
    public static class Input { public static bool GetKey(KeyCode k) => false; public static bool GetKeyDown(KeyCode k) => false; public static bool GetKeyUp(KeyCode k) => false; public static bool GetMouseButton(int b) => false; public static Vector2 mousePosition; }
    public static class Physics2D { public static Collider2D[] OverlapCircleAll(Vector2 p, float r) => Array.Empty<Collider2D>(); }
    public enum RuntimePlatform { Android, IPhonePlayer, WindowsEditor, OSXEditor, LinuxEditor }
    public enum LogType { Error, Assert, Warning, Log, Exception }
    public enum ScreenOrientation { Portrait, PortraitUpsideDown, LandscapeLeft, LandscapeRight }

    // Attributes
    [AttributeUsage(AttributeTargets.Field)] public sealed class SerializeField : Attribute {}
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Class)] public sealed class HideInInspector : Attribute {}
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)] public sealed class RequireComponent : Attribute { public RequireComponent(Type t) {} }
    [AttributeUsage(AttributeTargets.Class)] public sealed class AddComponentMenu : Attribute { public AddComponentMenu(string p) {} }
    [AttributeUsage(AttributeTargets.Class)] public sealed class DisallowMultipleComponent : Attribute {}
    [AttributeUsage(AttributeTargets.Class)] public sealed class ExecuteAlways : Attribute {}
    [AttributeUsage(AttributeTargets.Class)] public sealed class ExecuteInEditMode : Attribute {}
    [AttributeUsage(AttributeTargets.Method)] public sealed class RuntimeInitializeOnLoadMethod : Attribute { public RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType t = RuntimeInitializeLoadType.AfterSceneLoad) {} }
    [AttributeUsage(AttributeTargets.Class)] public sealed class CreateAssetMenu : Attribute { public string fileName; public string menuName; public int order; }
    [AttributeUsage(AttributeTargets.Field)] public sealed class Header : Attribute { public Header(string h) {} }
    [AttributeUsage(AttributeTargets.Field)] public sealed class Range : Attribute { public Range(float min, float max) {} public Range(int min, int max) {} }
    [AttributeUsage(AttributeTargets.Field)] public sealed class Tooltip : Attribute { public Tooltip(string t) {} }
    [AttributeUsage(AttributeTargets.Field)] public sealed class Min : Attribute { public Min(float min) {} }
    [AttributeUsage(AttributeTargets.Field)] public sealed class TextArea : Attribute { public TextArea() {} public TextArea(int min, int max) {} }
    [AttributeUsage(AttributeTargets.Field)] public sealed class Space : Attribute { public Space() {} public Space(float h) {} }
    public enum RuntimeInitializeLoadType { AfterSceneLoad, BeforeSceneLoad, AfterAssembliesLoaded, BeforeSplashScreen, SubsystemRegistration }
}

namespace UnityEngine.Events
{
    public abstract class UnityEventBase {}
    public class UnityEvent : UnityEventBase { public void AddListener(Action call) {} public void RemoveListener(Action call) {} public void Invoke() {} }
    public class UnityEvent<T0> : UnityEventBase { public void AddListener(Action<T0> call) {} public void RemoveListener(Action<T0> call) {} public void Invoke(T0 arg) {} }
    public class UnityEvent<T0,T1> : UnityEventBase { public void AddListener(Action<T0,T1> call) {} public void RemoveListener(Action<T0,T1> call) {} public void Invoke(T0 a0, T1 a1) {} }
}

namespace UnityEngine.UI
{
    public class Graphic : UnityEngine.Component { public Color color; public bool raycastTarget; }
    public class MaskableGraphic : Graphic {}
    public class Text : MaskableGraphic { public string text; public int fontSize; }
    public class Image : MaskableGraphic { public Sprite sprite; public float fillAmount; public bool preserveAspect; }
    public class RawImage : MaskableGraphic { public UnityEngine.Texture texture; }
    public class Selectable : UnityEngine.Component { public bool interactable; }
    public class Button : Selectable { public ButtonClickedEvent onClick; public class ButtonClickedEvent : UnityEngine.Events.UnityEvent {} }
    public class Slider : Selectable { public float value; public float minValue; public float maxValue; public SliderEvent onValueChanged; public class SliderEvent : UnityEngine.Events.UnityEvent<float> {} }
    public class ScrollRect : UnityEngine.Component {}
}

namespace UnityEngine.EventSystems
{
    public interface IPointerClickHandler { void OnPointerClick(PointerEventData e); }
    public interface IPointerDownHandler { void OnPointerDown(PointerEventData e); }
    public interface IPointerUpHandler { void OnPointerUp(PointerEventData e); }
    public interface IDragHandler { void OnDrag(PointerEventData e); }
    public interface IBeginDragHandler { void OnBeginDrag(PointerEventData e); }
    public interface IEndDragHandler { void OnEndDrag(PointerEventData e); }
    public class PointerEventData { public Vector2 position; public Vector2 delta; public Vector2 pressPosition; public PointerEventData.InputButton button; public enum InputButton { Left, Right, Middle } }
    public class EventSystem : UnityEngine.MonoBehaviour { public static EventSystem current; }
}

namespace UnityEngine.SceneManagement
{
    public struct Scene { public string name; public bool isLoaded; }
    public static class SceneManager { public static Scene GetActiveScene() => default; public static AsyncOperation LoadSceneAsync(string s, LoadSceneMode m = LoadSceneMode.Single) => default; public static AsyncOperation UnloadSceneAsync(string s) => default; public static void LoadScene(string s) {} }
    public enum LoadSceneMode { Single, Additive }
}

namespace UnityEngine.U2D
{
    public class SpriteAtlas : UnityEngine.Object { public Sprite GetSprite(string name) => default; }
}

namespace UnityEngine.AddressableAssets
{
    public class AssetReference { public string AssetGUID; public bool RuntimeKeyIsValid() => false; }
}
