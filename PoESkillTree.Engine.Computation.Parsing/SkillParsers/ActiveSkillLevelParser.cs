using EnumsNET;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Damage;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Partial parser of <see cref="ActiveSkillParser"/> that parses level-dependent modifiers.
    /// </summary>
    public class ActiveSkillLevelParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;

        private SkillModifierCollection? _modifiers;
        private SkillPreParseResult? _preParseResult;

        public ActiveSkillLevelParser(IBuilderFactories builderFactories)
            => _builderFactories = builderFactories;

        private IMetaStatBuilders MetaStats => _builderFactories.MetaStatBuilders;

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            _modifiers = new SkillModifierCollection(_builderFactories,
                preParseResult.IsMainSkill, preParseResult.LocalSource, preParseResult.ModifierSourceEntity);
            _preParseResult = preParseResult;
            var level = preParseResult.LevelDefinition;

            if (level.DamageEffectiveness is double effectiveness)
            {
                _modifiers.AddGlobalForMainSkill(MetaStats.DamageBaseAddEffectiveness,
                    Form.TotalOverride, effectiveness);
            }
            if (level.DamageMultiplier is double multiplier)
            {
                _modifiers.AddGlobalForMainSkill(MetaStats.DamageBaseSetEffectiveness,
                    Form.TotalOverride, multiplier);
            }
            if (level.CriticalStrikeChance is double crit)
            {
                _modifiers.AddGlobalForMainSkill(_builderFactories.ActionBuilders.CriticalStrike.Chance.WithHits,
                    Form.BaseSet, crit);
            }
            if (level.AttackSpeedMultiplier is int attackSpeedMultiplier)
            {
                _modifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.CastRate.With(DamageSource.Attack),
                    Form.More, attackSpeedMultiplier);
            }
            if (level.ManaCost is int cost)
            {
                var baseCostStat = MetaStats.SkillBaseCost(parsedSkill);
                var costStat = _builderFactories.SkillBuilders.FromId(_preParseResult.SkillDefinition.Id).Cost;
                _modifiers.AddGlobal(baseCostStat, Form.BaseSet, cost);
                _modifiers.AddGlobal(costStat, Form.BaseSet, baseCostStat.Value, _preParseResult.IsActiveSkill);
                _modifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.Pool.From(Pool.Mana).Cost,
                    Form.BaseSet, costStat.Value);
                ParseReservation(mainSkill, costStat);
            }
            if (level.Cooldown is int cooldown)
            {
                _modifiers.AddGlobalForMainSkill(_builderFactories.StatBuilders.Cooldown, Form.BaseSet, cooldown);
            }
            if (level.CanBypassCooldown)
            {
                _modifiers.AddGlobalForMainSkill(MetaStats.CanBypassSkillCooldown, Form.TotalOverride, 1);
            }

            var result = new PartialSkillParseResult(_modifiers.Modifiers, new UntranslatedStat[0]);
            _modifiers = null;
            _preParseResult = preParseResult;
            return result;
        }

        private void ParseReservation(Skill skill, IStatBuilder costStat)
        {
            var isReservation = MetaStats.SkillHasType(skill, ActiveSkillType.ManaCostIsReservation).IsTrue;
            var isReservationAndActive = isReservation.And(_preParseResult!.IsActiveSkill);
            var isPercentage = MetaStats.SkillHasType(skill, ActiveSkillType.ManaCostIsPercentage).IsTrue;
            var skillBuilder = _builderFactories.SkillBuilders.FromId(_preParseResult.SkillDefinition.Id);

            _modifiers!.AddGlobal(skillBuilder.Reservation, Form.BaseSet, costStat.Value, isReservationAndActive);

            foreach (var pool in Enums.GetValues<Pool>())
            {
                var poolBuilder = _builderFactories.StatBuilders.Pool.From(pool);
                var value = skillBuilder.Reservation.Value;
                value = _builderFactories.ValueBuilders
                    .If(isPercentage).Then(value.AsPercentage * poolBuilder.Value)
                    .Else(value);
                _modifiers.AddGlobal(poolBuilder.Reservation, Form.BaseAdd, value,
                    skillBuilder.ReservationPool.Value.Eq((double) pool).And(isReservationAndActive));
            }
        }
    }
}