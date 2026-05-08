using System.Collections.Generic;
using AchEngine.Player;

namespace AchEngine.Samples.Full.Data
{
    public class InventoryContainer : PlayerDataContainerBase<int, InventoryItem>
    {
        public InventoryContainer()
        {
            DataKey  = nameof(InventoryContainer);
            _dataDic = new Dictionary<int, InventoryItem>();
        }
    }
}
