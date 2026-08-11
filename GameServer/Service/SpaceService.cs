using Common.Proto;
using GameServer.Model;
using Summer;
using Summer.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Service
{
    /// <summary>
    /// 地图服务
    /// </summary>
    public class SpaceService :Singleton<SpaceService>
    {
        //地图字典
        private Dictionary<int, Space> spaceDict = new Dictionary<int, Space>();
        public void Start()
        {
            //位置同步请求
            MessageRouter.Instance.Subscribe<SpaceEntitySyncRequest>(_SpaceEntitySyncRequest);
            Space space = new Space();
            space.Name = "测试空间";
            space.Id = 3;
            spaceDict[space.Id] = space;
        }
        public Space GetSpace(int spaceId)
        {
            return spaceDict[spaceId];
        }

        public Space GetConnSpace(Connection conn)
        {
            foreach (Space space in spaceDict.Values)
            {
                if (space.HasConnection(conn))
                {
                    return space;
                }
            }
            return null;

        }

        private void _SpaceEntitySyncRequest(Connection conn, SpaceEntitySyncRequest msg)
        {
            //通过conn拿到角色
            Space space = GetConnSpace(conn);
            space.UpdateEntity(msg.EntitySync);
        }
    }
}
