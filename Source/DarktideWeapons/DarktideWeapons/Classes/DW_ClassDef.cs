using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DarktideWeapons.Classes
{
    public enum ClassNodeType
    {
        Perk,
        Skill,
    }

    /// <summary>
    /// Defines a single node in a class skill tree.
    /// May be a Perk (grants stat offsets) or a Skill (grants an active ability).
    /// Child nodes reference other DW_ClassNodeDef by defName in XML.
    /// </summary>
    public class DW_ClassNodeDef : Def
    {
        /// <summary>Whether this node is a passive perk or an active skill.</summary>
        public ClassNodeType nodeType = ClassNodeType.Perk;

        /// <summary>Number of skill points required to unlock this node.</summary>
        public int skillPointCost = 1;

        /// <summary>Stat offsets applied to the pawn while this perk node is unlocked.</summary>
        public List<StatModifier> statOffsets;

        /// <summary>The ability granted when this Skill node is unlocked.</summary>
        public AbilityDef abilityDef;

        /// <summary>Child nodes reachable from this node.</summary>
        public List<DW_ClassNodeDef> childNodes;

        /// <summary>Texture path for this node's icon.</summary>
        public string iconPath;

        [Unsaved]
        private Texture2D cachedIcon;

        public Texture2D Icon
        {
            get
            {
                if (cachedIcon != null) return cachedIcon;
                if (!iconPath.NullOrEmpty())
                    cachedIcon = ContentFinder<Texture2D>.Get(iconPath, false);
                return cachedIcon ?? BaseContent.BadTex;
            }
        }
    }

    /// <summary>
    /// Defines a class available to pawns.
    /// Each class has a root node that leads to the rest of its skill tree.
    /// </summary>
    public class DW_ClassDef : Def
    {
        /// <summary>Root node of this class's skill tree.</summary>
        public DW_ClassNodeDef rootNode;

        /// <summary>Texture path for this class's icon shown in the gizmo.</summary>
        public string iconPath;

        [Unsaved]
        private Texture2D cachedIcon;

        public Texture2D Icon
        {
            get
            {
                if (cachedIcon != null) return cachedIcon;
                if (!iconPath.NullOrEmpty())
                    cachedIcon = ContentFinder<Texture2D>.Get(iconPath, false);
                return cachedIcon ?? BaseContent.BadTex;
            }
        }
    }
}
