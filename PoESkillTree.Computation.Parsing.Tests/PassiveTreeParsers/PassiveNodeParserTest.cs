﻿using Moq;
using NUnit.Framework;
using PoESkillTree.Computation.Common;
using PoESkillTree.Computation.Parsing.PassiveTreeParsers;
using PoESkillTree.GameModel;
using PoESkillTree.GameModel.PassiveTree;
using static PoESkillTree.Computation.Parsing.Tests.ParserTestUtils;

namespace PoESkillTree.Computation.Parsing.Tests.PassiveTreeParsers
{
    [TestFixture]
    public class PassiveNodeParserTest
    {
        [Test]
        public void ParsesModifiersCorrectly()
        {
            var definition = CreateNode("+5 to maximum Life");
            var source = new ModifierSource.Global();
            var coreResult = CreateModifier("Life", Form.BaseAdd, 2);
            var expected = CreateConditionalModifier(definition, "Life", Form.BaseAdd, 2);
            var coreParser = Mock.Of<ICoreParser>(p =>
                p.Parse(new CoreParserParameter("+5 to maximum Life", source, Entity.Character))
                == ParseResult.Success(new[] { coreResult }));
            var sut = CreateSut(definition, coreParser);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        [Test]
        public void AddsToPassivePoints()
        {
            var definition = CreateNode();
            var expected = CreateConditionalModifier(definition, "PassivePoints", Form.BaseAdd, 1);
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.AreEqual(new[] { expected }, result.Modifiers);
        }

        [Test]
        public void AddsToAscendancyPassivePointsIfAscendancyNode()
        {
            var definition = CreateNode(true, 0);
            var expected =
                CreateConditionalModifier(definition, "AscendancyPassivePoints", Form.BaseAdd, 1);
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);
            
            Assert.AreEqual(new[] { expected }, result.Modifiers);
        }

        [Test]
        public void AddsPassivePointsGrantedToPassivePointsMaximum()
        {
            var definition = CreateNode(false, 3);
            var expected = CreateConditionalModifier(definition, "PassivePoints.Maximum", Form.BaseAdd, 3);
            var sut = CreateSut(definition);

            var result = sut.Parse(definition.Id);

            Assert.That(result.Modifiers, Has.Member(expected));
        }

        private static PassiveNodeParser CreateSut(PassiveNodeDefinition nodeDefinition, ICoreParser coreParser = null)
        {
            coreParser = coreParser ?? Mock.Of<ICoreParser>();
            var treeDefinition = new PassiveTreeDefinition(new[] { nodeDefinition });
            return new PassiveNodeParser(treeDefinition, CreateBuilderFactories(), coreParser);
        }

        private static PassiveNodeDefinition CreateNode(params string[] modifiers)
            => CreateNode(false, 0, modifiers);

        private static PassiveNodeDefinition CreateNode(
            bool isAscendancyNode, int passivePointsGranted, params string[] modifiers)
            => new PassiveNodeDefinition(42, PassiveNodeType.Normal, "node", isAscendancyNode, passivePointsGranted,
                modifiers);

        private static Modifier CreateConditionalModifier(
            PassiveNodeDefinition nodeDefinition, string stat, Form form, double value)
            => CreateModifier(stat, form, new FunctionalValue(null,
                $"Character.{nodeDefinition.Id}.Skilled.Value(Total, Global).IsSet ? {value} : null"));
    }
}