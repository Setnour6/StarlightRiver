﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.NPCs.Overgrow;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class CrusherTile : ModTile
    {
        public override string Texture => AssetDirectory.OvergrowTile + Name;

        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileMerge[Type][Mod.Find<ModTile>("BrickOvergrow").Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileLighted[Type] = true;
            DustType = DustType<Dusts.GoldNoMovement>();
            AddMapEntry(new Color(81, 77, 71));
        }

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
            (r, g, b) = (1.275f, 1f, 0.5f);
		}

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Vector2 pos = new Vector2(4 + i * 16, 4 + j * 16);

            if (!Main.npc.Any(NPC => NPC.type == NPCType<Crusher>() && (NPC.ModNPC as Crusher).Parent == Main.tile[i, j] && NPC.active))
            {
                int crusher = NPC.NewNPC(new EntitySource_WorldEvent(), (int)pos.X + 4, (int)pos.Y + 21, NPCType<Crusher>(), 0, pos.Y + 16);

                if (Main.npc[crusher].ModNPC is Crusher) 
                    (Main.npc[crusher].ModNPC as Crusher).Parent = Main.tile[i, j];
            }
        }

		public override void HitWire(int i, int j)
		{
            Vector2 pos = new Vector2(4 + i * 16, 4 + j * 16);
            var npc = Main.npc.FirstOrDefault(NPC => NPC.type == NPCType<Crusher>() && (NPC.ModNPC as Crusher).Parent == Main.tile[i, j] && NPC.active);

            if(npc != null && npc.ai[1] == 0 && System.Math.Abs(npc.velocity.Y) <= 0.1f)
                npc.ai[1] = 1;
        }
	}

    public class CrusherOvergrowItem : QuickTileItem 
    { 
        public CrusherOvergrowItem() : base("Crusher Trap", "", "CrusherTile", 0, AssetDirectory.OvergrowTile) { } 
    }
}