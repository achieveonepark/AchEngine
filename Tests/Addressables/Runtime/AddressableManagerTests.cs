using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AchEngine.Assets.Tests
{
    public class AddressableManagerTests
    {
        [Test]
        public void Instance_CreatesManagerAutomatically()
        {
            var manager = AddressableManager.Instance;
            Assert.IsNotNull(manager);
            Assert.IsNotNull(manager.gameObject);
        }

        [Test]
        public void Instance_ReturnsSameInstance()
        {
            var first = AddressableManager.Instance;
            var second = AddressableManager.Instance;
            Assert.AreSame(first, second);
        }

        [Test]
        public void IsLoaded_ReturnsFalseForUnloadedAsset()
        {
            var manager = AddressableManager.Instance;
            Assert.IsFalse(manager.IsLoaded("non_existent_asset"));
        }

        [Test]
        public void GetReferenceCount_ReturnsZeroForUnloadedAsset()
        {
            var manager = AddressableManager.Instance;
            Assert.AreEqual(0, manager.GetReferenceCount("non_existent_asset"));
        }

        [Test]
        public void RemoteContent_IsAccessible()
        {
            var manager = AddressableManager.Instance;
            Assert.IsNotNull(manager.RemoteContent);
        }

        [Test]
        public void RegisterAssetHandle_MarksKeyAsRegistered()
        {
            var manager = AddressableManager.Instance;

            manager.RegisterAssetHandle("hero", "Characters/Hero");

            Assert.IsTrue(manager.IsAssetHandleRegistered("hero"));
        }

        [Test]
        public void UnregisterAssetHandle_RemovesRegisteredKey()
        {
            var manager = AddressableManager.Instance;

            manager.RegisterAssetHandle("hero", "Characters/Hero");
            var removed = manager.UnregisterAssetHandle("hero");

            Assert.IsTrue(removed);
            Assert.IsFalse(manager.IsAssetHandleRegistered("hero"));
        }

        [Test]
        public void RegisterAssetHandle_ThrowsWhenRuntimeKeyAlreadyRegisteredToAnotherKey()
        {
            var manager = AddressableManager.Instance;

            manager.RegisterAssetHandle("hero", "Characters/Hero");

            Assert.Throws<InvalidOperationException>(() =>
                manager.RegisterAssetHandle("hero_duplicate", "Characters/Hero"));
        }

        [TearDown]
        public void TearDown()
        {
            var instance = AddressableManager.Instance;
            if (instance != null)
            {
                instance.ReleaseAll();
                Object.DestroyImmediate(instance.gameObject);
            }
        }
    }
}
