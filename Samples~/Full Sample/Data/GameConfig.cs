using UnityEngine;

namespace AchEngine.Samples.Full.Data
{
    /// <summary>
    /// 게임 전역 설정 ScriptableObject.
    /// AchScriptableObject.GetOrAdd&lt;GameConfig&gt;() 로 접근하면
    /// Assets/AchEngine/Resources/Settings/ 에 자동 생성됩니다.
    /// </summary>
    [CreateAssetMenu(menuName = "AchEngine/Samples/GameConfig", fileName = "GameConfig")]
    public class GameConfig : AchScriptableObject
    {
        public const string KeyBgmVolume  = "bgm_volume";
        public const string KeySfxVolume  = "sfx_volume";
        public const string KeyPlayerName = "player_name";

        [Header("Audio Defaults")]
        [Range(0f, 1f)] public float DefaultBgmVolume = 0.7f;
        [Range(0f, 1f)] public float DefaultSfxVolume = 1.0f;

        [Header("Player Defaults")]
        public string DefaultPlayerName = "Hero";
        public int    StartingGold      = 100;

        [Header("Server")]
        public string ServerUrl = "https://httpbin.org";

        [Header("Gameplay")]
        public int CardCount        = 8;
        public int RoundTimeSeconds = 60;
        public int MaxHp            = 5;
    }
}
