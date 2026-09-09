using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Battle
{
    /// <summary>
    /// 属性组装
    /// </summary>
    public class AttributesAssembly
    {
        private Attributes Basic;    //基础属性（初始+成长）
        private Attributes Equip;    //装备属性
        private Attributes Buffs;    //Buff属性
        public  Attributes Final;    //最终属性


        public void Init(Actor actor)
        {

            Basic = new Attributes();
            Equip = new Attributes();
            Buffs = new Attributes();
            Final = new Attributes();

            var define = actor.Define;
            var level = actor.Info.Level;

            //初始化属性
            var Initial = new Attributes();
            Initial.Speed = define.Speed;
            Initial.HPMax = define.HPMax;
            Initial.MPMax = define.MPMax;
            Initial.AD = define.AD;
            Initial.AP = define.AP;
            Initial.DEF = define.DEF;
            Initial.MDEF = define.MDEF;
            Initial.CRI = define.CRI;
            Initial.CRD = define.CRD;
            Initial.STR = define.STR;
            Initial.INT = define.INT;
            Initial.AGI = define.AGI;
            Initial.HitRate = define.HitRate;
            Initial.DodgeRate = define.DodgeRate;
            Initial.HpRegen = define.HpRegen;
            Initial.HpSteal = define.HpSteal;

            //成长属性
            var Growth = new Attributes();
            Growth.STR = define.GSTR * level;// 力量成长
            Growth.INT = define.GINT * level;// 智力成长
            Growth.AGI = define.GAGI * level;// 敏捷成长

            //基础属性（初始+成长）
            Basic.Add( Initial );
            Basic.Add( Growth );

            //todo 处理装备和buff

            //合并到最终属性
            Final.Add(Basic);
            Final.Add(Equip);
            Final.Add(Buffs);

            //附加属性
            var Extra = new Attributes();
            Extra.HPMax = Final.STR * 5;
            Extra.AP = Final.INT * 1.5f;
            Final.Add(Extra);
            /*
            Log.Information("初始属性：{0}", Initial);
            Log.Information("成长属性：{0}", Growth);
            Log.Information("装备属性：{0}", Equip);
            Log.Information("Buff属性：{0}", Buffs);
            Log.Information("属性附加：{0}", Extra);
            
            */
            Log.Information("最终属性：{0}", Final);
        }
    }
}
