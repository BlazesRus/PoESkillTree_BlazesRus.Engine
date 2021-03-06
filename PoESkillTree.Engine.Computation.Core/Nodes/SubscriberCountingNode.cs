using System;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Core.Events;

namespace PoESkillTree.Engine.Computation.Core.Nodes
{
    /// <summary>
    /// Base class for <see cref="ICalculationNode"/> implementations that also implement
    /// <see cref="ICountsSubsribers"/>.
    /// </summary>
    public abstract class SubscriberCountingNode : ICalculationNode, ICountsSubsribers
    {
        public abstract NodeValue? Value { get; }

        public int SubscriberCount => ValueChanged?.GetInvocationList().Length ?? 0;

        public event EventHandler? ValueChanged;

        protected virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}