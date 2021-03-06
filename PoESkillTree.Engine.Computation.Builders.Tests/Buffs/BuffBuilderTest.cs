using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Entities;
using PoESkillTree.Engine.Computation.Builders.Resolving;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Resolving;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Builders.Buffs
{
    [TestFixture]
    public class BuffBuilderTest
    {
        [Test]
        public void OnBuildsToCorrectResults()
        {
            var entityBuilder = new EntityBuilder(Entity.Enemy);
            var sut = CreateSut();

            var on = sut.On(entityBuilder);
            var stats = on.BuildToSingleResult().Stats;

            Assert.That(stats, Has.Exactly(3).Items);
            Assert.AreEqual("Enemy.test.Active", stats[0].ToString());
            Assert.AreEqual("Enemy.test.BuffActive", stats[1].ToString());
            Assert.AreEqual("Enemy.test.BuffSourceIs(Character)", stats[2].ToString());
        }

        [Test]
        public void AddStatBuildsToCorrectResultIfBuffActive()
        {
            var expectedStat = "stat";
            var expectedValue = (NodeValue?) 3;
            var statBuilder = StatBuilderUtils.FromIdentity(StatFactory, expectedStat, typeof(double));
            var valueBuilder = new ValueBuilderImpl(2);
            var activeStat = new Stat("test.Active");
            var buffActiveStat = new Stat("test.BuffActive");
            var buffSourceStat = new Stat("test.BuffSourceIs(Enemy)");
            var effectStat = new Stat($"test.EffectOn({default(Entity)})", Entity.Enemy);
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(activeStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true &&
                c.GetValue(buffActiveStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true &&
                c.GetValue(buffSourceStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true &&
                c.GetValue(effectStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) 1.5);
            var sut = CreateSut();

            var addStat = sut.AddStat(statBuilder);
            var (stats, _, valueConverter) = addStat.BuildToSingleResult();
            var actualStat = stats.Single().Identity;
            var actualValue = valueConverter(valueBuilder).Build().Calculate(context);

            Assert.AreEqual(expectedStat, actualStat);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void AddStatBuildsToCorrectResultIfNotAsBuffActive()
        {
            var expectedStat = "stat";
            var expectedValue = (NodeValue?) 2;
            var statBuilder = StatBuilderUtils.FromIdentity(StatFactory, expectedStat, typeof(double));
            var valueBuilder = new ValueBuilderImpl(2);
            var activeStat = new Stat("test.Active");
            var buffActiveStat = new Stat("test.BuffActive");
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(activeStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true &&
                c.GetValue(buffActiveStat, NodeType.Total, PathDefinition.MainPath) == (NodeValue?) false);
            var sut = CreateSut();

            var addStat = sut.AddStat(statBuilder);
            var (stats, _, valueConverter) = addStat.BuildToSingleResult();
            var actualStat = stats.Single().Identity;
            var actualValue = valueConverter(valueBuilder).Build().Calculate(context);

            Assert.AreEqual(expectedStat, actualStat);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void AddStatResolveResolvesIdentity()
        {
            var expectedIdentity = "buff";
            var statBuilder = StatBuilderUtils.FromIdentity(StatFactory, "stat", typeof(double));
            var resolveContext = new ResolveContext(null!, null!);
            var sut = new BuffBuilder(StatFactory, new UnresolvedCoreBuilder<string>("unresolved",
                c => CoreBuilder.Create(expectedIdentity)));

            var builder = sut.AddStat(statBuilder).Resolve(resolveContext);
            var value = builder.BuildToSingleResult().ValueConverter(new ValueBuilderImpl(1));
        }

        private static BuffBuilder CreateSut() =>
            new BuffBuilder(StatFactory, CoreBuilder.Create("test"));

        private static readonly IStatFactory StatFactory = new StatFactory();
    }
}