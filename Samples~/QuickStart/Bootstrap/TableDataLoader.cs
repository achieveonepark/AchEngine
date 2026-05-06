using AchEngine.DI;
using AchEngine.Table;
using UnityEngine;

namespace AchEngine.Samples
{
    /// <summary>
    /// Quick Start sample table loader.
    /// In a real project, generated loaders such as TableLoaderGenerated.LoadAll() are recommended.
    /// </summary>
    public class TableDataLoader : MonoBehaviour
    {
        private void Start()
        {
            TableManager.LoadFromJsonText<MonsterData>(
                "[" +
                "{\"Id\":1,\"Name\":\"Slime\",\"Hp\":100,\"Speed\":1.5}," +
                "{\"Id\":2,\"Name\":\"Orc Warrior\",\"Hp\":250,\"Speed\":2.0}," +
                "{\"Id\":3,\"Name\":\"Ancient Dragon\",\"Hp\":9999,\"Speed\":3.5}" +
                "]"
            );

            var slime = TableManager.Get<MonsterData>(1);
            Debug.Log($"[TableSample] ID 1: {slime.Name} (HP:{slime.Hp})");

            if (TableManager.TryGet<MonsterData>(99, out var boss))
            {
                Debug.Log($"[TableSample] Boss: {boss.Name}");
            }
            else
            {
                Debug.Log("[TableSample] ID 99 does not exist.");
            }

            foreach (var monster in TableManager.GetAll<MonsterData>())
            {
                Debug.Log($"[TableSample] Loaded {monster.Name} / HP:{monster.Hp} / Speed:{monster.Speed}");
            }
        }
    }
}
