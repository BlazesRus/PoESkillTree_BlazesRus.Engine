using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Buffs;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Entities;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Builders.Buffs
{
    public class BuffBuilderCollection : IBuffBuilderCollection
    {
        private readonly IStatFactory _statFactory;
        private readonly IReadOnlyList<BuffBuilderWithKeywords> _buffs;
        private readonly BuffRestrictionsBuilder _restrictionsBuilder;
        private readonly IEntityBuilder _source;
        private readonly IEntityBuilder _target;

        public BuffBuilderCollection(
            IStatFactory statFactory, IReadOnlyList<BuffBuilderWithKeywords> buffs,
            IEntityBuilder source, IEntityBuilder target)
            : this(statFactory, buffs, new BuffRestrictionsBuilder(), source, target)
        {
        }

        private BuffBuilderCollection(
            IStatFactory statFactory, IReadOnlyList<BuffBuilderWithKeywords> buffs,
            BuffRestrictionsBuilder restrictionsBuilder, IEntityBuilder source, IEntityBuilder target)
        {
            _statFactory = statFactory;
            _buffs = buffs;
            _restrictionsBuilder = restrictionsBuilder;
            _source = source;
            _target = target;
        }

        public IBuilderCollection Resolve(ResolveContext context)
        {
            var buffs = _buffs.Select(b => b.Resolve(context)).ToList();
            return new BuffBuilderCollection(_statFactory, buffs, _restrictionsBuilder.Resolve(context),
                _source, _target);
        }

        public ValueBuilder Count()
        {
            return new ValueBuilder(new ValueBuilderImpl(Build, c => Resolve(c).Count()));

            IValue Build(BuildParameters parameters) =>
                new CountingValue(CreateValues(parameters).ToList());
        }

        public IConditionBuilder Any() => Count() > 0;

        private IEnumerable<IValue> CreateValues(BuildParameters parameters)
        {
            var restrictions = _restrictionsBuilder.Build(parameters);
            var sourceEntities = _source.Build(parameters.ModifierSourceEntity);
            var targetEntities = _target.Build(parameters.ModifierSourceEntity);
            return from b in _buffs
                   where restrictions.AllowsBuff(b)
                   let buffIdentity = b.Buff.Build(parameters)
                   from t in targetEntities
                   let activeStat = _statFactory.BuffIsActive(t, buffIdentity)
                   let activeCondition = new StatValue(activeStat)
                   let buffSourceCondition = BuffSourceIsAny(t, buffIdentity, sourceEntities)
                   select new ConditionalValue(
                       c => activeCondition.Calculate(c).IsTrue() && buffSourceCondition.Calculate(c).IsTrue(),
                       $"{activeCondition} && {buffSourceCondition}");
        }

        private IValue BuffSourceIsAny(Entity target, string buffIdentity, IEnumerable<Entity> sources)
        {
            var statValues = sources
                .Select(s => _statFactory.BuffSourceIs(s, target, buffIdentity))
                .Select(s => new StatValue(s))
                .ToList();
            var count = new CountingValue(statValues);
            return new ConditionalValue(c => count.Calculate(c) > 0, $"{count} > 0");
        }

        public IStatBuilder On =>
            new StatBuilder(_statFactory,
                    new BuffCoreStatBuilder(_buffs, b => b.On(_target), _restrictionsBuilder))
                .For(_source);

        public IStatBuilder Effect =>
            new StatBuilder(_statFactory,
                    new BuffCoreStatBuilder(_buffs, b => b.EffectOn(_target), _restrictionsBuilder))
                .For(_source);

        public IStatBuilder AddStat(IStatBuilder stat) =>
            new StatBuilder(_statFactory,
                    new BuffCoreStatBuilder(_buffs, b => b.AddStatForSource(stat, _source), _restrictionsBuilder))
                .For(_target);

        public IStatBuilder ApplyToEntity(IEntityBuilder target)
            => _buffs
                .Select(b => ApplyToEntity(b.Buff, target))
                .Aggregate((l, r) => l.Concat(r));

        private IStatBuilder ApplyToEntity(IBuffBuilder buff, IEntityBuilder target) =>
            buff.On(target).WithCondition(buff.IsOn(_source, _target));

        public IBuffBuilderCollection With(IKeywordBuilder keyword) =>
            new BuffBuilderCollection(_statFactory, _buffs, _restrictionsBuilder.With(keyword), _source, _target);

        public IBuffBuilderCollection Without(IKeywordBuilder keyword) =>
            new BuffBuilderCollection(_statFactory, _buffs, _restrictionsBuilder.Without(keyword), _source, _target);
    }
}