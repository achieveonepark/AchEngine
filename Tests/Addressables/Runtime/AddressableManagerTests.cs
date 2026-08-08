#if ACHENGINE_ADDRESSABLES
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace AchEngine.Assets.Tests
{
    public class AddressableManagerTests
    {
        [Test]
        public void IsLoaded_ReturnsFalseBeforeAnAssetIsLoaded()
        {
            Assert.IsFalse(AddressableManager.IsLoaded("not_loaded"));
        }

        [Test]
        public void Release_IsSafeBeforeInitialization()
        {
            Assert.DoesNotThrow(() => AddressableManager.Release("not_loaded"));
        }

        [Test]
        public void AssetHandleCache_StoresAndRemovesSingleHandle()
        {
            var cache = new AssetHandleCache();
            var handle = Addressables.ResourceManager.CreateCompletedOperation<Object>(null, null);

            cache.AddAsset("icon", handle);

            Assert.IsTrue(cache.TryGetAssetHandle("icon", out _));

            cache.RemoveAsset("icon");

            Assert.IsFalse(cache.TryGetAssetHandle("icon", out _));
        }

        [TearDown]
        public void TearDown()
        {
            AddressableManager.ReleaseAll();
        }
    }
}
#endif
