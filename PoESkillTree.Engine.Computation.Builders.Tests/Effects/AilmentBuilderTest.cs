using System.Linq;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;

namespace PoESkillTree.Engine.Computation.Builders.Effects
{
    [TestFixture]
    public class AilmentBuilderTest
    {
        [Test]
        public void SourceBuildsToCorrectResults()
        {
            var damageTypes = Mock.Of<IDamageTypeBuilder>(b =>
                b.BuildDamageTypes(default) == new[] { DamageType.Fire, DamageType.Cold });
            var sut = new AilmentBuilder(new StatFactory(), CoreBuilder.Create(Ailment.Bleed));

            var statBuilder = sut.Source(damageTypes);
            var results = statBuilder.Build(default).ToList();

            Assert.That(results, Has.One.Items);
            var stats = results.Single().Stats;
            Assert.That(stats, Has.Exactly(2).Items);
            Assert.AreEqual("Bleed.HasSource.Fire", stats[0].Identity);
            Assert.AreEqual("Bleed.HasSource.Cold", stats[1].Identity);
        }
    }
}