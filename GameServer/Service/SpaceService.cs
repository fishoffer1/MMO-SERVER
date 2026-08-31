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
            
            //测试空间场景对象
            Space space = new Space();
            space.Name = "测试空间";
            space.Id = 3;
            spaceDict[space.Id] = space;
        }
        public Space? GetSpace(int spaceId)
        {
            return spaceDict[spaceId];
        }

        private void _SpaceEntitySyncRequest(Connection conn, SpaceEntitySyncRequest msg)
        {
            //获取当前角色
            var sp = conn.Get<Space>();
            if (sp == null) return;
            sp.UpdateEntity(msg.EntitySync);
        }
    }
}
