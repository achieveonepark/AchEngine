# ConfigManager

PlayerPrefs ベースのキー・バリュー設定ストアです。アプリの音量や言語設定など、シンプルなオプション値の保存に使用します。

## API

```csharp
var cfg = ServiceLocator.Resolve<ConfigManager>();

// 키 등록 (초기값 설정)
cfg.AddKey("bgmVolume", 1.0f);
cfg.AddKey("language",  "ko");

// 값 읽기
float vol = cfg.GetConfig<float>("bgmVolume", fallback: 1f);

// 값 쓰기 (자동으로 PlayerPrefs에 저장)
cfg.SetConfig("bgmVolume", 0.5f);
```

> `AddKey` で登録されていないキーに対しては `SetConfig` は動作しません。
