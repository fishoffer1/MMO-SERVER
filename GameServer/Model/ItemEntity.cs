using GameServer.Mgr;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using GameServer.InventorySystem;

namespace GameServer.Model
{
    /// <summary>
    /// 场景里的物品
    /// </summary>
    public class ItemEntity : Actor
    {
        //真正的物品对象
        public Item Item { get; set; }


        public ItemEntity(EntityType type, int tid, int level, Vector3Int position, Vector3Int direction) : base(type, tid, level, position, direction)
        {
        }

        /// <summary>
        /// 在场景里创建物品
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public static ItemEntity Create(Space space, Item item, Vector3Int pos, Vector3Int dir)
        {
            var entity = new ItemEntity(EntityType.Item,0,0, pos, dir);
            entity.Item = item;
            entity.Info.ItemInfo = entity.Item.ItemInfo;
            EntityManager.Instance.AddEntity(space.Id, entity);
            space.EntityEnter(entity);
            return entity;
        }

        public static ItemEntity Create(int spaceId, int itemId, int itemAmount, Vector3Int pos, Vector3Int dir)
        {
            Space space1 = SpaceManager.Instance.GetSpace(spaceId);
            var item = new Item(itemId, itemAmount);
            return ItemEntity.Create(space1,item,pos,dir);
        }

    }
}
