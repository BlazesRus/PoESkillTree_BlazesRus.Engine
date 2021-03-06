using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Engine.GameModel.Skills
{
    /// <summary>
    /// Keywords of gems as they are required for parsing. They are mostly derived from the skill's active skill types.
    /// Some are derived from its gem tags and some from its name.
    /// </summary>
    public enum Keyword
    {
        /// <summary>
        /// Equivalent to the ActiveSkillType "attack".
        /// </summary>
        Attack,

        /// <summary>
        /// Equivalent to the ActiveSkillType "spell".
        /// </summary>
        Spell,

        /// <summary>
        /// Equivalent to the union of the ActiveSkillTypes "projectile" and "explicit_deals_projectile_damage"
        /// (skills have this keyword if they have at least one of the two types).
        /// </summary>
        Projectile,

        /// <summary>
        /// Equivalent to the ActiveSkillType "aoe".
        /// </summary>
        AreaOfEffect,

        /// <summary>
        /// Equivalent to the ActiveSkillType "melee".
        /// </summary>
        Melee,

        /// <summary>
        /// Equivalent to the ActiveSkillType "totem".
        /// </summary>
        Totem,

        /// <summary>
        /// Equivalent to the ActiveSkillType "curse".
        /// </summary>
        Curse,

        /// <summary>
        /// Equivalent to the ActiveSkillType "trap".
        /// </summary>
        Trap,

        /// <summary>
        /// Equivalent to the ActiveSkillType "movement".
        /// </summary>
        Movement,

        /// <summary>
        /// Equivalent to the ActiveSkillType "mine".
        /// </summary>
        Mine,

        /// <summary>
        /// Equivalent to the ActiveSkillType "vaal".
        /// </summary>
        Vaal,

        /// <summary>
        /// Equivalent to the ActiveSkillType "aura".
        /// </summary>
        Aura,

        /// <summary>
        /// Equivalent to the ActiveSkillType "golem".
        /// </summary>
        Golem,

        /// <summary>
        /// Equivalent to the ActiveSkillType "minion".
        /// </summary>
        Minion,

        /// <summary>
        /// Equivalent to the gem tag "Warcry".
        /// </summary>
        Warcry,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Herald,

        /// <summary>
        /// Has no equivalent gem tag or ActiveSkillType.
        /// </summary>
        Offering,

        /// <summary>
        /// Equivalent to the ActiveSkillType "trigger_attack"
        /// </summary>
        CounterAttack,

        /// <summary>
        /// Equivalent to the gem tag.
        /// </summary>
        Bow,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Physical,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Lightning,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Cold,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Fire,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Chaos,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Brand,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Channelling,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Guard,

        /// <summary>
        /// Has no equivalent gem tag or ActiveSkillType.
        /// </summary>
        Banner,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Ballista,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Triggered,

        /// <summary>
        /// Equivalent to the ActiveSkillType.
        /// </summary>
        Travel,

        /// <summary>
        /// Equivalent to the gem tag.
        /// </summary>
        Stance,
    }

    public static class KeywordExtensions
    {
        private delegate bool KeywordApplies(
            string skillDisplayName, IReadOnlyCollection<string> activeSkillTypes, IReadOnlyCollection<string> gemTags);

        private static readonly IReadOnlyDictionary<Keyword, KeywordApplies> Conditions =
            new Dictionary<Keyword, KeywordApplies>
            {
                { Keyword.Attack, (_, types, __) => types.Contains(ActiveSkillType.Attack) },
                { Keyword.Spell, (_, types, __) => types.Contains(ActiveSkillType.Spell) },
                {
                    Keyword.Projectile, (_, types, __)
                        => types.ContainsAny(ActiveSkillType.Projectile, ActiveSkillType.ExplicitProjectileDamage)
                },
                { Keyword.AreaOfEffect, (_, types, __) => types.Contains(ActiveSkillType.AreaOfEffect) },
                { Keyword.Melee, (_, types, __) => types.Contains(ActiveSkillType.Melee) },
                { Keyword.Totem, (_, types, __) => types.Contains(ActiveSkillType.Totem) },
                { Keyword.Curse, (_, types, __) => types.Contains(ActiveSkillType.Curse) },
                { Keyword.Trap, (_, types, __) => types.Contains(ActiveSkillType.Trap) },
                { Keyword.Movement, (_, types, __) => types.Contains(ActiveSkillType.Movement) },
                { Keyword.Mine, (_, types, __) => types.Contains(ActiveSkillType.Mine) },
                { Keyword.Vaal, (_, types, __) => types.Contains(ActiveSkillType.Vaal) },
                { Keyword.Aura, (_, types, __) => types.Contains(ActiveSkillType.Aura) },
                { Keyword.Golem, (_, types, __) => types.Contains(ActiveSkillType.Golem) },
                { Keyword.Minion, (_, types, __) => types.Contains(ActiveSkillType.Minion) },
                { Keyword.Warcry, (_, __, tags) => tags.Contains("warcry") },
                { Keyword.Herald, (_, types, __) => types.Contains(ActiveSkillType.Herald) },
                { Keyword.Offering, (name, _, __) => name.EndsWith("Offering") },
                { Keyword.CounterAttack, (_, types, __) => types.Contains(ActiveSkillType.Attack) && types.Contains(ActiveSkillType.Triggered) },
                { Keyword.Bow, (_, __, tags) => tags.Contains("bow") },
                { Keyword.Physical, (_, types, __) => types.Contains(ActiveSkillType.Physical) },
                { Keyword.Lightning, (_, types, __) => types.Contains(ActiveSkillType.Lightning) },
                { Keyword.Cold, (_, types, __) => types.Contains(ActiveSkillType.Cold) },
                { Keyword.Fire, (_, types, __) => types.Contains(ActiveSkillType.Fire) },
                { Keyword.Chaos, (_, types, __) => types.Contains(ActiveSkillType.Chaos) },
                { Keyword.Brand, (_, types, __) => types.Contains(ActiveSkillType.Brand) },
                { Keyword.Channelling, (_, types, __) => types.Contains(ActiveSkillType.Channelling) },
                { Keyword.Guard, (_, types, __) => types.Contains(ActiveSkillType.Guard) },
                { Keyword.Banner, (_, types, __) => types.Contains(ActiveSkillType.Banner) },
                { Keyword.Ballista, (_, types, __) => types.Contains(ActiveSkillType.Ballista) },
                { Keyword.Triggered, (_, types, __) => types.Contains(ActiveSkillType.Triggered) },
                { Keyword.Travel, (_, types, __) => types.Contains(ActiveSkillType.Travel) },
                { Keyword.Stance, (_, __, tags) => tags.Contains("stance") },
            };

        public static bool IsOnSkill(this Keyword @this,
            string skillDisplayName, IReadOnlyCollection<string> activeSkillTypes, IReadOnlyCollection<string> gemTags)
            => Conditions[@this](skillDisplayName, activeSkillTypes, gemTags);
    }
}