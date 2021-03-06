using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Builders.Conditions;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Actions;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel;
using static PoESkillTree.Engine.Computation.Common.ExplicitRegistrationTypes;

namespace PoESkillTree.Engine.Computation.Builders.Actions
{
    public class ActionBuilder : IActionBuilder
    {
        private const int RecentlySeconds = 4;

        protected IStatFactory StatFactory { get; }
        private readonly ICoreBuilder<string> _identity;
        protected IEntityBuilder Entity { get; }

        public ActionBuilder(IStatFactory statFactory, ICoreBuilder<string> identity, IEntityBuilder entity)
        {
            StatFactory = statFactory;
            _identity = identity;
            Entity = entity;
        }

        public IActionBuilder Resolve(ResolveContext context) =>
            new ActionBuilder(StatFactory, _identity.Resolve(context), Entity);

        public IActionBuilder By(IEntityBuilder source) =>
            new ActionBuilder(StatFactory, _identity, source);

        public IConditionBuilder On =>
            new StatConvertingConditionBuilder(
                b => new StatBuilder(StatFactory,
                    new ParametrisedCoreStatBuilder<ICoreBuilder<string>>(
                        new StatBuilderAdapter(b), _identity, ConvertStat)),
                c => Resolve(c).On);

        private IEnumerable<IStat> ConvertStat(BuildParameters parameters, ICoreBuilder<string> identity, IStat stat)
        {
            var builtIdentity = identity.Build(parameters);
            return from e in Entity.Build(stat.Entity)
                   let i = $"On({builtIdentity}).By({e})"
                   let registrationType = GainOnAction(stat, builtIdentity, e)
                   select StatFactory.CopyWithSuffix(stat, i, stat.DataType, registrationType);
        }

        public IConditionBuilder InPastXSeconds(IValueBuilder seconds) =>
            new ValueConditionBuilder(ps => BuildInPastXSecondsValue(ps, seconds),
                c => Resolve(c).InPastXSeconds(seconds.Resolve(c)));

        private IValue BuildInPastXSecondsValue(BuildParameters parameters, IValueBuilder seconds)
        {
            var builtEntity = BuildEntity(parameters, Entity);
            var recentOccurrencesStat = BuildRecentOccurrencesStat(parameters, builtEntity);
            var lastOccurenceStat = BuildLastOccurrenceStat(parameters, builtEntity);
            var secondsValue = seconds.Build(parameters);
            return new ConditionalValue(Calculate,
                $"({RecentlySeconds} <= {secondsValue} && {recentOccurrencesStat} > 0) " +
                $"|| {lastOccurenceStat} <= {secondsValue}");

            bool Calculate(IValueCalculationContext context)
            {
                NodeValue? threshold = secondsValue.Calculate(context);
                if (RecentlySeconds <= threshold && context.GetValue(recentOccurrencesStat) > 0)
                    return true;
                return context.GetValue(lastOccurenceStat) <= threshold;
            }
        }

        public IConditionBuilder Recently =>
            InPastXSeconds(new ValueBuilderImpl(RecentlySeconds));

        public ValueBuilder CountRecently =>
            new ValueBuilder(new ValueBuilderImpl(BuildCountRecentlyValue, c => Resolve(c).CountRecently));

        private IValue BuildCountRecentlyValue(BuildParameters parameters)
            => new StatValue(BuildRecentOccurrencesStat(parameters, BuildEntity(parameters, Entity)));

        private IStat BuildLastOccurrenceStat(BuildParameters parameters, Entity entity)
            => StatFactory.FromIdentity($"{Build(parameters)}.LastOccurrence", entity, typeof(uint),
                UserSpecifiedValue(null));

        private IStat BuildRecentOccurrencesStat(BuildParameters parameters, Entity entity)
            => StatFactory.FromIdentity($"{Build(parameters)}.RecentOccurrences", entity, typeof(uint),
                UserSpecifiedValue(0));

        private static Entity BuildEntity(BuildParameters parameters, IEntityBuilder entity) =>
            entity.Build(parameters.ModifierSourceEntity).Single();

        public string Build(BuildParameters parameters) => _identity.Build(parameters);
    }
}