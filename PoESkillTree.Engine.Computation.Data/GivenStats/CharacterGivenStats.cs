using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Charges;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.Computation.Data.Base;
using PoESkillTree.Engine.Computation.Data.Collections;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;

namespace PoESkillTree.Engine.Computation.Data.GivenStats
{
    /// <summary>
    /// Given stats of player characters.
    /// </summary>
    /// <remarks>
    /// See http://pathofexile.gamepedia.com/Character and Metadata/Characters/Character.ot in GGPK.
    /// </remarks>
    public class CharacterGivenStats : UsesConditionBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;
        private readonly CharacterBaseStats _characterBaseStats;

        public CharacterGivenStats(
            IBuilderFactories builderFactories, IModifierBuilder modifierBuilder, CharacterBaseStats characterBaseStats)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _characterBaseStats = characterBaseStats;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = new[] { GameModel.Entity.Character };

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // while Dual Wielding
            "10% more Attack Speed while Dual Wielding",
            "+15% chance to block attack damage while Dual Wielding",
            // charges
            "4% additional Physical Damage Reduction per Endurance Charge",
            "+4% to all Elemental Resistances per Endurance Charge",
            "4% increased Attack and Cast Speed per Frenzy Charge",
            "4% more Damage per Frenzy Charge",
            "40% increased Critical Strike Chance per Power Charge",
            // level based
            "+12 to maximum Life per Level",
            "+2 to Accuracy Rating per Level",
            "+3 to Evasion Rating per Level",
            "+6 to maximum Mana per Level",
            // attribute conversions
            "+1 to maximum Life per 2 Strength",
            "+1 to Strength Damage Bonus per Strength",
            "1% increased Melee Physical Damage per 5 Strength Damage Bonus ceiled",
            "+2 to Accuracy Rating per 1 Dexterity",
            "+1 to Dexterity Evasion Bonus per Dexterity",
            "1% increased Evasion Rating per 5 Dexterity Evasion Bonus ceiled",
            "+1 to Mana per 2 Intelligence ceiled",
            "1% increased maximum Energy Shield per 5 Intelligence ceiled",
        };

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => Expand(new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            // passive points
            { BaseSet, Stat.PassivePoints.Maximum, Stat.Level.Value - 1 },
            { BaseAdd, Stat.PassivePoints.Maximum, 22 },
            { BaseSet, Stat.AscendancyPassivePoints.Maximum, 8 },
            // pools
            { BaseSet, Life, CharacterClassBased(_characterBaseStats.Life, "Life") },
            { BaseSet, Mana, CharacterClassBased(_characterBaseStats.Mana, "Mana") },
            { BaseSet, Mana.Regen.Percent, 1.75 },
            // other basic stats
            { BaseSet, Attribute.Strength, CharacterClassBased(_characterBaseStats.Strength, "Strength") },
            { BaseSet, Attribute.Dexterity, CharacterClassBased(_characterBaseStats.Dexterity, "Dexterity") },
            { BaseSet, Attribute.Intelligence, CharacterClassBased(_characterBaseStats.Intelligence, "Intelligence") },
            { BaseSet, Evasion, 53 },
            { BaseSet, Stat.Accuracy, -2 }, // 0 at level 1 with no dexterity
            { BaseSet, CriticalStrike.Multiplier, 150 },
            { BaseAdd, Ground.Consecrated.AddStat(Life.Regen), 6 },
            // resistances
            { BaseSet, AnyDamageType.DamageReduction.Maximum, 90 },
            { BaseSet, AnyDamageType.DamageReductionIncludingArmour.Maximum, 90 },
            // traps, mines and totems
            { BaseSet, Traps.CombinedInstances.Maximum, 15 },
            { BaseSet, Mines.CombinedInstances.Maximum, 15 },
            { BaseSet, Totems.CombinedInstances.Maximum, 1 },
            { BaseSet, Stat.Totem.BaseTime, 0.6 },
            { BaseSet, Stat.Trap.BaseTime, 0.5 },
            { BaseSet, Stat.Mine.BaseTime, 0.3 },
            // rage
            { BaseSet, Charge.Rage.Amount.Maximum, 50 },
            { BaseSet, Charge.RageEffect, 1 },
            {
                PercentIncrease, Damage.WithSkills(DamageSource.Attack),
                Charge.Rage.Amount.Value * Charge.RageEffect.Value
            },
            {
                PercentIncrease, Stat.CastRate.With(DamageSource.Attack),
                PerStat(Charge.Rage.Amount, 2) * Charge.RageEffect.Value
            },
            { PercentIncrease, Stat.MovementSpeed, PerStat(Charge.Rage.Amount, 5) * Charge.RageEffect.Value },
            { BaseSubtract, Life.Regen.Percent, 0.1 * Charge.Rage.Amount.Value * Charge.RageEffect.Value },
            // unarmed
            {
                BaseSet, Stat.Range,
                CharacterClassBased(_characterBaseStats.UnarmedRange, "UnarmedRange"), Not(MainHand.HasItem)
            },
            { BaseSet, CriticalStrike.Chance.With(AttackDamageHand.MainHand), 0, Not(MainHand.HasItem) },
            {
                BaseSet, Stat.BaseCastTime.With(AttackDamageHand.MainHand),
                CharacterClassBased(_characterBaseStats.UnarmedAttackTime, "UnarmedAttackTime") / 1000,
                Not(MainHand.HasItem)
            },
            {
                BaseSet, Physical.Damage.WithSkills.With(AttackDamageHand.MainHand),
                Stat.CharacterClass.Value.Select(
                    c => UnarmedPhysicalDamage((CharacterClass) (int) c.Single),
                    c => $"{c}.UnarmedPhysicalDamage"),
                Not(MainHand.HasItem)
            },
            { BaseSet, MainHand.ItemClass, (double) ItemClass.Unarmed, Not(MainHand.HasItem) },
            // buff configuration
            { TotalOverride, Buff.Onslaught.On(Self), 1, Condition.Unique("Onslaught.ExplicitlyActive") },
            { TotalOverride, Buff.UnholyMight.On(Self), 1, Condition.Unique("UnholyMight.ExplicitlyActive") },
            { TotalOverride, Buff.Fortify.On(Self), 1, Condition.Unique("Fortify.ExplicitlyActive") },
            { TotalOverride, Buff.Tailwind.On(Self), 1, Condition.Unique("Tailwind.ExplicitlyActive") },
            { TotalOverride, Buff.Infusion.On(Self), 1, Condition.Unique("Infusion.ExplicitlyActive") },
            { TotalOverride, Buff.Elusive.On(Self), 1, Condition.Unique("Elusive.ExplicitlyActive") },
            { PercentReduce, Buff.Elusive.Effect, 20 * Stat.UniqueAmount("Elusive.SecondsOfDecay", 2.5) },
            // character class connections
            {
                TotalOverride, PassiveTree.ConnectsToClass(CharacterClass.Scion), 1,
                Stat.CharacterClass.Value.Eq((int) CharacterClass.Scion)
            },
            {
                TotalOverride, PassiveTree.ConnectsToClass(CharacterClass.Marauder), 1,
                Stat.CharacterClass.Value.Eq((int) CharacterClass.Marauder)
            },
            {
                TotalOverride, PassiveTree.ConnectsToClass(CharacterClass.Ranger), 1,
                Stat.CharacterClass.Value.Eq((int) CharacterClass.Ranger)
            },
            {
                TotalOverride, PassiveTree.ConnectsToClass(CharacterClass.Witch), 1,
                Stat.CharacterClass.Value.Eq((int) CharacterClass.Witch)
            },
            {
                TotalOverride, PassiveTree.ConnectsToClass(CharacterClass.Duelist), 1,
                Stat.CharacterClass.Value.Eq((int) CharacterClass.Duelist)
            },
            {
                TotalOverride, PassiveTree.ConnectsToClass(CharacterClass.Templar), 1,
                Stat.CharacterClass.Value.Eq((int) CharacterClass.Templar)
            },
            {
                TotalOverride, PassiveTree.ConnectsToClass(CharacterClass.Shadow), 1,
                Stat.CharacterClass.Value.Eq((int) CharacterClass.Shadow)
            },
        });

        private GivenStatCollection Expand(GivenStatCollection coll)
        {
            // charge configuration
            foreach (var chargeType in Enums.GetValues<ChargeType>())
            {
                var chargeStat = Charge.From(chargeType);
                coll.Add(TotalOverride, chargeStat.Amount, chargeStat.Amount.Maximum.Value,
                    Condition.Unique($"{chargeType}.Charge.Amount.SetToMaximum"));
            }
            return coll;
        }

        private ValueBuilder CharacterClassBased(Func<CharacterClass, int> selector, string identity)
            => Stat.CharacterClass.Value.Select(v => selector((CharacterClass) (int) v), v => $"{v}.{identity}");

        private static ValueBuilder PerStat(IStatBuilder stat, double divideBy)
            => (stat.Value / divideBy).Floor();

        private NodeValue UnarmedPhysicalDamage(CharacterClass c)
            => new NodeValue(_characterBaseStats.UnarmedPhysicalDamageMinimum(c),
                _characterBaseStats.UnarmedPhysicalDamageMaximum(c));
    }
}