using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Shared.Damage
{
    /// <summary>
    ///     Component that allows entities to take damage.
    /// </summary>
    /// <remarks>
    ///     The supported damage types are specified using a <see cref="DamageContainerPrototype"/>s. DamageContainers
    ///     may also have resistances to certain damage types, defined via a <see cref="DamageModifierSetPrototype"/>.
    /// </remarks>
    [RegisterComponent]
    [NetworkedComponent()]
    [Access(typeof(DamageableSystem))]
    public sealed class DamageableComponent : Component
    {
        /// <summary>
        ///     This <see cref="DamageContainerPrototype"/> specifies what damage types are supported by this component.
        ///     If null, all damage types will be supported.
        /// </summary>
        [DataField("damageContainer", customTypeSerializer: typeof(PrototypeIdSerializer<DamageContainerPrototype>))]
        [Access(typeof(DamageableSystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
        public string? DamageContainerID;

        /// <summary>
        ///     This <see cref="DamageModifierSetPrototype"/> will be applied to any damage that is dealt to this container,
        ///     unless the damage explicitly ignores resistances.
        /// </summary>
        /// <remarks>
        ///     Though DamageModifierSets can be deserialized directly, we only want to use the prototype version here
        ///     to reduce duplication.
        /// </remarks>
        [ViewVariables(VVAccess.ReadWrite)]
        [DataField("damageModifierSet", customTypeSerializer: typeof(PrototypeIdSerializer<DamageModifierSetPrototype>))]
        public string? DamageModifierSetId;

        /// <summary>
        ///     All the damage information is stored in this <see cref="DamageSpecifier"/>.
        /// </summary>
        /// <remarks>
        ///     If this data-field is specified, this allows damageable components to be initialized with non-zero damage.
        /// </remarks>
        [DataField("damage")]
        [ViewVariables(VVAccess.ReadWrite)]
        [Access(typeof(DamageableSystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
        public DamageSpecifier Damage = new();

        /// <summary>
        ///     Damage, indexed by <see cref="DamageGroupPrototype"/> ID keys.
        /// </summary>
        /// <remarks>
        ///     Groups which have no members that are supported by this component will not be present in this
        ///     dictionary.
        /// </remarks>
        [ViewVariables] public Dictionary<string, FixedPoint2> DamagePerGroup = new();

        /// <summary>
        ///     The sum of all damages in the DamageableComponent.
        /// </summary>
        [ViewVariables]
        [Access(typeof(DamageableSystem), Other = AccessPermissions.ReadExecute)] // FIXME Friends
        public FixedPoint2 TotalDamage;

        // Really these shouldn't be here. OnExplosion() and RadiationAct() should be handled elsewhere.
        [ViewVariables]
        [DataField("radiationDamageTypes", customTypeSerializer: typeof(PrototypeIdListSerializer<DamageTypePrototype>))]
        public List<string> RadiationDamageTypeIDs = new() {"Radiation"};
        [ViewVariables]
        [DataField("explosionDamageTypes", customTypeSerializer: typeof(PrototypeIdListSerializer<DamageTypePrototype>))]
        public List<string> ExplosionDamageTypeIDs = new() { "Piercing", "Heat" };
    }

    [Serializable, NetSerializable]
    public sealed class DamageableComponentState : ComponentState
    {
        public readonly Dictionary<string, FixedPoint2> DamageDict;
        public readonly string? ModifierSetId;

        public DamageableComponentState(
            Dictionary<string, FixedPoint2> damageDict,
            string? modifierSetId)
        {
            DamageDict = damageDict;
            ModifierSetId = modifierSetId;
        }
    }
}
