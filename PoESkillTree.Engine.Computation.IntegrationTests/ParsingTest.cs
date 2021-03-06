using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Forms;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.Computation.Data.GivenStats;
using PoESkillTree.Engine.Computation.Parsing;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.Utils.Extensions;
using static PoESkillTree.Engine.Computation.IntegrationTests.ParsingTestUtils;

namespace PoESkillTree.Engine.Computation.IntegrationTests
{
    [TestFixture]
    public class ParsingTest : CompositionRootTestBase
    {
#pragma warning disable 8618 // Initialized in SetUpAsync
        private IParser _parser;
        private IBuilderFactories _f;
#pragma warning restore 8618

        [SetUp]
        public async Task SetUpAsync()
        {
            _parser = await ParserTask.ConfigureAwait(false);
            _f = await BuilderFactoriesTask.ConfigureAwait(false);
        }

        [Test, TestCaseSource(nameof(ReadParseableStatLines))]
        public void Parses(string statLine, ModifierSource modifierSource)
        {
            var actual = Parse(statLine, modifierSource);

            AssertIsParsedSuccessfully(actual);
        }

        [Test, TestCaseSource(nameof(ReadNotParseableStatLines))]
        public void DoesNotParse(string statLine)
        {
            var actual = Parse(statLine, new ModifierSource.Global(new ModifierSource.Local.Item(ItemSlot.Belt)));

            AssertIsParsedUnsuccessfully(actual);
        }

        private static IEnumerable<object[]> ReadParseableStatLines()
        {
            ModifierSource passiveNodeSource = new ModifierSource.Global(new ModifierSource.Local.PassiveNode(0));
            ModifierSource itemSource = new ModifierSource.Global(new ModifierSource.Local.Item(ItemSlot.Belt));
            ModifierSource givenSource = new ModifierSource.Global(new ModifierSource.Local.Given());
            var unparsedGivenStats = new GivenStatsCollection(null!, null!, null!).SelectMany(s => s.GivenStatLines);
            return ReadDataLines("SkillTreeStatLines").Select(s => (s, passiveNodeSource))
                .Concat(ReadDataLines("ItemAffixes").Select(s => (s, itemSource)))
                .Concat(ReadDataLines("ParseableStatLines").Select(s => (s, passiveNodeSource)))
                .Concat(unparsedGivenStats.Select(s => (s, givenSource)))
                .Where(t => !NotParseableStatLines.Value.Contains(t.s.ToLowerInvariant()))
                .Select(t => new object[] {t.s, t.Item2});
        }

        private static IEnumerable<string> ReadNotParseableStatLines() => ParsingTestUtils.ReadNotParseableStatLines();

        [Test]
        public void Dexterity()
        {
            var expected = CreateModifier(
                _f.StatBuilders.Attribute.Dexterity,
                _f.FormBuilders.BaseAdd,
                _f.ValueBuilders.Create(10));
            var actual = Parse("+10 to Dexterity").Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ManaPerGrandSpectrum()
        {
            var expected = new[]
            {
                CreateModifier(
                    _f.StatBuilders.GrandSpectrumJewelsSocketed,
                    _f.FormBuilders.BaseAdd,
                    _f.ValueBuilders.Create(1)),
                CreateModifier(
                    _f.StatBuilders.Pool.From(Pool.Mana),
                    _f.FormBuilders.BaseAdd,
                    _f.ValueBuilders.Create(30).Multiply(_f.StatBuilders.GrandSpectrumJewelsSocketed.Value)),
            }.Flatten();
            var actual = Parse("Gain 30 Mana per Grand Spectrum").Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void CorruptedEnergy()
        {
            var expected = new[]
            {
                CreateModifier(
                    _f.DamageTypeBuilders.Chaos.DamageTakenFrom(_f.StatBuilders.Pool.From(Pool.EnergyShield))
                        .Before(_f.StatBuilders.Pool.From(Pool.Life)),
                    _f.FormBuilders.BaseAdd,
                    _f.ValueBuilders.Create(50),
                    _f.EquipmentBuilders.Equipment.Count(e => e.Corrupted.IsTrue) >= 5),
                CreateModifier(
                    _f.DamageTypeBuilders.Physical.DamageTakenFrom(_f.StatBuilders.Pool.From(Pool.EnergyShield))
                        .Before(_f.StatBuilders.Pool.From(Pool.Life)),
                    _f.FormBuilders.BaseSubtract,
                    _f.ValueBuilders.Create(50),
                    _f.EquipmentBuilders.Equipment.Count(e => e.Corrupted.IsTrue) >= 5)
            }.Flatten();
            var actual = Parse(
                    "With 5 Corrupted Items Equipped: 50% of Chaos Damage does not bypass Energy Shield, and 50% of Physical Damage bypasses Energy Shield")
                .Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void VaalPact()
        {
            var life = _f.StatBuilders.Pool.From(Pool.Life);
            var expected = new[]
            {
                CreateModifier(
                    life.Leech.Rate,
                    _f.FormBuilders.PercentMore,
                    _f.ValueBuilders.Create(100)),
                CreateModifier(
                    life.Leech.RateLimit,
                    _f.FormBuilders.PercentMore,
                    _f.ValueBuilders.Create(100)),
                CreateModifier(
                    life.Regen,
                    _f.FormBuilders.PercentLess,
                    _f.ValueBuilders.Create(100))
            }.Flatten();
            var actual = Parse(
                    "Life Leeched per Second is doubled\nMaximum Life Leech Rate is doubled\nLife Regeneration has no effect")
                .Modifiers;

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void ParagonOfCalamity()
        {
            var expected = new[]
            {
                ParagonOfCalamityFor(_f.DamageTypeBuilders.Fire),
                ParagonOfCalamityFor(_f.DamageTypeBuilders.Lightning),
                ParagonOfCalamityFor(_f.DamageTypeBuilders.Cold),
            }.Flatten();
            var actual = Parse(
                    "For each Element you've been hit by Damage of Recently, 8% reduced Damage taken of that Element")
                .Modifiers;

            Assert.AreEqual(expected, actual);

            IEnumerable<Modifier> ParagonOfCalamityFor(IDamageTypeBuilder damageType) =>
                CreateModifier(
                    damageType.Damage.Taken,
                    _f.FormBuilders.PercentReduce,
                    _f.ValueBuilders.Create(8),
                    _f.ActionBuilders.HitWith(damageType).By(_f.EntityBuilders.OpponentsOfSelf).Recently);
        }

        [Test]
        public void AurasGrantCastRate()
        {
            var expected = new[]
            {
                CreateModifier(
                    _f.BuffBuilders.Buffs(_f.EntityBuilders.Self, _f.EntityBuilders.Self, _f.EntityBuilders.Ally)
                        .With(_f.KeywordBuilders.Aura).Without(_f.KeywordBuilders.Curse)
                        .AddStat(_f.StatBuilders.CastRate),
                    _f.FormBuilders.PercentIncrease,
                    _f.ValueBuilders.Create(3))
            }.Flatten();
            var actual = Parse("Auras from your Skills grant 3% increased Attack and Cast Speed to you and Allies")
                .Modifiers;

            Assert.AreEqual(expected, actual);
        }

        private static IEnumerable<Modifier> CreateModifier(
            IStatBuilder statBuilder, IFormBuilder formBuilder, IValueBuilder valueBuilder)
        {
            var statBuilderResults = statBuilder.Build(default(BuildParameters).With(new ModifierSource.Global()));
            var (form, formValueConverter) = formBuilder.Build();
            foreach (var (stats, source, statValueConverter) in statBuilderResults)
            {
                var value = formValueConverter(statValueConverter(valueBuilder)).Build(default);
                yield return new Modifier(stats, form, value, source);
            }
        }

        private static IEnumerable<Modifier> CreateModifier(
            IStatBuilder statBuilder, IFormBuilder formBuilder, IValueBuilder valueBuilder,
            IConditionBuilder conditionBuilder)
        {
            return CreateModifier(statBuilder.WithCondition(conditionBuilder), formBuilder, valueBuilder);
        }

        private ParseResult Parse(string stat) => Parse(stat, new ModifierSource.Global());

        private ParseResult Parse(string stat, ModifierSource modifierSource) => _parser.ParseRawModifier(stat, modifierSource, Entity.Character);
    }
}