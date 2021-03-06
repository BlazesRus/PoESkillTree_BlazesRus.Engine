using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Contains stat builders that do not partake in parsing but are relevant for calculations.
    /// </summary>
    /// <remarks>
    /// "EffectiveChance" stats are probabilities between 0 and 1. "Chance" stats are percentages between 0 and 100,
    /// same as for non-meta stats.
    /// </remarks>
    public interface IMetaStatBuilders
    {
        IStatBuilder EffectiveRegen(Pool pool);
        IStatBuilder EffectiveDegeneration(Pool pool, DamageType damageType);
        IStatBuilder EffectiveDegeneration(Pool pool);
        IStatBuilder NetRegen(Pool pool);

        IStatBuilder EffectiveRecharge(Pool pool);
        IStatBuilder RechargeStartDelay(Pool pool);

        IStatBuilder EffectiveLeechRate(Pool pool);
        IStatBuilder AbsoluteLeechRate(Pool pool);
        IStatBuilder AbsoluteLeechRateLimit(Pool pool);
        IStatBuilder TimeToReachLeechRateLimit(Pool pool);

        /// <summary>
        /// The value of the pool that is the target pool of <see cref="sourcePool"/>'s regen.
        /// </summary>
        ValueBuilder RegenTargetPoolValue(Pool sourcePool);

        // Damage calculation
        IDamageRelatedStatBuilder Damage(DamageType damageType); // like DamageStatBuilder, but !canApplyToSkillDamage
        IDamageRelatedStatBuilder EnemyDamageReductionFromArmourAgainstNonCrits { get; } // with hits
        IDamageRelatedStatBuilder EnemyDamageReductionFromArmourAgainstCrits { get; } // with hits
        IDamageRelatedStatBuilder EnemyResistanceAgainstNonCrits(DamageType damageType); // with hits
        IDamageRelatedStatBuilder EnemyResistanceAgainstCrits(DamageType damageType); // with hits
        IDamageRelatedStatBuilder EffectiveDamageMultiplierWithNonCrits(DamageType damageType);
        IDamageRelatedStatBuilder EffectiveDamageMultiplierWithCrits(DamageType damageType); // with hits and ailments
        IDamageRelatedStatBuilder DamageWithNonCrits(DamageType damageType);
        IDamageRelatedStatBuilder DamageWithCrits(DamageType damageType);
        IDamageRelatedStatBuilder DamageWithNonCrits();
        IDamageRelatedStatBuilder DamageWithCrits();
        IDamageRelatedStatBuilder AverageDamagePerHit { get; } // with hits
        IDamageRelatedStatBuilder AverageDamage { get; }
        IStatBuilder AverageHitDamage { get; }
        IStatBuilder SkillDpsWithHits { get; }
        IStatBuilder SkillDpsWithDoTs { get; }
        IStatBuilder AverageAilmentDamage(Ailment ailment);
        IStatBuilder AilmentInstanceLifetimeDamage(Ailment ailment);
        IStatBuilder AilmentDps(Ailment ailment);

        IStatBuilder ImpaleRecordedDamage { get; }
        IDamageRelatedStatBuilder EnemyResistanceAgainstNonCritImpales { get; } // with hits
        IDamageRelatedStatBuilder EnemyResistanceAgainstCritImpales { get; } // with hits
        IDamageRelatedStatBuilder ImpaleDamageMultiplier { get; } // with hits
        IDamageRelatedStatBuilder EffectiveImpaleDamageMultiplierAgainstNonCrits { get; } // with hits
        IDamageRelatedStatBuilder EffectiveImpaleDamageMultiplierAgainstCrits { get; } // with hits

        IStatBuilder CastRate { get; }
        IStatBuilder CastTime { get; }

        IStatBuilder AilmentDealtDamageType(Ailment ailment);
        IDamageRelatedStatBuilder AilmentChanceWithCrits(Ailment ailment);
        IDamageRelatedStatBuilder AilmentEffectiveChance(Ailment ailment);
        IStatBuilder AilmentCombinedEffectiveChance(Ailment ailment);
        IStatBuilder AilmentEffectiveInstances(Ailment ailment);

        IDamageRelatedStatBuilder EffectiveCritChance { get; }

        IStatBuilder MitigationAgainstHits(DamageType damageType);
        IStatBuilder MitigationAgainstDoTs(DamageType damageType);
        IStatBuilder ChanceToAvoidMeleeAttacks { get; }
        IStatBuilder ChanceToAvoidProjectileAttacks { get; }
        IStatBuilder ChanceToAvoidSpells { get; }

        IDamageRelatedStatBuilder EffectiveStunThreshold { get; }
        IStatBuilder StunAvoidanceWhileCasting { get; }

        IStatBuilder SkillHitDamageSource { get; }
        IStatBuilder SkillUsesHand(AttackDamageHand hand);
        IStatBuilder SkillDoubleHitsWhenDualWielding { get; }
        IStatBuilder SkillDpsWithHitsCalculationMode { get; }

        IStatBuilder MainSkillId { get; }
        IStatBuilder MainSkillHasKeyword(Keyword keyword);
        IStatBuilder MainSkillPartHasKeyword(Keyword keyword);
        IStatBuilder MainSkillPartCastRateHasKeyword(Keyword keyword);
        IStatBuilder MainSkillPartDamageHasKeyword(Keyword keyword, DamageSource damageSource);
        IStatBuilder MainSkillPartAilmentDamageHasKeyword(Keyword keyword);

        IStatBuilder ActiveSkillItemSlot(string skillId);
        IStatBuilder ActiveSkillSocketIndex(string skillId);

        IStatBuilder MainSkillItemSlot { get; }
        IStatBuilder MainSkillSocketIndex { get; }
        IStatBuilder MainSkillSkillIndex { get; }

        IStatBuilder SkillBaseCost(Skill skill);
        IStatBuilder SkillHasType(Skill skill, string activeSkillType);
        IStatBuilder SkillIsEnabled(Skill skill);

        IStatBuilder ActiveCurses { get; }
        ValueBuilder ActiveCurseIndex(int numericSkillId);

        IStatBuilder DamageBaseAddEffectiveness { get; }
        IStatBuilder DamageBaseSetEffectiveness { get; }

        IStatBuilder CanBypassSkillCooldown { get; }

        IStatBuilder SelectedBandit { get; }
        IStatBuilder SelectedQuestPart { get; }
        IStatBuilder SelectedBossType { get; }
    }

    public static class MetaStatBuildersExtensions
    {
        public static IConditionBuilder IsActiveSkill(this IMetaStatBuilders @this, Skill skill)
        {
            var activeSkillItemSlot = @this.ActiveSkillItemSlot(skill.Id);
            var activeSkillSocketIndex = @this.ActiveSkillSocketIndex(skill.Id);
            return activeSkillItemSlot.Value.Eq((double) skill.ItemSlot)
                .And(activeSkillSocketIndex.Value.Eq(skill.SocketIndex));
        }

        public static IConditionBuilder IsMainSkill(this IMetaStatBuilders @this, Skill skill)
            => @this.MainSkillItemSlot.Value.Eq((double) skill.ItemSlot)
                .And(@this.MainSkillSocketIndex.Value.Eq(skill.SocketIndex))
                .And(@this.MainSkillSkillIndex.Value.Eq(skill.SkillIndex));
    }
}