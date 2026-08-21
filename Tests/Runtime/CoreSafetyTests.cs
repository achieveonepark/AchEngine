using System;
using System.Reflection;
using System.Threading;
using AchEngine.Managers;
using AchEngine.Pathfinding;
using AchEngine.Player;
using AchEngine.Table;
using NUnit.Framework;
using UnityEngine;

namespace AchEngine.Tests
{
    public sealed class CoreSafetyTests
    {
        [Test]
        public void TableDatabase_중복Id를_거부한다()
        {
            var database = new TableDatabase();
            var rows = new[] { new TestTableRow(1), new TestTableRow(1) };

            Assert.Throws<ArgumentException>(() => database.Register(rows));
        }

        [Test]
        public void AStar_모서리통과옵션을_존중한다()
        {
            var grid = new AStarGrid(2, 2);
            grid.SetWalkable(1, 0, false);
            grid.SetWalkable(0, 1, false);

            var compatiblePath = AStarPathfinder.FindPath(
                grid, Vector2Int.zero, Vector2Int.one, diagonal: true);
            var safePath = AStarPathfinder.FindPath(
                grid, Vector2Int.zero, Vector2Int.one, diagonal: true, allowCornerCutting: false);

            Assert.That(compatiblePath, Has.Count.EqualTo(1));
            Assert.That(safePath, Is.Empty);
        }

        [Test]
        public void AchTask_WhenAny는_빈입력을_거부한다()
        {
            Assert.Throws<ArgumentException>(() => AchTask.WhenAny(Array.Empty<AchTask>()));
        }

        [Test]
        public void AchTimer_즉시완료와_선취소를_처리한다()
        {
            Assert.That(AchTimer.Start(0f).IsDone, Is.True);

            using var source = new CancellationTokenSource();
            source.Cancel();
            var cancelled = AchTimer.Start(1f, cancellationToken: source.Token);

            Assert.That(cancelled.IsDone, Is.True);
            Assert.That(cancelled.IsCancelled, Is.True);
        }

        [Test]
        public void PlayerManager_사용자DataKey로_타입조회한다()
        {
            var manager = new PlayerManager();
            var container = new TestPlayerContainer { DataKey = "profile", Value = 7 };

            manager.Add(container);

            Assert.That(manager.Get<TestPlayerContainer>(), Is.SameAs(container));
        }

        [Test]
        public void PlayerManager_동일타입이_둘이면_모호함을_알린다()
        {
            var manager = new PlayerManager();
            manager.Add(new TestPlayerContainer { DataKey = "first" });
            manager.Add(new TestPlayerContainer { DataKey = "second" });

            Assert.Throws<InvalidOperationException>(() => manager.Get<TestPlayerContainer>());
        }

#if USE_QUICK_SAVE
        [Test]
        public void PlayerManager_저장레코드로_컨테이너를_복원한다()
        {
            var manager = new PlayerManager();
            manager.Add(new TestPlayerContainer { DataKey = "profile", Value = 42 });
            var property = typeof(PlayerManager).GetProperty(
                "SerializedContainers", BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.That(property, Is.Not.Null);
            var records = property.GetValue(manager);
            var restored = new PlayerManager();
            property.SetValue(restored, records);

            Assert.That(restored.Get<TestPlayerContainer>().Value, Is.EqualTo(42));
        }
#endif

        private sealed class TestTableRow : ITableData
        {
            public int Id { get; }
            public TestTableRow(int id) => Id = id;
        }

        public sealed class TestPlayerContainer : IPlayerDataContainerBase
        {
            public string DataKey { get; set; }
            public int Value { get; set; }
        }
    }
}
