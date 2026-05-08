using AchEngine.DI;
using AchEngine.Managers;
using AchEngine.Samples.Full.Data;
using AchEngine.UI;
using UnityEngine;
using UnityEngine.UI;

namespace AchEngine.Samples.Full.UI
{
    /// <summary>
    /// UIViewCatalog에 "Inventory" ID로 등록하세요. Layer: Popup.
    /// </summary>
    public class InventoryPopup : UIView
    {
        [Header("Layout")]
        [SerializeField] private Transform itemListRoot;
        [SerializeField] private GameObject itemRowPrefab;

        [Header("Buttons")]
        [SerializeField] private Button btnClose;

        protected override void OnInitialize()
        {
            btnClose?.onClick.AddListener(CloseSelf);
        }

        protected override void OnOpened(object payload)
        {
            ClearList();
            PopulateList();
        }

        private void ClearList()
        {
            if (itemListRoot == null) return;
            foreach (Transform child in itemListRoot)
                Destroy(child.gameObject);
        }

        private void PopulateList()
        {
            if (itemListRoot == null || itemRowPrefab == null) return;

            var pm  = ServiceLocator.Get<PlayerManager>();
            var inv = pm.GetContainer<InventoryContainer>();
            if (inv == null) return;

            foreach (var kvp in inv.GetAll())
            {
                var row = Instantiate(itemRowPrefab, itemListRoot);
                var lbl = row.GetComponentInChildren<Text>();
                if (lbl != null)
                    lbl.text = $"{kvp.Value.Name}  x{kvp.Value.Quantity}";
            }
        }
    }
}
