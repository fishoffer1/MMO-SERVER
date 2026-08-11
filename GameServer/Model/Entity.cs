using Common.Proto;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Model
{
    //世界进行同步的实体
    public class Entity
    {
        private int _entityid;
        private Vector3Int position;    //位置
        private Vector3Int direction;   //方向
        
        public int entityId { get { return _entityid; } }

        public Vector3Int Position 
        { 
            get { return position; } 
            set { position = value; }
        }
        public Vector3Int Direction
        {
            get { return direction; }
            set { direction = value; }
        }

        public Entity(int id, Vector3Int position, Vector3Int direction)
        {
            this._entityid = id;
            this.position = position;
            this.direction = direction;
        }

        public NEntity GetData()
        {
            var data = new NEntity();
            data.Id = this.entityId;
            data.Position = new NVector3() { X = position.x, Y = position.y, Z = position.z };
            data.Rotation = new NVector3() { X = direction.x, Y = -direction.y, Z = -direction.z };
            return data;
        }
    }
}
