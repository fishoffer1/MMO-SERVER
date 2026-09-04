using Common.Database;
using FreeSql;
using GameServer.Model;
using Summer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GameServer.Mgr
{
    public class CharacterManager:Singleton<CharacterManager>
    {
        private ConcurrentDictionary<int, Character> Characters = new ConcurrentDictionary<int, Character>();

        IBaseRepository<DbCharacter> repo = Db.fsql.GetRepository<DbCharacter>();
        public CharacterManager()
        {
            //每隔2秒保存一次数据
            var repo = Db.fsql.GetRepository<DbCharacter>();
            Schedule.Instance.AddTask(Save, 2);

        }

        public Character CreateCharacter(DbCharacter dbchr)
        {
            Character chr = new Character(dbchr);
            Characters[chr.Id] = chr;
            EntityManager.Instance.AddEntity(dbchr.SpaceId ,chr);
            return chr;
        }

        public void RemoveCharacter(int chrId)
        {
           
                Character chr;
            if (Characters.TryRemove(chrId, out chr))
            {
                EntityManager.Instance.RemoveEntity(chr.Data.SpaceId, chr);
            }

            }

        public Character GetCharacter(int chrId)
        {
            return Characters.GetValueOrDefault(chrId, null);
        }

        public void Clear()
        {
            Characters.Clear();
        }

        private void Save()
        {
            foreach(var chr in Characters.Values)
            {
                repo.UpdateAsync(chr.Data);
            }
        }
    }
}
