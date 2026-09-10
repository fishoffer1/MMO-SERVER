using Proto;
using GameServer.Battle;
using GameServer.Model;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Mgr;

/// <summary>
/// 技能管理器，每个Actor都有独立的技能管理器
/// </summary>
public class SkillManager
{
    //归属的角色
    private Actor owner;
    //技能列表
    public List<Skill> Skills = new();

    public SkillManager(Actor owner)
    {
        this.owner = owner;
        this.InitSkills();
    }

    public void InitSkills()
    {
        //角色按单位类型(UnitDefine.TID)加载技能；怪物加载通用野怪技能(SkillDefine.TID==0)
        //正常是通过读取数据库来加载技能信息
        bool isMonster = this.owner is Monster;
        foreach (var def in DataManager.Instance.Skills.Values)
        {
            if (def.TID == this.owner.Define.TID || (isMonster && def.TID == 0))
            {
                loadSkill(def.Code);
            }
        }
    }

    private void loadSkill(params int[] ids)
    {
        foreach(int skid in ids)
        {
            var skill = new Skill(owner, skid);
            Skills.Add(skill);
            owner.Info.Skills.Add(new SkillInfo() { Id = skill.Def.Code });
            Log.Information("角色[{0}]加载技能[{1}-{2}]", owner.Name, skill.Def.ID, skill.Def.Name);
        }
    }

    public Skill GetSkill(int id)
    {
        return Skills.FirstOrDefault(s => s.Def.Code == id);
    }

    public void Update()
    {
        foreach (Skill skill in Skills)
        {
            skill.Update();
        }
    }
}
