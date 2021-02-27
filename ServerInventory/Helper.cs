using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerInventory
{
    public static class Helper
    {

        public static ZPackage PackageInventory(String characterName, Inventory myInventory)
        {
            ZPackage inventoryPackage = new ZPackage();
            var items = myInventory.GetAllItems();
            inventoryPackage.Write(characterName);
            inventoryPackage.Write(items.Count);

            foreach (var itemData in items)
            {
                inventoryPackage.Write(itemData.m_shared.m_name);
                inventoryPackage.Write(itemData.m_stack);
                inventoryPackage.Write(itemData.m_quality);
                inventoryPackage.Write(itemData.m_variant);
            }

            return inventoryPackage;
        }

    }
}
