﻿using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Terraria.ModLoader.ModContent;
using Terraria;
using StarlightRiver.Items;
using StarlightRiver.Codex.Entries;

namespace StarlightRiver.Pickups
{
    class StaminaShardPickup : AbilityPickup
    {
        Tile Parent => Framing.GetTileSafely((int)npc.Center.X / 16, (int)npc.Center.Y / 16);

        public override string Texture => GetStaminaTexture();

        public override Color GlowColor => new Color(255, 100, 30);

        public override bool Fancy => false;

        public override bool CanPickup(Player player)
        {
            AbilityHandler ah = player.GetModPlayer<AbilityHandler>();
            return !ah.shards[Parent.frameX];
        }

        public override void Visuals()
        {
            if(Main.rand.Next(2) == 0) Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedByRandom(Math.PI) * Main.rand.NextFloat(16), DustType<Dusts.Stamina>(), Vector2.UnitY * -1);
        }

        public override void PickupEffects(Player player)
        {
            AbilityHandler ah = player.GetModPlayer<AbilityHandler>();
            
            ah.shardCount++;
            ah.shards[Parent.frameX] = true;

            if (ah.shardCount >= 3)
            {
                ah.StatStaminaMaxPerm++;
                ah.shardCount = 0;
                StarlightRiver.Instance.textcard.Display("Stamina Vessel", "Your maximum stamina has increased by 1", null, 240, 0.8f);
            }
            else
            {
                StarlightRiver.Instance.textcard.Display("Stamina Vessel Shard", "Collect " + (3 - ah.shardCount) + " more to increase your maximum stamina", null, 240, 0.6f);
            }

            Helper.UnlockEntry<StaminaShardEntry>(Main.LocalPlayer);
        }

        private static string GetStaminaTexture()
        {
            if (Main.gameMenu) return "StarlightRiver/Pickups/Stamina1";

            AbilityHandler ah = Main.LocalPlayer.GetModPlayer<AbilityHandler>();
            return "StarlightRiver/Pickups/Stamina" + (ah.shardCount + 1);
        }
    }

    class StaminaShardTile : AbilityPickupTile
    {
        public override int PickupType => NPCType<StaminaShardPickup>();

        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            fail = true;

            Tile tile = Framing.GetTileSafely(i, j);

            tile.frameX++;
            if (tile.frameX > 2) tile.frameX = 0;
            Main.NewText("pickup set to stamina shard number " + tile.frameX, Color.Orange);
        }
    }

    class StaminaShardTileItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Pickups/Stamina1";

        public StaminaShardTileItem() : base("Stamina Shard", "PENIS", TileType<StaminaShardTile>(), 1) { }
    }
}