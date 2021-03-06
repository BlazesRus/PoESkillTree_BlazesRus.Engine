using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EnumsNET;
using PoESkillTree.Engine.Computation.Common;
using PoESkillTree.Engine.GameModel;

namespace PoESkillTree.Engine.Computation.Builders.Stats
{
    public class Stat : IStat
    {
        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(double), typeof(int), typeof(uint)
        };

        public Stat(string identity, Entity entity = default, Type? dataType = null,
            ExplicitRegistrationType? explicitRegistrationType = null, IReadOnlyList<Behavior>? behaviors = null,
            Func<NodeValue?, NodeValue?>? rounding = null)
        {
            if (!IsDataTypeValid(dataType))
                throw new ArgumentException($"Stats only support double, int, bool or enum data types, {dataType} given",
                    nameof(dataType));

            Identity = identity;
            Entity = entity;
            ExplicitRegistrationType = explicitRegistrationType;
            DataType = dataType ?? typeof(double);
            _hasRange = NumericTypes.Contains(DataType);
            Behaviors = behaviors ?? new Behavior[0];
            _rounding = rounding;
        }

        private static bool IsDataTypeValid(Type? dataType)
            => dataType == null || dataType == typeof(bool) || NumericTypes.Contains(dataType) || dataType.IsEnum;

        private readonly bool _hasRange;
        public string Identity { get; }
        public Entity Entity { get; }
        public ExplicitRegistrationType? ExplicitRegistrationType { get; }
        public Type DataType { get; }
        public IReadOnlyList<Behavior> Behaviors { get; }

        public IStat? Minimum => MinOrMax();
        public IStat? Maximum => MinOrMax();

        private IStat? MinOrMax([CallerMemberName] string identitySuffix = "") =>
            _hasRange ? new Stat(Identity + "." + identitySuffix, Entity, DataType) : null;

        private readonly Func<NodeValue?, NodeValue?>? _rounding;

        public NodeValue? Round(NodeValue? value)
        {
            if (_rounding != null)
                return _rounding(value);
            else if (!NumericTypes.Contains(DataType) || DataType == typeof(double))
                return value;
            else
                return RoundingBehaviors.Floor(value);
        }

        private string? _stringRepresentation;

        public override string ToString()
            => _stringRepresentation ??= Entity.GetName() + "." + Identity;

        public override bool Equals(object? obj) =>
            (obj == this) || (obj is IStat other && Equals(other));

        public bool Equals(IStat other) =>
            (other != null) && Identity.Equals(other.Identity) && Entity == other.Entity;

        public override int GetHashCode() =>
            (Identity, Entity).GetHashCode();
    }
}