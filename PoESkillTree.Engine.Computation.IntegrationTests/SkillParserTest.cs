using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Data.Steps;
using PoESkillTree.Engine.Computation.Parsing;
using PoESkillTree.Engine.Computation.Parsing.SkillParsers;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils.Extensions;
using static PoESkillTree.Engine.Computation.IntegrationTests.ParsingTestUtils;

namespace PoESkillTree.Engine.Computation.IntegrationTests
{
    [TestFixture]
    public class SkillParserTest : CompositionRootTestBase
    {
#pragma warning disable 8618 // Initialized in ClassInit
        private SkillDefinitions _skillDefinitions;
        private Parser<ParsingStep> _parser;
#pragma warning restore 8618

        [SetUp]
        public async Task SetUpAsync()
        {
            _skillDefinitions = await GameData.Skills.ConfigureAwait(false);
            _parser = await ParserTask.ConfigureAwait(false);
        }

        [Test]
        public void ParseFrenzyReturnsCorrectResult()
        {
            var frenzyGem = new Gem("Frenzy", 20, 20, ItemSlot.Boots, 0, 0, true);
            var frenzy = Skill.FromGem(frenzyGem, true);
            var definition = _skillDefinitions.GetSkillById("Frenzy");
            var levelDefinition = definition.Levels[20];
            var local = new ModifierSource.Local.Skill("Frenzy", "Frenzy");
            var global = new ModifierSource.Global(local);
            var valueCalculationContextMock = new Mock<IValueCalculationContext>();
            var isMainSkillStat = SetupIsActiveSkillInContext(valueCalculationContextMock, frenzy);
            var offHandTagsStat = new Stat("OffHand.ItemTags");
            valueCalculationContextMock.Setup(c => c.GetValue(offHandTagsStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(Tags.Weapon.EncodeAsDouble()));
            var mainHandTagsStat = new Stat("MainHand.ItemTags");
            valueCalculationContextMock
                .Setup(c => c.GetValue(mainHandTagsStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(Tags.Ranged.EncodeAsDouble()));
            var frenzyAmountStat = new Stat("Frenzy.Amount");
            valueCalculationContextMock
                .Setup(c => c.GetValue(frenzyAmountStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(3));
            var baseCostStat = new Stat("Boots.0.0.Cost");
            valueCalculationContextMock
                .Setup(c => c.GetValue(baseCostStat, NodeType.Total, PathDefinition.MainPath))
                .Returns((NodeValue?) levelDefinition.ManaCost);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source, bool mainSkillOnly)[]
                {
                    ("SkillHitDamageSource", Form.TotalOverride, (int) DamageSource.Attack, global, true),
                    ("SkillUses.MainHand", Form.TotalOverride, 1, global, true),
                    ("SkillUses.OffHand", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Id", Form.TotalOverride, definition.NumericId, global, true),
                    ("MainSkillPart.Maximum", Form.TotalOverride, definition.PartNames.Count - 1, global, true),
                    ("BaseCastTime.Spell.Skill", Form.BaseSet, definition.ActiveSkill.CastTime / 1000D, global, true),
                    ("BaseCastTime.Secondary.Skill", Form.BaseSet, definition.ActiveSkill.CastTime / 1000D, global,
                        true),
                    ("Frenzy.ActiveSkillItemSlot", Form.BaseSet, (double) frenzy.ItemSlot, global, false),
                    ("Frenzy.ActiveSkillSocketIndex", Form.BaseSet, frenzy.SocketIndex, global, false),
                    ("Boots.0.0.IsEnabled", Form.TotalOverride, 1, global, false),
                    ("Frenzy.Reservation", Form.Increase, null, global, false),
                    ("Frenzy.Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Attack].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Projectile].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Melee].Instances", Form.BaseAdd, 1, global, false),
                    ("Skills[Bow].Instances", Form.BaseAdd, 1, global, false),
                    ("MainSkill.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Has.Melee", Form.TotalOverride, 1, global, true),
                    ("MainSkill.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Has.Melee", Form.TotalOverride, null, global, true),
                    ("MainSkillPart.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.CastRate.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.CastRate.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.CastRate.Has.Melee", Form.TotalOverride, null, global, true),
                    ("MainSkillPart.CastRate.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Attack", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Melee", Form.TotalOverride, null, global, true),
                    ("MainSkillPart.Damage.Attack.Has.Bow", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Spell.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("MainSkillPart.Damage.Secondary.Has.Projectile", Form.TotalOverride, 1, global, true),
                    ("Boots.0.0.Type.attack", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.projectile_attack", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.mirage_archer_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.projectile", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.volley_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.totem_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.trap_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.remote_mine_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.melee_single_target_initial_hit", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.multistrike_supportable", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.melee", Form.TotalOverride, 1, global, false),
                    ("Boots.0.0.Type.triggerable", Form.TotalOverride, 1, global, false),
                    ("DamageBaseAddEffectiveness", Form.TotalOverride, levelDefinition.DamageEffectiveness, global,
                        true),
                    ("DamageBaseSetEffectiveness", Form.TotalOverride, levelDefinition.DamageMultiplier, global, true),
                    ("Boots.0.0.Cost", Form.BaseSet, levelDefinition.ManaCost, global, false),
                    ("Frenzy.Cost", Form.BaseSet, levelDefinition.ManaCost, global, false),
                    ("Mana.Cost", Form.BaseSet, null, global, true),
                    ("Frenzy.Reservation", Form.BaseSet, null, global, false),
                    ("Life.Reservation", Form.BaseAdd, null, global, false),
                    ("EnergyShield.Reservation", Form.BaseAdd, null, global, false),
                    ("Mana.Reservation", Form.BaseAdd, null, global, false),
                    ("CastRate.Attack.MainHand.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("CastRate.Attack.OffHand.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Physical.Damage.Attack.MainHand.Skill", Form.More, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Attack.OffHand.Skill", Form.More, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("Physical.Damage.Spell.Skill", Form.More, null, global,
                        true),
                    ("Physical.Damage.Secondary.Skill", Form.More, null, global,
                        true),
                    ("Physical.Damage.OverTime.Skill", Form.More, null, global,
                        true),
                    ("CastRate.Attack.MainHand.Skill", Form.More, levelDefinition.Stats[1].Value * 3, global,
                        true),
                    ("CastRate.Attack.OffHand.Skill", Form.More, levelDefinition.Stats[1].Value * 3, global, true),
                    ("Range.Attack.MainHand.Skill", Form.BaseAdd, null, global, true),
                    ("Range.Attack.OffHand.Skill", Form.BaseAdd, null, global, true),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source, t.mainSkillOnly)).ToArray();

            var actual = _parser.ParseActiveSkill(new ActiveSkillParserParameter(frenzy, Entity.Character));

            AssertCorrectModifiers(valueCalculationContextMock, isMainSkillStat, expectedModifiers, actual);
        }

        [Test]
        public void ParseAddedColdDamageSupportReturnsCorrectResult()
        {
            var frenzyGem = new Gem("Frenzy", 20, 20, ItemSlot.Boots, 0, 0, true);
            var frenzy = Skill.FromGem(frenzyGem, true);
            var supportGem = new Gem("SupportAddedColdDamage", 20, 20, ItemSlot.Boots, 1, 0, true);
            var support = Skill.FromGem(supportGem, true);
            var definition = _skillDefinitions.GetSkillById(support.Id);
            var levelDefinition = definition.Levels[20];
            var local = new ModifierSource.Local.Skill("Frenzy", "Added Cold Damage Support");
            var global = new ModifierSource.Global(local);
            var valueCalculationContextMock = new Mock<IValueCalculationContext>();
            var isMainSkillStat = SetupIsActiveSkillInContext(valueCalculationContextMock, frenzy);
            var addedDamageValue = new NodeValue(levelDefinition.Stats[0].Value, levelDefinition.Stats[1].Value);
            var expectedModifiers =
                new (string stat, Form form, double? value, ModifierSource source, bool mainSkillOnly)[]
                {
                    ("SupportAddedColdDamage.ActiveSkillItemSlot",
                        Form.BaseSet, (double) support.ItemSlot, global, false),
                    ("SupportAddedColdDamage.ActiveSkillSocketIndex",
                        Form.BaseSet, support.SocketIndex, global, false),
                    ("Boots.1.0.IsEnabled", Form.TotalOverride, 1, global, false),
                    ("Frenzy.Cost", Form.More, levelDefinition.ManaMultiplier * 100 - 100, global, false),
                    ("Cold.Damage.Attack.MainHand.Skill", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Skill", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Spell.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.OverTime.Skill", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Attack.MainHand.Ignite", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.MainHand.Bleed", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.MainHand.Poison", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Ignite", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Bleed", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Attack.OffHand.Poison", Form.Increase,
                        levelDefinition.QualityStats[0].Value * 20 / 1000, global, true),
                    ("Cold.Damage.Spell.Ignite", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Spell.Bleed", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Spell.Poison", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Ignite", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Bleed", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                    ("Cold.Damage.Secondary.Poison", Form.Increase, levelDefinition.QualityStats[0].Value * 20 / 1000,
                        global, true),
                }.Select(t => (t.stat, t.form, (NodeValue?) t.value, t.source, t.mainSkillOnly)).ToArray();
            expectedModifiers = expectedModifiers.Append(
                    ("Cold.Damage.Attack.MainHand.Skill", Form.BaseAdd, addedDamageValue, global, true),
                    ("Cold.Damage.Attack.OffHand.Skill", Form.BaseAdd, addedDamageValue, global, true),
                    ("Cold.Damage.Spell.Skill", Form.BaseAdd, addedDamageValue, global, true),
                    ("Cold.Damage.Secondary.Skill", Form.BaseAdd, addedDamageValue, global, true))
                .ToArray();

            var actual = _parser.ParseSupportSkill(new SupportSkillParserParameter(frenzy, support, Entity.Character));

            AssertCorrectModifiers(valueCalculationContextMock, isMainSkillStat, expectedModifiers, actual);
        }

        private static Stat SetupIsActiveSkillInContext(
            Mock<IValueCalculationContext> contextMock, Skill frenzy)
        {
            var activeSkillItemSlotStat = new Stat("Frenzy.ActiveSkillItemSlot");
            contextMock
                .Setup(c => c.GetValue(activeSkillItemSlotStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue((double) frenzy.ItemSlot));
            var activeSkillSocketIndexStat = new Stat("Frenzy.ActiveSkillSocketIndex");
            contextMock
                .Setup(c => c.GetValue(activeSkillSocketIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue(frenzy.SocketIndex));

            var mainSkillItemSlotStat = new Stat("MainSkill.ItemSlot");
            contextMock
                .Setup(c => c.GetValue(mainSkillItemSlotStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(new NodeValue((double) frenzy.ItemSlot));

            var isMainSkillStat = new Stat("IsMainSkill");
            var mainSkillSocketIndexStat = new Stat("MainSkillSocketIndex");
            contextMock
                .Setup(c => c.GetValue(mainSkillSocketIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(() => new NodeValue(contextMock.Object.GetValue(isMainSkillStat).IsTrue()
                    ? frenzy.SocketIndex
                    : -1));

            var mainSkillSkillIndexStat = new Stat("MainSkillSkillIndex");
            contextMock.Setup(c => c.GetValue(mainSkillSkillIndexStat, NodeType.Total, PathDefinition.MainPath))
                .Returns(() => new NodeValue(contextMock.Object.GetValue(isMainSkillStat).IsTrue() ? frenzy.SkillIndex : -1));
            return isMainSkillStat;
        }

        private static void AssertCorrectModifiers(
            Mock<IValueCalculationContext> contextMock,
            Stat isMainSkillStat,
            (string stat, Form form, NodeValue? value, ModifierSource source, bool mainSkillOnly)[] expectedModifiers,
            ParseResult result)
        {
            var (failedLines, remainingSubstrings, modifiers) = result;

            Assert.IsEmpty(failedLines);
            Assert.IsEmpty(remainingSubstrings);
            for (var i = 0; i < modifiers.Count && i < expectedModifiers.Length; i++)
            {
                var expected = expectedModifiers[i];
                var actual = modifiers[i];
                Assert.AreEqual(expected.stat, actual.Stats[0].Identity, expected.ToString());
                Assert.AreEqual(Entity.Character, actual.Stats[0].Entity, expected.ToString());
                Assert.AreEqual(expected.form, actual.Form, expected.ToString());
                Assert.AreEqual(expected.source, actual.Source, expected.ToString());

                contextMock
                    .Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) true);
                var expectedValue = expected.value;
                var actualValue = actual.Value.Calculate(contextMock.Object);
                Assert.AreEqual(expectedValue, actualValue, expected.ToString());

                contextMock
                    .Setup(c => c.GetValue(isMainSkillStat, NodeType.Total, PathDefinition.MainPath))
                    .Returns((NodeValue?) false);
                expectedValue = expected.mainSkillOnly ? null : expected.value;
                actualValue = actual.Value.Calculate(contextMock.Object);
                Assert.AreEqual(expectedValue, actualValue, expected.ToString());
            }
            Assert.AreEqual(expectedModifiers.Length, modifiers.Count);
        }

        [TestCaseSource(nameof(ReadParseableSkills))]
        public void SkillIsParsedSuccessfully(string skillId)
        {
            var actual = Parse(skillId);

            AssertIsParsedSuccessfully(actual, NotParseableStatLines.Value);
        }

        [TestCaseSource(nameof(ReadNotParseableSkills))]
        public void SkillIsParsedUnsuccessfully(string skillId)
        {
            var actual = Parse(skillId);

            AssertIsParsedUnsuccessfully(actual);
        }

        private ParseResult Parse(string skillId)
        {
            var definition = _skillDefinitions.GetSkillById(skillId);
            var level = Math.Min(definition.Levels.Keys.Max(), 20);
            if (definition.IsSupport)
            {
                var activeGem = new Gem("BloodRage", 20, 20, default, 0, 0, true);
                var activeSkill = Skill.FromGem(activeGem, true);
                var supportGem = new Gem(skillId, level, 20, default, 1, 0, true);
                var supportSkill = Skill.FromGem(supportGem, true);
                return _parser.ParseSupportSkill(new SupportSkillParserParameter(activeSkill, supportSkill, Entity.Character));
            }
            else
            {
                var gem = new Gem(skillId, level, 20, default, 0, 0, true);
                var skill = Skill.FromGem(gem, true);
                return _parser.ParseActiveSkill(new ActiveSkillParserParameter(skill, Entity.Character));
            }
        }

        private static IEnumerable<string> ReadParseableSkills()
            => ReadDataLines("ParseableSkills");

        private static IEnumerable<string> ReadNotParseableSkills()
            => ReadDataLines("NotParseableSkills");
    }
}