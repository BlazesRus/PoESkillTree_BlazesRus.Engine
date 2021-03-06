using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common;

namespace PoESkillTree.Engine.Computation.Core.Graphs
{
    /// <summary>
    /// Defines the rules <see cref="ICalculationGraphPruner"/> uses to select what can be removed.
    /// </summary>
    public interface IGraphPruningRuleSet
    {
        bool CanStatBeConsideredForRemoval(IStat stat, IReadOnlyStatGraph statGraph);

        IEnumerable<NodeSelector> SelectRemovableNodesByNodeType(IReadOnlyStatGraph statGraph);

        IEnumerable<FormNodeSelector> SelectRemovableNodesByForm(IReadOnlyStatGraph statGraph);

        bool CanStatGraphBeRemoved(IReadOnlyStatGraph statGraph);
    }
}