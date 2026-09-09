using GameServer.Mgr;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Core
{
    public class Game
    {
        public static Actor GetUnit(int entityId)
        {
            return EntityManager.Instance.GetEntity(entityId) as Actor;
        }
        /// <summary>
        /// 查找范围内的人物
        /// </summary>
        /// <param name="spaceId"></param>
        /// <param name="position"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static List<Actor> RangeUnit(int spaceId, Vector3 position, int range)
        {
            Predicate<Actor> match = (e) => {
                return Vector3Int.Distance(position, e.Position) <= range;
            };
            return EntityManager.Instance.GetEntityList(spaceId, match);
        }
    }
}
