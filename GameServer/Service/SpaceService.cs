using Common;
using GameServer.Core;
using GameServer.Mgr;
using GameServer.Model;
using Proto;
using Serilog;
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
       
        public void Start()
        {
            //初始化地图
            SpaceManager.Instance.Init();
            //位置同步请求
            MessageRouter.Instance.Subscribe<SpaceEntitySyncRequest>(_SpaceEntitySyncRequest);
            
            
        }
        public Space? GetSpace(int spaceId)
        {
            return SpaceManager.Instance.GetSpace(spaceId);
        }

        private void _SpaceEntitySyncRequest(Connection conn, SpaceEntitySyncRequest msg)
        {
            //获取当前角色所在的地图
            var space = conn.Get<Session>().Space;
            if (space == null) return;

            //同步请求信息
            NetEntity netEntity = msg.EntitySync.Entity;
            //服务端实际的角色信息
            Entity serEntity = EntityManager.Instance.GetEntity(netEntity.Id);
            //计算距离
            float dist = Vector3Int.Distance(netEntity.Position, serEntity.Position);
            //使用服务器移动速度
            netEntity.Speed = serEntity.Speed;
            //计算时间差
            float dt = Math.Min(serEntity.PositionTime, 1.5f);
            //计算限额
            float limit = serEntity.Speed * dt * 2f;
            Log.Information("距离{0},阈值{1},间隔{2}", dist, limit, dt);
            if (float.IsNaN(dist) || dist > limit)
            {
                //拉回原位置
                SpaceEntitySyncResponse resp = new SpaceEntitySyncResponse();
                resp.EntitySync = new NetEntitySync();
                resp.EntitySync.Entity = serEntity.EntityData;
                resp.EntitySync.Force = true;
                conn.Send(resp);
                return;
            }

            //广播同步信息
            space.UpdateEntity(msg.EntitySync);
        }
    }
}
