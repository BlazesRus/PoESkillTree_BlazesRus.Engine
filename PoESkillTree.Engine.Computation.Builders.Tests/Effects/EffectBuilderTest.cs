using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Entities;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Builders.Effects
{
    [TestFixture]
    public class EffectBuilderTest
    {
        [Test]
        public void OnBuildsToCorrectResults()
        {
            var entityBuilder = new EntityBuilder(Entity.Enemy);
            var sut = CreateSut();

            var on = sut.On(entityBuilder);
            var stat = on.BuildToSingleStat();

            Assert.AreEqual("Enemy.test.Active", stat.ToString());
        }

        [TestCase(true)]
        [TestCase(false)]
        public void AddStatBuildsToCorrectResult(bool effectActive)
        {
            var expectedStat = "stat";
            var expectedValue = effectActive ? (NodeValue?) 2 : null;
            var statBuilder = StatBuilderUtils.FromIdentity(new StatFactory(), expectedStat, typeof(double));
            var valueBuilder = new ValueBuilderImpl(2);
            var activeStat = new Stat("test.Active");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(activeStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) effectActive);
            var sut = CreateSut();

            var addedStat = sut.AddStat(statBuilder);
            var (stats, _, valueConverter) = addedStat.BuildToSingleResult();
            var actualStat = stats.Single().Identity;
            var actualValue = valueConverter(valueBuilder).Build().Calculate(context);

            Assert.AreEqual(expectedStat, actualStat);
            Assert.AreEqual(expectedValue, actualValue);
        }

        private static EffectBuilder CreateSut() =>
            new EffectBuilder(new StatFactory(), CoreBuilder.Create("test"));
    }
}