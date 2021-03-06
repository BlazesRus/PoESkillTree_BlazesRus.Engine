using PoESkillTree.Engine.Computation.Common;

namespace PoESkillTree.Engine.Computation.Core.Nodes
{
    /// <summary>
    /// <see cref="IValue"/> for <see cref="NodeType.Total"/>.
    /// </summary>
    public class TotalValue : IValue
    {
        private readonly IStat _stat;

        public TotalValue(IStat stat)
        {
            _stat = stat;
        }

        public NodeValue? Calculate(IValueCalculationContext context) =>
            _stat.Round(context.GetValue(_stat, NodeType.TotalOverride) ?? context.GetValue(_stat, NodeType.Subtotal));
    }
}