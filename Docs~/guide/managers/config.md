# ConfigManager

PlayerPrefs 기반의 키-값 설정 저장소입니다. 앱 볼륨, 언어 설정 등 간단한 옵션값을 저장할 때 사용합니다.

## API

```csharp
var cfg = ServiceLocator.Get<ConfigManager>();

// 키 등록 (초기값 설정)
cfg.AddKey("bgmVolume", 1.0f);
cfg.AddKey("language",  "ko");

// 값 읽기
float vol = cfg.GetConfig<float>("bgmVolume", fallback: 1f);

// 값 쓰기 (자동으로 PlayerPrefs에 저장)
cfg.SetConfig("bgmVolume", 0.5f);
```

> 등록(`AddKey`)하지 않은 키에는 `SetConfig`가 동작하지 않습니다.
