using System.Collections;
using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common.Builders.Effects;
using PoESkillTree.Engine.Computation.Common.Builders.Forms;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;

namespace PoESkillTree.Engine.Computation.Data.Collections
{
    /// <summary>
    /// Collection of <see cref="IIntermediateModifier"/> that allows collection initialization syntax for adding
    /// entries. Uses <see cref="IEffectBuilder.AddStat"/> for adding stats.
    /// </summary>
    public class EffectStatCollection : IEnumerable<IIntermediateModifier>
    {
        private readonly IModifierBuilder _modifierBuilder;
        private readonly IValueBuilders _valueFactory;

        private readonly List<IIntermediateModifier> _data = new List<IIntermediateModifier>();

        public EffectStatCollection(IModifierBuilder modifierBuilder, IValueBuilders valueFactory)
        {
            _modifierBuilder = modifierBuilder;
            _valueFactory = valueFactory;
        }

        public IEnumerator<IIntermediateModifier> GetEnumerator() => _data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(IEffectBuilder effect, IFormBuilder form, IStatBuilder stat, double value)
        {
            Add(effect, form, stat, _valueFactory.Create(value));
        }

        public void Add(IEffectBuilder effect, IFormBuilder form, IStatBuilder stat, IValueBuilder value)
        {
            var builder = _modifierBuilder
                .WithForm(form)
                .WithStat(effect.AddStat(stat))
                .WithValue(value);
            _data.Add(builder.Build());
        }
    }
}