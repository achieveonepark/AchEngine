# ConfigManager

基于 PlayerPrefs 的键值设置存储。用于保存应用音量、语言设置等简单的选项值。

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

> 对于未通过 `AddKey` 注册的键,`SetConfig` 不会生效。
