using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;

namespace PoESkillTree.Engine.Computation.Common.Builders.Stats
{
    /// <summary>
    /// Factory interface for stats and values related to the passive skill tree
    /// </summary>
    public interface IPassiveTreeBuilders
    {
        IStatBuilder NodeAllocated(ushort nodeId);
        IStatBuilder NodeEffectiveness(ushort nodeId);
        IStatBuilder NodeSkillPointSpent(ushort nodeId);

        IStatBuilder ConnectsToClass(CharacterClass characterClass);

        ValueBuilder AllocatedNodeCountInModifierSourceJewelRadius { get; }
        ValueBuilder AllocatedNotableCountInModifierSourceJewelRadius { get; }

        ValueBuilder TotalInModifierSourceJewelRadius(IStatBuilder stat);
        ValueBuilder AllocatedInModifierSourceJewelRadius(IStatBuilder stat);
        ValueBuilder UnallocatedInModifierSourceJewelRadius(IStatBuilder stat);

        /// <summary>
        /// Returns a builder that builds targetAttribute AsPassiveNodePropertyFor each node in the modifier source
        /// jewel's radius with the BaseSet value of sourceAttribute for that node as a value multiplier.
        /// </summary>
        IStatBuilder MultipliedAttributeForNodesInModifierSourceJewelRadius(
            IStatBuilder sourceAttribute, IStatBuilder targetAttribute);

        IStatBuilder ModifyNodeEffectivenessInModifierSourceJewelRadius(
            bool onlyIfSkilled, params PassiveNodeType[] affectedNodeTypes);

        IStatBuilder ConnectJewelToNodesInModifierSourceJewelRadius { get; }
    }
}