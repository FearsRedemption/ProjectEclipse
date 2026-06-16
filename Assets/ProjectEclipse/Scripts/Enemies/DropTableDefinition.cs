using System.Collections.Generic;
using UnityEngine;

namespace ProjectEclipse.Enemies
{
    [CreateAssetMenu(menuName = "Project Eclipse/Enemies/Drop Table")]
    public class DropTableDefinition : ScriptableObject
    {
        [SerializeField] private string tableId = "drop_table";
        [SerializeField] private List<DropTableEntry> entries = new List<DropTableEntry>();
        [SerializeField] private List<DropTableEntry> rareEntries = new List<DropTableEntry>();

        public string TableId { get { return tableId; } }
        public IReadOnlyList<DropTableEntry> Entries { get { return entries; } }
        public IReadOnlyList<DropTableEntry> RareEntries { get { return rareEntries; } }
    }
}
