using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Modifiers;
using PoESkillTree.Engine.GameModel.PassiveTree;

namespace PoESkillTree.Engine.Computation.Parsing.PassiveTreeParsers
{
    /// <summary>
    /// Parser for passive tree nodes. Adding parsed modifiers to a calculator does nothing on its own.
    /// Use <see cref="SkilledPassiveNodeParser"/> to activate nodes that are skilled. Nodes, keystones in particular,
    /// can also be activated from items and skills. Because of that, the whole passive tree has to be parsed and
    /// added to the calculator initially.
    /// </summary>
    public class PassiveNodeParser : IParser<ushort>
    {
        private const Entity ModifierSourceEntity = Entity.Character;

        private readonly PassiveTreeDefinition _passiveTreeDefinition;
        private readonly IBuilderFactories _builderFactories;
        private readonly ICoreParser _coreParser;

        public PassiveNodeParser(
            PassiveTreeDefinition passiveTreeDefinition, IBuilderFactories builderFactories, ICoreParser coreParser)
            => (_passiveTreeDefinition, _builderFactories, _coreParser) =
                (passiveTreeDefinition, builderFactories, coreParser);

        public ParseResult Parse(ushort nodeId)
        {
            var nodeDefinition = _passiveTreeDefinition.GetNodeById(nodeId);
            var localSource = new ModifierSource.Local.PassiveNode(nodeId, nodeDefinition.Name);
            var globalSource = new ModifierSource.Global(localSource);
            var isAllocatedStat = _builderFactories.PassiveTreeBuilders.NodeAllocated(nodeId);
            var skillPointSpentStat = _builderFactories.PassiveTreeBuilders.NodeSkillPointSpent(nodeId);
            var effectivenessStat = _builderFactories.PassiveTreeBuilders.NodeEffectiveness(nodeId);
            var effectiveness = effectivenessStat.Value;

            var results = new List<ParseResult>(nodeDefinition.Modifiers.Count + 1);
            foreach (var modifier in nodeDefinition.Modifiers)
            {
                var result = ModifierLocalityTester.AffectsPassiveNodeProperty(modifier)
                    ? Parse(modifier + " (AsPassiveNodeBaseProperty)", globalSource)
                    : Parse(modifier, globalSource).ApplyMultiplier(effectiveness.Build, ModifierSourceEntity);
                results.Add(result);
            }
            
            var modifiers = new ModifierCollection(_builderFactories, localSource, ModifierSourceEntity);
            modifiers.AddGlobal(isAllocatedStat, Form.BaseSet, false);
            modifiers.AddGlobal(skillPointSpentStat, Form.BaseSet, false);
            modifiers.AddGlobal(effectivenessStat, Form.BaseSet, isAllocatedStat.Value);

            if (nodeDefinition.CostsPassivePoint)
            {
                var passivePointStat = nodeDefinition.IsAscendancyNode
                    ? _builderFactories.StatBuilders.AscendancyPassivePoints
                    : _builderFactories.StatBuilders.PassivePoints;
                modifiers.AddGlobal(passivePointStat, Form.BaseAdd, 1, skillPointSpentStat.IsTrue);
            }

            var attributes = _builderFactories.StatBuilders.Attribute;
            SetupProperty(modifiers, attributes.Strength, effectiveness);
            SetupProperty(modifiers, attributes.Dexterity, effectiveness);
            SetupProperty(modifiers, attributes.Intelligence, effectiveness);

            results.Add(ParseResult.Success(modifiers.Modifiers));

            return ParseResult.Aggregate(results);
        }

        private static void SetupProperty(ModifierCollection modifiers, IStatBuilder stat, ValueBuilder effectiveness)
        {
            modifiers.AddGlobal(stat, Form.BaseAdd, effectiveness * stat.AsPassiveNodeProperty.Value);
            modifiers.AddGlobal(stat.AsPassiveNodeProperty, Form.BaseSet, stat.AsPassiveNodeBaseProperty.Value);
        }

        private ParseResult Parse(string modifierLine, ModifierSource modifierSource)
            => _coreParser.Parse(modifierLine, modifierSource, ModifierSourceEntity);
    }
}