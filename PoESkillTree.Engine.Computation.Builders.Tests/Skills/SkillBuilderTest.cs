using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Builders.Skills
{
    [TestFixture]
    public class SkillBuilderTest
    {
        [TestCase(42)]
        [TestCase(1)]
        public void SkillIdBuildsToCorrectValue(int expected)
        {
            var coreBuilder = CreateCoreBuilder("", expected);
            var sut = CreateSut(coreBuilder);

            var actual = sut.SkillId.Build().Calculate(null!);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void SkillIdResolveBuildsToCorrectValue()
        {
            var expected = 42;
            var coreBuilder = CreateCoreBuilder("", expected);
            var unresolved = Mock.Of<ICoreBuilder<SkillDefinition>>(b => b.Resolve(null!) == coreBuilder);
            var sut = CreateSut(unresolved);

            var actual = sut.SkillId.Resolve(null!).Build().Calculate(null!);

            Assert.AreEqual((NodeValue?) expected, actual);
        }

        [Test]
        public void InstancesBuildsToCorrectResults()
        {
            var coreBuilder = CreateCoreBuilder("skill");
            var sut = CreateSut(coreBuilder);

            var stat = sut.Instances.BuildToSingleStat();

            Assert.AreEqual("skill.Instances", stat.Identity);
        }

        [Test]
        public void CastBuildsToCorrectResult()
        {
            var coreBuilder = CreateCoreBuilder("skill");
            var sut = CreateSut(coreBuilder);

            var actual = sut.Cast.Build(default);

            Assert.AreEqual("skill.Cast", actual);
        }

        private static ICoreBuilder<SkillDefinition> CreateCoreBuilder(string id, int numericId = 0) =>
            CoreBuilder.Create(CreateSkill(id, numericId));

        private static SkillDefinition CreateSkill(string id, int numericId)
            => SkillDefinition.CreateActive(
                id, numericId, "", null, new[] { "" }, null,
                new ActiveSkillDefinition(id, 0, new string[0], new string[0], new Keyword[0],
                    new[] { new Keyword[0], }, false, null, new ItemClass[0]),
                new Dictionary<int, SkillLevelDefinition>());

        private static SkillBuilder CreateSut(ICoreBuilder<SkillDefinition> coreBuilder) =>
            new SkillBuilder(new StatFactory(), coreBuilder);
    }
}