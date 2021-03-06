using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Engine.Computation.Parsing.JewelParsers
{
    public class JewelInSkillTreeParser : IParser<JewelInSkillTreeParserParameter>
    {
        private const Entity ModifierSourceEntity = Entity.Character;

        private readonly PassiveTreeDefinition _tree;
        private readonly ITransformationJewelParser _transformationParser;
        private readonly ICoreParser _coreParser;

        public JewelInSkillTreeParser(
            PassiveTreeDefinition tree, IBuilderFactories builderFactories, ICoreParser coreParser)
        {
            _tree = tree;
            _transformationParser = CompositeTransformationJewelParser.Create(
                i => builderFactories.PassiveTreeBuilders.NodeEffectiveness(i).Value);
            _coreParser = coreParser;
        }

        public ParseResult Parse(JewelInSkillTreeParserParameter parameter)
        {
            var (item, radius, nodeId) = parameter;
            if (!item.IsEnabled)
                return ParseResult.Empty;

            var localSource = new ModifierSource.Local.Jewel(radius, nodeId, item.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var nodesInRadius = _tree.GetNodesInRadius(nodeId, radius).ToList();

            var results = new List<ParseResult>(item.Modifiers.Count);
            foreach (var modifier in item.Modifiers)
            {
                results.Add(ParseModifier(modifier, globalSource, nodesInRadius));
            }

            return ParseResult.Aggregate(results);
        }

        private ParseResult ParseModifier(
            string modifier, ModifierSource modifierSource, IEnumerable<PassiveNodeDefinition> nodesInRadius)
            => _transformationParser.IsTransformationJewelModifier(modifier)
                ? ParseTransformationModifier(modifier, modifierSource, nodesInRadius)
                : _coreParser.Parse(modifier, modifierSource, ModifierSourceEntity);

        private ParseResult ParseTransformationModifier(string modifier, ModifierSource modifierSource,
            IEnumerable<PassiveNodeDefinition> nodesInRadius)
        {
            var transformedNodeModifiers = _transformationParser.ApplyTransformation(modifier, nodesInRadius).ToList();
            var results = new List<ParseResult>(transformedNodeModifiers.Count);
            foreach (var transformedModifier in transformedNodeModifiers)
            {
                var parseResult = _coreParser.Parse(transformedModifier.Modifier, modifierSource, ModifierSourceEntity)
                    .ApplyMultiplier(transformedModifier.ValueMultiplier.Build, ModifierSourceEntity);
                results.Add(parseResult);
            }
            return ParseResult.Aggregate(results);
        }
    }

    public class JewelInSkillTreeParserParameter : ValueObject
    {
        public JewelInSkillTreeParserParameter(Item item, JewelRadius jewelRadius, ushort passiveNodeId)
        {
            Item = item;
            JewelRadius = jewelRadius;
            PassiveNodeId = passiveNodeId;
        }

        public Item Item { get; }
        public JewelRadius JewelRadius { get; }
        public ushort PassiveNodeId { get; }

        public void Deconstruct(out Item item, out JewelRadius jewelRadius, out ushort passiveNodeId)
            => (item, jewelRadius, passiveNodeId) = (Item, JewelRadius, PassiveNodeId);

        protected override object ToTuple()
            => (Item, JewelRadius, PassiveNodeId);
    }
}