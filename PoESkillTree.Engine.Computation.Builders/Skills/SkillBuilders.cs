using PoESkillTree.Engine.Computation.Builders.Stats;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.Computation.Common.Builders;
using PoESkillTree.Engine.Computation.Common.Builders.Skills;
using PoESkillTree.Engine.Computation.Common.Parsing;
using PoESkillTree.Engine.GameModel.Skills;

namespace PoESkillTree.Engine.Computation.Builders.Skills
{
    public class SkillBuilders : ISkillBuilders
    {
        private readonly IStatFactory _statFactory;
        private readonly SkillDefinitions _skills;

        public SkillBuilders(IStatFactory statFactory, SkillDefinitions skills)
        {
            _statFactory = statFactory;
            _skills = skills;
        }

        public ISkillBuilderCollection AllSkills => CreateCollection();
        public ISkillBuilderCollection this[IKeywordBuilder keyword] => CreateCollection(keyword);

        private ISkillBuilderCollection CreateCollection(params IKeywordBuilder[] keywords)
            => new SkillBuilderCollection(_statFactory, keywords, _skills.Skills);

        public ISkillBuilder SummonSkeletons => FromId("SummonSkeletons");
        public ISkillBuilder VaalSummonSkeletons => FromId("VaalSummonSkeletons");
        public ISkillBuilder RaiseSpectre => FromId("RaiseSpectre");
        public ISkillBuilder RaiseZombie => FromId("RaiseZombie");
        public ISkillBuilder DetonateMines => FromId("GemDetonateMines");

        public ISkillBuilder FromId(string skillId)
            => new SkillBuilder(_statFactory, CoreBuilder.Create(_skills.GetSkillById(skillId)));

        public ISkillBuilder ModifierSourceSkill
            => new SkillBuilder(_statFactory, CoreBuilder.Create(BuildModifierSourceSkill));

        private SkillDefinition BuildModifierSourceSkill(BuildParameters parameters) =>
            parameters.ModifierSource.GetLocalSource() switch
            {
                ModifierSource.Local.Skill skillSource => _skills.GetSkillById(skillSource.SkillId),
                ModifierSource.Local.Gem gemSource => _skills.GetSkillById(gemSource.SourceGem.SkillId),
                _ => throw new ParseException($"ModifierSource must be a skill, {parameters.ModifierSource} given")
            };
    }
}