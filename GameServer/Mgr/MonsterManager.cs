using GameServer.Model;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    /// <summary>
    /// 每个地图都有怪物管理器
    /// </summary>
    public class MonsterManager 
    {
        private Space _space;
        //<entityId,Monster>
        private Dictionary<int, Monster> _dict = new Dictionary<int, Monster>();

        public void Init(Space space)
        {
            this._space = space;
        }
        /// <summary>
        /// 创建怪物
        /// </summary>
        /// <param name="tid"></param>
        /// <param name="level"></param>
        /// <param name="pos"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public Monster Create(int tid, int level, Vector3Int pos, Vector3Int dir)
        {
            Monster monster = new Monster(tid, level, pos, dir);
            EntityManager.Instance.AddEntity(_space.Id, monster);
            monster.Info.SpaceId = _space.Id;
            monster.Info.EntityId= monster.entityId;
            _dict[monster.entityId] = monster;
            //和entityId保持一致
            monster.Id = monster.entityId;
            this._space.MonsterEnter(monster);
            return monster;
        }
    }
}
