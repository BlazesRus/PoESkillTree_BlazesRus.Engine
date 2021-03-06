using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Common;

namespace PoESkillTree.Engine.Computation.Builders.Behaviors
{
    /// <summary>
    /// Behavior of Source.ConvertTo(Target) and Source.GainAs(Target).
    /// Applies to Target.UncappedSubtotal.
    /// Modifies the context to append the missing conversion paths from Source when querying Target's paths.
    /// </summary>
    public class ConversionTargeUncappedSubtotalValue : IValue
    {
        private readonly IStat _target;
        private readonly IStat _source;
        private readonly IValue _transformedValue;

        public ConversionTargeUncappedSubtotalValue(IStat source, IStat target, IValue transformedValue)
        {
            _target = target;
            _source = source;
            _transformedValue = transformedValue;
        }

        public NodeValue? Calculate(IValueCalculationContext context)
        {
            // Make sure paths for conversion chains are created
            context.GetValue(_source, NodeType.UncappedSubtotal, PathDefinition.MainPath);

            var modifiedContext = new ModifiedValueCalculationContext(context, GetPaths);
            return _transformedValue.Calculate(modifiedContext);
        }

        private IReadOnlyCollection<PathDefinition> GetPaths(IValueCalculationContext context, IStat stat)
        {
            if (!_target.Equals(stat))
                return context.GetPaths(stat);

            var originalPaths = context.GetPaths(_target);
            var conversionPaths = context.GetPaths(_source)
                .Select(p => new PathDefinition(p.ModifierSource, p.ConversionStats.Prepend(_source).ToArray()));
            var paths = new HashSet<PathDefinition>(originalPaths);
            paths.UnionWith(conversionPaths);
            return paths;
        }
    }
}