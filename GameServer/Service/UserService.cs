using Common.Database;
using Common.Proto;
using GameServer.Mgr;
using GameServer.Model;
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
    /// 玩家服务
    /// 注册，登录，登出，角色创建，角色删除，角色选择
    /// </summary>
    internal class UserService : Singleton<UserService>
    {
       
        public void Start()
        {
            MessageRouter.Instance.Subscribe<GameEnterRequest>(_GameEnterRequest);
            MessageRouter.Instance.Subscribe<UserLoginRequest>(_UserLoginRequest);
            MessageRouter.Instance.Subscribe<CharacterCreateRequest>(_CharacterCreateRequest);
            MessageRouter.Instance.Subscribe<CharacterListRequest>(_CharacterListRequest);  
            MessageRouter.Instance.Subscribe<CharacterDeleteRequest>(_CharacterDeleteRequest);  
            
            
        }
        /// <summary>
        /// 删除角色的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _CharacterDeleteRequest(Connection conn, CharacterDeleteRequest msg)
        {
            var player = conn.Get<DbPlayer>();
            Db.fsql.Delete<DbCharacter>()
                    .Where(t=>t.Id == msg.CharacterId)
                    .Where(t=>t.PlayerId == player.Id)
                    .ExecuteAffrows();
            //给客户端响应
            CharacterDeleteResponse cdr = new CharacterDeleteResponse();
            cdr.Success = true;
            cdr.Message = "执行完成";
            conn.Send(cdr);
        }

        /// <summary>
        /// 查询角色列表的请求
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="msg"></param>
        private void _CharacterListRequest(Connection conn, CharacterListRequest msg)
        {
            var player = conn.Get<DbPlayer>();
            //从数据库查询出当前玩家的全部角色
            var list = Db.fsql.Select<DbCharacter>().Where(t => t.PlayerId == player.Id).ToList();
            CharacterListResponse listResp = new CharacterListResponse();
            foreach (var item in list)  
            {
                listResp.CharacterList.Add(new NCharacter()
                {
                    Id = item.Id,
                    Name = item.Name,
                    TypeId = item.JobId,
                    //EntityId
                    Level = item.Level,
                    Exp = item.Exp,
                    SpaceId = item.SpaceId,
                    Gold = item.Gold,
                    //NEntity
                });
            }
            conn.Send(listResp);    
        }

        /// <summary>
        /// 创建角色
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="msg"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void _CharacterCreateRequest(Connection conn, CharacterCreateRequest msg)
        {
            CharacterCreateResponse resp = new CharacterCreateResponse();
            Log.Information("创建角色:{0}", msg);
           var player =  conn.Get<DbPlayer>();
            if(player == null)
            {
                resp.Success = false;
                resp.Message = "未登录，不能创建角色";
                conn.Send(resp);
                //未登录，不能创建角色
            }
            long count = Db.fsql.Select<DbCharacter>().Where(t => t.PlayerId.Equals(player.Id)).Count();  
            if(count >= 4)
            {
                //角色数量最多4个
                Log.Information("创建角色失败，角色数量最多4个");
                resp.Success = false;
                resp.Message = "创建角色失败，角色数量最多4个";
                conn.Send(resp);
                return;
            }

            //判断角色名是否为空
            if (string.IsNullOrWhiteSpace(msg.Name))
            {
                Log.Information("创建角色失败，角色名不能为空");
                resp.Success = false;
                resp.Message = "创建角色失败，角色名不能为空";
                conn.Send(resp);
                return;
            }
            string name = msg.Name.Trim();
            if (name.Length > 20)
            {
                Log.Information("创建角色失败，超过名字最大长度");
                resp.Success = false;
                resp.Message = "创建角色失败，超过名字最大长度";
                conn.Send(resp);
                return;
            }

            if(Db.fsql.Select<DbCharacter>().Where(t=> t.Name.Equals(name)).Count() > 0)
            {
                Log.Information("创建角色失败，名字已经被占用");
                resp.Success = false;
                resp.Message = "创建角色失败，名字已经被占用";
                conn.Send(resp);
                return;
            }
            DbCharacter dc = new DbCharacter()
            {
                Name = msg.Name,
                JobId = msg.JobType,
                Hp = 100,
                Mp = 100,
                Level = 1,
                Exp = 0,
                SpaceId = 6,
                Gold = 0,
                PlayerId = player.Id
            };
            int aff = Db.fsql.Insert(dc).ExecuteAffrows();
            if(aff> 0)
            {
                resp.Success = true;
                resp.Message = "创建角色成功";
                conn.Send(resp);
            }
        }

        private void _UserLoginRequest(Connection conn, UserLoginRequest msg)
        {
           var dbPlayer= Db.fsql.Select<DbPlayer>()
                            .Where(p => p.UserName == msg.Username)
                            .Where(p => p.Password == msg.Password)
                            .First();
            Log.Information("登录结果：" + dbPlayer);
            UserLoginResponse resp = new UserLoginResponse();
            if(dbPlayer != null)
            {
                
                resp.Success = true; // 登录成功
                resp.Message = "登录成功";
                conn.Set<DbPlayer>(dbPlayer); //登录成功，在conn里记录用户信息
            }
            else
            {
                resp.Success = false;
                resp.Message = "用户名或密码不正确";
            }
            conn.Send(resp);
        }

        private void _GameEnterRequest(Connection conn, GameEnterRequest msg)
        {

            Log.Information($"收到玩家进入游戏请求，角色ID：{msg.CharacterId}");
            
            //获取当前玩家
            var player = conn.Get<DbPlayer>();
            //查询数据库的角色
            var dbRole = Db.fsql.Select<DbCharacter>()
                .Where(t => t.PlayerId == player.Id)
                .Where(t => t.Id == msg.CharacterId)
                .First();
            //把数据库角色变成游戏角色
            Character character = dbRole;

            //通知玩家登录成功
            GameEnterResponse resp = new GameEnterResponse();
            resp.Success = true;
            resp.Entity = character.GetData();
            resp.Character = character.Info;
            conn.Send(resp);
            //将新角色加入到地图
            var space = SpaceService.Instance.GetSpace(3);
            space.CharacterJoin(conn, character);
        }
    }
}
