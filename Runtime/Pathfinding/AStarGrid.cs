using System;
using UnityEngine;

namespace AchEngine.Pathfinding
{
    /// <summary>
    /// A* 탐색에 사용할 격자 맵입니다.
    /// 각 셀마다 통과 가능 여부와 이동 비용을 설정할 수 있습니다.
    /// </summary>
    public class AStarGrid
    {
        private readonly bool[,]  _walkable;
        private readonly float[,] _cost;

        /// <summary>격자 너비 (X축 셀 수)</summary>
        public int Width  { get; }

        /// <summary>격자 높이 (Y축 셀 수)</summary>
        public int Height { get; }

        /// <param name="width">격자 너비</param>
        /// <param name="height">격자 높이</param>
        /// <param name="defaultWalkable">초기 통과 가능 여부 (기본 true)</param>
        public AStarGrid(int width, int height, bool defaultWalkable = true)
        {
            if (width  <= 0) throw new ArgumentOutOfRangeException(nameof(width));
            if (height <= 0) throw new ArgumentOutOfRangeException(nameof(height));

            Width    = width;
            Height   = height;
            _walkable = new bool[width, height];
            _cost     = new float[width, height];

            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                _walkable[x, y] = defaultWalkable;
                _cost[x, y]     = 1f;
            }
        }

        /// <summary>
        /// bool 배열로 격자를 초기화합니다.
        /// array[x, y] == true 인 셀만 통과 가능합니다.
        /// </summary>
        public AStarGrid(bool[,] walkableMap)
        {
            Width    = walkableMap.GetLength(0);
            Height   = walkableMap.GetLength(1);
            _walkable = (bool[,])walkableMap.Clone();
            _cost     = new float[Width, Height];

            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                _cost[x, y] = 1f;
        }

        /// <summary>셀의 통과 가능 여부를 설정합니다.</summary>
        public void SetWalkable(int x, int y, bool walkable)
        {
            AssertInBounds(x, y);
            _walkable[x, y] = walkable;
        }

        /// <summary>셀의 이동 비용을 설정합니다. 높을수록 해당 셀을 우회하는 경로를 선호합니다.</summary>
        /// <param name="cost">이동 비용 (1 이상)</param>
        public void SetCost(int x, int y, float cost)
        {
            AssertInBounds(x, y);
            if (cost < 1f) throw new ArgumentException("이동 비용은 1 이상이어야 합니다.", nameof(cost));
            _cost[x, y] = cost;
        }

        /// <summary>셀이 범위 내에 있고 통과 가능한지 반환합니다.</summary>
        public bool IsWalkable(int x, int y)
            => IsInBounds(x, y) && _walkable[x, y];

        /// <summary>셀의 이동 비용을 반환합니다.</summary>
        public float GetCost(int x, int y) => _cost[x, y];

        /// <summary>좌표가 격자 범위 안에 있는지 반환합니다.</summary>
        public bool IsInBounds(int x, int y)
            => x >= 0 && x < Width && y >= 0 && y < Height;

        private void AssertInBounds(int x, int y)
        {
            if (!IsInBounds(x, y))
                throw new ArgumentOutOfRangeException($"좌표 ({x}, {y})가 격자 범위({Width}×{Height})를 벗어났습니다.");
        }
    }
}
