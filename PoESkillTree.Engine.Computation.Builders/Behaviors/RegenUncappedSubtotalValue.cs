using System;
using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;

namespace PoESkillTree.Engine.Computation.Builders.Behaviors
{
    public class RegenUncappedSubtotalValue : IValue
    {
        private readonly Pool _pool;
        private readonly Func<Pool, IStat> _regens;
        private readonly Func<Pool, IStat> _targetPools;
        private readonly IValue _transformedValue;

        public RegenUncappedSubtotalValue(
            Pool pool, Func<Pool, IStat> regens, Func<Pool, IStat> targetPools, IValue transformedValue)
        {
            _pool = pool;
            _regens = regens;
            _targetPools = targetPools;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var applyingPools =
                from pool in Enum.GetValues(typeof(Pool)).Cast<Pool>()
                let targetPoolStat = _targetPools(pool)
                let targetPoolValue = context.GetValue(targetPoolStat)
                where targetPoolValue.HasValue && (Pool) targetPoolValue.Single() == _pool
                select pool;
            var modifiedContext = new ModifiedContext(this, applyingPools.ToList(), context);
            return _transformedValue.Calculate(modifiedContext);
        }

        private class ModifiedContext : IValueCalculationContext
        {
            private readonly RegenUncappedSubtotalValue _value;
            private readonly IReadOnlyList<Pool> _applyingPools;
            private readonly IValueCalculationContext _originalContext;

            public ModifiedContext(
                RegenUncappedSubtotalValue value, IReadOnlyList<Pool> applyingPools,
                IValueCalculationContext originalContext)
            {
                _value = value;
                _applyingPools = applyingPools;
                _originalContext = originalContext;
            }

            public PathDefinition CurrentPath => _originalContext.CurrentPath;

            public IReadOnlyCollection<PathDefinition> GetPaths(IStat stat)
            {
                if (!_value._regens(_value._pool).Equals(stat))
                    return _originalContext.GetPaths(stat);

                return _applyingPools
                    .SelectMany(p => _originalContext.GetPaths(_value._regens(p)))
                    .Distinct().ToList();
            }

            public NodeValue? GetValue(IStat stat, NodeType nodeType, PathDefinition path)
            {
                if (nodeType != NodeType.PathTotal || !_value._regens(_value._pool).Equals(stat))
                    return _originalContext.GetValue(stat, nodeType, path);

                NodeValue? result = null;
                foreach (var pool in _applyingPools)
                {
                    var value = _originalContext.GetValue(_value._regens(pool), nodeType, path);
                    result = result.SumWhereNotNull(value);
                }
                return result;
            }

            public List<NodeValue?> GetValues(Form form, IEnumerable<(IStat stat, PathDefinition path)> paths)
                => _originalContext.GetValues(form, paths);
        }
    }
}