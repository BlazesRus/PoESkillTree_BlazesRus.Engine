using System.Collections.Generic;
using System.Linq;
using Moq;
using MoreLinq;
using NUnit.Framework;
using PoESkillTree.Engine.Computation.Builders;
using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.PassiveTree;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.GameModel.StatTranslation;
using PoESkillTree.Engine.Utils.Extensions;
using static PoESkillTree.Engine.Computation.Common.Helper;
using static PoESkillTree.Engine.Computation.Parsing.ParserTestUtils;
using static PoESkillTree.Engine.Computation.Parsing.SkillParsers.SkillParserTestUtils;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    [TestFixture]
    public class ActiveSkillParserTest
    {
        #region Frenzy

        [TestCase(Tags.Shield)]
        [TestCase(Tags.Weapon)]
        public void FrenzyUsesOffHandIfWeapon(Tags offHandTags)
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("OffHand.ItemTags", offHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.OffHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(offHandTags.HasFlag(Tags.Weapon), actual.IsTrue());
        }

        [TestCase(Tags.Default)]
        [TestCase(Tags.Ranged)]
        public void FrenzyHasMeleeKeywordIfNotRanged(Tags mainHandTags)
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("MainHand.ItemTags", mainHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "MainSkillPart.Has.Melee")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(!mainHandTags.HasFlag(Tags.Ranged), actual.IsTrue());
        }

        [TestCase(Tags.Default)]
        [TestCase(Tags.Ranged)]
        public void FrenzyHasProjectileKeywordIfRanged(Tags mainHandTags)
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("MainHand.ItemTags", mainHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "MainSkillPart.Has.Projectile")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(mainHandTags.HasFlag(Tags.Ranged), actual.IsTrue());
        }

        [Test]
        public void FrenzySetsManaCost()
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("Frenzy.Cost", 20));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "Mana.Cost")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(20), actual);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void FrenzyAddsToSkillInstances(bool isActiveSkill)
        {
            var expected = isActiveSkill ? (NodeValue?) 1 : null;
            var (definition, skill) = CreateFrenzyDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContext(skill, false, isActiveSkill);
            
            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actualForFrenzy = GetValueForIdentity(modifiers, "Frenzy.Instances").Calculate(context);
            Assert.AreEqual(expected, actualForFrenzy);
            var actualForAllSkills = GetValueForIdentity(modifiers, "Skills[].Instances").Calculate(context);
            Assert.AreEqual(expected, actualForAllSkills);
            var actualForMeleeSkills = GetValueForIdentity(modifiers, "Skills[Melee].Instances").Calculate(context);
            Assert.AreEqual(expected, actualForMeleeSkills);
            var actualForProjectileSkills = GetValueForIdentity(modifiers, "Skills[Projectile].Instances").Calculate(context);
            Assert.AreEqual(expected, actualForProjectileSkills);
        }

        [Test]
        public void ParseReturnsEmptyResultForDisabledGem()
        {
            var (definition, skill) = CreateFrenzyDefinition(gemIsEnabled: false);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            Assert.IsEmpty(result.Modifiers);
        }

        [Test]
        public void ParseReturnsOnlyIsEnabledStatForDisabledSkill()
        {
            var (definition, skill) = CreateFrenzyDefinition(skillIsEnabled: false);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            Assert.IsEmpty(result.Modifiers);
        }

        [Test]
        public void ParseReturnsIsEnabledStatForEnabledSkill()
        {
            var (definition, skill) = CreateFrenzyDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForInactiveSkill(skill);

            var (_, _, modifiers) = Parse(sut, skill);

            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "Belt.0.0.IsEnabled"));
            Assert.AreEqual((NodeValue?) true, GetValueForIdentity(modifiers, "Belt.0.0.IsEnabled").Calculate(context));
        }

        private static (SkillDefinition, Skill) CreateFrenzyDefinition(bool gemIsEnabled = true, bool skillIsEnabled = true) =>
            CreateFrenzyDefinition(gemIsEnabled, skillIsEnabled, (1, 10));

        private static (SkillDefinition, Skill) CreateFrenzyDefinition(params (int level, int manaCost)[] levels) =>
            CreateFrenzyDefinition(true, true, levels);

        private static (SkillDefinition, Skill) CreateFrenzyDefinition(bool gemIsEnabled, bool skillIsEnabled, params (int level, int manaCost)[] levels)
        {
            var activeSkill = CreateActiveSkillDefinition("Frenzy", new[] { "attack" },
                new[] { Keyword.Melee, Keyword.Projectile });
            var levelDefinitions = levels
                .Select(t => (t.level, CreateLevelDefinition(manaCost: t.manaCost)))
                .ToDictionary();
            return (CreateActive("Frenzy", activeSkill, levelDefinitions),
                CreateSkillFromGem("Frenzy", 1, 0, gemIsEnabled, skillIsEnabled));
        }

        #endregion

        #region Flame Totem

        [Test]
        public void FlameTotemHasSpellHitDamageSource()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillHitDamageSource")
                .Calculate(valueCalculationContext);
            Assert.AreEqual((double) DamageSource.Spell, actual.Single());
        }

        [Test]
        public void FlameTotemHasCorrectKeywords()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Has.Projectile"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Spell.Has.Spell"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Ailment.Has.Totem"));
        }

        [Test]
        public void FlameTotemHasCastTimeModifier()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var expected = (NodeValue?) definition.ActiveSkill.CastTime / 1000D;
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "BaseCastTime.Spell.Skill").Calculate(valueCalculationContext);
            Assert.AreEqual(expected, actual);
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "BaseCastTime.Secondary.Skill"));
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "BaseCastTime.Attack.MainHand.Skill"));
        }

        [Test]
        public void FlameTotemHasTotemLifeModifier()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var lifeModifier = result.Modifiers.First(m => m.Stats.First().Identity == "Life");
            Assert.AreEqual(Entity.Totem, lifeModifier.Stats.First().Entity);
            Assert.AreEqual(Form.More, lifeModifier.Form);
            var actual = lifeModifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(62, actual?.Minimum, 1e-10);
            Assert.AreEqual(62, actual?.Maximum, 1e-10);
        }

        [Test]
        public void FlameTotemSetsCriticalStrikeChance()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifier = result.Modifiers.First(m => m.Stats.First().Identity == "CriticalStrike.Chance.Spell.Skill");
            Assert.AreEqual(Form.BaseSet, modifier.Form);
            var actual = modifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(5), actual);
        }

        [Test]
        public void FlameTotemSetsSpellDamage()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "Fire.Damage.Spell.Skill").Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(5, 10), actual);
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "Fire.Damage.Spell.Ignite"));
        }

        [Test]
        public void FlameTotemStatsAreParsedCorrectly()
        {
            var (definition, skill) = CreateFlameTotemDefinition();
            var source = new ModifierSource.Local.Skill("FlameTotem", "Flame Totem");
            var parseResults = new[]
            {
                ParseResult.Success(new[] { MockModifier(new Stat("s1")) }),
                ParseResult.Success(new[] { MockModifier(new Stat("s2")) }),
            };
            var parseParameters = new[]
            {
                new UntranslatedStatParserParameter(source, new[]
                {
                    new UntranslatedStat("totem_life_+%", 10),
                }),
                new UntranslatedStatParserParameter(source, new[]
                {
                    new UntranslatedStat("number_of_additional_projectiles", 2),
                }),
            };
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(parseParameters[0]) == parseResults[0] &&
                p.Parse(parseParameters[1]) == parseResults[1] &&
                p.Parse(EmptyParserParameter(source)) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);

            var result = sut.Parse(skill, Entity.Character);

            var modifiers = result.Modifiers;
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "s1"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "s2"));
        }

        private static (SkillDefinition, Skill) CreateFlameTotemDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Flame Totem", 250, new[] { "spell" },
                new[] { Keyword.Spell, Keyword.Projectile, Keyword.Totem }, totemLifeMultiplier: 1.62);
            var qualityStats = new[]
            {
                new UntranslatedStat("totem_life_+%", 1000),
            };
            var stats = new[]
            {
                new UntranslatedStat("spell_minimum_base_fire_damage", 5),
                new UntranslatedStat("spell_maximum_base_fire_damage", 10),
                new UntranslatedStat("number_of_additional_projectiles", 2),
            };
            var level = CreateLevelDefinition(criticalStrikeChance: 5, qualityStats: qualityStats, stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("FlameTotem", activeSkill, levels),
                CreateSkillFromGem("FlameTotem", 1, 10));
        }

        #endregion

        #region Contagion

        [Test]
        public void ContagionHasNoHitDamageSource()
        {
            var (definition, skill) = CreateContagionDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "SkillHitDamageSource"));
        }

        [Test]
        public void ContagionHasCorrectKeywords()
        {
            var (definition, skill) = CreateContagionDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.OverTime.Has.Spell"));
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.OverTime.Has.AreaOfEffect"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.OverTime.Has.Chaos"));
        }

        [Test]
        public void ContagionSetsDamageOverTime()
        {
            var (definition, skill) = CreateContagionDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "Chaos.Damage.OverTime.Skill")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(1), actual);
        }

        [Test]
        public void ContagionHasNoRequirementsWhenNotAGem()
        {
            var (definition, skill) = CreateContagionDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "Level.Required"));
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "Intelligence.Required"));
        }

        private static (SkillDefinition, Skill) CreateContagionDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Contagion", new[] { "spell" },
                new[] { Keyword.Spell, Keyword.AreaOfEffect, Keyword.Chaos });
            var stats = new[]
            {
                new UntranslatedStat("base_chaos_damage_to_deal_per_minute", 60),
            };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("Contagion", activeSkill, levels),
                CreateSkillFromItem("Contagion", 1));
        }

        #endregion

        #region Shield Charge

        [Test]
        public void ShieldChargeHasCorrectKeywords()
        {
            var (definition, skill) = CreateShieldChargeDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Attack.Has.AreaOfEffect"));
        }

        [Test]
        public void ShieldChargeDoesNotUseMainHand()
        {
            var (definition, skill) = CreateShieldChargeDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("OffHand.ItemTags", Tags.Shield.EncodeAsDouble()),
                ("OffHand.ItemClass", (int) ItemClass.Shield));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.MainHand")
                .Calculate(valueCalculationContext);
            Assert.IsFalse(actual.IsTrue());
        }

        [TestCase(Tags.Shield, ItemClass.Shield)]
        [TestCase(Tags.Weapon, ItemClass.OneHandSword)]
        public void ShieldChargeUsesOffHandIfItHasShield(Tags offHandTags, ItemClass offHandItemClass)
        {
            var (definition, skill) = CreateShieldChargeDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("OffHand.ItemTags", offHandTags.EncodeAsDouble()),
                ("OffHand.ItemClass", (int) offHandItemClass));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.OffHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(offHandTags.HasFlag(Tags.Shield), actual.IsTrue());
        }

        private static (SkillDefinition, Skill) CreateShieldChargeDefinition()
        {
            var types = new[]
                { ActiveSkillType.Attack, ActiveSkillType.RequiresShield };
            var activeSkill = CreateActiveSkillDefinition("ShieldCharge", types,
                new[] {Keyword.Attack, Keyword.AreaOfEffect}, weaponRestrictions: new[] {ItemClass.Shield});
            var stats = new[] { new UntranslatedStat("is_area_damage", 1), };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("ShieldCharge", activeSkill, levels),
                CreateSkillFromItem("ShieldCharge", 1));
        }

        #endregion

        #region Dual Strike

        [TestCase(Tags.Shield)]
        [TestCase(Tags.Weapon)]
        public void DualStrikeUsesMainHandIfOffHandHasWeapon(Tags offHandTags)
        {
            var (definition, skill) = CreateDualStrikeDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("MainHand.ItemClass", (double) ItemClass.Claw),
                ("OffHand.ItemClass", (double) ItemClass.Claw),
                ("OffHand.ItemTags", offHandTags.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.MainHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(offHandTags.HasFlag(Tags.Weapon), actual.IsTrue());
        }

        [TestCase(ItemClass.Claw, ItemClass.Claw)]
        [TestCase(ItemClass.Claw, ItemClass.Wand)]
        [TestCase(ItemClass.Wand, ItemClass.Claw)]
        [TestCase(ItemClass.Wand, ItemClass.Wand)]
        public void DualStrikeUsesBothHandsIfWeaponRestrictionsAreSatisfied(
            ItemClass mainHandClass, ItemClass offHandClass)
        {
            var (definition, skill) = CreateDualStrikeDefinition();
            var expected = definition.ActiveSkill.WeaponRestrictions.Contains(mainHandClass)
                           && definition.ActiveSkill.WeaponRestrictions.Contains(offHandClass);
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("MainHand.ItemClass", (double) mainHandClass),
                ("OffHand.ItemClass", (double) offHandClass),
                ("OffHand.ItemTags", Tags.Weapon.EncodeAsDouble()));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillUses.MainHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(expected, actual.IsTrue());
            actual = GetValueForIdentity(result.Modifiers, "SkillUses.OffHand")
                .Calculate(valueCalculationContext);
            Assert.AreEqual(expected, actual.IsTrue());
        }

        private static (SkillDefinition, Skill) CreateDualStrikeDefinition()
        {
            var types = new[] { ActiveSkillType.Attack, ActiveSkillType.RequiresDualWield };
            var activeSkill = CreateActiveSkillDefinition("DualStrike", types, new[] { Keyword.Attack },
                weaponRestrictions: new[] { ItemClass.Claw, ItemClass.Dagger });
            var level = CreateLevelDefinition();
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("DualStrike", activeSkill, levels),
                CreateSkillFromItem("DualStrike", 1));
        }

        #endregion

        #region Caustic Arrow

        [Test]
        public void CausticArrowHasCorrectKeywords()
        {
            var (definition, skill) = CreateCausticArrowDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            Assert.IsFalse(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.Attack.Has.AreaOfEffect"));
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, "MainSkillPart.Damage.OverTime.Has.AreaOfEffect"));
        }

        [Test]
        public void CausticArrowConversionIsLocal()
        {
            var (definition, skill) = CreateCausticArrowDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var expectedIdentity =
                "Physical.Damage.Attack.MainHand.Skill.ConvertTo(Chaos.Damage.Attack.MainHand.Skill)";
            Assert.IsTrue(AnyModifierHasIdentity(modifiers, expectedIdentity));
            var modifier = GetFirstModifierWithIdentity(modifiers, expectedIdentity);
            Assert.IsInstanceOf<ModifierSource.Local.Skill>(modifier.Source);
        }

        private static (SkillDefinition, Skill) CreateCausticArrowDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Caustic Arrow", new[] { "attack" },
                new[] { Keyword.Attack, Keyword.AreaOfEffect });
            var stats = new[]
            {
                new UntranslatedStat("base_chaos_damage_to_deal_per_minute", 60),
                new UntranslatedStat("skill_physical_damage_%_to_convert_to_chaos", 60),
                new UntranslatedStat("skill_dot_is_area_damage", 1),
            };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("PoisonArrow", activeSkill, levels),
                CreateSkillFromItem("PoisonArrow", 1));
        }

        #endregion

        #region Abyssal Cry

        [Test]
        public void AbyssalCryHasSecondaryHitDamageSource()
        {
            var (definition, skill) = CreateAbyssalCryDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "SkillHitDamageSource")
                .Calculate(valueCalculationContext);
            Assert.AreEqual((double) DamageSource.Secondary, actual.Single());
        }

        [Test]
        public void AbyssalCryHasCooldown()
        {
            var (definition, skill) = CreateAbyssalCryDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill);
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifier = result.Modifiers.First(m => m.Stats.First().Identity == "Cooldown");
            Assert.AreEqual(Form.BaseSet, modifier.Form);
            var actual = modifier.Value.Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(4000), actual);
        }

        private static (SkillDefinition, Skill) CreateAbyssalCryDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("AbyssalCry", new[] { "spell" },
                new[] { Keyword.Spell, Keyword.Projectile, Keyword.Totem });
            var stats = new[]
            {
                new UntranslatedStat("display_skill_deals_secondary_damage", 1),
            };
            var level = CreateLevelDefinition(cooldown: 4000, stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("AbyssalCry", activeSkill, levels),
                CreateSkillFromItem("AbyssalCry", 1));
        }

        #endregion

        #region Double Strike

        [Test]
        public void DoubleStrikeAddsToHitsPerCast()
        {
            var (definition, skill) = CreateDoubleStrikeDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            Assert.IsTrue(AnyModifierHasIdentity(result.Modifiers, "SkillNumberOfHitsPerCast"));
        }

        private static (SkillDefinition, Skill) CreateDoubleStrikeDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("DoubleStrike", new[] { "attack" }, new[] { Keyword.Attack });
            var stats = new[]
            {
                new UntranslatedStat("base_skill_number_of_additional_hits", 1),
            };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("DoubleStrike", activeSkill, levels),
                CreateSkillFromItem("DoubleStrike", 1));
        }

        #endregion

        #region Cleave

        [Test]
        public void CleaveSetsHitsWithBothWeaponsAtOnce()
        {
            var (definition, skill) = CreateCleaveDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            Assert.IsTrue(AnyModifierHasIdentity(result.Modifiers, "SkillDoubleHitsWhenDualWielding"));
        }

        private static (SkillDefinition, Skill) CreateCleaveDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Cleave", new[] { "attack" }, new[] { Keyword.Attack });
            var stats = new[]
            {
                new UntranslatedStat("skill_double_hits_when_dual_wielding", 1),
            };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("Cleave", activeSkill, levels),
                CreateSkillFromItem("Cleave", 1));
        }

        #endregion

        #region Clarity

        [Test]
        public void ClaritySetsSkillBaseCost()
        {
            var (definition, skill) = CreateClarityDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifier = GetFirstModifierWithIdentity(result.Modifiers, "Belt.0.0.Cost");
            var actualValue = modifier.Value.Calculate(null!);
            Assert.AreEqual(new NodeValue(10), actualValue);
        }

        [Test]
        public void ClaritySetsSkillReservation()
        {
            var (definition, skill) = CreateClarityDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForActiveSkill(skill,
                ($"Belt.0.0.Type.{ActiveSkillType.ManaCostIsReservation}", 1),
                ("Clarity.Cost", 20));

            var result = Parse(sut, skill);

            var modifier = GetModifiersWithIdentity(result.Modifiers, "Clarity.Reservation")
                .First(m => m.Form == Form.BaseSet);
            var actualValue = modifier.Value.Calculate(context);
            Assert.AreEqual(new NodeValue(20), actualValue);
        }

        [TestCase(Pool.Mana)]
        [TestCase(Pool.Life)]
        public void ClaritySetsPoolReservation(Pool pool)
        {
            var otherPool = pool == Pool.Life ? Pool.Mana : Pool.Life;
            var (definition, skill) = CreateClarityDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForActiveSkill(skill,
                ($"Belt.0.0.Type.{ActiveSkillType.ManaCostIsReservation}", 1),
                ("Clarity.Reservation", 20),
                ("Clarity.ReservationPool", (double) pool));

            var result = Parse(sut, skill);

            var modifier = GetFirstModifierWithIdentity(result.Modifiers, pool + ".Reservation");
            Assert.AreEqual(new NodeValue(20), modifier.Value.Calculate(context));
            modifier = GetFirstModifierWithIdentity(result.Modifiers, otherPool + ".Reservation");
            Assert.IsNull(modifier.Value.Calculate(context));
        }

        [Test]
        public void ClarityDoesNotSetReservationIfNotActive()
        {
            var (definition, skill) = CreateClarityDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForInactiveSkill(skill,
                ($"Belt.0.0.Type.{ActiveSkillType.ManaCostIsReservation}", 1),
                ("Clarity.Reservation", 20),
                ("Clarity.ReservationPool", (double) Pool.Mana));

            var result = Parse(sut, skill);

            var modifier = GetFirstModifierWithIdentity(result.Modifiers, "Clarity.Reservation");
            Assert.IsNull(modifier.Value.Calculate(context));
            modifier = GetFirstModifierWithIdentity(result.Modifiers, "Mana.Reservation");
            Assert.IsNull(modifier.Value.Calculate(context));
        }

        [Test]
        public void ClaritySetsActiveSkill()
        {
            var (definition, skill) = CreateClarityDefinition();
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifier = GetFirstModifierWithIdentity(result.Modifiers, "Clarity.ActiveSkillItemSlot");
            Assert.AreEqual(new NodeValue((double) skill.ItemSlot), modifier.Value.Calculate(null!));
            modifier = GetFirstModifierWithIdentity(result.Modifiers, "Clarity.ActiveSkillSocketIndex");
            Assert.AreEqual(new NodeValue(skill.SocketIndex), modifier.Value.Calculate(null!));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ClarityBuffsManaRegenIfActive(bool isActiveSkill)
        {
            var expectedValues = new[] { isActiveSkill ? (NodeValue?) 6 : null, isActiveSkill ? (NodeValue?) 8 : null };
            var expectedEntities = new[] { Entity.Character, Entity.Minion };
            var (definition, skill) = CreateClarityDefinition();
            var source = new ModifierSource.Local.Skill("Clarity", "Clarity");
            var results = new[]
            {
                ParseResult.Success(new[]
                    { MockModifier(new Stat("Mana.Regen"), value: new Constant(4)) }),
                ParseResult.Success(new[]
                    { MockModifier(new Stat("Mana.Regen", Entity.Minion), value: new Constant(4)) }),
            };
            var parameters = new[]
            {
                new UntranslatedStatParserParameter(source, Entity.Character, new[]
                    { new UntranslatedStat("base_mana_regeneration_rate_per_minute", 240), }),
                new UntranslatedStatParserParameter(source, Entity.Minion, new[]
                    { new UntranslatedStat("base_mana_regeneration_rate_per_minute", 240), }),
            };
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(parameters[0]) == results[0] &&
                p.Parse(parameters[1]) == results[1] &&
                p.Parse(EmptyParserParameter(source)) == EmptyParseResult);
            var emptyStatParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(EmptyParserParameter(source)) == EmptyParseResult);
            var sut = CreateSut(definition, CreateParser);
            var context = MockValueCalculationContext(skill, false, isActiveSkill,
                ("Clarity.EffectOn(Character)", default, 1.5),
                ("Clarity.EffectOn(Minion)", default, 2),
                ("Clarity.BuffActive", Entity.Character, 1),
                ("Clarity.BuffActive", Entity.Minion, 1),
                ("Clarity.BuffSourceIs(Character)", Entity.Character, 1),
                ("Clarity.BuffSourceIs(Character)", Entity.Minion, 1));

            var result = Parse(sut, skill);

            var modifiers = GetModifiersWithIdentity(result.Modifiers, "Mana.Regen").ToList();
            var actualValues = modifiers.Select(m => m.Value).Calculate(context).ToList();
            Assert.AreEqual(expectedValues, actualValues);
            var actualEntities = modifiers.Select(m => m.Stats.Single().Entity).ToList();
            Assert.AreEqual(expectedEntities, actualEntities);

            IParser<UntranslatedStatParserParameter> CreateParser(IReadOnlyList<string> statTranslationFileNames)
                => statTranslationFileNames[0] == "stat_translations" ? statParser : emptyStatParser;
        }

        [Test]
        public void ClarityUsesSkillStatTranslationFileForBuffStatIfMainCantTranslate()
        {
            var (definition, skill) = CreateClarityDefinition();
            var source = new ModifierSource.Local.Skill("Clarity", "Clarity");
            var results = new[]
            {
                ParseResult.Success(new[]
                    { MockModifier(new Stat("Mana.Regen"), value: new Constant(4)) }),
                ParseResult.Success(new[]
                    { MockModifier(new Stat("Mana.Regen", Entity.Minion), value: new Constant(4)) }),
            };
            var parameters = new[]
            {
                new UntranslatedStatParserParameter(source, Entity.Character, new[]
                    { new UntranslatedStat("base_mana_regeneration_rate_per_minute", 240), }),
                new UntranslatedStatParserParameter(source, Entity.Minion, new[]
                    { new UntranslatedStat("base_mana_regeneration_rate_per_minute", 240), }),
            };
            var skillStatParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(parameters[0]) == results[0] &&
                p.Parse(parameters[1]) == results[1]);
            var emptyStatParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(EmptyParserParameter(source)) == EmptyParseResult);
            var sut = CreateSut(definition, CreateParser);

            var result = Parse(sut, skill);

            Assert.IsTrue(AnyModifierHasIdentity(result.Modifiers, "Mana.Regen"));

            IParser<UntranslatedStatParserParameter> CreateParser(IReadOnlyList<string> statTranslationFileNames)
            {
                if (statTranslationFileNames.Count == 2 &&
                    statTranslationFileNames[0] == StatTranslationFileNames.Main &&
                    statTranslationFileNames[1] == StatTranslationFileNames.Skill)
                {
                    return skillStatParser;
                }
                return emptyStatParser;
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ClarityActivatesBuffIfActive(bool isActiveSkill)
        {
            var expectedValue = (NodeValue?) isActiveSkill;
            var expectedEntities = new[] { Entity.Character, Entity.Minion }.Repeat(3);
            var expectedIdentities =
                new[] { "Clarity.Active", "Clarity.BuffActive", "Clarity.BuffSourceIs(Character)" }
                    .SelectMany(s => Enumerable.Repeat(s, 2));
            var (definition, skill) = CreateClarityDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContext(skill, false, isActiveSkill);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var activeModifier = GetFirstModifierWithIdentity(modifiers, "Clarity.Active");
            var actualValue = activeModifier.Value.Calculate(context);
            Assert.AreEqual(expectedValue, actualValue);
            var actualIdentities = activeModifier.Stats.Select(s => s.Identity).ToList();
            Assert.AreEqual(expectedIdentities, actualIdentities);
            var actualEntities = activeModifier.Stats.Select(s => s.Entity).ToList();
            Assert.AreEqual(expectedEntities, actualEntities);
        }

        private static (SkillDefinition, Skill) CreateClarityDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Clarity",
                new[] { "aura", "mana_cost_is_reservation" },
                new[] { Keyword.Aura }, providesBuff: true);
            var buffStats = new[]
            {
                new BuffStat(new UntranslatedStat("base_mana_regeneration_rate_per_minute", 240),
                    _ => new[] { Entity.Character, Entity.Minion }),
            };
            var level = CreateLevelDefinition(manaCost: 10, buffStats: buffStats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("Clarity", activeSkill, levels),
                CreateSkillFromGem("Clarity", 1));
        }

        #endregion

        #region Hatred

        [Test]
        public void HatredSetsPoolReservation()
        {
            var (definition, skill) = CreateHatredDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForActiveSkill(skill,
                ($"Belt.0.0.Type.{ActiveSkillType.ManaCostIsReservation}", 1),
                ($"Belt.0.0.Type.{ActiveSkillType.ManaCostIsPercentage}", 1),
                ("Hatred.Reservation", 60),
                ("Hatred.ReservationPool", (double) Pool.Mana),
                ("Mana", 200));

            var result = Parse(sut, skill);

            var modifier = GetFirstModifierWithIdentity(result.Modifiers, "Mana.Reservation");
            Assert.AreEqual(new NodeValue(120), modifier.Value.Calculate(context));
        }

        private static (SkillDefinition, Skill) CreateHatredDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Hatred",
                new[] { "aura", "mana_cost_is_reservation", "mana_cost_is_percentage" },
                new[] { Keyword.Aura }, providesBuff: true);
            var level = CreateLevelDefinition(manaCost: 50);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("Hatred", activeSkill, levels),
                CreateSkillFromGem("Hatred", 1));
        }

        #endregion

        #region Blade Flurry

        [TestCase(0)]
        [TestCase(1)]
        public void BladeFlurryStatsDependOnSkillPart(int skillPart)
        {
            var (definition, skill) = CreateBladeFlurryDefinition();
            var source = new ModifierSource.Local.Skill("ChargedAttack", "Blade Flurry");
            var parseResults = new[]
            {
                ParseResult.Success(new[]
                    { MockModifier(new Stat("CastRate.Attack.MainHand.Skill"), value: new Constant(60)) }),
                ParseResult.Success(new[]
                    { MockModifier(new Stat("Physical.Damage.Attack.MainHand.Skill"), value: new Constant(80)) })
            };
            var parseParameters = new[]
            {
                new UntranslatedStatParserParameter(source, new[]
                    { new UntranslatedStat("active_skill_attack_speed_+%_final", 60), }),
                new UntranslatedStatParserParameter(source, new[]
                    { new UntranslatedStat("hit_ailment_damage_+%_final", 80), })
            };
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(parseParameters[0]) == parseResults[0] &&
                p.Parse(parseParameters[1]) == parseResults[1] &&
                p.Parse(EmptyParserParameter(source)) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForMainSkill(skill,
                ("MainSkillPart", skillPart));

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actualCastRate = GetValueForIdentity(modifiers, "CastRate.Attack.MainHand.Skill").Calculate(context);
            Assert.AreEqual(new NodeValue(60), actualCastRate);
            var actualStageMaximum = GetValueForIdentity(modifiers, "SkillStage.Maximum").Calculate(context);
            var expectedStageMaximum = skillPart == 0 ? (NodeValue?) 6 : null;
            Assert.AreEqual(expectedStageMaximum, actualStageMaximum);
            var actualHitsPerCast = GetValueForIdentity(modifiers, "SkillNumberOfHitsPerCast").Calculate(context);
            var expectedHitsPerCast = skillPart == 0 ? null : (NodeValue?) 1;
            Assert.AreEqual(expectedHitsPerCast, actualHitsPerCast);
            var actualDamage = GetValueForIdentity(modifiers, "Physical.Damage.Attack.MainHand.Skill")
                .Calculate(context);
            var expectedDamage = skillPart == 0 ? null : (NodeValue?) 80;
            Assert.AreEqual(expectedDamage, actualDamage);
        }

        [Test]
        public void BladeFlurryHasMaximumMainSkillPartStat()
        {
            var (definition, skill) = CreateBladeFlurryDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForMainSkill(skill);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "MainSkillPart.Maximum").Calculate(context);
            Assert.AreEqual(new NodeValue(definition.PartNames.Count - 1), actual);
        }

        [Test]
        public void BladeFlurryHasAttackSpeedStat()
        {
            var (definition, skill) = CreateBladeFlurryDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForMainSkill(skill);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "CastRate.Attack.MainHand.Skill").Calculate(context);
            Assert.AreEqual(new NodeValue(60), actual);
        }

        private static (SkillDefinition, Skill) CreateBladeFlurryDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Blade Flurry",
                new[] { "attack" }, new[] { Keyword.Attack });
            var stats = new[] { new UntranslatedStat("active_skill_attack_speed_+%_final", 60), };
            var additionalStatsPerPart = new[]
            {
                new[] { new UntranslatedStat("maximum_stages", 6), },
                new[]
                {
                    new UntranslatedStat("base_skill_number_of_additional_hits", 1),
                    new UntranslatedStat("hit_ailment_damage_+%_final", 80),
                },
            };
            var level = CreateLevelDefinition(attackSpeedMultiplier: 60, stats: stats,
                additionalStatsPerPart: additionalStatsPerPart);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("ChargedAttack", activeSkill, levels),
                CreateSkillFromItem("ChargedAttack", 1));
        }

        #endregion

        #region Wild Strike

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void WildStrikeStatsDependOnSkillPart(int skillPart)
        {
            var (definition, skill) = CreateWildStrikeDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForMainSkill(skill,
                ("MainSkillPart", skillPart));

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var fireConversionIdentity =
                "Physical.Damage.Attack.MainHand.Skill.ConvertTo(Fire.Damage.Attack.MainHand.Skill)";
            var actualFireConversion = GetValueForIdentity(modifiers, fireConversionIdentity).Calculate(context);
            var expectedFireConversion = skillPart == 0 ? (NodeValue?) 100 : null;
            Assert.AreEqual(expectedFireConversion, actualFireConversion);
            var coldConversionIdentity =
                "Physical.Damage.Attack.MainHand.Skill.ConvertTo(Cold.Damage.Attack.MainHand.Skill)";
            var actualColdConversion = GetValueForIdentity(modifiers, coldConversionIdentity).Calculate(context);
            var expectedColdConversion = skillPart == 2 ? (NodeValue?) 100 : null;
            Assert.AreEqual(expectedColdConversion, actualColdConversion);
            var actualCastRateHasMelee = GetValuesForIdentity(modifiers, "MainSkillPart.CastRate.Has.Melee")
                .Select(m => m.Calculate(context)).ToList();
            var expectedCastRateHasMelee = skillPart == 1 ? (NodeValue?) 1 : null;
            Assert.That(actualCastRateHasMelee, Has.Exactly(6).Items);
            Assert.AreEqual(expectedCastRateHasMelee, actualCastRateHasMelee[3]);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(5)]
        public void WildStrikeKeywordsDependOnSkillPart(int skillPart)
        {
            var (definition, skill) = CreateWildStrikeDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForMainSkill(skill,
                ("MainSkillPart", skillPart));

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actualHasAttack = GetValuesForIdentity(modifiers, "MainSkillPart.Has.Attack").Calculate(context);
            AssertSparse(6, skillPart, true, actualHasAttack);
            var actualHasMelee = GetValuesForIdentity(modifiers, "MainSkillPart.Has.Melee").Calculate(context);
            AssertSparse(3, 0, skillPart == 0, actualHasMelee);
            var actualHasProjectile =
                GetValuesForIdentity(modifiers, "MainSkillPart.Has.Projectile").Calculate(context);
            AssertSparse(6, skillPart, skillPart == 3, actualHasProjectile);
            var actualHasAoE = GetValuesForIdentity(modifiers, "MainSkillPart.Has.AreaOfEffect").Calculate(context);
            AssertSparse(6, skillPart, true, actualHasAoE);
            var actualHasAoEDamage =
                GetValuesForIdentity(modifiers, "MainSkillPart.Damage.Attack.Has.AreaOfEffect").Calculate(context);
            AssertSparse(1, 0, skillPart == 1, actualHasAoEDamage);
        }

        private static (SkillDefinition, Skill) CreateWildStrikeDefinition()
        {
            var keywords = new[] { Keyword.Attack, Keyword.Melee, Keyword.Projectile, Keyword.AreaOfEffect };
            var keywordsWithoutMelee = keywords.Except(Keyword.Melee).ToArray();
            var keywordsPerPart = new[]
            {
                keywords, keywordsWithoutMelee, keywords, keywordsWithoutMelee, keywords, keywordsWithoutMelee
            };
            var activeSkill = CreateActiveSkillDefinition("Wild Strike", activeSkillTypes: new[] { "attack" },
                keywords: keywords, keywordsPerPart: keywordsPerPart);
            var additionalStatsPerPart = new[]
            {
                new[] { new UntranslatedStat("skill_physical_damage_%_to_convert_to_fire", 100), },
                new[]
                {
                    new UntranslatedStat("skill_physical_damage_%_to_convert_to_fire", 100),
                    new UntranslatedStat("cast_rate_is_melee", 1),
                    new UntranslatedStat("is_area_damage", 1),
                },
                new[] { new UntranslatedStat("skill_physical_damage_%_to_convert_to_cold", 100), },
                new[]
                {
                    new UntranslatedStat("skill_physical_damage_%_to_convert_to_cold", 100),
                    new UntranslatedStat("cast_rate_is_melee", 1),
                    new UntranslatedStat("base_is_projectile", 1),
                },
                new[] { new UntranslatedStat("skill_physical_damage_%_to_convert_to_lightning", 100), },
                new[]
                {
                    new UntranslatedStat("skill_physical_damage_%_to_convert_to_lightning", 100),
                    new UntranslatedStat("cast_rate_is_melee", 1),
                },
            };
            var level = CreateLevelDefinition(additionalStatsPerPart: additionalStatsPerPart);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("WildStrike", activeSkill, levels),
                CreateSkillFromItem("WildStrike", 1));
        }

        #endregion

        #region Infernal Blow

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void InfernalBlowHitDamageSourceDependOnSkillPart(int skillPart)
        {
            var (definition, skill) = CreateInfernalBlowDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForMainSkill(skill,
                ("MainSkillPart", skillPart));

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValuesForIdentity(modifiers, "SkillHitDamageSource").Calculate(context).ToList();
            var expected = new[]
            {
                skillPart == 0 ? (NodeValue?) (int) DamageSource.Attack : null,
                skillPart == 1 ? (NodeValue?) (int) DamageSource.Secondary : null,
                skillPart == 2 ? (NodeValue?) (int) DamageSource.Secondary : null,
            };
            Assert.AreEqual(expected, actual);
        }

        private static (SkillDefinition, Skill) CreateInfernalBlowDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Infernal Blow", new[] { "attack" },
                new[] { Keyword.Attack, Keyword.Melee, Keyword.AreaOfEffect });
            var additionalStatsPerPart = new[]
            {
                new UntranslatedStat[0],
                new[] { new UntranslatedStat("display_skill_deals_secondary_damage", 1), },
                new[] { new UntranslatedStat("display_skill_deals_secondary_damage", 1), },
            };
            var level = CreateLevelDefinition(additionalStatsPerPart: additionalStatsPerPart);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("InfernalBlow", activeSkill, levels),
                CreateSkillFromItem("InfernalBlow", 1));
        }

        #endregion

        #region Ice Spear

        [TestCase(0)]
        [TestCase(1)]
        public void IceSpearStatsDependOnSkillPart(int skillPart)
        {
            var (definition, skill) = CreateIceSpearDefinition();
            var source = new ModifierSource.Local.Skill("IceSpear", "Ice Spear");
            var expected = new[]
            {
                skillPart == 0 ? (NodeValue?) 5 : null,
                skillPart == 1 ? (NodeValue?) 6 : null,
            };
            var parseResults = new[]
            {
                ParseResult.Success(new[] { MockModifier(new Stat("s1"), value: new Constant(5)) }),
                ParseResult.Success(new[] { MockModifier(new Stat("s2"), value: new Constant(6)) }),
            };
            var parseParameters = new[]
            {
                new UntranslatedStatParserParameter(source, definition.Levels[1].AdditionalStatsPerPart[0]),
                new UntranslatedStatParserParameter(source, definition.Levels[1].AdditionalStatsPerPart[1]),
            };
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(parseParameters[0]) == parseResults[0] &&
                p.Parse(parseParameters[1]) == parseResults[1] &&
                p.Parse(EmptyParserParameter(source)) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForMainSkill(skill,
                ("MainSkillPart", skillPart));

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "s1").Calculate(context);
            Assert.AreEqual(expected[0], actual);
            actual = GetValueForIdentity(modifiers, "s2").Calculate(context);
            Assert.AreEqual(expected[1], actual);
        }

        private static (SkillDefinition, Skill) CreateIceSpearDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Ice Spear",
                new[] { "spell" }, new[] { Keyword.Spell });
            var stats = new UntranslatedStat[0];
            var additionalStatsPerPart = new[]
            {
                new[] { new UntranslatedStat("always_pierce", 1), },
                new[] { new UntranslatedStat("critical_strike_chance_+%", 100), },
            };
            var level = CreateLevelDefinition(stats: stats, additionalStatsPerPart: additionalStatsPerPart);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("IceSpear", activeSkill, levels),
                CreateSkillFromItem("IceSpear", 1));
        }

        #endregion

        #region Blade Vortex

        [Test]
        public void BladeVortexHasCorrectHitRate()
        {
            var (definition, skill) = CreateBladeVortexDefinition();
            var sut = CreateSut(definition);
            var context = MockValueCalculationContextForMainSkill(skill);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "HitRate").Calculate(context);
            Assert.AreEqual((NodeValue?) 1000 / 600D, actual);
        }

        private static (SkillDefinition, Skill) CreateBladeVortexDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("BladeVortex", new[] { "spell" }, new[] { Keyword.Spell });
            var stats = new[]
            {
                new UntranslatedStat("hit_rate_ms", 600),
            };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("BladeVortex", activeSkill, levels),
                CreateSkillFromItem("BladeVortex", 1));
        }

        #endregion

        #region Herald Of Ice

        [Test]
        public void HeraldOfIceQualityBuffStatHasCorrectValue()
        {
            var (definition, skill) = CreateHeraldOfIceDefinition();
            var source = new ModifierSource.Local.Skill("HeraldOfIce", "HeraldOfIce");
            var parseResult = ParseResult.Success(new[]
                { MockModifier(new Stat("Cold.Damage.Attack.MainHand.Skill"), value: new Constant(15)) });
            var parameter = new UntranslatedStatParserParameter(source, new[]
                { new UntranslatedStat("herald_of_ice_cold_damage_+%", 15), });
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == EmptyParseResult &&
                p.Parse(parameter) == parseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForActiveSkill(skill);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "Cold.Damage.Attack.MainHand.Skill").Calculate(context);
            Assert.AreEqual((NodeValue?) 15, actual);
        }

        [Test]
        public void HeraldOfIceBuffStatHasCorrectValue()
        {
            var (definition, skill) = CreateHeraldOfIceDefinition();
            var source = new ModifierSource.Local.Skill("HeraldOfIce", "HeraldOfIce");
            var expected = new NodeValue(38, 56);
            var parseResult = ParseResult.Success(new[]
                { MockModifier(new Stat("Cold.Damage.Spell.Skill"), value: new Constant(expected)) });
            var parameter = new UntranslatedStatParserParameter(source, new[]
            {
                new UntranslatedStat("spell_minimum_added_cold_damage", 38),
                new UntranslatedStat("spell_maximum_added_cold_damage", 56),
            });
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == EmptyParseResult &&
                p.Parse(parameter) == parseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForActiveSkill(skill);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "Cold.Damage.Spell.Skill").Calculate(context);
            Assert.AreEqual(expected, actual);
        }

        private static (SkillDefinition, Skill) CreateHeraldOfIceDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("HeraldOfIce",
                new[] { "aura", "mana_cost_is_reservation" },
                new[] { Keyword.Aura }, providesBuff: true);
            var buffStats = new[]
            {
                new BuffStat(new UntranslatedStat("spell_minimum_added_cold_damage", 38),
                    _ => new[] { Entity.Character }),
                new BuffStat(new UntranslatedStat("spell_maximum_added_cold_damage", 56),
                    _ => new[] { Entity.Character }),
            };
            var qualityBuffStats = new[]
            {
                new BuffStat(new UntranslatedStat("herald_of_ice_cold_damage_+%", 750),
                    _ => new[] { Entity.Character }),
            };
            var level = CreateLevelDefinition(manaCost: 10, buffStats: buffStats, qualityBuffStats: qualityBuffStats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("HeraldOfIce", activeSkill, levels),
                CreateSkillFromItem("HeraldOfIce", 1, 20));
        }

        #endregion

        #region Cold Snap

        [Test]
        public void ColdSnapCanBypassCooldown()
        {
            var (definition, skill) = CreateColdSnapDefinition();
            var source = new ModifierSource.Local.Skill("ColdSnap", "Cold Snap");
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(EmptyParserParameter(source)) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForMainSkill(skill);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "CanBypassSkillCooldown").Calculate(context);
            Assert.AreEqual((NodeValue?) true, actual);
        }

        private static (SkillDefinition, Skill) CreateColdSnapDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Cold Snap",
                new[] { "spell" }, new[] { Keyword.Spell });
            var stats = new UntranslatedStat[0];
            var level = CreateLevelDefinition(stats: stats, canBypassCooldown: true);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("ColdSnap", activeSkill, levels),
                CreateSkillFromItem("ColdSnap", 1));
        }

        #endregion

        #region Flammability

        [Test]
        public void FlammabilityAddsToActiveCurses()
        {
            var (definition, skill) = CreateFlammabilityDefinition();
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForActiveSkill(skill);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "ActiveCurses").Calculate(context);
            Assert.AreEqual((NodeValue?) definition.NumericId, actual);
        }

        [Test]
        public void FlammabilityIsNotAppliedIfCurseLimitIsExceeded()
        {
            var (definition, skill) = CreateFlammabilityDefinition();
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForCurse(skill, 1, false, 11, 12);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "Flammability.Active").Calculate(context);
            Assert.AreEqual((NodeValue?) false, actual);
        }

        [TestCase(2, 11, 12)]
        [TestCase(1, 12, 11)]
        public void FlammabilityIsNotAppliedIfCurseLimitIsNotExceeded(int curseLimit, int activeCurse1, int activeCurse2)
        {
            var (definition, skill) = CreateFlammabilityDefinition();
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForCurse(skill, curseLimit, false, activeCurse1, activeCurse2);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "Flammability.Active").Calculate(context);
            Assert.AreEqual((NodeValue?) true, actual);
        }

        [Test]
        public void FlammabilityIsAppliedIfCurseLimitIsExceededAndIgnored()
        {
            var (definition, skill) = CreateFlammabilityDefinition();
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == EmptyParseResult);
            var sut = CreateSut(definition, statParser);
            var context = MockValueCalculationContextForCurse(skill, 1, true, 11, 12);

            var result = Parse(sut, skill);

            var actual = GetValueForIdentity(result.Modifiers, "Flammability.Active").Calculate(context);
            Assert.AreEqual((NodeValue?) true, actual);
        }

        private static (SkillDefinition, Skill) CreateFlammabilityDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Flammability",
                new[] { "spell" }, new[] { Keyword.Spell, Keyword.Curse }, true);
            var buffStats = new[]
            {
                new BuffStat(new UntranslatedStat("base_fire_damage_resistance_%", -10), _ => new[] {Entity.Enemy}),
            };
            var level = CreateLevelDefinition(buffStats: buffStats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (SkillDefinition.CreateActive("Flammability", 12, "", null, new string[0], null, activeSkill, levels),
                CreateSkillFromItem("Flammability", 1));
        }

        private static IValueCalculationContext MockValueCalculationContextForCurse(
            Skill skill, int curseLimit, bool ignoresCurseLimit, params int[] activeCurses)
        {
            var context = MockValueCalculationContextForActiveSkill(skill,
                ("CurseLimit", curseLimit),
                ($"{skill.Id}.IgnoresCurseLimit", ignoresCurseLimit ? (double?) 1 : null));
            var contextMock = Mock.Get(context);
            (IStat, PathDefinition) path = (new Stat("ActiveCurses"), PathDefinition.MainPath);
            contextMock.Setup(c => c.GetValues(Form.BaseAdd, new[] {path}))
                .Returns(activeCurses.Select(i => (NodeValue?) i).ToList());
            return context;
        }

        #endregion

        #region Bodyswap

        [Test]
        public void BodyswapSetsSpellDamageBasedOnLife()
        {
            var (definition, skill) = CreateBodyswapDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("Life", 100));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "Fire.Damage.Spell.Skill").Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(8, 13), actual);
        }

        private static (SkillDefinition, Skill) CreateBodyswapDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Bodyswap");
            var stats = new[]
            {
                new UntranslatedStat("spell_minimum_base_fire_damage", 5),
                new UntranslatedStat("spell_maximum_base_fire_damage", 10),
                new UntranslatedStat("spell_base_fire_damage_%_maximum_life", 3),
            };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("CorpseWarp", activeSkill, levels),
                CreateSkillFromGem("CorpseWarp", 1, 10));
        }

        #endregion

        #region Righteous Fire

        [Test]
        public void RighteousFireSetsDamageOverTimeBasedOnPools()
        {
            var (definition, skill) = CreateRighteousFireDefinition();
            var valueCalculationContext = MockValueCalculationContextForMainSkill(skill,
                ("Life", 100),
                ("EnergyShield", 200));
            var sut = CreateSut(definition);

            var result = Parse(sut, skill);

            var modifiers = result.Modifiers;
            var actual = GetValueForIdentity(modifiers, "Fire.Damage.OverTime.Skill").Calculate(valueCalculationContext);
            Assert.AreEqual(new NodeValue(8), actual);
        }

        private static (SkillDefinition, Skill) CreateRighteousFireDefinition()
        {
            var activeSkill = CreateActiveSkillDefinition("Righteous Fire");
            var stats = new[]
            {
                new UntranslatedStat("base_righteous_fire_%_of_max_life_to_deal_to_nearby_per_minute", 60),
                new UntranslatedStat("base_righteous_fire_%_of_max_energy_shield_to_deal_to_nearby_per_minute", 120),
                new UntranslatedStat("base_fire_damage_to_deal_per_minute", 180),
            };
            var level = CreateLevelDefinition(stats: stats);
            var levels = new Dictionary<int, SkillLevelDefinition> { { 1, level } };
            return (CreateActive("RighteousFire", activeSkill, levels),
                CreateSkillFromGem("RighteousFire", 1, 10));
        }

        #endregion

        private static Skill CreateSkillFromGem(string skillId, int level, int quality = 0, bool gemIsEnabled = true, bool skillIsEnabled = true) =>
            Skill.FromGem(new Gem(skillId, level, quality, ItemSlot.Belt, 0, 0, gemIsEnabled), skillIsEnabled);

        private static Skill CreateSkillFromItem(string skillId, int level, int quality = 0) =>
            Skill.FromItem(skillId, level, quality, ItemSlot.Belt, 0, true);

        private static ActiveSkillParser CreateSut(SkillDefinition skillDefinition)
        {
            var statParser = Mock.Of<IParser<UntranslatedStatParserParameter>>(p =>
                p.Parse(It.IsAny<UntranslatedStatParserParameter>()) == EmptyParseResult);
            return CreateSut(skillDefinition, _ => statParser);
        }

        private static ActiveSkillParser CreateSut(
            SkillDefinition skillDefinition, IParser<UntranslatedStatParserParameter> statParser)
            => CreateSut(skillDefinition, _ => statParser);

        private static ActiveSkillParser CreateSut(
            SkillDefinition skillDefinition, UntranslatedStatParserFactory statParserFactory)
        {
            var skillDefinitions = new SkillDefinitions(new[] { skillDefinition });
            var builderFactories =
                new BuilderFactories(new PassiveTreeDefinition(new PassiveNodeDefinition[0]),skillDefinitions);
            return new ActiveSkillParser(skillDefinitions, builderFactories, statParserFactory);
        }

        private static void AssertSparse(
            int valueCount, int expectedIndex, bool expectedValue, IEnumerable<NodeValue?> actualValues)
        {
            var expected = Enumerable.Repeat(false, valueCount).ToArray();
            expected[expectedIndex] = expectedValue;
            var actual = actualValues.Select(v => v.IsTrue()).ToArray();
            Assert.AreEqual(expected, actual);
        }

        private static ParseResult Parse(IParser<ActiveSkillParserParameter> sut, Skill activeSkill) =>
            sut.Parse(activeSkill, Entity.Character);
    }
}