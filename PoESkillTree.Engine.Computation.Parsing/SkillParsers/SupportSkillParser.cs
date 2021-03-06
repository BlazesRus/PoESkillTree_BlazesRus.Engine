using System.Collections.Generic;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Skills;
using PoESkillTree.Engine.Utils;

namespace PoESkillTree.Engine.Computation.Parsing.SkillParsers
{
    /// <summary>
    /// Parser for support skills
    /// </summary>
    public class SupportSkillParser : IParser<SupportSkillParserParameter>
    {
        private readonly SkillDefinitions _skillDefinitions;
        private readonly IBuilderFactories _builderFactories;
        private readonly UntranslatedStatParserFactory _statParserFactory;

        public SupportSkillParser(
            SkillDefinitions skillDefinitions, IBuilderFactories builderFactories,
            UntranslatedStatParserFactory statParserFactory)
            => (_skillDefinitions, _builderFactories, _statParserFactory) =
                (skillDefinitions, builderFactories, statParserFactory);

        public ParseResult Parse(SupportSkillParserParameter parameter)
        {
            var (active, support, _) = parameter;
            if (!active.IsEnabled || !support.IsEnabled ||
                (active.Gem != null && !active.Gem.IsEnabled) || (support.Gem != null && !support.Gem.IsEnabled))
                return ParseResult.Empty;

            var modifiers = new List<Modifier>();
            var parsedStats = new List<UntranslatedStat>();

            var preParser = new SkillPreParser(_skillDefinitions, _builderFactories.MetaStatBuilders);
            var preParseResult = preParser.ParseSupport(parameter);

            foreach (var partialParser in CreatePartialParsers())
            {
                var (newlyParsedModifiers, newlyParsedStats) = partialParser.Parse(active, support, preParseResult);
                modifiers.AddRange(newlyParsedModifiers);
                parsedStats.AddRange(newlyParsedStats);
            }

            var translatingParser = new TranslatingSkillParser(_builderFactories, _statParserFactory);
            return translatingParser.Parse(support, preParseResult, new PartialSkillParseResult(modifiers, parsedStats));
        }

        private IPartialSkillParser[] CreatePartialParsers()
            => new[]
            {
                new SupportSkillGeneralParser(_builderFactories),
                SkillKeywordParser.CreateSupport(_builderFactories),
                SkillTypeParser.CreateSupport(_builderFactories),
                new SupportSkillLevelParser(_builderFactories),
                new SkillStatParser(_builderFactories),
            };
    }

    public static class SupportSkillParserExtensions
    {
        public static ParseResult Parse(this IParser<SupportSkillParserParameter> @this,
            Skill activeSkill, Skill supportSkill, Entity entity) =>
            @this.Parse(new SupportSkillParserParameter(activeSkill, supportSkill, entity));
    }

    public class SupportSkillParserParameter : ValueObject
    {
        public SupportSkillParserParameter(Skill activeSkill, Skill supportSkill, Entity entity)
            => (ActiveSkill, SupportSkill, Entity) = (activeSkill, supportSkill, entity);

        public void Deconstruct(out Skill activeSkill, out Skill supportSkill, out Entity entity)
            => (activeSkill, supportSkill, entity) = (ActiveSkill, SupportSkill, Entity);

        public Skill ActiveSkill { get; }
        public Skill SupportSkill { get; }
        public Entity Entity { get; }

        protected override object ToTuple() => (ActiveSkill, SupportSkill, Entity);
    }
}