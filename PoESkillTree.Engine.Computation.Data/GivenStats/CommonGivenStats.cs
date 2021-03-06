using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Charges;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.Computation.Data.Base;
using PoESkillTree.Engine.Computation.Data.Collections;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Data.GivenStats
{
    /// <summary>
    /// Given stats shared between all entities.
    /// </summary>
    public class CommonGivenStats : UsesStatBuilders, IGivenStats
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly Lazy<IReadOnlyList<IIntermediateModifier>> _lazyGivenStats;

        public CommonGivenStats(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
            _lazyGivenStats = new Lazy<IReadOnlyList<IIntermediateModifier>>(() => CreateCollection().ToList());
        }

        public IReadOnlyList<Entity> AffectedEntities { get; } = Enums.GetValues<Entity>().ToList();

        public IReadOnlyList<string> GivenStatLines { get; } = new[]
        {
            // other
            "100% of non-chaos damage is taken from energy shield before life",
        };

        public IReadOnlyList<IIntermediateModifier> GivenModifiers => _lazyGivenStats.Value;

        private GivenStatCollection CreateCollection() => new GivenStatCollection(_modifierBuilder, ValueFactory)
        {
            // pools
            { BaseSet, EnergyShield.Recharge, 20 },
            { BaseSet, Life.RecoveryRate, 1 },
            { BaseSet, Mana.RecoveryRate, 1 },
            { BaseSet, EnergyShield.RecoveryRate, 1 },
            { BaseSet, Life.Regen.TargetPool, (int) Pool.Life },
            { BaseSet, Mana.Regen.TargetPool, (int) Pool.Mana },
            { BaseSet, EnergyShield.Regen.TargetPool, (int) Pool.EnergyShield },
            { BaseSet, EnergyShield.Recharge.Start, 1 },
            { BaseSet, AllSkills.ReservationPool, (int) Pool.Mana },
            // flasks
            { BaseSet, Flask.Effect, 1 },
            { BaseSet, Flask.LifeRecoverySpeed, 1 },
            { BaseSet, Flask.ManaRecoverySpeed, 1 },
            // Damage Multiplier
            { BaseSet, AnyDamageType.DamageMultiplierWithNonCrits, 100 },
            { BaseSet, AnyDamageType.DamageMultiplierWithCrits.WithSkills, 100 },
            { BaseSet, AnyDamageType.DamageMultiplierWithCrits.WithAilments, 150 },
            // speed
            { BaseSet, Stat.ActionSpeed, 1 },
            { BaseSet, Stat.MovementSpeed, 1 },
            // crit
            { BaseSet, CriticalStrike.Chance.Maximum, 100 },
            { BaseSet, CriticalStrike.Chance.Minimum, 0 },
            { BaseSet, CriticalStrike.ExtraDamageTaken, 1 },
            // projectiles
            { BaseSet, Projectile.Count, 1 },
            // evasion
            { BaseSet, Evasion.Chance.Maximum, 95 },
            { BaseSet, Evasion.Chance.Minimum, 0 },
            { BaseSet, Stat.ChanceToHit.Maximum, 100 },
            { BaseSet, Stat.ChanceToHit.Minimum, 5 },
            // block
            { BaseSet, Block.AttackChance.Maximum, 75 },
            { BaseSet, Block.SpellChance.Maximum, 75 },
            // dodge
            { BaseSet, Stat.Dodge.AttackChance.Maximum, 75 },
            { BaseSet, Stat.Dodge.SpellChance.Maximum, 75 },
            // charges
            { BaseSet, Charge.Endurance.Amount.Maximum, 3 },
            { BaseSet, Charge.Frenzy.Amount.Maximum, 3 },
            { BaseSet, Charge.Power.Amount.Maximum, 3 },
            { BaseSet, Charge.From(ChargeType.GhostShroud).Amount.Maximum, 3 },
            { BaseSet, Charge.From(ChargeType.Inspiration).Amount.Maximum, 5 },
            { BaseSet, Charge.Endurance.Amount.Minimum, 0 },
            { BaseSet, Charge.Frenzy.Amount.Minimum, 0 },
            { BaseSet, Charge.Power.Amount.Minimum, 0 },
            { BaseSet, Charge.Endurance.Duration, 10 },
            { BaseSet, Charge.Frenzy.Duration, 10 },
            { BaseSet, Charge.Power.Duration, 10 },
            // leech
            { BaseSet, Life.Leech.RateLimit, 20 },
            { BaseSet, Mana.Leech.RateLimit, 20 },
            { BaseSet, EnergyShield.Leech.RateLimit, 10 },
            { BaseSet, Life.Leech.Rate, 2 },
            { BaseSet, Mana.Leech.Rate, 2 },
            { BaseSet, EnergyShield.Leech.Rate, 2 },
            { BaseSet, Life.Leech.MaximumRecoveryPerInstance, 10 },
            { BaseSet, Mana.Leech.MaximumRecoveryPerInstance, 10 },
            { BaseSet, EnergyShield.Leech.MaximumRecoveryPerInstance, 10 },
            // resistances
            { BaseSet, Elemental.Resistance.Maximum, 75 },
            { BaseSet, Chaos.Resistance.Maximum, 75 },
            { BaseSet, Elemental.Resistance.Maximum.Maximum, 90 },
            { BaseSet, Chaos.Resistance.Maximum.Maximum, 90 },
            { BaseSet, Damage.Taken, 1 },
            // ailments
            { BaseSet, Ailment.Ignite.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Ignite.Source(Fire), 1 },
            { BaseSet, Ailment.Ignite.Duration, 4 },
            { TotalOverride, Ailment.Ignite.CriticalStrikesAlwaysInflict, 1 },
            { PercentLess, Damage.With(Ailment.Ignite), 50 },
            { BaseSet, Ailment.Shock.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Shock.Source(Lightning), 1 },
            { BaseSet, Ailment.Shock.Duration, 2 },
            { TotalOverride, Ailment.Shock.CriticalStrikesAlwaysInflict, 1 },
            { BaseSet, Ailment.ShockEffect, 1 },
            { BaseSet, Ailment.Chill.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Chill.Source(Cold), 1 },
            { BaseSet, Ailment.Chill.Duration, 2 },
            { BaseAdd, Ailment.Chill.Chance, 100 },
            { BaseSet, Ailment.ChillEffect, 1 },
            { BaseSet, Ailment.Freeze.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Freeze.Source(Cold), 1 },
            { BaseSet, Ailment.Freeze.Duration, 0.06 },
            { TotalOverride, Ailment.Freeze.CriticalStrikesAlwaysInflict, 1 },
            { BaseSet, Ailment.Bleed.InstancesOn(Self).Maximum, 1 },
            { BaseSet, Ailment.Bleed.Source(Physical), 1 },
            { BaseSet, Ailment.Bleed.Duration, 5 },
            { TotalOverride, Ailment.Bleed.Chance.With(DamageSource.Spell), 0 },
            { TotalOverride, Ailment.Bleed.Chance.With(DamageSource.Secondary), 0 },
            { PercentLess, Damage.With(Ailment.Bleed), 30 },
            { BaseSet, Ailment.Poison.Source(Physical.And(Chaos)), 1 },
            { BaseSet, Ailment.Poison.Duration, 2 },
            { PercentLess, Damage.With(Ailment.Poison), 80 },
            // buffs
            { BaseSet, Effect.ExpirationModifier, 1 },
            { TotalOverride, Effect.ExpirationModifier.Minimum, 0.25 },
            { BaseSet, Buff.CurseLimit, 1 },
            { BaseSet, Buffs(Self).Effect, 1 },
            { BaseSet, Buff.Fortify.Duration, 4 },
            { BaseSet, Buff.Maim.Duration, 4 },
            { BaseSet, Buff.Taunt.Duration, 3 },
            { BaseSet, Buff.ArcaneSurge.Duration, 4 },
            { TotalOverride, Buff.Maim.Chance.With(DamageSource.Spell), 0 },
            { TotalOverride, Buff.Maim.Chance.With(DamageSource.Secondary), 0 },
            { TotalOverride, Buff.Hinder.Chance.With(DamageSource.Attack), 0 },
            { BaseSet, Buff.Impale.StackCount.Maximum, 5 },
            { BaseSet, Buff.Rampage.StackCount.Maximum, 1000 },
            { BaseSet, Buff.Withered.StackCount.Maximum, 15 },
            // stun
            { BaseSet, Effect.Stun.Threshold, 1 },
            { BaseSet, Effect.Stun.Recovery, 1 },
            { BaseSet, Effect.Stun.Duration, 0.35 },
            // other
            { TotalOverride, Stat.Level.Minimum, 1 },
            { TotalOverride, Stat.Level.Maximum, 100 },
            { BaseSet, Stat.AreaOfEffect, 1 },
            { BaseSet, Stat.CooldownRecoverySpeed, 1 },
            { BaseSet, Stat.SkillStage.Minimum, 0 },
            { BaseSet, Stat.AttachedBrands.Maximum, 1 },
            { BaseSet, Stat.BannerStage.Maximum, 50 },
            { BaseSet, Stat.Warcry.PowerMultiplier, 1 },
        };
    }
}