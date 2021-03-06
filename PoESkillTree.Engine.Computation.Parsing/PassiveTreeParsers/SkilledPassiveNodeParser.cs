using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.PassiveTree;

namespace PoESkillTree.Engine.Computation.Parsing.PassiveTreeParsers
{
    /// <summary>
    /// Parses passive nodes so the returned modifier can be used to activate skilled nodes in the calculator
    /// whose modifiers were parsed and added previously using <see cref="PassiveNodeParser"/>.
    /// </summary>
    public class SkilledPassiveNodeParser : IParser<ushort>
    {
        private const Entity ModifierSourceEntity = Entity.Character;

        private readonly PassiveTreeDefinition _passiveTreeDefinition;
        private readonly IBuilderFactories _builderFactories;

        public SkilledPassiveNodeParser(
            PassiveTreeDefinition passiveTreeDefinition, IBuilderFactories builderFactories)
            => (_passiveTreeDefinition, _builderFactories) = (passiveTreeDefinition, builderFactories);

        public ParseResult Parse(ushort nodeId)
        {
            var nodeDefinition = _passiveTreeDefinition.GetNodeById(nodeId);
            var localSource = new ModifierSource.Local.PassiveNode(nodeId, nodeDefinition.Name);
            var modifiers = new ModifierCollection(_builderFactories, localSource, ModifierSourceEntity);
            modifiers.AddGlobal(_builderFactories.PassiveTreeBuilders.NodeAllocated(nodeId), Form.TotalOverride, 1);
            modifiers.AddGlobal(_builderFactories.PassiveTreeBuilders.NodeSkillPointSpent(nodeId), Form.TotalOverride, 1);
            return ParseResult.Success(modifiers.Modifiers);
        }
    }
}