using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Builders.Effects;
using PoESkillTree.Engine.Computation.Builders.Entities;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Buffs;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Engine.Computation.Builders.Buffs
{
    public class BuffBuilder : EffectBuilder, IBuffBuilder
    {
        public BuffBuilder(IStatFactory statFactory, ICoreBuilder<string> identity) : base(statFactory, identity)
        {
        }

        public override IEffectBuilder Resolve(ResolveContext context) =>
            new BuffBuilder(StatFactory, Identity.Resolve(context));

        public IStatBuilder NotAsBuffOn(IEntityBuilder target) =>
            InternalOn(target);

        public IStatBuilder Effect => EffectOn(EntityBuilder.AllEntities);

        public IStatBuilder EffectOn(IEntityBuilder target)
        {
            return new StatBuilder(StatFactory, FromStatFactory(BuildStats));

            IEnumerable<IStat> BuildStats(BuildParameters parameters, Entity source, string identity)
                => target.Build(parameters.ModifierSourceEntity)
                    .Select(t => BuildEffectStat(source, t, identity));
        }

        public IStatBuilder StackCount
            => FromIdentity("StackCount", typeof(uint),
                ExplicitRegistrationTypes.UserSpecifiedValue(double.PositiveInfinity));

        public IStatBuilder Power => FromIdentity("Power", typeof(uint));

        public IStatBuilder IgnoresCurseLimit => FromIdentity("IgnoresCurseLimit", typeof(bool));

        public override IStatBuilder On(IEntityBuilder target) =>
            base.On(target)
                .CombineWith(new StatBuilder(StatFactory, FromStatFactory(BuildBuffActiveStat)))
                .CombineWith(new StatBuilder(StatFactory, FromStatFactory(BuildBuffSourceStat)))
                .For(target);

        private IStat BuildBuffSourceStat(BuildParameters parameters, Entity target, string identity) =>
            BuildBuffSourceStat(parameters.ModifierSourceEntity, target, identity);

        public IConditionBuilder IsOn(IEntityBuilder source, IEntityBuilder target) =>
            IsOn(target).And(IsFromSource(source, target));

        private IConditionBuilder IsFromSource(IEntityBuilder source, IEntityBuilder target)
        {
            var core = FromStatFactory(BuildStats);
            return new StatBuilder(StatFactory, core).For(target).IsTrue;

            IEnumerable<IStat> BuildStats(BuildParameters parameters, Entity t, string identity)
                => source.Build(parameters.ModifierSourceEntity)
                    .Select(s => BuildBuffSourceStat(s, t, identity));
        }

        public override IStatBuilder AddStat(IStatBuilder stat) => AddStatForSource(stat, EntityBuilder.AllEntities);

        public IStatBuilder AddStatForSource(IStatBuilder stat, IEntityBuilder source)
        {
            var baseCoreBuilder = new StatBuilderAdapter(base.AddStat(stat));
            var coreBuilder = new StatBuilderWithValueConverter(baseCoreBuilder,
                target => CreateAddStatMultiplier(source, target),
                (l, r) => l.Multiply(r));
            return new StatBuilder(StatFactory, coreBuilder);
        }

        private IValueBuilder CreateAddStatMultiplier(IEntityBuilder source, Entity target)
            => new ValueBuilderImpl(
                ps => BuildAddStatMultiplier(Build(ps), source.Build(ps.ModifierSourceEntity), target),
                c => ((BuffBuilder) Resolve(c)).CreateAddStatMultiplier(source, target));

        public IValue BuildAddStatMultiplier(BuildParameters parameters, IReadOnlyCollection<Entity> possibleSources)
            => BuildAddStatMultiplier(Build(parameters), possibleSources, parameters.ModifierSourceEntity);

        private IValue BuildAddStatMultiplier(
            string identity, IReadOnlyCollection<Entity> possibleSources, Entity target)
        {
            if (possibleSources.IsEmpty())
                return new Constant(1);

            var buffActiveValue = new StatValue(BuildBuffActiveStat(target, identity));
            var buffSourceValues = possibleSources.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildBuffSourceStat(e, target, identity)));
            var buffEffectValues = possibleSources.ToDictionary(Funcs.Identity,
                e => new StatValue(BuildEffectStat(e, target, identity)));

            return new FunctionalValue(Calculate,
                $"AddStatMultiplier(buffActive:{buffActiveValue}, buffSources:{string.Join(",", buffSourceValues)}, " +
                $"buffEffects:{string.Join(",", buffEffectValues)})");

            NodeValue? Calculate(IValueCalculationContext context)
            {
                if (!buffActiveValue.Calculate(context).IsTrue())
                    return new NodeValue(1);

                // If multiple entities apply the same (de-)buff, it depends on the buff which one would actually apply.
                // Because that shouldn't happen in these calculations, simply the first one is taken.
                var sourceEntity = possibleSources.First(e => buffSourceValues[e].Calculate(context).IsTrue());
                return buffEffectValues[sourceEntity].Calculate(context);
            }
        }

        private ICoreStatBuilder FromStatFactory(Func<Entity, string, IStat> statFactory) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity, statFactory);

        private ICoreStatBuilder FromStatFactory(Func<BuildParameters, Entity, string, IStat> statFactory) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity, statFactory);

        private ICoreStatBuilder FromStatFactory(CoreStatBuilderFromCoreBuilder<string>.StatFactory statFactory) =>
            new CoreStatBuilderFromCoreBuilder<string>(Identity, statFactory);

        private IStat BuildEffectStat(Entity source, Entity target, string identity) =>
            StatFactory.BuffEffect(source, target, identity);

        private IStat BuildBuffActiveStat(Entity target, string identity) =>
            StatFactory.BuffIsActive(target, identity);

        private IStat BuildBuffSourceStat(Entity source, Entity target, string identity) =>
            StatFactory.BuffSourceIs(source, target, identity);
    }
}