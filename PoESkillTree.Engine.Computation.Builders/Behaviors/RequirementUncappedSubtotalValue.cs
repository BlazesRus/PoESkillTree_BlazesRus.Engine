using System.Collections.Generic;
using MoreLinq;
using PoESkillTree.Engine.Computation.Common;

namespace PoESkillTree.Engine.Computation.Builders.Behaviors
{
    public class RequirementUncappedSubtotalValue : IValue
    {
        private readonly IStat _transformedStat;
        private readonly IValue _transformedValue;

        public RequirementUncappedSubtotalValue(IStat transformedStat, IValue transformedValue)
            => (_transformedStat, _transformedValue) = (transformedStat, transformedValue);

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            var modifiedContext = new ModifiedValueCalculationContext(context, getPaths: GetPaths);
            return _transformedValue.Calculate(modifiedContext);
        }

        private IReadOnlyCollection<PathDefinition> GetPaths(IValueCalculationContext context, IStat stat)
        {
            var originalPaths = context.GetPaths(stat);
            if (!stat.Equals(_transformedStat))
                return originalPaths;

            var maxPath = originalPaths.MaxBy(
                p => context.GetValue(_transformedStat, NodeType.PathTotal, p).SingleOrNull()).First();
            return new[] { maxPath };
        }
    }
}