using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using PoESkillTree.Engine.Utils;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Engine.GameModel.Skills
{
    /// <summary>
    /// Contains the skill data that is not accessible through the game data but required for parsing.
    /// Used by <see cref="SkillJsonDeserializer"/> to extend the deserialized <see cref="SkillDefinition"/>s.
    /// </summary>
    public class SkillDefinitionExtensions
    {
        private readonly SkillDefinitionExtension _emptyExtension =
            new SkillDefinitionExtension(new SkillPartDefinitionExtension(),
                new Dictionary<string, Func<Entity, IEnumerable<Entity>>>(), new string[0]);

        private readonly IReadOnlyDictionary<string, SkillDefinitionExtension> _extensions;

        public SkillDefinitionExtension GetExtensionForSkill(string skillId)
            => _extensions.TryGetValue(skillId, out var result) ? result : _emptyExtension;

        public SkillDefinitionExtensions()
        {
            _extensions = CreateCollection().ToDictionary();
        }

        private static SkillDefinitionExtensionCollection CreateCollection() => new SkillDefinitionExtensionCollection
        {
            {
                "AbyssalCry", // Infernal Cry
                EnemyBuff("abyssal_cry_movement_velocity_+%_per_one_hundred_nearby_enemies",
                    "infernal_cry_covered_in_ash_fire_damage_taken_%_per_5_monster_power"),
                Passive("warcry_count_power_from_enemies",
                    "infernal_cry_empowered_attacks_trigger_combust_display")
            },
            {
                "AccuracyAndCritsAura", // Precision
                Aura("accuracy_rating", "critical_strike_chance_+%")
            },
            {
                "AncestorTotemSlam", // Ancestral Warchief
                new SkillPartDefinitionExtension(
                    ReplaceStat("slam_ancestor_totem_grant_owner_melee_damage_+%_final", "melee_damage_+%_final")),
                SelfBuff("slam_ancestor_totem_grant_owner_melee_damage_+%_final")
            },
            {
                "AncestralCry",
                new SkillPartDefinitionExtension(
                    RemoveStat("skill_empower_limitation_specifier_for_stat_description")),
                SelfBuff("ancestral_cry_x_melee_range_per_5_monster_power",
                    "ancestral_cry_physical_damage_reduction_rating_per_5_MP",
                    "ancestral_cry_max_physical_damage_reduction_rating"),
                Passive("ancestral_cry_empowered_attacks_strike_X_additional_enemies",
                    "warcry_count_power_from_enemies")
            },
            {
                "VaalAncestralWarchief",
                new SkillPartDefinitionExtension(
                    ReplaceStat("slam_ancestor_totem_grant_owner_melee_damage_+%_final", "melee_damage_+%_final")),
                SelfBuff("slam_ancestor_totem_grant_owner_melee_damage_+%_final")
            },
            {
                "Anger",
                SelfBuff("spell_minimum_added_fire_damage", "spell_maximum_added_fire_damage",
                    "attack_minimum_added_fire_damage", "attack_maximum_added_fire_damage")
            },
            {
                "ArcaneCloak",
                SelfBuff("arcane_cloak_gain_%_of_consumed_mana_as_lightning_damage")
            },
            {
                "ArcticArmour",
                SelfBuff("new_arctic_armour_physical_damage_taken_when_hit_+%_final",
                    "new_arctic_armour_fire_damage_taken_when_hit_+%_final")
            },
            {
                "AssassinsMark",
                new SkillPartDefinitionExtension(
                    ReplaceStat("chance_to_grant_power_charge_on_death_%", "add_power_charge_on_kill_%_chance")
                        .AndThen(ReplaceStat("life_granted_when_killed", "base_life_gained_on_enemy_death"))
                        .AndThen(ReplaceStat("mana_granted_when_killed", "base_mana_gained_on_enemy_death"))
                        .AndThen(ReplaceStat("enemy_additional_critical_strike_chance_against_self",
                            "additional_critical_strike_chance_permyriad"))),
                Buff(("base_self_critical_strike_multiplier_-%", Opponents),
                    ("additional_critical_strike_chance_permyriad", SelfAndAllies),
                    ("add_power_charge_on_kill_%_chance", SelfAndAllies),
                    ("base_life_gained_on_enemy_death", SelfAndAllies),
                    ("base_mana_gained_on_enemy_death", SelfAndAllies))
            },
            {
                "BearTrap",
                new SkillPartDefinitionExtension(
                    ReplaceStat("bear_trap_damage_taken_+%_from_traps_and_mines",
                        "damage_taken_from_traps_and_mines_+%")),
                EnemyBuff("bear_trap_damage_taken_+%_from_traps_and_mines")
            },
            {
                "Berserk",
                SelfBuff("berserk_attack_damage_+%_final",
                    "berserk_attack_speed_+%_final",
                    "berserk_movement_speed_+%_final",
                    "berserk_base_damage_taken_+%_final",
                    "base_actor_scale_+%")
            },
            {
                "Bladestorm",
                SelfBuff("bladestorm_attack_speed_+%_final_while_in_bloodstorm",
                    "bladestorm_movement_speed_+%_while_in_sandstorm"),
                ("Attack", new SkillPartDefinitionExtension(RemoveStat("bladestorm_storm_damage_+%_final"))),
                ("Bladestorm", new SkillPartDefinitionExtension(
                    ReplaceStat("bladestorm_storm_damage_+%_final", "damage_+%_final")))
            },
            {
                "BladeVortex",
                new SkillPartDefinitionExtension(
                    RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                    AddStat("hit_rate_ms", 600),
                    ReplaceStat("maximum_number_of_spinning_blades", "maximum_stages"))
            },
            {
                "VaalBladeVortex",
                new SkillPartDefinitionExtension(ReplaceStat("base_blade_vortex_hit_rate_ms", "hit_rate_ms"))
            },
            {
                "BlastRain",
                ("Single Explosion", new SkillPartDefinitionExtension()),
                ("All Explosions", new SkillPartDefinitionExtension())
            },
            {
                "Blight",
                EnemyBuff("base_movement_velocity_+%")
            },
            {
                "VaalBlight",
                EnemyBuff("base_movement_velocity_+%")
            },
            {
                "BloodRage",
                SelfBuff("life_leech_from_physical_attack_damage_permyriad",
                    "base_physical_damage_%_of_maximum_life_to_deal_per_minute",
                    "base_physical_damage_%_of_maximum_energy_shield_to_deal_per_minute",
                    "add_frenzy_charge_on_kill_%_chance",
                    "attack_speed_+%_granted_from_skill")
            },
            {
                "BloodSandArmour", // Flesh and Stone
                // This stat can't be parsed but is required for the buff to be applied to enemies
                EnemyBuff("damage_taken_+%_final_from_enemies_unaffected_by_sand_armour"),
                Passive("support_maimed_enemies_physical_damage_taken_+%")
            },
            {
                "BloodSandStance", // Blood and Sand
                SelfBuff("blood_sand_stance_melee_skills_area_of_effect_+%_final_in_blood_stance",
                    "blood_sand_stance_melee_skills_area_damage_+%_final_in_blood_stance",
                    "blood_sand_stance_melee_skills_area_of_effect_+%_final_in_sand_stance",
                    "blood_sand_stance_melee_skills_area_damage_+%_final_in_sand_stance")
            },
            {
                "BloodstainedBanner", // War Banner
                Buff(("physical_damage_taken_+%", Opponents),
                    ("accuracy_rating_+%", SelfAndAllies)),
                Passive("aura_effect_+%", "banner_buff_effect_+%_per_stage")
            },
            {
                "Bodyswap",
                ("Self Explosion", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStat("display_skill_deals_secondary_damage", 1)))
            },
            {
                "BrandSupport", // Arcanist Brand
                BrandExtension
            },
            { "CataclysmSigil", BrandExtension }, // Armageddon Brand
            {
                "ChargedDash",
                new SkillPartDefinitionExtension(RemoveStats("base_skill_show_average_damage_instead_of_dps",
                    "charged_dash_damage_+%_final_per_stack"))
            },
            {
                "ChargedAttack", // Blade Flurry
                RemoveShowAverageDamageExtension,
                ("No Release", new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 6))),
                ("Release at 6 Stages", new SkillPartDefinitionExtension(
                    RemoveStat("charged_attack_damage_per_stack_+%_final"),
                    AddStats(
                        // For releasing
                        ("base_skill_number_of_additional_hits", 1),
                        // Average stage multiplier, slightly smaller than the perfect 85
                        ("hit_ailment_damage_+%_final", 80))))
            },
            { "Clarity", Aura("base_mana_regeneration_rate_per_minute") },
            { "VaalClarity", Aura("no_mana_cost") },
            { "ClusterBurst", SecondaryExplosionProjectileParts }, // Kinetic Blast
            {
                "ColdImpurity", // Vaal Impurity of Ice
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_immune_to_freeze", "base_avoid_freeze_%", 100)
                        .AndThen(ReplaceStat("base_immune_to_chill", "base_avoid_chill_%", 100))),
                Buff(("cold_damage_taken_+%", SelfAndAllies),
                    ("base_avoid_freeze_%", SelfAndAllies),
                    ("base_avoid_chill_%", SelfAndAllies),
                    ("hits_ignore_my_cold_resistance", Opponents)),
                Passive("aura_effect_+%")
            },
            {
                "ColdResistAura", // Purity of Ice
                Aura("base_cold_damage_resistance_%", "base_maximum_cold_damage_resistance_%")
            },
            {
                "ColdSnap",
                new SkillPartDefinitionExtension(RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                    AddStat("skill_dot_is_area_damage", 1))
            },
            { "VaalColdSnap", SkillDotIsAreaDamageExtension },
            {
                "Conductivity",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_self_shock_duration_-%", "shock_duration_+%", v => -v)
                        .AndThen(ReplaceStat("chance_to_be_shocked_%", "base_chance_to_shock_%"))),
                Buff(("base_lightning_damage_resistance_%", Opponents),
                    ("shock_duration_+%", SelfAndAllies),
                    ("base_chance_to_shock_%", SelfAndAllies))
            },
            { "ConduitSigil", BrandExtension }, // Storm Brand
            { "Convocation", Buff(Minions, "life_regeneration_rate_per_minute_%") },
            { "CorpseEruption", CorpseExplodingSpellParts }, // Cremation
            {
                "DamageOverTimeAura", // Malevolence
                new SkillPartDefinitionExtension(
                    ReplaceStat("delirium_aura_damage_over_time_+%_final", "damage_over_time_+%_final")
                        .AndThen(ReplaceStat("delirium_skill_effect_duration_+%", "skill_effect_duration_+%"))),
                Aura("damage_over_time_+%_final", "skill_effect_duration_+%")
            },
            {
                "DarkPact",
                ("Cast on Self", new SkillPartDefinitionExtension(
                    ReplaceStat("skeletal_chains_aoe_%_health_dealt_as_chaos_damage",
                            "spell_base_chaos_damage_%_maximum_life")
                        .AndThen(ReplaceStat("skeletal_chains_no_minions_damage_+%_final",
                            "hit_ailment_damage_+%_final")))),
                ("Cast on Skeleton", new SkillPartDefinitionExtension())
            },
            {
                "DarkRitual", // Bane
                new SkillPartDefinitionExtension(RemoveStat("display_linked_curse_effect_+%"))
            },
            { "Desecrate", SkillDotIsAreaDamageExtension },
            {
                "Despair",
                new SkillPartDefinitionExtension(
                    ReplaceStat("minimum_added_chaos_damage_taken", "global_minimum_added_chaos_damage")
                        .AndThen(ReplaceStat("maximum_added_chaos_damage_taken", "global_maximum_added_chaos_damage"))),
                Buff(("degen_effect_+%", Opponents),
                    ("base_chaos_damage_resistance_%", Opponents),
                    ("global_minimum_added_chaos_damage", SelfAndAllies),
                    ("global_maximum_added_chaos_damage", SelfAndAllies))
            },
            {
                "Determination",
                new SkillPartDefinitionExtension(
                    ReplaceStat("determination_aura_armour_+%_final", "armour_+%_final")),
                Aura("armour_+%_final")
            },
            { "DetonateDead", CorpseExplodingSpellParts },
            { "VaalDetonateDead", CorpseExplodingSpellParts },
            { "Discipline", Aura("energy_shield_recharge_rate_+%", "base_maximum_energy_shield") },
            { "VaalDiscipline", Aura("energy_shield_recharge_not_delayed_by_damage") },
            {
                "DoubleSlash", // Lacerate
                ("Single Slash", new SkillPartDefinitionExtension()),
                ("Both Slashes", new SkillPartDefinitionExtension(
                    AddStat("base_skill_number_of_additional_hits", 1)))
            },
            {
                "DivineTempest", // Divine Ire
                ("Channelling", new SkillPartDefinitionExtension()),
                ("Release", new SkillPartDefinitionExtension(
                    AddStats(("maximum_stages", 20), ("base_skill_show_average_damage_instead_of_dps", 1))))
            },
            { "Earthquake", EarthquakeParts },
            { "VaalEarthquake", EarthquakeParts },
            {
                "ElementalHit",
                ("Fire", new SkillPartDefinitionExtension()),
                ("Cold", new SkillPartDefinitionExtension()),
                ("Lightning", new SkillPartDefinitionExtension())
            },
            { "ElementalWeakness", EnemyBuff("base_resist_all_elements_%") },
            {
                "EnduringCry",
                SelfBuff("resist_all_elements_%_per_endurance_charge",
                    "physical_damage_reduction_%_per_endurance_charge"),
                Passive("warcry_count_power_from_enemies")
            },
            {
                "Enfeeble",
                new SkillPartDefinitionExtension(
                    ReplaceStat("enfeeble_damage_+%_final", "damage_+%_vs_normal_or_magic_final")
                        .AndThen(ReplaceStat("enfeeble_damage_+%_vs_rare_or_unique_final",
                            "damage_+%_vs_rare_or_unique_final"))),
                EnemyBuff("critical_strike_chance_+%", "accuracy_rating_+%",
                    "damage_+%_vs_normal_or_magic_final", "damage_+%_vs_rare_or_unique_final",
                    "base_critical_strike_multiplier_+")
            },
            {
                "EnsnaringArrow",
                Passive("tethered_movement_speed_+%_final_per_rope", "tethered_movement_speed_+%_final_per_rope_vs_rare",
                    "tethered_movement_speed_+%_final_per_rope_vs_unique", "tethered_enemies_take_attack_projectile_damage_taken_+%",
                    "tethering_arrow_display_rope_limit")
            },
            {
                "ExpandingFireCone", // Incinerate
                ("Channeling", new SkillPartDefinitionExtension(
                    RemoveStat("expanding_fire_cone_final_wave_always_ignite"),
                    ReplaceStat("expanding_fire_cone_maximum_number_of_stages", "maximum_stages"))),
                ("Release", new SkillPartDefinitionExtension(
                    AddStat("base_skill_show_average_damage_instead_of_dps", 1),
                    ReplaceStat("expanding_fire_cone_final_wave_always_ignite", "always_ignite")
                        .AndThen(ReplaceStat("expanding_fire_cone_maximum_number_of_stages", "maximum_stages", 0))))
            },
            {
                "ExplosiveArrow",
                ("Attack", new SkillPartDefinitionExtension()),
                ("Explosion", AddShowAverageDamageExtension)
            },
            { "Fireball", SecondaryExplosionProjectileParts },
            { "VaalFireball", SecondaryExplosionProjectileParts },
            {
                "FireBeam", // Scorching Ray
                new SkillPartDefinitionExtension(RemoveStat("base_fire_damage_resistance_%")),
                EnemyBuff("base_fire_damage_resistance_%")
            },
            {
                "FireImpurity", // Vaal Impurity of Fire
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_immune_to_ignite", "base_avoid_ignite_%", 100)),
                Buff(("fire_damage_taken_+%", SelfAndAllies),
                    ("base_avoid_ignite_%", SelfAndAllies),
                    ("hits_ignore_my_fire_resistance", Opponents)),
                Passive("aura_effect_+%")
            },
            {
                "FireResistAura", // Purity of Fire
                Aura("base_fire_damage_resistance_%", "base_maximum_fire_damage_resistance_%")
            },
            { "FireTrap", SkillDotIsAreaDamageExtension },
            { "Flameblast", new SkillPartDefinitionExtension(AddStat("maximum_stages", 9)) },
            { "FlameDash", SkillDotIsAreaDamageExtension },
            {
                "Flammability",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_self_ignite_duration_-%", "ignite_duration_+%", v => -v)
                        .AndThen(ReplaceStat("chance_to_be_ignited_%", "base_chance_to_ignite_%"))),
                Buff(("base_fire_damage_resistance_%", Opponents),
                    ("ignite_duration_+%", SelfAndAllies),
                    ("base_chance_to_ignite_%", SelfAndAllies))
            },
            { "FlickerStrike", RemoveShowAverageDamageExtension },
            {
                "Frostbite",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_self_freeze_duration_-%", "freeze_duration_+%", v => -v)
                        .AndThen(ReplaceStat("chance_to_be_frozen_%", "base_chance_to_freeze_%"))),
                Buff(("base_cold_damage_resistance_%", Opponents),
                    ("freeze_duration_+%", SelfAndAllies),
                    ("base_chance_to_freeze_%", SelfAndAllies))
            },
            { "FrostBlades", SecondaryProjectileMeleeAttackParts },
            {
                "FrostBomb",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_cold_damage_resistance_%", "cold_exposure_%")),
                EnemyBuff("cold_exposure_%", "life_regeneration_rate_+%",
                    "energy_shield_regeneration_rate_+%", "energy_shield_recharge_rate_+%")
            },
            { "FrostBoltNova", SkillDotIsAreaDamageExtension }, // Vortex
            {
                "FrostFury", // Winter Orb
                new SkillPartDefinitionExtension(
                    RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                    ReplaceStat("frost_fury_base_fire_interval_ms", "hit_rate_ms")
                        .AndThen(ReplaceStat("frost_fury_max_number_of_stages", "maximum_stages")))
            },
            {
                "GeneralsCry",
                Passive("warcry_gain_mp_from_corpses",
                    "warcry_count_power_from_enemies")
            },
            { "Grace", Aura("base_evasion_rating") },
            { "VaalGrace", Aura("base_chance_to_dodge_%", "base_chance_to_dodge_spells_%") },
            {
                "Haste",
                Aura("attack_speed_+%_granted_from_skill", "cast_speed_+%_granted_from_skill",
                    "base_movement_velocity_+%")
            },
            {
                "VaalHaste",
                Aura("attack_speed_+%_granted_from_skill", "cast_speed_+%_granted_from_skill",
                    "base_movement_velocity_+%")
            },
            {
                "Hatred",
                new SkillPartDefinitionExtension(
                    ReplaceStat("hatred_aura_cold_damage_+%_final", "cold_damage_+%_final")),
                Aura("physical_damage_%_to_add_as_cold", "cold_damage_+%_final")
            },
            {
                "HeraldOfAgony",
                SelfBuff("skill_buff_grants_chance_to_poison_%", "herald_of_agony_poison_damage_+%_final",
                    "herald_of_agony_add_stack_on_poison")
            },
            {
                "HeraldOfAsh",
                SelfBuff("physical_damage_%_to_add_as_fire", "herald_of_ash_fire_damage_+%",
                    "herald_of_ash_spell_fire_damage_+%_final", "herald_of_ash_burning_damage_+%_final")
            },
            {
                "HeraldOfIce",
                SelfBuff("herald_of_ice_cold_damage_+%",
                    "spell_minimum_added_cold_damage", "spell_maximum_added_cold_damage",
                    "attack_minimum_added_cold_damage", "attack_maximum_added_cold_damage")
            },
            {
                "HeraldOfPurity",
                SelfBuff("herald_of_light_spell_minimum_added_physical_damage",
                    "herald_of_light_spell_maximum_added_physical_damage",
                    "herald_of_light_attack_minimum_added_physical_damage",
                    "herald_of_light_attack_maximum_added_physical_damage")
            },
            {
                "HeraldOfThunder",
                new SkillPartDefinitionExtension(
                    RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                    ReplaceStat("herald_of_thunder_bolt_base_frequency", "hit_rate_ms")),
                SelfBuff("herald_of_thunder_lightning_damage_+%",
                    "spell_minimum_added_lightning_damage", "spell_maximum_added_lightning_damage",
                    "attack_minimum_added_lightning_damage", "attack_maximum_added_lightning_damage")
            },
            {
                "IceCrash",
                ("First Hit", new SkillPartDefinitionExtension(
                    RemoveStats("ice_crash_second_hit_damage_+%_final", "ice_crash_third_hit_damage_+%_final"))),
                ("Second Hit", new SkillPartDefinitionExtension(
                    RemoveStat("ice_crash_third_hit_damage_+%_final"),
                    ReplaceStat("ice_crash_second_hit_damage_+%_final", "damage_+%_final"))),
                ("Third Hit", new SkillPartDefinitionExtension(
                    RemoveStat("ice_crash_second_hit_damage_+%_final"),
                    ReplaceStat("ice_crash_third_hit_damage_+%_final", "damage_+%_final")))
            },
            {
                "IceDash", // Frostblink
                AddShowAverageDamageExtension
            },
            {
                "IceShot",
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Cone", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1)))
            },
            {
                "IceSpear",
                ("First Form (Single Projectile)", IceSpearFirstFormExtension),
                ("First Form (All Projectiles)", IceSpearFirstFormExtension),
                ("Second Form (Single Projectile)", IceSpearSecondFormExtension),
                ("Second Form (All Projectiles)", IceSpearSecondFormExtension)
            },
            {
                "ImmolationSigil", // Wintertide Brand
                BrandExtension
            },
            {
                "InfernalBlow",
                ("Attack", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStats(
                        ("display_skill_deals_secondary_damage", 1),
                        ("base_skill_show_average_damage_instead_of_dps", 1)))),
                ("6 Charge Explosion", new SkillPartDefinitionExtension(
                    RemoveStat("corpse_explosion_monster_life_%"),
                    AddStats(
                        ("display_skill_deals_secondary_damage", 1),
                        ("base_skill_show_average_damage_instead_of_dps", 1))))
            },
            {
                "IntimidatingCry",
                SelfBuff("intimidating_cry_enemy_phys_reduction_%_penalty_vs_hit_per_5_MP"),
                Passive("warcry_count_power_from_enemies",
                    "intimidating_cry_empowerd_attacks_deal_double_damage_display",
                    "enemies_taunted_by_your_warcies_are_intimidated")
            },
            {
                "LancingSteel",
                ("Primary Projectile", new SkillPartDefinitionExtension(
                    ReplaceStat("primary_projectile_impale_chance_%", "attacks_impale_on_hit_%_chance"))),
                ("Secondary Projectile", new SkillPartDefinitionExtension(
                    RemoveStat("primary_projectile_impale_chance_%")))
            },
            {
                "LightningImpurity", // Vaal Impurity of Lightning
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_immune_to_shock", "base_avoid_shock_%", 100)),
                Buff(("lightning_damage_taken_+%", SelfAndAllies),
                    ("base_avoid_shock_%", SelfAndAllies),
                    ("hits_ignore_my_lightning_resistance", Opponents)),
                Passive("aura_effect_+%")
            },
            {
                "LightningResistAura", // Purity of Lightning
                Aura("base_lightning_damage_resistance_%", "base_maximum_lightning_damage_resistance_%")
            },
            { "LightningStrike", SecondaryProjectileMeleeAttackParts },
            { "VaalLightningStrike", SecondaryProjectileMeleeAttackParts },
            {
                "LightningTowerTrap", // Lightning Spire Trap
                new SkillPartDefinitionExtension(ReplaceStat("lightning_tower_trap_base_interval_duration_ms", "hit_rate_ms"))
            },
            {
                "MagmaSigil", // Penance Brand
                BrandExtension,
                ("Pulse", new SkillPartDefinitionExtension()),
                ("Explosion", new SkillPartDefinitionExtension())
            },
            { "MoltenShell", SelfBuff("base_physical_damage_reduction_rating") },
            {
                "VaalMoltenShell",
                new SkillPartDefinitionExtension(ReplaceStat("vaal_molten_shall_armour_+%_final", "armour_+%_final")),
                SelfBuff("base_physical_damage_reduction_rating", "armour_+%_final")
            },
            {
                "MoltenStrike",
                ("Melee Attack", new SkillPartDefinitionExtension(
                    RemoveStat("active_skill_damage_over_time_from_projectile_hits_+%_final"))),
                ("Projectiles", new SkillPartDefinitionExtension(
                    AddStats(("cast_rate_is_melee", 1), ("base_is_projectile", 1), ("is_area_damage", 1)),
                    ReplaceStat("active_skill_damage_over_time_from_projectile_hits_+%_final",
                        "damage_over_time_+%_final"),
                    removedKeywords: new[] { Keyword.Melee }))
            },
            {
                "NewShieldCharge", // Shield Charge
                ("Unspecified Charge Distance", new SkillPartDefinitionExtension()),
                ("Maximum Charge Distance", new SkillPartDefinitionExtension())
            },
            {
                "NewSunder", // Sunder
                ("Initial Hit", new SkillPartDefinitionExtension()),
                ("Shockwave", new SkillPartDefinitionExtension())
            },
            {
                "OrbOfStorms",
                new SkillPartDefinitionExtension(ReplaceStat("orb_of_storms_base_bolt_frequency_ms", "hit_rate_ms"))
            },
            {
                "PhysicalDamageAura", // Pride
                EnemyBuff("physical_damage_taken_+%_final"),
                ("Initial effect", new SkillPartDefinitionExtension(
                    RemoveStat("physical_damage_aura_nearby_enemies_physical_damage_taken_+%_max"),
                    ReplaceStat("physical_damage_aura_nearby_enemies_physical_damage_taken_+%", "physical_damage_taken_+%_final"))),
                ("Maximum effect", new SkillPartDefinitionExtension(
                    RemoveStat("physical_damage_aura_nearby_enemies_physical_damage_taken_+%"),
                    ReplaceStat("physical_damage_aura_nearby_enemies_physical_damage_taken_+%_max", "physical_damage_taken_+%_final")))
            },
            {
                "ProjectileWeakness",
                new SkillPartDefinitionExtension(
                    ReplaceStat("projectiles_always_pierce_you", "always_pierce")
                        .AndThen(ReplaceStat("chance_to_be_knocked_back_%", "base_global_chance_to_knockback_%"))),
                Buff(("projectile_damage_taken_+%", Opponents),
                    ("always_pierce", SelfAndAllies),
                    ("base_global_chance_to_knockback_%", SelfAndAllies))
            },
            {
                "PoachersMark",
                new SkillPartDefinitionExtension(
                    ReplaceStat("evasion_rating_+%_final_from_poachers_mark", "evasion_rating_+%")
                        .AndThen(ReplaceStat("life_granted_when_hit_by_attacks", "life_gain_per_target"))
                        .AndThen(ReplaceStat("mana_granted_when_hit_by_attacks", "mana_gain_per_target"))
                        .AndThen(ReplaceStat("chance_to_grant_frenzy_charge_on_death_%",
                            "add_frenzy_charge_on_kill_%_chance"))),
                Buff(("monster_slain_flask_charges_granted_+%", Opponents),
                    ("evasion_rating_+%", Opponents),
                    ("life_gain_per_target", SelfAndAllies),
                    ("mana_gain_per_target", SelfAndAllies),
                    ("add_frenzy_charge_on_kill_%_chance", SelfAndAllies))
            },
            { "PoisonArrow", SkillDotIsAreaDamageExtension }, // Caustic Arrow
            {
                "Punishment",
                new SkillPartDefinitionExtension(
                    ReplaceStat("newpunishment_attack_speed_+%", "attack_speed_+%")
                        .AndThen(ReplaceStat("newpunishment_melee_damage_+%_final", "melee_damage_+%_final"))),
                Buff(("attack_speed_+%", Self),
                    ("melee_damage_+%_final", Self),
                    ("base_additional_physical_damage_reduction_%", Opponents))
            },
            {
                "PuresteelBanner", // Dread Banner
                new SkillPartDefinitionExtension(
                    RemoveStat("puresteel_banner_fortify_effect_+%_per_stage"),
                    ReplaceStat("puresteel_banner_accuracy_rating_+%_final", "accuracy_rating_+%_final")),
                Buff(("accuracy_rating_+%_final", Opponents),
                    ("attacks_impale_on_hit_%_chance", SelfAndAllies),
                    ("impale_debuff_effect_+%", SelfAndAllies)),
                Passive("aura_effect_+%", "banner_buff_effect_+%_per_stage")
            },
            { "Purity", Aura("base_resist_all_elements_%") }, // Purity of Elements
            { "Reave", new SkillPartDefinitionExtension(AddStat("maximum_stages", 8)) },
            { "VaalReave", new SkillPartDefinitionExtension(AddStat("maximum_stages", 8)) },
            { "RainOfSpores", SkillDotIsAreaDamageExtension }, // Toxic Rain
            {
                "RallyingCry",
                Aura("rallying_cry_weapon_damage_%_for_allies_per_5_monster_power"),
                Passive("rallying_cry_damage_+%_final_from_osm_per_nearby_ally",
                    "rallying_cry_buff_effect_on_minions_+%_final",
                    "warcry_gain_mp_from_allies",
                    "warcry_count_power_from_enemies")
            },
            { "RejuvenationTotem", Aura("base_mana_regeneration_rate_per_minute") },
            { "RighteousFire", SkillDotIsAreaDamageExtension, SelfBuff("righteous_fire_spell_damage_+%_final") },
            {
                "VaalRighteousFire",
                new SkillPartDefinitionExtension(
                    AddStat("skill_dot_is_area_damage", 1),
                    VaalRighteousFireReplaceStats),
                SelfBuff("righteous_fire_spell_damage_+%_final")
            },
            {
                "Sanctify", // Purifying Flame
                ("Initial Hit", new SkillPartDefinitionExtension()),
                ("Shockwave", new SkillPartDefinitionExtension())
            },
            {
                "ScourgeArrow",
                new SkillPartDefinitionExtension(
                    ReplaceStat("virulent_arrow_maximum_number_of_stacks", "maximum_stages")),
                ("Primary Projectile", new SkillPartDefinitionExtension(
                    RemoveStat("virulent_arrow_pod_projectile_damage_+%_final"),
                    AddStat("always_pierce", 1))),
                ("Thorn Arrows", new SkillPartDefinitionExtension(
                    ReplaceStat("virulent_arrow_pod_projectile_damage_+%_final", "damage_+%_final")))
            },
            {
                "SeismicCry",
                new SkillPartDefinitionExtension(
                    RemoveStat("skill_empower_limitation_specifier_for_stat_description")),
                SelfBuff("seismic_cry_+%_enemy_stun_threshold_per_5_MP"),
                Passive("seismic_cry_base_slam_skill_area_+%",
                    "seismic_cry_base_slam_skill_damage_+%_final",
                    "seismic_cry_slam_skill_area_+%_increase_per_repeat",
                    "warcry_count_power_from_enemies")
            },
            {
                "ShatteringSteel",
                new SkillPartDefinitionExtension(
                    RemoveStat("shattering_steel_damage_+%_final_scaled_by_projectile_distance")),
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Cone", new SkillPartDefinitionExtension(AddStat("is_area_damage", 1))),
                ("All Projectiles", new SkillPartDefinitionExtension())
            },
            {
                "ShockNova",
                ("Ring", new SkillPartDefinitionExtension(
                    ReplaceStat("newshocknova_first_ring_damage_+%_final", "damage_+%_final"))),
                ("Nova", new SkillPartDefinitionExtension(
                    RemoveStat("newshocknova_first_ring_damage_+%_final")))
            },
            {
                "ShrapnelShot", // Galvanic Arrow
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Cone", new SkillPartDefinitionExtension(
                    AddStat("is_area_damage", 1)))
            },
            {
                "Slither", // Withering Step
                SelfBuff("phase_through_objects"), Passive("elusive_effect_+%")
            },
            {
                "Smite",
                new SkillPartDefinitionExtension(
                    ReplaceStat("base_chance_to_shock_%_from_skill", "base_chance_to_shock_%")
                        .AndThen(ReplaceStat("minimum_added_lightning_damage_from_skill",
                            "global_minimum_added_lightning_damage"))
                        .AndThen(ReplaceStat("maximum_added_lightning_damage_from_skill",
                            "global_maximum_added_lightning_damage"))),
                Aura("base_chance_to_shock_%",
                    "global_minimum_added_lightning_damage", "global_maximum_added_lightning_damage")
            },
            { "SmokeMine", SelfBuff("base_movement_velocity_+%") },
            {
                "SpellDamageAura", // Zealotry
                new SkillPartDefinitionExtension(
                    ReplaceStat("spell_damage_aura_spell_damage_+%_final", "spell_damage_+%_final")),
                Aura("spell_damage_+%_final", "spell_critical_strike_chance_+%")
            },
            {
                "SpikeSlam", // Earthshatter
                ("Slam", new SkillPartDefinitionExtension()),
                ("Shattering Spikes", AddShowAverageDamageExtension)
            },
            {
                "StaticStrike",
                new SkillPartDefinitionExtension(
                    AddStat("maximum_stages", 3)),
                ("Melee Attack", new SkillPartDefinitionExtension(
                    RemoveStat("static_strike_base_zap_frequency_ms"))),
                ("Beams", new SkillPartDefinitionExtension(
                    ReplaceStat("static_strike_base_zap_frequency_ms", "hit_rate_ms")))
            },
            {
                "StormBurstNew",
                new SkillPartDefinitionExtension(
                    RemoveStat("storm_burst_new_damage_+%_final_per_remaining_teleport_zap"))
            },
            {
                "SummonChaosGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("chaos_golem_grants_additional_physical_damage_reduction_%",
                        "base_additional_physical_damage_reduction_%")),
                SelfBuff("base_additional_physical_damage_reduction_%")
            },
            {
                "SummonFireGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("fire_golem_grants_damage_+%", "damage_+%")),
                SelfBuff("damage_+%")
            },
            {
                "SummonIceGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("ice_golem_grants_critical_strike_chance_+%", "critical_strike_chance_+%")
                        .AndThen(ReplaceStat("ice_golem_grants_accuracy_+%", "accuracy_rating_+%"))),
                SelfBuff("critical_strike_chance_+%", "accuracy_rating_+%")
            },
            {
                "SummonLightningGolem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("lightning_golem_grants_attack_and_cast_speed_+%", "attack_and_cast_speed_+%")),
                SelfBuff("attack_and_cast_speed_+%")
            },
            {
                "SummonRockGolem", // Summon Stone Golem
                new SkillPartDefinitionExtension(
                    ReplaceStat("stone_golem_grants_base_life_regeneration_rate_per_minute",
                        "base_life_regeneration_rate_per_minute")),
                SelfBuff("base_life_regeneration_rate_per_minute")
            },
            { "TempestShield", SelfBuff("shield_block_%", "shield_spell_block_%") },
            {
                "TemporalChains",
                new SkillPartDefinitionExtension(
                    ReplaceStat("temporal_chains_action_speed_+%_final", "action_speed_+%_vs_normal_or_magic_final")
                        .AndThen(ReplaceStat("temporal_chains_action_speed_+%_vs_rare_or_unique_final",
                            "action_speed_+%_vs_rare_or_unique_final"))),
                EnemyBuff("buff_time_passed_+%_other_than_temporal_chains",
                    "action_speed_+%_vs_normal_or_magic_final", "action_speed_+%_vs_rare_or_unique_final"),
                Passive("curse_effect_+%_vs_players")
            },
            {
                "ThrownShield", // Spectral Shield Throw
                ("Primary Projectile", new SkillPartDefinitionExtension()),
                ("Shards", new SkillPartDefinitionExtension())
            },
            { "ThrownWeapon", new SkillPartDefinitionExtension(AddStat("always_pierce", 1)) }, // Spectral Throw
            {
                "TotemMelee", // Ancestral Protector
                new SkillPartDefinitionExtension(
                    ReplaceStat("melee_ancestor_totem_grant_owner_attack_speed_+%_final",
                        "active_skill_attack_speed_+%_final")),
                SelfBuff("melee_ancestor_totem_grant_owner_attack_speed_+%_final")
            },
            { "Vitality", Aura("life_regeneration_rate_per_minute_%") },
            { "VolatileDead", CorpseExplodingSpellParts },
            {
                "Vulnerability",
                new SkillPartDefinitionExtension(
                    ReplaceStat("receive_bleeding_chance_%_when_hit_by_attack", "bleed_on_hit_with_attacks_%")
                        .AndThen(ReplaceStat("chance_to_be_maimed_when_hit_%", "maim_on_hit_%"))),
                Buff(("base_physical_damage_over_time_taken_+%", Opponents),
                    ("physical_damage_taken_+%", Opponents),
                    ("bleed_on_hit_with_attacks_%", SelfAndAllies),
                    ("maim_on_hit_%", SelfAndAllies))
            },
            {
                "WarlordsMark",
                new SkillPartDefinitionExtension(
                    ReplaceStat("life_leech_on_any_damage_when_hit_by_attack_permyriad",
                            "base_life_leech_from_attack_damage_permyriad")
                        .AndThen(ReplaceStat("mana_leech_on_any_damage_when_hit_by_attack_permyriad",
                            "base_mana_leech_from_attack_damage_permyriad"))
                        .AndThen(ReplaceStat("chance_to_grant_endurance_charge_on_death_%",
                            "endurance_charge_on_kill_%"))),
                Buff(("chance_to_be_stunned_%", Opponents),
                    ("base_stun_recovery_+%", Opponents),
                    ("base_life_leech_from_attack_damage_permyriad", SelfAndAllies),
                    ("base_mana_leech_from_attack_damage_permyriad", SelfAndAllies),
                    ("endurance_charge_on_kill_%", SelfAndAllies))
            },
            {
                "WildStrike",
                ("Fire", new SkillPartDefinitionExtension(
                    AddStat("skill_physical_damage_%_to_convert_to_fire", 100))),
                ("Fire Explosion", new SkillPartDefinitionExtension(
                    AddStats(
                        ("skill_physical_damage_%_to_convert_to_fire", 100),
                        ("cast_rate_is_melee", 1),
                        ("is_area_damage", 1)),
                    removedKeywords: new[] { Keyword.Melee })),
                ("Cold", new SkillPartDefinitionExtension(
                    AddStat("skill_physical_damage_%_to_convert_to_cold", 100))),
                ("Cold Wave", new SkillPartDefinitionExtension(
                    AddStats(
                        ("skill_physical_damage_%_to_convert_to_cold", 100),
                        ("cast_rate_is_melee", 1),
                        ("base_is_projectile", 1)),
                    removedKeywords: new[] { Keyword.Melee })),
                ("Lightning", new SkillPartDefinitionExtension(
                    AddStat("skill_physical_damage_%_to_convert_to_lightning", 100))),
                ("Lightning Bolt", new SkillPartDefinitionExtension(
                    AddStats(
                        ("skill_physical_damage_%_to_convert_to_lightning", 100),
                        ("cast_rate_is_melee", 1)),
                    removedKeywords: new[] { Keyword.Melee }))
            },
            {
                "Wither",
                new SkillPartDefinitionExtension(RemoveStat("chaos_damage_taken_+%")),
                EnemyBuff("base_movement_velocity_+%")
            },
            {
                "Wrath",
                new SkillPartDefinitionExtension(
                    ReplaceStat("wrath_aura_spell_lightning_damage_+%_final", "spell_lightning_damage_+%_final")),
                SelfBuff("attack_minimum_added_lightning_damage", "attack_maximum_added_lightning_damage",
                    "spell_lightning_damage_+%_final")
            },

            {
                "AncestralSlamSupport", // Fist of War Support
                new SkillPartDefinitionExtension(AddStat("base_skill_show_average_damage_instead_of_dps", 1),
                    ReplaceStat("support_ancestral_slam_big_hit_hit_damage_+%_final", "hit_damage_+%_final")
                        .AndThen(ReplaceStat("support_ancestral_slam_big_hit_ailment_damage_+%_final", "support_better_ailments_ailment_damage_+%_final"))
                        .AndThen(ReplaceStat("support_ancestral_slam_big_hit_area_+%", "base_skill_area_of_effect_+%")))
            },
            {
                "GeneralsCrySupport",
                new SkillPartDefinitionExtension(
                    RemoveStat("triggered_by_spiritual_cry"),
                    addedKeywords: new []{Keyword.Triggered})
            },
            {
                // The Arcane Surge buff always has added stats. Modify the ones granted by the support so the resulting
                // values end up being the same as with just the (unmodified) support and no stats added outside of it.
                // For BaseAdd and Increase that is just a reduction. For More, the multiplier has to be adjusted.
                // Because of rounding to int, the result is not precise.
                "SupportArcaneSurge",
                new SkillPartDefinitionExtension(
                    ReplaceStat("support_arcane_surge_spell_damage_+%_final", v => (int) Math.Round((v - 10) / 1.1))
                        .AndThen(ReplaceStat("support_arcane_surge_cast_speed_+%", v => v - 10))
                        .AndThen(ReplaceStat("support_arcane_surge_mana_regeneration_rate_per_minute_%", v => v - 30))),
                Passive("support_arcane_surge_spell_damage_+%_final", "support_arcane_surge_cast_speed_+%",
                    "support_arcane_surge_mana_regeneration_rate_per_minute_%")
            },
            { "SupportBlasphemy", Passive("curse_effect_+%") },
            { "SupportBlasphemyPlus", Passive("curse_effect_+%") },
            {
                "SupportBonechill",
                Passive("support_chills_also_grant_cold_damage_taken_per_minute_+%",
                    "support_chills_also_grant_cold_damage_taken_+%_equal_to_slow_amount")
            },
            {
                "SupportCastOnDeath",
                new SkillPartDefinitionExtension(
                    ReplaceStat("area_of_effect_+%_while_dead", "base_skill_area_of_effect_+%")
                        .AndThen(ReplaceStat("cast_on_death_damage_+%_final_while_dead", "damage_+%_final")))
            },
            {
                "SupportCastWhileChannelling",
                new SkillPartDefinitionExtension(ReplaceStat("cast_while_channelling_time_ms", "hit_rate_ms"))
            },
            {
                "SupportCastWhileChannellingPlus",
                new SkillPartDefinitionExtension(ReplaceStat("cast_while_channelling_time_ms", "hit_rate_ms"))
            },
            { "SupportChanceToIgnite", Passive("ignites_apply_fire_resistance_+") },
            { "SupportDarkRitual", Passive("apply_linked_curses_with_dark_ritual") },
            {
                "SupportGemFrenzyPowerOnTrapTrigger", // Charged Traps
                new SkillPartDefinitionExtension(
                    ReplaceStat("trap_critical_strike_multiplier_+_per_power_charge",
                        "critical_strike_multiplier_+_per_power_charge"))
            },
            { "SupportGenerosity", Passive("aura_cannot_affect_self", "non_curse_aura_effect_+%") },
            { "SupportGenerosityPlus", Passive("aura_cannot_affect_self", "non_curse_aura_effect_+%") },
            { "SupportMaim", Passive("support_maimed_enemies_physical_damage_taken_+%") },
            {
                "SupportOnslaughtOnSlayingShockedEnemy", // Innervate
                Passive("support_innervate_minimum_added_lightning_damage",
                    "support_innervate_maximum_added_lightning_damage")
            },
            {
                "SupportPuncturingWeapon", // Nightblade Support
                Passive("elusive_effect_+%")
            },
            {
                "SupportRangedAttackTotem", // Ballista Totem Support
                new SkillPartDefinitionExtension(
                    ReplaceStat("support_attack_totem_attack_speed_+%_final", "active_skill_attack_speed_+%_final"),
                    addedKeywords: new [] {Keyword.Ballista})
            },
            {
                "SupportSpellTotem",
                new SkillPartDefinitionExtension(
                    ReplaceStat("support_spell_totem_cast_speed_+%_final", "active_skill_cast_speed_+%_final"))
            },
        };

        private static SkillPartDefinitionExtension SkillDotIsAreaDamageExtension
            => new SkillPartDefinitionExtension(AddStat("skill_dot_is_area_damage", 1));

        private static SkillPartDefinitionExtension RemoveShowAverageDamageExtension
            => new SkillPartDefinitionExtension(RemoveStat("base_skill_show_average_damage_instead_of_dps"));

        private static SkillPartDefinitionExtension AddShowAverageDamageExtension =>
            new SkillPartDefinitionExtension(AddStat("base_skill_show_average_damage_instead_of_dps", 1));

        private static (string name, SkillPartDefinitionExtension extension)[] CorpseExplodingSpellParts
            => new[]
            {
                ("Spell", new SkillPartDefinitionExtension()),
                ("Corpse Explosion", new SkillPartDefinitionExtension(
                    AddStat("display_skill_deals_secondary_damage", 1)))
            };

        private static (string name, SkillPartDefinitionExtension extension)[] SecondaryProjectileMeleeAttackParts
            => new[]
            {
                ("Melee Attack", new SkillPartDefinitionExtension(
                    RemoveStat("active_skill_damage_over_time_from_projectile_hits_+%_final"))),
                ("Projectiles", new SkillPartDefinitionExtension(
                    AddStats(("cast_rate_is_melee", 1), ("base_is_projectile", 1)),
                    ReplaceStat("active_skill_damage_over_time_from_projectile_hits_+%_final",
                        "damage_over_time_+%_final"),
                    removedKeywords: new[] { Keyword.Melee }))
            };

        private static (string name, SkillPartDefinitionExtension extension)[] SecondaryExplosionProjectileParts
            => new[]
            {
                ("Projectile", new SkillPartDefinitionExtension()),
                ("Explosion", new SkillPartDefinitionExtension(AddStat("is_area_damage", 1)))
            };

        private static SkillPartDefinitionExtension BrandExtension
            => new SkillPartDefinitionExtension(
                RemoveStat("base_skill_show_average_damage_instead_of_dps"),
                ReplaceStat("base_sigil_repeat_frequency_ms", "hit_rate_ms"));

        private static (string name, SkillPartDefinitionExtension extension)[] EarthquakeParts
            => new[]
            {
                ("Initial Hit", new SkillPartDefinitionExtension()),
                ("Aftershock", AddShowAverageDamageExtension)
            };

        private static SkillPartDefinitionExtension IceSpearFirstFormExtension
            => new SkillPartDefinitionExtension(
                RemoveStats("ice_spear_second_form_critical_strike_chance_+%",
                    "ice_spear_second_form_critical_strike_multiplier_+",
                    "ice_spear_second_form_projectile_speed_+%_final"));

        private static SkillPartDefinitionExtension IceSpearSecondFormExtension
            => new SkillPartDefinitionExtension(
                AddStat("always_pierce", 1),
                ReplaceStat("ice_spear_second_form_critical_strike_chance_+%", "critical_strike_chance_+%")
                    .AndThen(ReplaceStat("ice_spear_second_form_critical_strike_multiplier_+",
                        "base_critical_strike_multiplier_+"))
                    .AndThen(ReplaceStat("ice_spear_second_form_projectile_speed_+%_final",
                        "projectile_speed_+%_final")));

        private static IEnumerable<string> RemoveStat(string statId)
            => new[] { statId };

        private static IEnumerable<string> RemoveStats(params string[] statIds)
            => statIds;

        private static IEnumerable<UntranslatedStat> AddStat(string statId, int value)
            => AddStats((statId, value));

        private static IEnumerable<UntranslatedStat> AddStats(params (string statId, int value)[] stats)
            => stats.Select(t => new UntranslatedStat(t.statId, t.value));

        private static Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> ReplaceStat(
            string oldStatId, string newStatId)
            => ReplaceStat(oldStatId, newStatId, Funcs.Identity);

        private static Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> ReplaceStat(
            string oldStatId, string newStatId, int newValue)
            => ReplaceStat(oldStatId, newStatId, _ => newValue);

        private static Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> ReplaceStat(
            string statId, Func<int, int> replaceValue)
            => ReplaceStat(statId, statId, replaceValue);

        private static Func<IEnumerable<UntranslatedStat>, IEnumerable<UntranslatedStat>> ReplaceStat(
            string oldStatId, string newStatId, Func<int, int> replaceValue)
        {
            return stats => stats.Select(Replace);

            UntranslatedStat Replace(UntranslatedStat stat)
                => stat.StatId == oldStatId ? new UntranslatedStat(newStatId, replaceValue(stat.Value)) : stat;
        }

        /// <summary>
        /// Replaces the pool sacrifice and sacrifice as damage stats of Vaal Righteous Fire with the burn stats used
        /// by normal Righteous Fire.
        /// </summary>
        private static IEnumerable<UntranslatedStat> VaalRighteousFireReplaceStats(IEnumerable<UntranslatedStat> stats)
        {
            var enumeratedStats = stats.ToList();

            var poolToLoseOnUse =
                enumeratedStats.FirstOrDefault(s => s.StatId == "vaal_righteous_fire_life_and_es_%_to_lose_on_use");
            if (poolToLoseOnUse is null)
                return enumeratedStats;

            var sacrificedPoolDamagePerSecond =
                enumeratedStats.First(s => s.StatId == "vaal_righteous_fire_life_and_es_%_as_damage_per_second");

            // (x / 100) * (y / 100) * 100 * 60 = x * y * 0.6 [combining the percentages, converting seconds to minutes]
            // No loss of precision with current values: x is always 30 -> poolDamagePerMinute = 18 * y
            var poolDamagePerMinute =
                (int) Math.Round(poolToLoseOnUse.Value * sacrificedPoolDamagePerSecond.Value * 0.6);
            return enumeratedStats.Append(
                new UntranslatedStat("base_righteous_fire_%_of_max_life_to_deal_to_nearby_per_minute",
                    poolDamagePerMinute),
                new UntranslatedStat("base_righteous_fire_%_of_max_energy_shield_to_deal_to_nearby_per_minute",
                    poolDamagePerMinute));
        }

        private static IReadOnlyDictionary<string, Func<Entity, IEnumerable<Entity>>> SelfBuff(params string[] statIds)
            => Buff(Self, statIds);

        private static IReadOnlyDictionary<string, Func<Entity, IEnumerable<Entity>>> EnemyBuff(params string[] statIds)
            => Buff(Opponents, statIds);

        private static IReadOnlyDictionary<string, Func<Entity, IEnumerable<Entity>>> Aura(params string[] statIds)
            => Buff(SelfAndAllies, statIds);

        private static IReadOnlyDictionary<string, Func<Entity, IEnumerable<Entity>>> Buff(
            Func<Entity, IEnumerable<Entity>> affectedEntities, params string[] statIds)
            => Buff(statIds.Select(s => (s, affectedEntities)).ToArray());

        private static IReadOnlyDictionary<string, Func<Entity, IEnumerable<Entity>>> Buff(
            params (string statId, Func<Entity, IEnumerable<Entity>> affectedEntities)[] stats)
            => stats.ToDictionary(t => t.statId, t => t.affectedEntities);

        private static IEnumerable<Entity> Opponents(Entity entity) => entity.Opponents();
        private static IEnumerable<Entity> SelfAndAllies(Entity entity) => entity.SelfAndAllies();
        private static IEnumerable<Entity> Self(Entity entity) => new[] {entity};
        private static IEnumerable<Entity> Minions(Entity entity) => entity.Minions();

        private static IEnumerable<string> Passive(params string[] statIds)
            => statIds;
    }
}