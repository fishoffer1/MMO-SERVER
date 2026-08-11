using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    internal class Monster : Actor
    {
        public Monster(int id, Vector3Int position, Vector3Int direction) : base(id, position, direction)
        {

        }
    }
}
