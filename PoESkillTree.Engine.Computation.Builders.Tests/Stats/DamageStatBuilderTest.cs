using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    [TestFixture]
    public class DamageStatBuilderTest
    {
        [Test]
        public void TakenBuildsToCorrectResults()
        {
            var expected = "test.Spell.Skill.Taken";
            var sut = CreateSut();

            var taken = sut.Taken.WithSkills(DamageSource.Spell);
            var results = taken.Build(default).ToList();
            
            Assert.That(results, Has.One.Items);
            var (stats, _, _) = results.Single();
            Assert.That(stats, Has.One.Items);
            var actual = stats.Single().Identity;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void WithKeywordHasCorrectValueConverters()
        {
            var keyword = Keyword.Projectile;
            var keywordBuilder = Mock.Of<IKeywordBuilder>(b => b.Build(default) == keyword);
            var valueBuilder = new ValueBuilderImpl(2);
            var context = SetupKeywordContext(keyword);
            var sut = CreateSut();

            var results = sut.WithHits.With(keywordBuilder).Build(default).ToList();

            Assert.That(results, Has.Exactly(4).Items);
            var attackValue = results[0].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(new NodeValue(2), attackValue);
            var spellValue = results[2].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(null, spellValue);
        }

        [Test]
        public void WithKeywordIsResolved()
        {
            var keyword = Keyword.Projectile;
            var keywordBuilder = Mock.Of<IKeywordBuilder>(b => b.Build(default) == keyword);
            var unresolvedKeywordBuilder = Mock.Of<IKeywordBuilder>(b => b.Resolve(null!) == keywordBuilder);
            var valueBuilder = new ValueBuilderImpl(2);
            var context = SetupKeywordContext(keyword);
            var sut = CreateSut();

            var resolved = sut.WithHits.With(unresolvedKeywordBuilder).Resolve(null!);
            var results = resolved.Build(default).ToList();

            Assert.That(results, Has.Exactly(4).Items);
            var attackValue = results[0].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(new NodeValue(2), attackValue);
            var spellValue = results[2].ValueConverter(valueBuilder).Build().Calculate(context);
            Assert.AreEqual(null, spellValue);
        }

        private static IValueCalculationContext SetupKeywordContext(Keyword keyword)
        {
            var statFactory = new StatFactory();
            var context = Mock.Of<IValueCalculationContext>(c =>
                c.GetValue(statFactory.MainSkillPartDamageHasKeyword(default, keyword, DamageSource.Attack),
                    NodeType.Total, PathDefinition.MainPath) == (NodeValue?) true &&
                c.GetValue(statFactory.MainSkillPartDamageHasKeyword(default, keyword, DamageSource.Spell),
                    NodeType.Total, PathDefinition.MainPath) == (NodeValue?) false);
            return context;
        }

        private static DamageStatBuilder CreateSut()
        {
            var statFactory = new StatFactory();
            var coreStatBuilder = LeafCoreStatBuilder.FromIdentity(statFactory, "test", typeof(double));
            return new DamageStatBuilder(statFactory, coreStatBuilder);
        }
    }
}