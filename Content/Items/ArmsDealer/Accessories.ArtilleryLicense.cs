﻿using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.Items.ArmsDealer
{
	internal class ArtilleryLicense : SmartAccessory
	{
		public override string Texture => AssetDirectory.Debug;

		public ArtilleryLicense() : base("Artillery License", "+1 Sentry slot\n`Totally not forged`") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.buyPrice(gold: 10);
			Item.rare = ItemRarityID.Green;
		}

		public override void SafeUpdateEquip(Player player)
		{
			player.maxTurrets += 1;
		}
	}
}

