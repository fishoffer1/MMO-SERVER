using GameServer.Model;
using Google.Protobuf;
using Proto;
using Serilog;
using Summer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.InventorySystem
{
    /// <summary>
    /// 库存对象
    /// </summary>
    public class Inventory
    {

        public Character Chr { get; private set; }
        //背包容量
        public int Capacity { get; private set; }
        //物品列表
        protected List<Item> itemList = new();
        //默认为true确保首次可执行
        private bool hasChanged = true;

        public Inventory(Character _chr)
        {
            Chr = _chr;
            
        }

        public void Init(byte[] bytes)
        {
            if(bytes == null)
            {
                Capacity = 10;
                for (int i = 0; i < Capacity; i++)
                {
                    itemList.Add(null);
                }
            }
            else
            {
                InventoryInfo inv = InventoryInfo.Parser.ParseFrom(bytes);
                Log.Information("数据还原：" + inv);
                Capacity = inv.Capacity;
                //创建格子
                for (int i = 0; i < Capacity; i++)
                {
                    itemList.Add(null);
                }
                //创建物品
                foreach(var itemInfo in inv.List)
                {
                    itemList[itemInfo.Position] = new Item(itemInfo);
                }
            }
        }

        private InventoryInfo _inventoryInfo;
        public InventoryInfo InventoryInfo
        {
            get
            {
                if(_inventoryInfo == null)
                {
                    _inventoryInfo = new InventoryInfo();
                }
                if(hasChanged)
                {
                    _inventoryInfo.Capacity = Capacity;
                    _inventoryInfo.List.Clear();
                    for (int i = 0; i < Capacity; i++)
                    {
                        var item = itemList[i];
                        if (item == null) continue;
                        _inventoryInfo.List.Add(item.ItemInfo);
                    }
                    hasChanged = false;
                }
                return _inventoryInfo;
            }
        }



        /// <summary>
        /// 物品加入库存
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool AddItem(int itemId)
        {
            if (!DataManager.Instance.Items.TryGetValue(itemId, out var def))
            {
                Log.Information("物品id不存在:{0}", itemId);
                return false;
            }

            //找一个和item的id一样的格子进行存放
            var item = FindSameItemAndNotFull(itemId);
            if (item != null)
            {
                item.amount++;
            }
            else
            {
                int index = FindEmptyIndex();
                if (index > -1)
                {
                    itemList[index] = new Item(def, 1, index);
                }
                else
                {
                    Log.Information("没有空的物品槽");
                    return false;
                }
            }
            hasChanged = true;
            return true;
        }

        /// <summary>
        /// 交换物品位置
        /// </summary>
        /// <param name="index"></param>
        /// <param name="targetIndex"></param>
        /// <returns></returns>
        public bool Exchange(int index, int targetIndex)
        {
            var temp = itemList[index];
            itemList[index] = itemList[targetIndex];
            itemList[targetIndex] = temp;
            hasChanged = true;
            return true;
        }


        //移除指定数量的物品
        public int RemoveItem(int itemId, int amount = 1)
        {
            int removedAmount = 0;

            while (amount > 0)
            {
                var item = FindSameItem(itemId);

                if (item == null)
                {
                    break;
                }
                // 判断要移除的数量是否大于物品的当前数量
                int currentAmount = Math.Min(amount, item.amount);
                item.amount -= currentAmount;
                removedAmount += currentAmount;
                amount -= currentAmount;
                hasChanged = true;
                //清空物品槽
                if (item.amount == 0)
                {
                    itemList[itemList.IndexOf(item)] = null;
                }
            }

            return removedAmount;
        }



        //查找ID相同的物品
        private Item FindSameItem(int itemId)
        {
            return itemList.FirstOrDefault(item => item?.Id == itemId);
        }
        //查找ID相同且未满的物品
        private Item FindSameItemAndNotFull(int itemId)
        {
            return itemList.FirstOrDefault(item => item!=null 
            && item.Id==itemId && item.amount < item.Capicity);
        }
        //查找空的位置
        private int FindEmptyIndex()
        {
            return itemList.FindIndex(item => item is null);
        }
    }
}
