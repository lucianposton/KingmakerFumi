﻿using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using System.Linq;

namespace FumisCodex.NewComponents
{
    [AllowedOn(typeof(BlueprintAbility))]
    public class AbilityRequirementHasBuff : BlueprintComponent, IAbilityAvailabilityProvider
    {
        public bool Not;
        public BlueprintBuff[] Buffs;

        public bool IsAvailableFor(AbilityData ability)
        {
            foreach (var buff in Buffs)
            {
                bool hasBuff = ability.Caster.Buffs.GetBuff(buff) != null;
                if (!hasBuff && !this.Not || hasBuff && this.Not)
                {
                    return false;
                }
            }
            return true;
        }

        public string GetReason()
        {
            return (string)LocalizedTexts.Instance.Reasons.NoRequiredCondition;
        }
    }
}
