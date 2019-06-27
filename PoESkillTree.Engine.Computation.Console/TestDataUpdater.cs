﻿using System.Collections.Generic;
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
        public static void UpdateSkillTreeStatLines()
        {
            // From PoESkillTree.Engine.Computation.Console/bin/Debug/netcoreapp2.2/ to the same folder under WPFSKillTree
            var treePath = "../../../../WPFSKillTree/bin/Debug/net462/Data/SkillTree.txt";
            var json = JObject.Parse(File.ReadAllText(treePath));
            var nodes = json.Value<JObject>("nodes");
            var statLines = nodes.PropertyValues()
                .OrderBy(t => t.Value<int>("id")) // Order for more useful diffs
                .SelectMany(t => t["sd"].Values<string>())
                .Select(s => s.Replace("\n", " "));

            var statLinesPath = "../../../../PoESkillTree.Engine.GameModel/Data/SkillTreeStatLines.txt";
            File.WriteAllLines(statLinesPath, statLines);
        }

        public static void UpdateParseableBaseItems(BaseItemDefinitions baseItemDefinitions)
        {
            var seenImplicits = new HashSet<string>();
            var seenBuffs = new HashSet<string>();
            var baseIds = baseItemDefinitions.BaseItems
                .Where(d => d.ReleaseState != ReleaseState.Unreleased)
                .Where(d => d.ImplicitModifiers.Any(s => seenImplicits.Add(s.StatId))
                            || d.BuffStats.Any(s => seenBuffs.Add(s.StatId)))
                .Select(d => d.MetadataId);

            var parseablePath = "../../../../PoESkillTree.Engine.Computation.IntegrationTests/Data/ParseableBaseItems.txt";
            File.WriteAllLines(parseablePath, baseIds);
        }

        public static void UpdateItemAffixes(ModifierDefinitions modifierDefinitions, StatTranslators statTranslators)
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

            var path = "../../../../PoESkillTree.Engine.GameModel/Data/ItemAffixes.txt";
            File.WriteAllLines(path, affixLines);
        }
    }
}