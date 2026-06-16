using ProjectEclipse.Player;
using UnityEngine;

namespace ProjectEclipse.Equipment
{
    [System.Serializable]
    public class ClassRestriction
    {
        [SerializeField] private bool unrestricted = true;
        [SerializeField] private PlayerClassArchetype requiredClass = PlayerClassArchetype.Warrior;

        public bool Unrestricted { get { return unrestricted; } }
        public PlayerClassArchetype RequiredClass { get { return requiredClass; } }

        public bool Allows(PlayerClassDefinition playerClass)
        {
            return unrestricted || playerClass == null || playerClass.Archetype == requiredClass;
        }
    }
}
