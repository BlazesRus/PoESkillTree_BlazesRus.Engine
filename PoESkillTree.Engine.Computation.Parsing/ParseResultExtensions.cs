using System;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Parsing
{
    public static class ParseResultExtensions
    {
        /// <summary>
        /// Applies a condition to the values of all modifiers
        /// </summary>
        public static ParseResult ApplyCondition(this ParseResult @this,
            Func<BuildParameters, ConditionBuilderResult> buildCondition, Entity modifierSourceEntity)
        {
            return @this.ApplyToModifiers(ApplyMultiplier);

            Modifier ApplyMultiplier(Modifier modifier)
            {
                var buildParameters = new BuildParameters(modifier.Source, modifierSourceEntity, modifier.Form);
                var conditionResult = buildCondition(buildParameters);
                if (conditionResult.HasStatConverter)
                    throw new InvalidOperationException("Can only handle value conditions");

                var condition = conditionResult.Value;
                var value = modifier.Value;
                return new Modifier(modifier.Stats, modifier.Form,
                    new FunctionalValue(
                        c => condition.Calculate(c).IsTrue() ? value.Calculate(c) : null,
                        condition + " ? " + value + " : null"),
                    modifier.Source);
            }
        }

        /// <summary>
        /// Applies a multiplier to the values of all modifiers
        /// </summary>
        public static ParseResult ApplyMultiplier(this ParseResult @this,
            Func<BuildParameters, IValue> buildMultiplier, Entity modifierSourceEntity)
            => @this.ApplyConditionalMultiplier(buildMultiplier, _ => true, modifierSourceEntity);

        /// <summary>
        /// Applies a multiplier to the values of all modifiers
        /// </summary>
        public static ParseResult ApplyConditionalMultiplier(this ParseResult @this,
            Func<BuildParameters, IValue> buildMultiplier, Predicate<Modifier> predicate,
            Entity modifierSourceEntity)
        {
            return @this.ApplyToModifiers(ApplyMultiplier);

            Modifier ApplyMultiplier(Modifier modifier)
            {
                if (!predicate(modifier))
                    return modifier;

                var buildParameters = new BuildParameters(modifier.Source, modifierSourceEntity, modifier.Form);
                var multiplier = buildMultiplier(buildParameters);
                var value = modifier.Value;
                return new Modifier(modifier.Stats, modifier.Form,
                    new FunctionalValue(Calculate, multiplier + " * " + value),
                    modifier.Source);

                NodeValue? Calculate(IValueCalculationContext context)
                    => multiplier.Calculate(context) is NodeValue left ? left * value.Calculate(context) : null;
            }
        }
    }
}