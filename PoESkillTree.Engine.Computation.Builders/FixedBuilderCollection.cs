using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Builders.Conditions;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Computation.Common.Parsing;
namespace PoESkillTree.Engine.Computation.Builders
{
    public class FixedBuilderCollection<TKey, TBuilder> : IBuilderCollection<TBuilder>, IEnumerable<TBuilder>
    {
        private readonly IReadOnlyList<TKey> _keys;
        private readonly Func<TKey, TBuilder> _builderFactory;
        private readonly ConcurrentDictionary<TKey, TBuilder> _builders = new ConcurrentDictionary<TKey, TBuilder>();

        public FixedBuilderCollection(IReadOnlyList<TKey> keys, Func<TKey, TBuilder> builderFactory)
        {
            _keys = keys;
            _builderFactory = builderFactory;
        }

        public IBuilderCollection Resolve(ResolveContext context) => this;

        public ValueBuilder Count() =>
            new ValueBuilder(new ValueBuilderImpl(_keys.Count));

        public IConditionBuilder Any() =>
            ConstantConditionBuilder.Create(_keys.Any());

        public ValueBuilder Count(Func<TBuilder, IConditionBuilder> predicate)
        {
            var conditions = this.Select(predicate).ToList();
            var valueBuilder = new ValueBuilderImpl(
                ps => Build(ps, conditions),
                c => ((IBuilderCollection<TBuilder>) Resolve(c)).Count(predicate));
            return new ValueBuilder(valueBuilder);

            IValue Build(BuildParameters parameters, IEnumerable<IConditionBuilder> cs)
            {
                var builtConditions = cs.Select(c => BuildConditionToValue(c, parameters)).ToList();
                return new CountingValue(builtConditions);
            }
        }

        private IValue BuildConditionToValue(IConditionBuilder condition, BuildParameters parameters)
        {
            var result = condition.Build(parameters);
            if (!result.HasValue)
                throw new ParseException(
                    $"Can only use value conditions in {nameof(Count)}");
            return result.Value;
        }

        public IConditionBuilder Any(Func<TBuilder, IConditionBuilder> predicate) =>
            this.Select(predicate).Aggregate((l, r) => l.Or(r));

        public IEnumerator<TBuilder> GetEnumerator() =>
            _keys.Select(k => this[k]).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public TBuilder this[TKey key] =>
            _builders.GetOrAdd(key, _builderFactory);
    }
}