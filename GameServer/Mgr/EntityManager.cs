using Common.Proto;
using GameServer.Model;
using Summer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr
{
    internal class EntityManager:Singleton<EntityManager>
    {
        private int index = 1;
        private Dictionary<int, Model.Entity> AllEntities = new Dictionary<int, Model.Entity>();

        public Entity CreateEntity() 
        {
            var entity = new Entity(index++, Vector3Int.zero , Vector3Int.zero );
            AllEntities[entity.entityId] = entity;
            return entity;
        }
        public int NewEntityId()
        {
            lock(this)
            {
                    return index++; 
                }

           }
            
        }
    }

