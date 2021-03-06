using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using MoreLinq;
using Newtonsoft.Json.Linq;
using PoESkillTree.Engine.GameModel;
using PoESkillTree.Engine.GameModel.Items;
using PoESkillTree.Engine.GameModel.Modifiers;
using PoESkillTree.Engine.GameModel.StatTranslation;

namespace PoESkillTree.Engine.Computation.Console
{
    internal static class TestDataUpdater
    {
        /// <param name="skillTreeTxtPath">Path to the SkillTree.txt in your PoESkillTree repo or installation.
        /// E.g. "C:/.../PoESkillTree/WPFSKillTree/bin/Debug/net462/Data/SkillTree.txt"
        /// </param>
        /// <param name="baseTargetPath"></param>
        public static void UpdateSkillTreeStatLines(string skillTreeTxtPath, string baseTargetPath)
        {
            var json = JObject.Parse(File.ReadAllText(skillTreeTxtPath));
            var nodes = json.Value<JObject>("nodes");
            var statLines = nodes.PropertyValues()
                .OrderBy(t => t.Value<int>("skill")) // Order for more useful diffs
                .SelectMany(t => t["stats"]?.Values<string>() ?? Enumerable.Empty<string>())
                .Select(s => s.Replace("\n", " "));

            var path = baseTargetPath + "PoESkillTree.Engine.GameModel/Data/SkillTreeStatLines.txt";
            File.WriteAllLines(path, statLines);
        }

        public static void UpdateParseableBaseItems(BaseItemDefinitions baseItemDefinitions, string baseTargetPath)
        {
            var seenImplicits = new HashSet<string>();
            var seenBuffs = new HashSet<string>();
            var baseIds = baseItemDefinitions.BaseItems
                .Where(d => d.ReleaseState != ReleaseState.Unreleased && d.ItemClass != ItemClass.Map)
                .Where(d => d.ImplicitModifiers.Any(s => seenImplicits.Add(s.StatId))
                            || d.BuffStats.Any(s => seenBuffs.Add(s.StatId)))
                .Select(d => d.MetadataId);

            var path = baseTargetPath + "PoESkillTree.Engine.Computation.IntegrationTests/Data/ParseableBaseItems.txt";
            File.WriteAllLines(path, baseIds);
        }

        public static void UpdateItemAffixes(
            ModifierDefinitions modifierDefinitions, StatTranslators statTranslators, string baseTargetPath)
        {
            var domainWhitelist = new[]
                { ModDomain.AbyssJewel, ModDomain.Crafted, ModDomain.Flask, ModDomain.Item, ModDomain.Misc };

            var statTranslator = statTranslators[StatTranslationFileNames.Main];
            var affixLines = modifierDefinitions.Modifiers
                .Where(d => d.GenerationType == ModGenerationType.Prefix
                            || d.GenerationType == ModGenerationType.Suffix)
                .Where(d => domainWhitelist.Contains(d.Domain))
                .Select(d => d.Stats.Select(s => new UntranslatedStat(s.StatId, (s.MinValue + s.MaxValue) / 2)))
                .Select(statTranslator.Translate)
                .SelectMany(r => r.TranslatedStats)
                .Select(s => s.Replace('\n', ' ').Replace('\r', ' '))
                .Select(s => (s, Regex.Replace(s, @"\d+(\.\d+)?", "#")))
                .DistinctBy(t => t.Item2)
                .OrderBy(t => t.Item2)
                .Select(t => t.s);

            var path = baseTargetPath + "PoESkillTree.Engine.GameModel/Data/ItemAffixes.txt";
            File.WriteAllLines(path, affixLines);
        }
    }
}