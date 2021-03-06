using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MoreLinq;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Skills;
using static PoESkillTree.Engine.Computation.Common.ExplicitRegistrationTypes;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    internal class StatBuilders : StatBuildersBase, IStatBuilders
    {
        public StatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Level => FromIdentity(typeof(uint));
        public IStatBuilder CharacterClass => FromIdentity(typeof(CharacterClass));
        public IStatBuilder PassivePoints => FromIdentity(typeof(uint));
        public IStatBuilder AscendancyPassivePoints => FromIdentity(typeof(uint));

        public IStatBuilder Armour => FromIdentity(typeof(uint));

        public IEvasionStatBuilder Evasion => new EvasionStatBuilder(StatFactory);

        public IDamageRelatedStatBuilder Accuracy
            => DamageRelatedFromIdentity(typeof(uint)).WithSkills(DamageSource.Attack);

        public IDamageRelatedStatBuilder ChanceToHit
            => DamageRelatedFromIdentity(typeof(uint)).WithSkills(DamageSource.Attack);

        public IStatBuilder MovementSpeed => FromIdentity(typeof(double));
        public IStatBuilder ActionSpeed => FromIdentity(typeof(double));

        public IDamageRelatedStatBuilder CastRate => new CastRateStatBuilder(StatFactory);
        public IDamageRelatedStatBuilder BaseCastTime => DamageRelatedFromIdentity(typeof(double)).WithHits;
        public IStatBuilder HitRate => FromIdentity(typeof(double));
        public IStatBuilder AdditionalCastRate => FromIdentity(typeof(double));

        public IStatBuilder DamageHasKeyword(DamageSource damageSource, IKeywordBuilder keyword)
        {
            var coreBuilder = new CoreStatBuilderFromCoreBuilder<Keyword>(
                CoreBuilder.Proxy(keyword, (ps, b) => b.Build(ps)),
                (e, k) => StatFactory.MainSkillPartDamageHasKeyword(e, k, damageSource));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        public IStatBuilder AreaOfEffect => FromIdentity(typeof(int));
        public IStatBuilder Radius => PrimaryRadius.Concat(SecondaryRadius).Concat(TertiaryRadius);
        public IStatBuilder PrimaryRadius => FromIdentity(typeof(uint));
        public IStatBuilder SecondaryRadius => FromIdentity(typeof(uint));
        public IStatBuilder TertiaryRadius => FromIdentity(typeof(uint));

        public IDamageRelatedStatBuilder Range
            => DamageRelatedFromIdentity(typeof(uint)).WithSkills(DamageSource.Attack);

        public IStatBuilder Cooldown => FromIdentity(typeof(double));
        public IStatBuilder CooldownRecoverySpeed => FromIdentity(typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder SecondaryDuration => FromIdentity(typeof(double));
        public IStatBuilder SkillNumberOfHitsPerCast => FromIdentity(typeof(uint));
        public IStatBuilder SkillRepeats => FromIdentity(typeof(uint));
        public IStatBuilder DamageMultiplierOverRepeatCycle => FromIdentity(typeof(int));
        public IStatBuilder SkillStage => FromIdentity(typeof(uint), UserSpecifiedValue(double.MaxValue));
        public IStatBuilder MainSkillPart => FromIdentity(typeof(uint));

        public ITrapStatBuilders Trap => new TrapStatBuilders(StatFactory);
        public IMineStatBuilders Mine => new MineStatBuilders(StatFactory);
        public ISkillEntityStatBuilders Totem => new TotemStatBuilders(StatFactory);

        public IStatBuilder ItemQuantity => FromIdentity(typeof(int));
        public IStatBuilder ItemRarity => FromIdentity(typeof(int));

        public IStatBuilder PrimordialJewelsSocketed => FromIdentity(typeof(uint));
        public IStatBuilder GrandSpectrumJewelsSocketed => FromIdentity(typeof(uint));
        public IStatBuilder AbyssalSockets => FromIdentity(typeof(uint));

        public IStatBuilder AttachedBrands => FromIdentity(typeof(uint));
        public IStatBuilder BannerStage => FromIdentity(typeof(uint));

        public IStatBuilder RuthlessBlowPeriod => FromIdentity(typeof(double));

        public ValueBuilder RuthlessBlowBonus
        {
            get
            {
                var applicationBuilder = FromIdentity(typeof(RuthlessBlowBonusCalculation), UserSpecifiedValue(0)).Value;
                var periodBuilder = RuthlessBlowPeriod.Value;
                return new ValueBuilder(new ValueBuilderImpl(Build, _ => Build));

                IValue Build(BuildParameters ps)
                {
                    var application = applicationBuilder.Build(ps);
                    var period = periodBuilder.Build(ps);
                    return new FunctionalValue(c => Calculate(c, application, period),
                        $"{application} switch {{ Never => 0, Average => {period}, Always => 1, _ => null }}");
                }

                static NodeValue? Calculate(IValueCalculationContext context, IValue application, IValue period)
                {
                    return (RuthlessBlowBonusCalculation?) application.Calculate(context).SingleOrNull() switch
                    {
                        RuthlessBlowBonusCalculation.Never => new NodeValue(0),
                        RuthlessBlowBonusCalculation.Average => period.Calculate(context),
                        RuthlessBlowBonusCalculation.Always => new NodeValue(1),
                        _ => null
                    };
                }
            }
        }

        private enum RuthlessBlowBonusCalculation
        {
            Never,
            Average,
            Always,
        }

        public IStatBuilder CursesLinkedToBane => FromIdentity(typeof(uint));

        public IStatBuilder SealGainFrequency => FromIdentity(typeof(double));

        public IStatBuilder DamageTakenGainedAsMana => FromIdentity(typeof(uint));

        public ValueBuilder UniqueAmount(string name, double defaultValue = 0)
            => FromIdentity(name, typeof(uint), UserSpecifiedValue(defaultValue)).Value;

        public ValueBuilder UniqueEnum<T>(string name) where T : Enum
            => FromIdentity(name, typeof(T), UserSpecifiedValue(0)).Value;

        public IStatBuilder IndependentMultiplier(string identity)
            => FromIdentity(identity, typeof(uint), IndependentResult(NodeType.Increase));

        public IStatBuilder IndependentTotal(string identity)
            => FromIdentity(identity, typeof(uint), IndependentResult(NodeType.Total));

        public IAttributeStatBuilders Attribute => new AttributeStatBuilders(StatFactory);
        public IRequirementStatBuilders Requirements => new RequirementStatBuilders(StatFactory);
        public IPoolStatBuilders Pool => new PoolStatBuilders(StatFactory);
        public IDodgeStatBuilders Dodge => new DodgeStatBuilders(StatFactory);
        public IFlaskStatBuilders Flask => new FlaskStatBuilders(StatFactory);
        public IProjectileStatBuilders Projectile => new ProjectileStatBuilders(StatFactory);
        public IFlagStatBuilders Flag => new FlagStatBuilders(StatFactory);
        public IGemStatBuilders Gem => new GemStatBuilders(StatFactory);
        public IWarcryStatBuilders Warcry => new WarcryStatBuilders(StatFactory);
    }

    internal class TrapStatBuilders : PrefixedStatBuildersBase, ITrapStatBuilders
    {
        public TrapStatBuilders(IStatFactory statFactory) : base(statFactory, "Trap")
        {
        }

        public IStatBuilder Speed => FromIdentity("ThrowingSpeed", typeof(double));
        public IStatBuilder BaseTime => FromIdentity("BaseThrowingTime", typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder TriggerAoE => FromIdentity(typeof(int));
    }

    internal class MineStatBuilders : PrefixedStatBuildersBase, IMineStatBuilders
    {
        public MineStatBuilders(IStatFactory statFactory) : base(statFactory, "Mine")
        {
        }

        public IStatBuilder Speed => FromIdentity("ThrowingSpeed", typeof(double));
        public IStatBuilder BaseTime => FromIdentity("BaseThrowingTime", typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder DetonationAoE => FromIdentity(typeof(int));
    }

    internal class TotemStatBuilders : PrefixedStatBuildersBase, ISkillEntityStatBuilders
    {
        public TotemStatBuilders(IStatFactory statFactory) : base(statFactory, "Totem")
        {
        }

        public IStatBuilder Speed => FromIdentity("PlacementSpeed", typeof(double));
        public IStatBuilder BaseTime => FromIdentity("BasePlacementTime", typeof(double));
        public IStatBuilder Duration => FromIdentity(typeof(double));
    }

    internal class AttributeStatBuilders : StatBuildersBase, IAttributeStatBuilders
    {
        public AttributeStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Strength => FromIdentity(typeof(uint));
        public IStatBuilder Dexterity => FromIdentity(typeof(uint));
        public IStatBuilder Intelligence => FromIdentity(typeof(uint));
        public IStatBuilder StrengthDamageBonus => FromIdentity(typeof(uint));
        public IStatBuilder DexterityEvasionBonus => FromIdentity(typeof(uint));
    }

    internal class RequirementStatBuilders : StatBuildersBase, IRequirementStatBuilders
    {
        public RequirementStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder Level => Requirement();
        public IStatBuilder Strength => Requirement();
        public IStatBuilder Dexterity => Requirement();
        public IStatBuilder Intelligence => Requirement();

        private IStatBuilder Requirement([CallerMemberName] string requiredStat = "")
            => FromStatFactory(e => StatFactory.Requirement(StatFactory.FromIdentity(requiredStat, e, typeof(uint))));
    }

    internal class DodgeStatBuilders : PrefixedStatBuildersBase, IDodgeStatBuilders
    {
        public DodgeStatBuilders(IStatFactory statFactory) : base(statFactory, "Dodge")
        {
        }

        public IStatBuilder AttackChance => FromIdentity(typeof(uint));
        public IStatBuilder SpellChance => FromIdentity(typeof(uint));
    }

    internal class FlaskStatBuilders : PrefixedStatBuildersBase, IFlaskStatBuilders
    {
        public FlaskStatBuilders(IStatFactory statFactory) : base(statFactory, "Flask")
        {
        }

        public IStatBuilder Effect => FromIdentity(typeof(int));
        public IStatBuilder Duration => FromIdentity(typeof(double));
        public IStatBuilder LifeRecovery => FromIdentity(typeof(int));
        public IStatBuilder ManaRecovery => FromIdentity(typeof(int));
        public IStatBuilder LifeRecoverySpeed => FromIdentity(typeof(double));
        public IStatBuilder ManaRecoverySpeed => FromIdentity(typeof(double));
        public IStatBuilder InstantRecovery => FromIdentity(typeof(uint));
        public IStatBuilder ChargesUsed => FromIdentity(typeof(int));
        public IStatBuilder ChargesGained => FromIdentity(typeof(double));
        public IStatBuilder MaximumCharges => FromIdentity(typeof(uint));
        public IStatBuilder ChanceToGainCharge => FromIdentity(typeof(double));
    }

    internal class ProjectileStatBuilders : PrefixedStatBuildersBase, IProjectileStatBuilders
    {
        public ProjectileStatBuilders(IStatFactory statFactory) : base(statFactory, "Projectile")
        {
        }

        public IStatBuilder Speed => FromIdentity(typeof(int));
        public IStatBuilder Count => FromIdentity(typeof(uint));

        public IStatBuilder PierceCount => FromIdentity(typeof(uint));
        public IStatBuilder ChainCount => FromIdentity(typeof(uint));
        public IStatBuilder Fork => FromIdentity(typeof(bool));

        public ValueBuilder TravelDistance => FromIdentity(typeof(uint), UserSpecifiedValue(35)).Value;
    }

    internal class FlagStatBuilders : StatBuildersBase, IFlagStatBuilders
    {
        public FlagStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IStatBuilder ShieldModifiersApplyToMinionsInstead => FromIdentity(typeof(bool));

        public IStatBuilder IgnoreHexproof => FromIdentity(typeof(bool));
        public IStatBuilder CriticalStrikeChanceIsLucky => FromIdentity(typeof(bool));
        public IStatBuilder FarShot => FromIdentity(typeof(bool));

        public IConditionBuilder AlwaysMoving
            => FromIdentity("Are you always moving?", typeof(bool), UserSpecifiedValue(false)).IsTrue;

        public IConditionBuilder AlwaysStationary
            => FromIdentity("Are you always stationary?", typeof(bool), UserSpecifiedValue(false)).IsTrue;

        public IConditionBuilder IsBrandAttachedToEnemy
            => FromIdentity("Is your Brand attached to an enemy?", typeof(bool), UserSpecifiedValue(false)).IsTrue;

        public IConditionBuilder IsBannerPlanted
            => FromIdentity("Is your Banner planted?", typeof(bool), UserSpecifiedValue(false)).IsTrue;

        public IConditionBuilder InBloodStance => StanceValue.Eq((int) Stance.BloodStance);
        public IConditionBuilder InSandStance => StanceValue.Eq((int) Stance.SandStance);

        private ValueBuilder StanceValue
            => FromIdentity("Stance", typeof(Stance), UserSpecifiedValue(0)).Value;

        private enum Stance
        {
            None,
            BloodStance,
            SandStance,
        }

        public IConditionBuilder BypassSkillCooldown => FromIdentity(typeof(bool), UserSpecifiedValue(false)).IsTrue;

        public IStatBuilder IncreasesToSourceApplyToTarget(IStatBuilder source, IStatBuilder target)
            => new StatBuilder(StatFactory,
                new ModifiersApplyToOtherStatCoreStatBuilder(source, target, Form.Increase, StatFactory));

        public IStatBuilder BaseAddsToSourceApplyToTarget(IStatBuilder source, IStatBuilder target)
            => new StatBuilder(StatFactory,
                new ModifiersApplyToOtherStatCoreStatBuilder(source, target, Form.BaseAdd, StatFactory));

        private class ModifiersApplyToOtherStatCoreStatBuilder : ICoreStatBuilder
        {
            private readonly IStatBuilder _target;
            private readonly IStatBuilder _source;
            private readonly Form _form;
            private readonly IStatFactory _statFactory;

            public ModifiersApplyToOtherStatCoreStatBuilder(
                IStatBuilder source, IStatBuilder target, Form form, IStatFactory statFactory)
                => (_target, _source, _form, _statFactory) = (target, source, form, statFactory);

            public ICoreStatBuilder Resolve(ResolveContext context)
                => new ModifiersApplyToOtherStatCoreStatBuilder(
                    _source.Resolve(context), _target.Resolve(context), _form, _statFactory);

            public ICoreStatBuilder WithEntity(IEntityBuilder entityBuilder)
                => new ModifiersApplyToOtherStatCoreStatBuilder(
                    _source.For(entityBuilder), _target.For(entityBuilder), _form, _statFactory);

            public IEnumerable<StatBuilderResult> Build(BuildParameters parameters)
            {
                return _source.Build(parameters).EquiZip(_target.Build(parameters), MergeResults);

                StatBuilderResult MergeResults(StatBuilderResult source, StatBuilderResult target)
                {
                    var mergedStats = source.Stats.EquiZip(target.Stats,
                        (s, t) => _statFactory.StatIsAffectedByModifiersToOtherStat(t, s, _form));
                    return new StatBuilderResult(mergedStats.ToList(), source.ModifierSource, source.ValueConverter);
                }
            }
        }
    }

    internal class WarcryStatBuilders : PrefixedStatBuildersBase, IWarcryStatBuilders
    {
        public WarcryStatBuilders(IStatFactory statFactory) : base(statFactory, "Warcry")
        {
        }

        public IStatBuilder PowerMultiplier => FromIdentity(typeof(double));
        public IStatBuilder MinimumPower => FromIdentity(typeof(uint));
        public IStatBuilder AttackAreExerted => FromIdentity(typeof(bool), UserSpecifiedValue(false));
        public IStatBuilder ExertedAttackCount => FromIdentity(typeof(uint));

        public ValueBuilder AllyPower => FromIdentity(typeof(int), UserSpecifiedValue(0)).Value;
        public ValueBuilder EnemyPower => FromIdentity(typeof(int), UserSpecifiedValue(0)).Value;
        public ValueBuilder CorpsePower => FromIdentity(typeof(int), UserSpecifiedValue(0)).Value;
        public ValueBuilder LastPower => PowerMultiplier.Value * FromIdentity(typeof(int), UserSpecifiedValue(0)).Value;
    }
}