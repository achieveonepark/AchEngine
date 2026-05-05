// Generated sample data model used by the Quick Start scene.
using System;
using AchEngine.Table;
#if ACHENGINE_MEMORYPACK
using MemoryPack;
#endif

[Serializable]
#if ACHENGINE_MEMORYPACK
[MemoryPackable]
#endif
public partial class MonsterData : ITableData
{
#if ACHENGINE_MEMORYPACK
    [MemoryPackOrder(0)]
#endif
    public int Id { get; set; }

#if ACHENGINE_MEMORYPACK
    [MemoryPackOrder(1)]
#endif
    public string Name { get; set; }

#if ACHENGINE_MEMORYPACK
    [MemoryPackOrder(2)]
#endif
    public int Hp { get; set; }

#if ACHENGINE_MEMORYPACK
    [MemoryPackOrder(3)]
#endif
    public float Speed { get; set; }
}
