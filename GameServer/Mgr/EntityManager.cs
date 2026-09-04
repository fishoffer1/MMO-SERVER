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
    /// <summary>
    /// Entity管理器(角色，怪物，Npc，陷阱)
    /// </summary>
    public class EntityManager:Singleton<EntityManager>
    {
        private int index = 1;
        //记录全部的Entity对象，<EntityId,Entity>
        private Dictionary<int, Model.Entity> AllEntities = new Dictionary<int, Model.Entity>();
        //记录场景里的Entity列表，<SpaceId,EntityList>
        private Dictionary<int, List<Entity>> SpaceEntities = new Dictionary<int, List<Entity>>();
        
        public EntityManager() { }

        public void AddEntity(int spaceId, Entity entity)
        {
            lock (this)
            {
                //统一管理的对象分配ID
                entity.EntityData.Id = NewEntityId();
                AllEntities[entity.entityId] = entity;
                if (!SpaceEntities.ContainsKey(spaceId))
                {
                    SpaceEntities[spaceId] = new List<Entity>();
                }
                SpaceEntities[spaceId].Add(entity);
            }
        }

        public void RemoveEntity(int spaceId,Entity entity)
        {
            lock (this)
            {
                AllEntities.Remove(entity.entityId);
                SpaceEntities[spaceId].Remove(entity);
            }
        }

        public Entity GetEntity(int entityId)
        {
           return AllEntities.GetValueOrDefault(entityId, null);
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

