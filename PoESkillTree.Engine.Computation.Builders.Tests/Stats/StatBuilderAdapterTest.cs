using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    // Most of the tests for StatBuilderAdapter are in StatBuilderTest. This only tests things not tested through
    // StatBuilder.
    [TestFixture]
    public class StatBuilderAdapterTest
    {
        [Test]
        public void ResolveResolvesCondition()
        {
            var statBuilder = Mock.Of<IStatBuilder>();
            var conditionBuilder = new Mock<IConditionBuilder>();
            var sut = new StatBuilderAdapter(statBuilder, conditionBuilder.Object);

            sut.Resolve(null!);

            conditionBuilder.Verify(b => b.Resolve(null!));
        }

        [Test]
        public void BuildReturnsAllResults()
        {
            var results = new[]
            {
                new StatBuilderResult(new IStat[0], new ModifierSource.Global(), Funcs.Identity),
                new StatBuilderResult(new IStat[0], new ModifierSource.Local.Given(), Funcs.Identity),
            };
            var statBuilder = Mock.Of<IStatBuilder>(b => b.Build(default) == results);
            var sut = new StatBuilderAdapter(statBuilder);

            var actual = sut.Build(default);

            Assert.That(actual, Has.Exactly(2).Items);
        }

        [Test]
        public void BuildDoesNotConvertValueIfConditionHasNoValue()
        {
            var expected = Mock.Of<IValueBuilder>();
            var results = new[]
            {
                new StatBuilderResult(new IStat[0], new ModifierSource.Global(), Funcs.Identity),
            };
            var statBuilder = Mock.Of<IStatBuilder>(b => b.Build(default) == results);
            var conditionBuilder =
                Mock.Of<IConditionBuilder>(b => b.Build(default) == new ConditionBuilderResult(null, null));
            var sut = new StatBuilderAdapter(statBuilder, conditionBuilder);

            var (_, _, valueConverter) = sut.Build(default).ToList()[0];
            var actual = valueConverter(expected);

            Assert.AreEqual(expected, actual);
        }
    }
}