using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Conditions;
using PoESkillTree.Engine.Computation.Common.Builders.Modifiers;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.Computation.Common.Builders.Values;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Parsing
{
    /// <summary>
    /// Collection and factory for creating <see cref="Modifier"/>s
    /// </summary>
    public class ModifierCollection
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly ModifierSource.Local _localModifierSource;
        private readonly ModifierSource.Global _globalModifierSource;
        private readonly Entity _modifierSourceEntity;
        private readonly IModifierBuilder _modifierBuilder = ModifierBuilder.Empty;
        private readonly List<Modifier> _modifiers = new List<Modifier>();

        public ModifierCollection(
            IBuilderFactories builderFactories, ModifierSource.Local localModifierSource,
            Entity modifierSourceEntity)
        {
            (_builderFactories, _localModifierSource, _modifierSourceEntity) =
                (builderFactories, localModifierSource, modifierSourceEntity);
            _globalModifierSource = new ModifierSource.Global(_localModifierSource);
        }

        public IReadOnlyList<Modifier> Modifiers => _modifiers;

        public void AddLocal(IStatBuilder stat, Form form, double value, IConditionBuilder? condition = null)
            => AddLocal(stat, form, _builderFactories.ValueBuilders.Create(value), condition);

        public void AddLocal(IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder? condition = null)
            => Add(stat, form, value, condition, _localModifierSource);

        public void AddGlobal(IStatBuilder stat, Form form, double value, IConditionBuilder? condition = null)
            => AddGlobal(stat, form, _builderFactories.ValueBuilders.Create(value), condition);

        public void AddGlobal(IStatBuilder stat, Form form, bool value, IConditionBuilder? condition = null)
            => AddGlobal(stat, form, _builderFactories.ValueBuilders.Create(value), condition);

        public void AddGlobal(IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder? condition = null)
            => Add(stat, form, value, condition, _globalModifierSource);

        private void Add(
            IStatBuilder stat, Form form, IValueBuilder value, IConditionBuilder? condition,
            ModifierSource modifierSource)
        {
            var builder = _modifierBuilder
                .WithStat(stat)
                .WithForm(_builderFactories.FormBuilders.From(form))
                .WithValue(value);
            if (condition != null)
                builder = builder.WithCondition(condition);
            var intermediateModifier = builder.Build();
            _modifiers.AddRange(intermediateModifier.Build(modifierSource, _modifierSourceEntity));
        }
    }
}