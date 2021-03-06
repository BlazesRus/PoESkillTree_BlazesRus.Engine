using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Builders.Values;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Skills;
using static PoESkillTree.Engine.Computation.Parsing.ParserTestUtils;
using static PoESkillTree.Engine.Computation.Parsing.SkillParsers.SkillParserTestUtils;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    [TestFixture]
    public class AdditionalSkillQualityParserTest
    {
        [Test]
        public void GivenActiveSkillWithNoSupportingSkillsAndNoAdditionalQualityStats_WhenParsing_ThenValueIsZero()
        {
            var active = CreateSkillFromGem("a");
            var context = MockValueCalculationContextForActiveSkill(active);
            var sut = CreateSut();

            var (_, _, modifiers) = sut.Parse(active, Array.Empty<Skill>(), default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(0));
        }

        [Test]
        public void GivenSupportingSkillWithNoAdditionalQualityStats_WhenParsing_ThenValuesAreZero()
        {
            var active = CreateSkillFromGem("a");
            var support = CreateSkillFromGem("s1");
            var context = MockValueCalculationContextForActiveSkill(active);
            var sut = CreateSut();

            var (_, _, modifiers) = sut.Parse(active, new[] {support}, default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(0));
            GetValueForIdentity(modifiers, StatIdentity(support)).Calculate(context).Should().BeEquivalentTo(new NodeValue(0));
        }

        [Test]
        public void GivenActiveSkillWithAdditionalLevelStatsAndNoSupportingSkills_WhenParsing_ThenValueIsAdditionalLevels()
        {
            var active = CreateSkillFromGem("a");
            var context = MockValueCalculationContextForActiveSkill(active,
                ("Gem.AdditionalQuality.Belt", 3));
            var sut = CreateSut();

            var (_, _, modifiers) = sut.Parse(active, Array.Empty<Skill>(), default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(3));
        }

        [Test]
        public void GivenActiveSkillFromItem_WhenParsing_ThenValueIsZero()
        {
            var active = Skill.FromItem("a", 1, 0, ItemSlot.Belt, 0, true);
            var context = MockValueCalculationContextForActiveSkill(active,
                ("Gem.AdditionalQuality.Belt", 2));
            var sut = CreateSut();

            var (_, _, modifiers) = sut.Parse(active, Array.Empty<Skill>(), default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(0));
        }

        [TestCase("Gem.AdditionalQuality.Belt", 3)]
        [TestCase("Gem.AdditionalQuality.SupportSkill.Belt", 7)]
        public void GivenSupportSkillWithAdditionalQualityStats_WhenParsing_ThenValueIsAdditionalLevels(
            string statId, int statValue)
        {
            var active = CreateSkillFromGem("a");
            var support = CreateSkillFromGem("s1");
            var context = MockValueCalculationContextForActiveSkill(active,
                (statId, statValue));
            var sut = CreateSut();

            var (_, _, modifiers) = sut.Parse(active, new[] {support}, default);

            GetValueForIdentity(modifiers, StatIdentity(support)).Calculate(context).Should().BeEquivalentTo(new NodeValue(statValue));
        }

        [Test]
        public void GivenSupportSkillFromItem_WhenParsing_ThenValueIsZero()
        {
            var active = CreateSkillFromGem("a");
            var support = Skill.FromItem("s1", 1, 0, ItemSlot.Belt, 0, true);
            var context = MockValueCalculationContextForActiveSkill(active,
                ("Gem.AdditionalQuality.Belt", 2));
            var sut = CreateSut();

            var (_, _, modifiers) = sut.Parse(active, new[] {support}, default);

            GetValueForIdentity(modifiers, StatIdentity(support)).Calculate(context).Should().BeEquivalentTo(new NodeValue(0));
        }

        [Test]
        public void GivenActiveSkillAffectedBySupportAddingQuality_WhenParsing_ThenValueIsIncreased()
        {
            var active = CreateSkillFromGem("a");
            var support = CreateSkillFromGem("s1");
            var levelDefinitions = new Dictionary<int, SkillLevelDefinition>
            {
                {1, CreateLevelDefinition(stats: new[] {new UntranslatedStat("supported_active_skill_gem_quality_%", 6),})}
            };
            var context = MockValueCalculationContextForActiveSkill(active,
                ($"Belt.{support.SocketIndex}.0.IsEnabled", 1),
                ($"Skill.AdditionalLevels.Belt.{support.SocketIndex}.0", 0));
            var sut = CreateSut(levelDefinitions);

            var (_, _, modifiers) = sut.Parse(active, new[] {support}, default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(6));
        }

        [Test]
        public void GivenActiveSkillNotAffectedBySupportAddingQuality_WhenParsing_ThenValueIsNotIncreased()
        {
            var active = CreateSkillFromGem("a");
            var support = CreateSkillFromGem("s1");
            var levelDefinitions = new Dictionary<int, SkillLevelDefinition>
            {
                {1, CreateLevelDefinition(stats: new[] {new UntranslatedStat("some_stat", 5),})}
            };
            var context = MockValueCalculationContextForActiveSkill(active,
                ($"Belt.{support.SocketIndex}.0.IsEnabled", 1),
                ($"Skill.AdditionalLevels.Belt.{support.SocketIndex}.0", 0));
            var sut = CreateSut(levelDefinitions);

            var (_, _, modifiers) = sut.Parse(active, new[] {support}, default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(0));
        }

        [Test]
        public void GivenActiveSkillAffectedBySupportAddingQuality_WhenParsing_ThenSupportLevelCanBeIncreased()
        {
            var active = CreateSkillFromGem("a");
            var support = CreateSkillFromGem("s1");
            var levelDefinitions = new Dictionary<int, SkillLevelDefinition>
            {
                {1, CreateLevelDefinition(stats: new[] {new UntranslatedStat("supported_active_skill_gem_quality_%", 1),})},
                {2, CreateLevelDefinition(stats: new[] {new UntranslatedStat("supported_active_skill_gem_quality_%", 2),})},
                {3, CreateLevelDefinition(stats: new[] {new UntranslatedStat("supported_active_skill_gem_quality_%", 3),})},
                {4, CreateLevelDefinition(stats: new[] {new UntranslatedStat("supported_active_skill_gem_quality_%", 4),})},
            };
            var context = MockValueCalculationContextForActiveSkill(active,
                ($"Belt.{support.SocketIndex}.0.IsEnabled", 1),
                ($"Skill.AdditionalLevels.Belt.{support.SocketIndex}.0", 2));
            var sut = CreateSut(levelDefinitions);

            var (_, _, modifiers) = sut.Parse(active, new[] {support}, default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(3));
        }

        [Test]
        public void GivenActiveSkillAffectedByDisabledSupportAddingQuality_WhenParsing_ThenValueIsNotIncreased()
        {
            var active = CreateSkillFromGem("a");
            var support = CreateSkillFromGem("s1");
            var levelDefinitions = new Dictionary<int, SkillLevelDefinition>
            {
                {1, CreateLevelDefinition(stats: new[] {new UntranslatedStat("supported_active_skill_gem_quality_%", 6),})}
            };
            var context = MockValueCalculationContextForActiveSkill(active,
                ($"Belt.{support.SocketIndex}.0.IsEnabled", null),
                ($"Skill.AdditionalLevels.Belt.{support.SocketIndex}.0", 0));
            var sut = CreateSut(levelDefinitions);

            var (_, _, modifiers) = sut.Parse(active, new[] {support}, default);

            GetValueForIdentity(modifiers, StatIdentity(active)).Calculate(context).Should().BeEquivalentTo(new NodeValue(0));
        }

        private static Skill CreateSkillFromGem(string id) =>
            Skill.FromGem(new Gem(id, 1, 0, ItemSlot.Belt, id.GetHashCode(), 0, true), true);

        private static AdditionalSkillQualityParser CreateSut(Dictionary<int, SkillLevelDefinition>? supportLevelDefinitions = null)
        {
            supportLevelDefinitions ??= new Dictionary<int, SkillLevelDefinition>();
            var skillDefinitions = new SkillDefinitions(new[]
            {
                SkillDefinition.CreateActive("a", 0, "", null, Array.Empty<string>(),
                    new SkillBaseItemDefinition("a", "a", ReleaseState.Released, Array.Empty<string>()),
                    CreateActiveSkillDefinition("a"), new Dictionary<int, SkillLevelDefinition>()),
                SkillDefinition.CreateSupport("s1", 2, "", null, Array.Empty<string>(),
                    new SkillBaseItemDefinition("s1", "s1", ReleaseState.Released, Array.Empty<string>()),
                    CreateSupportSkillDefinition(), supportLevelDefinitions),
            });
            var statFactory = new StatFactory();
            return new AdditionalSkillQualityParser(skillDefinitions,
                new GemStatBuilders(statFactory),
                new ValueBuilders(),
                new MetaStatBuilders(statFactory));
        }

        private static string StatIdentity(Skill skill) =>
            $"Skill.AdditionalQuality.{skill.ItemSlot}.{skill.SocketIndex}.{skill.SkillIndex}";
    }
}