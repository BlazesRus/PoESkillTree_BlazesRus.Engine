using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Data;
using PoESkillTree.Engine.Computation.Data.Base;
using PoESkillTree.Engine.Computation.Data.Collections;

namespace PoESkillTree.Engine.Computation.Data
{
    /// <inheritdoc />
    /// <summary>
    /// <see cref="IStatMatchers"/> implementation matching stat parts specifying forms and values.
    /// </summary>
    public class FormMatchers : StatMatchersBase
    {
        private readonly IModifierBuilder _modifierBuilder;

        public FormMatchers(IBuilderFactories builderFactories, IModifierBuilder modifierBuilder)
            : base(builderFactories)
        {
            _modifierBuilder = modifierBuilder;
        }

        protected override IReadOnlyList<MatcherData> CreateCollection() =>
            new FormMatcherCollection(_modifierBuilder, ValueFactory)
            {
                { "#% increased", PercentIncrease, Value },
                { "#% reduced", PercentReduce, Value },
                { "#% more", PercentMore, Value },
                { "#% less", PercentLess, Value },
                { @"\+#%? to", BaseAdd, Value },
                { @"\+#(?!%)", BaseAdd, Value },
                { @"\+?#%( global)?(?= chance)", BaseAdd, Value },
                { @"\+?#% of", BaseAdd, Value },
                { "(?<!chance to )gain #", BaseAdd, Value },
                { "#% additional", BaseAdd, Value },
                { "an additional", BaseAdd, 1 },
                { "adds # to", BaseAdd, Value },
                { "can (have|summon) up to # additional", BaseAdd, Value },
                { "-#%? to", BaseSubtract, Value },
                { "-#(?!%)", BaseSubtract, Value },
                { "-#% of", BaseSubtract, Value },
            };

        // Add (that word appearing is required for ReSharper to highlight these regex patterns ...)
    }
}