using System;
using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Stats;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Partial parser of <see cref="ActiveSkillParser"/> and <see cref="SupportSkillParser"/> that parses
    /// active skill types.
    /// </summary>
    public class SkillTypeParser : IPartialSkillParser
    {
        private readonly IBuilderFactories _builderFactories;
        private readonly Func<SkillDefinition, IEnumerable<string>> _selectTypes;

        private SkillTypeParser(
            IBuilderFactories builderFactories, Func<SkillDefinition, IEnumerable<string>> selectTypes)
            => (_builderFactories, _selectTypes) =
                (builderFactories, selectTypes);

        public static IPartialSkillParser CreateActive(IBuilderFactories builderFactories)
            => new SkillTypeParser(builderFactories, d => d.ActiveSkill.ActiveSkillTypes);

        public static IPartialSkillParser CreateSupport(IBuilderFactories builderFactories)
            => new SkillTypeParser(builderFactories, d => d.SupportSkill.AddedActiveSkillTypes);

        private IMetaStatBuilders MetaStats => _builderFactories.MetaStatBuilders;

        public PartialSkillParseResult Parse(Skill mainSkill, Skill parsedSkill, SkillPreParseResult preParseResult)
        {
            var modifiers = new ModifierCollection(_builderFactories, preParseResult.LocalSource, preParseResult.ModifierSourceEntity);

            foreach (var type in _selectTypes(preParseResult.SkillDefinition))
            {
                modifiers.AddGlobal(MetaStats.SkillHasType(mainSkill, type), Form.TotalOverride, 1);
            }

            return new PartialSkillParseResult(modifiers.Modifiers, new UntranslatedStat[0]);
        }
    }
}