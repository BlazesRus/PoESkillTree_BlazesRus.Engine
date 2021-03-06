using System;
using System.Runtime.CompilerServices;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using static PoESkillTree.Engine.Computation.Common.ExplicitRegistrationTypes;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    internal class PoolStatBuilders : StatBuildersBase, IPoolStatBuilders
    {
        public PoolStatBuilders(IStatFactory statFactory) : base(statFactory)
        {
        }

        public IPoolStatBuilder From(Pool pool) => new PoolStatBuilder(StatFactory, CoreBuilder.Create(pool));
    }

    public class PoolStatBuilder : StatBuilderWithPool, IPoolStatBuilder
    {
        public PoolStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, "", typeof(uint))
        {
        }

        private PoolStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, "")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new PoolStatBuilder(StatFactory, coreStatBuilder, pool);

        public new IPoolStatBuilder For(IEntityBuilder entity) =>
            (IPoolStatBuilder) base.For(entity);

        public IRegenStatBuilder Regen => new RegenStatBuilder(StatFactory, Pool);

        public IStatBuilder Degeneration(IDamageTypeBuilder damageType) =>
            new StatBuilder(StatFactory,
                new ParametrisedCoreStatBuilder<IKeywordBuilder>(CreateCoreStatBuilder("Degeneration", typeof(double)), damageType,
                    (ps, kb, s) => StatFactory.CopyWithSuffix(s, kb.Build(ps).ToString(), typeof(double))));

        public IRechargeStatBuilder Recharge => new RechargeStatBuilder(StatFactory, Pool);
        public IStatBuilder RecoveryRate => FromIdentity(typeof(double));
        public IStatBuilder Cost => FromIdentity(typeof(uint));
        public IStatBuilder Reservation => FromIdentity(typeof(uint));
        public ILeechStatBuilder Leech => new LeechStatBuilder(StatFactory, Pool);
        public IDamageRelatedStatBuilder Gain => DamageRelatedFromIdentity(typeof(int)).WithHits;

        public IConditionBuilder IsFull =>
            (Reservation.Value <= 0).And(FromIdentity(typeof(bool), UserSpecifiedValue(false)).IsTrue);

        public IConditionBuilder IsLow =>
            (Reservation.Value >= 0.65 * Value).Or(FromIdentity(typeof(bool), UserSpecifiedValue(false)).IsTrue);

        public IConditionBuilder IsEmpty
            => (Value <= 0).Or(Reservation.Value >= 100).Or(FromIdentity(typeof(bool), UserSpecifiedValue(true)).IsTrue);

        public Pool BuildPool(BuildParameters parameters) => Pool.Build(parameters);
    }

    internal class RechargeStatBuilder : StatBuilderWithPool, IRechargeStatBuilder
    {
        public RechargeStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, ".Recharge", typeof(double))
        {
        }

        private RechargeStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, ".Recharge")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new RechargeStatBuilder(StatFactory, coreStatBuilder, pool);

        public IStatBuilder Start => FromIdentity(typeof(double));

        public IConditionBuilder StartedRecently => FromIdentity(typeof(bool), UserSpecifiedValue(false)).IsTrue;
    }

    internal class RegenStatBuilder : StatBuilderWithPool, IRegenStatBuilder
    {
        public RegenStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, ".Regen", typeof(double))
        {
        }

        private RegenStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, ".Regen")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new RegenStatBuilder(StatFactory, coreStatBuilder, pool);

        public IStatBuilder Percent => FromIdentity(typeof(int));

        public IStatBuilder TargetPool =>
            new StatBuilder(StatFactory, new CoreStatBuilderFromCoreBuilder<Pool>(Pool, StatFactory.RegenTargetPool));
    }

    internal class LeechStatBuilder : StatBuilderWithPool, ILeechStatBuilder
    {
        public LeechStatBuilder(IStatFactory statFactory, ICoreBuilder<Pool> pool)
            : base(statFactory, pool, ".Leech", typeof(double))
        {
        }

        private LeechStatBuilder(IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool)
            : base(statFactory, coreStatBuilder, pool, ".Leech")
        {
        }

        protected override IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool) =>
            new LeechStatBuilder(StatFactory, coreStatBuilder, pool);

        public IStatBuilder Of(IDamageRelatedStatBuilder damage)
        {
            var damageCoreBuilder = new StatBuilderAdapter(damage.WithHits);
            var coreBuilder = new ParametrisedCoreStatBuilder<ICoreBuilder<Pool>>(damageCoreBuilder, Pool,
                (ps, p, s) => StatFactory.CopyWithSuffix(s, $"LeechTo({p.Build(ps)})", typeof(uint)));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        public IStatBuilder RateLimit => FromIdentity(typeof(uint));
        public IStatBuilder Rate => FromIdentity(typeof(double));
        public IStatBuilder MaximumRecoveryPerInstance => FromIdentity(typeof(double));
        public IConditionBuilder IsActive => FromIdentity(typeof(bool), UserSpecifiedValue(false)).IsTrue;
        public IStatBuilder IsInstant => FromIdentity(typeof(bool));
    }

    public abstract class StatBuilderWithPool : StatBuilder
    {
        private readonly string _identitySuffix;
        protected ICoreBuilder<Pool> Pool { get; }

        protected StatBuilderWithPool(
            IStatFactory statFactory, ICoreBuilder<Pool> pool, string identitySuffix, Type dataType)
            : this(statFactory,
                new CoreStatBuilderFromCoreBuilder<Pool>(pool,
                    (e, p) => statFactory.FromIdentity(p.ToString() + identitySuffix, e, dataType)),
                pool,
                identitySuffix)
        {
        }

        protected StatBuilderWithPool(
            IStatFactory statFactory, ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool, string identitySuffix)
            : base(statFactory, coreStatBuilder)
        {
            Pool = pool;
            _identitySuffix = identitySuffix;
        }

        protected override IStatBuilder With(ICoreStatBuilder coreStatBuilder) =>
            Create(coreStatBuilder, Pool);

        public override IStatBuilder Resolve(ResolveContext context) =>
            Create(CoreStatBuilder.Resolve(context), Pool.Resolve(context));

        protected abstract IStatBuilder Create(ICoreStatBuilder coreStatBuilder, ICoreBuilder<Pool> pool);

        protected IStatBuilder FromIdentity(
            Type dataType, ExplicitRegistrationType? explicitRegistrationType = null,
            [CallerMemberName] string identitySuffix = "")
            => new StatBuilder(StatFactory, CreateCoreStatBuilder(identitySuffix, dataType, explicitRegistrationType));

        protected IDamageRelatedStatBuilder DamageRelatedFromIdentity(
            Type dataType, ExplicitRegistrationType? explicitRegistrationType = null,
            [CallerMemberName] string identitySuffix = "")
            => DamageRelatedStatBuilder.Create(StatFactory,
                CreateCoreStatBuilder(identitySuffix, dataType, explicitRegistrationType));

        protected ICoreStatBuilder CreateCoreStatBuilder(
            string identitySuffix, Type dataType, ExplicitRegistrationType? explicitRegistrationType = null)
            => new CoreStatBuilderFromCoreBuilder<Pool>(Pool,
                (e, p) => StatFactory.FromIdentity($"{p}{_identitySuffix}.{identitySuffix}", e, dataType,
                    explicitRegistrationType));
    }
}