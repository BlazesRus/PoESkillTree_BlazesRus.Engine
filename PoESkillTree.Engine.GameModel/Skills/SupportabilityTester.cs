using System.Collections.Generic;
using System.Linq;
using PoESkillTree.Engine.Utils.Extensions;

namespace PoESkillTree.Engine.GameModel.Skills
{
    /// <summary>
    /// Determines the support skills that can support a given active skill.
    /// </summary>
    public class SupportabilityTester
    {
        private readonly SkillDefinitions _skillDefinitions;

        public SupportabilityTester(SkillDefinitions skillDefinitions)
            => _skillDefinitions = skillDefinitions;

        public IEnumerable<Skill> SelectSupportingSkills(Skill activeSkill, IEnumerable<Skill> supportSkills)
        {
            var supports = supportSkills.Where(s => activeSkill.ItemSlot == s.ItemSlot)
                .OrderBy(s => s.SocketIndex).ToList();

            if (activeSkill.Gem is Gem gem)
            {
                supports = supports.Where(s => s.Gem is null || s.Gem.Group == gem.Group).ToList();
            }
            else
            {
                supports = supports.Where(s => !GetDefinition(s).SupportsGemsOnly).ToList();
            }

            var activeDefinition = _skillDefinitions.GetSkillById(activeSkill.Id);
            var activeTypes = activeDefinition.ActiveSkill.ActiveSkillTypes
                .Concat(activeDefinition.ActiveSkill.MinionActiveSkillTypes)
                .ToHashSet();

            foreach (var support in supports)
            {
                if (CanSupport(support, activeTypes))
                    activeTypes.UnionWith(GetDefinition(support).AddedActiveSkillTypes);
            }
            return supports.Where(s => CanSupport(s, activeTypes));
        }

        private bool CanSupport(Skill supportSkill, IReadOnlyCollection<string> activeTypes)
        {
            var definition = GetDefinition(supportSkill);
            return Allows(definition, activeTypes) && !Excludes(definition, activeTypes);
        }

        private static bool Excludes(SupportSkillDefinition supportDefinition, IReadOnlyCollection<string> activeTypes)
            => EvaluateTypes(supportDefinition.ExcludedActiveSkillTypes, activeTypes);

        private static bool Allows(SupportSkillDefinition supportDefinition, IReadOnlyCollection<string> activeTypes) =>
            supportDefinition.AllowedActiveSkillTypes.IsEmpty() || EvaluateTypes(supportDefinition.AllowedActiveSkillTypes, activeTypes);

        private static bool EvaluateTypes(IEnumerable<string> supportTypes, IReadOnlyCollection<string> activeTypes)
        {
            var stack = new Stack<bool>();
            foreach (var type in supportTypes)
            {
                switch (type)
                {
                    case ActiveSkillType.Not:
                        stack.Push(!stack.Pop());
                        break;
                    case ActiveSkillType.And:
                        stack.Push(stack.Pop() & stack.Pop());
                        break;
                    case ActiveSkillType.Or:
                        stack.Push(stack.Pop() | stack.Pop());
                        break;
                    default:
                        stack.Push(activeTypes.Contains(type));
                        break;
                }
            }
            return stack.Any(b => b);
        }

        private SupportSkillDefinition GetDefinition(Skill supportSkill)
            => _skillDefinitions.GetSkillById(supportSkill.Id).SupportSkill;
    }
}