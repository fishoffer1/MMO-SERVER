using Common.Proto;
using GameServer.Mgr;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    public class Actor : Entity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Speed { get; set; }
        public NCharacter Info { get; set; } = new NCharacter();
        public Actor(int entityId, Vector3Int position, Vector3Int direction) : base(entityId, position, direction)
        {
            
        }
    }
}
