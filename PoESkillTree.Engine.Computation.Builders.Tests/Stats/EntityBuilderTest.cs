using System.Linq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Entities;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    [TestFixture]
    public class EntityBuilderTest
    {
        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character, Entity.Totem, Entity.Minion)]
        public void BuildReturnsConstructorArgument(params Entity[] expected)
        {
            var sut = new EntityBuilder(expected);

            var actual = sut.Build(default);

            Assert.AreEqual(expected, actual);
        }
    }

    [TestFixture]
    public class ModifierSourceEntityBuilderTest
    {
        [TestCase(Entity.Enemy)]
        [TestCase(Entity.Character)]
        public void BuildReturnsArgument(Entity expected)
        {
            var sut = new ModifierSourceEntityBuilder();

            var actual = sut.Build(expected);

            Assert.AreEqual(expected, actual.Single());
        }
    }
}