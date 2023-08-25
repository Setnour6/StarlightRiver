﻿//Todo on falling trees:

//Cache frame data (done? test it more to see)
//Make top of trees separate tile
//Sfx (done)
//Make it include the bottom when you chop it
//Make them harder to chop
//Make the bottom choppable by axe

using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Forest
{
	internal class ThickTree : ModTile
	{
		private static readonly HashSet<Point> CutPositions = new();
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			LocalizedText name = CreateMapEntryName();
			name.SetDefault("Large Tree");

			TileID.Sets.IsATreeTrunk[Type] = true;
			Main.tileAxe[Type] = true;
			AddMapEntry(new Color(169, 125, 93), name);

			RegisterItemDrop(ItemID.Wood);
		}

		public static float GetLeafSway(float offset, float magnitude, float speed)
		{
			return (float)Math.Sin(Main.GameUpdateCount * speed + offset) * magnitude;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (right && !up && down || !up && !down)
				Main.instance.TilesRenderer.AddSpecialLegacyPoint(new Point(i, j));
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (right && !up && down)
			{
				Texture2D tex = ModContent.Request<Texture2D>(Texture + "Top").Value;
				Vector2 pos = (new Vector2(i + 1, j) + Helpers.Helper.TileAdj) * 16;

				Color color = Lighting.GetColor(i, j);

				spriteBatch.Draw(tex, pos - Main.screenPosition, null, color, GetLeafSway(3, 0.05f, 0.008f), new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);

				Texture2D tex2 = ModContent.Request<Texture2D>(AssetDirectory.ForestTile + "Godray").Value;
				var godrayColor = new Color();
				float godrayRot = 0;

				if (Main.dayTime)
				{
					godrayColor = new Color(255, 255, 200) * 0.5f;
					godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 54000f * 3.14f), 3);
					godrayRot = -0.5f * 1.57f + (float)Main.time / 54000f * 3.14f;
				}
				else
				{
					godrayColor = new Color(200, 210, 255) * 0.5f;
					godrayColor *= (float)Math.Pow(Math.Sin(Main.time / 24000f * 3.14f), 3) * 0.25f;
					godrayRot = -0.5f * 1.57f + (float)Main.time / 24000f * 3.14f;
				}

				if (Main.raining)
					godrayColor *= 1 - Main.cloudAlpha * 2;

				godrayColor.A = 0;

				pos += new Vector2(0, -100);

				int daySeed = i + (int)Main.GetMoonPhase();

				if (daySeed % 3 == 0)
					spriteBatch.Draw(tex2, pos - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, 0.85f, 0, 0);

				pos += new Vector2(-60, 80);

				if (daySeed % 5 == 0)
					spriteBatch.Draw(tex2, pos - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, 0.65f, 0, 0);

				pos += new Vector2(150, -60);

				if (daySeed % 7 == 0)
					spriteBatch.Draw(tex2, pos - Main.screenPosition, null, godrayColor, godrayRot, Vector2.Zero, 0.75f, 0, 0);
			}

			if (!up && !down)
			{
				Texture2D sideTex = Terraria.GameContent.TextureAssets.TreeTop[0].Value;
				Vector2 sidePos = (new Vector2(i + 1, j) + Helpers.Helper.TileAdj) * 16;

				if (left)
					spriteBatch.Draw(sideTex, sidePos + new Vector2(20, 0) - Main.screenPosition, null, Color.White, 0, Vector2.Zero, 1, 0, 0);

				if (right)
					spriteBatch.Draw(sideTex, sidePos + new Vector2(0, 20) - Main.screenPosition, null, Color.White, 0, Vector2.Zero, 1, 0, 0);
			}
		}

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (right && !up && down)
			{
				Texture2D tex = ModContent.Request<Texture2D>(Texture + "Top").Value;
				Vector2 pos = (new Vector2(i + 1, j) + Helpers.Helper.TileAdj) * 16;

				Color color = Lighting.GetColor(i, j);

				spriteBatch.Draw(tex, pos + new Vector2(50, 40) - Main.screenPosition, null, color.MultiplyRGB(Color.Gray), GetLeafSway(0, 0.05f, 0.01f), new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
				spriteBatch.Draw(tex, pos + new Vector2(-30, 80) - Main.screenPosition, null, color.MultiplyRGB(Color.DarkGray), GetLeafSway(2, 0.025f, 0.012f), new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
			}

			return true;
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (Main.rand.NextBool(20) && right && !up && down)
			{
				if (Main.dayTime && !Main.raining && Main.time > 10000 && Main.time < 44000)
				{
					float godrayRot = (float)Main.time / 54000f * 3.14f;
					Dust.NewDustPerfect(new Vector2(i, j) * 16 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(100), ModContent.DustType<Dusts.GoldSlowFade>(), Vector2.UnitX.RotatedBy(godrayRot) * Main.rand.NextFloat(0.25f, 0.5f), 255, default, 0.75f);
				}
			}
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;

			Framing.GetTileSafely(i, j).HasTile = false;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>() && Framing.GetTileSafely(i - 1, j).HasTile;
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>() && Framing.GetTileSafely(i + 1, j).HasTile;
			bool up = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>() && Framing.GetTileSafely(i + 1, j).HasTile;
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>() && Framing.GetTileSafely(i, j - 1).HasTile ||
				Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTreeBase>() && Framing.GetTileSafely(i, j - 1).HasTile;

			if (Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>() && !noItem)
				SpawnFallingTree(new EntitySource_TileBreak(i, j), i, j, 1);

			if (Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>() && !noItem)
				SpawnFallingTree(new EntitySource_TileBreak(i - 1, j), i - 1, j, -1);

			noItem = true;

			if (left)
			{
				KillTile(i - 1, j, ref fail, ref effectOnly, ref noItem);
			}

			if (right)
			{
				KillTile(i + 1, j, ref fail, ref effectOnly, ref noItem);
			}

			if (up)
			{
				KillTile(i, j - 1, ref fail, ref effectOnly, ref noItem);
			}

			if (down)
			{
				KillTile(i, j - 1, ref fail, ref effectOnly, ref noItem);
			}

			CutPositions.Add(new Point(i, j));
			Terraria.Audio.SoundEngine.PlaySound(new Terraria.Audio.SoundStyle($"{nameof(StarlightRiver)}/Sounds/TreeFalling"));
		}

		public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
		{
			var treeRand = new Random(i + j);
			short x = 0;
			short y = 0;

			bool left = Framing.GetTileSafely(i - 1, j).TileType == ModContent.TileType<ThickTree>();
			bool right = Framing.GetTileSafely(i + 1, j).TileType == ModContent.TileType<ThickTree>();
			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();
			bool down = Framing.GetTileSafely(i, j + 1).TileType == ModContent.TileType<ThickTree>();

			if (up || down)
			{
				if (right)
					x = 0;

				if (left)
					x = 18;

				y = (short)(treeRand.Next(3) * 18);

				if (treeRand.Next(3) == 1)
					x += 18 * 2;
			}

			Tile tile = Framing.GetTileSafely(i, j);
			tile.TileFrameX = x;
			tile.TileFrameY = y;

			return false;
		}
		private static void SpawnFallingTree(IEntitySource source, int i, int j, int direction)
		{
			int height = 1;
			for (; height < 500; height++)
			{
				if (Framing.GetTileSafely(i, j - height).TileType != ModContent.TileType<ThickTree>() || CutPositions.Contains(new Point(i, j - height)))
					break;
			}

			height++;
			Vector2 tilePosition = new Vector2(i, j);
			Vector2 worldPosition = tilePosition * 16;
			Projectile proj = Projectile.NewProjectileDirect(source, worldPosition, Vector2.Zero, ModContent.ProjectileType<ThickTreeFalling>(), 500, 0, Main.LocalPlayer.whoAmI);

			ThickTreeFalling modProjectile = proj.ModProjectile as ThickTreeFalling;

			modProjectile.height = height;
			modProjectile.direction = direction;
			modProjectile.originalBase = tilePosition.ToPoint();

		}
	}

	class ThickTreeBase : ModTile
	{
		public override string Texture => AssetDirectory.ForestTile + Name;

		public override void SetStaticDefaults()
		{
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile, 4, 0);
			Main.tileAxe[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
			TileID.Sets.PreventsTileReplaceIfOnTopOfIt[Type] = true;

			this.QuickSetFurniture(4, 4, 0, SoundID.Dig, true, new Color(169, 125, 93));//a
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			if (fail || effectOnly)
				return;

			Framing.GetTileSafely(i, j).HasTile = false;

			bool up = Framing.GetTileSafely(i, j - 1).TileType == ModContent.TileType<ThickTree>();

			if (up)
				WorldGen.KillTile(i, j - 1);
		}
		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			Tile tile = Main.tile[i, j];
			if (!Framing.GetTileSafely(i + 1, j - 1).HasTile && tile.TileFrameX == 18 && tile.TileFrameY == 0)
			{
				Texture2D tex = ModContent.Request<Texture2D>(Texture + "_TopEdge").Value;
				var pos = ((new Vector2(i, j) + Helpers.Helper.TileAdj) * 16) - new Vector2(0, 4);
				spriteBatch.Draw(tex, pos - Main.screenPosition, null, Lighting.GetColor(new Point(i, j)), 0f, Vector2.Zero, 1, SpriteEffects.None, 0f);
			}

			return base.PreDraw(i, j, spriteBatch);
		}
	}
	public class ThickTreeFalling : ModProjectile
	{
		public override string Texture => AssetDirectory.ForestTile + "ThickTree";

		public int direction = 1;

		public int height;

		public Point originalBase;

		private float acceleration = 0.0003f;
		private float maxVelocity = 0.05f;
		private float rotationalVelocity = 0;
		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.tileCollide = false;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 1000;
			//Projectile.hide = true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Falling Large Tree");
		}

		public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI)
		{
			//behindNPCsAndTiles.Add(index);
		}

		public override void AI()
		{
			if (Math.Abs(Projectile.rotation) < 1.75f)
			{
				if (rotationalVelocity < maxVelocity)
					rotationalVelocity += acceleration * direction;
			}
			else
			{
				Projectile.velocity.Y += Math.Abs(maxVelocity) * 2;
				Projectile.rotation += rotationalVelocity * 0.2f;
				rotationalVelocity = MathHelper.Lerp(rotationalVelocity, maxVelocity * 0.2f * direction, 0.1f);
			}

			Projectile.rotation += rotationalVelocity;

			Vector2 headPos = Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * (height + 2) * 16);
			Vector2 gorePos = headPos + Main.rand.NextVector2Circular(128, 128);
			Vector2 goreVel = Projectile.rotation.ToRotationVector2().RotatedByRandom(0.5f) * direction * Main.rand.NextFloat(12);
			Tile tile = Main.tile[(int)gorePos.X / 16, (int)gorePos.Y / 16];

			if (!tile.HasTile || !Main.tileSolid[tile.TileType])
				Gore.NewGoreDirect(new EntitySource_DropAsItem(Projectile), gorePos, goreVel, GoreID.TreeLeaf_Normal);

			if (TouchingTile())
				Projectile.Kill();
		}

		public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			Vector2 headPos = Projectile.Center + ((Projectile.rotation - 1.57f).ToRotationVector2() * (height + 2) * 16);
			return Collision.CheckAABBvAABBCollision(targetHitbox.TopLeft(), targetHitbox.Size(), headPos - new Vector2(64, 64), new Vector2(128, 128));
		}

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;

			Texture2D edgeTex = ModContent.Request<Texture2D>(Texture + "_BottomEdge").Value;

			Point basePos = (Projectile.Center / 16).ToPoint();
			Vector2 origin = new Vector2(16 + (16 * direction), 16);
			Vector2 edgePos = ((Projectile.Center / 16 + new Vector2(1, 1)) * 16) - Main.screenPosition;
			edgePos.X += 16 * direction;
			Main.spriteBatch.Draw(edgeTex, edgePos, null, Lighting.GetColor(basePos), Projectile.rotation, new Vector2(16 + (16 * direction), 0), Projectile.scale, SpriteEffects.None, 0f);

			for (int j = 1 - height; j <= 0; j++)
			{
				for (int i = 1; i > -1; i--)
				{
					Point framePoint = new Point(originalBase.X + i, originalBase.Y + j);

					Vector2 drawPos = Projectile.Center - Main.screenPosition;

					var treeRand = new Random(framePoint.X + framePoint.Y);

					int xFrame = 0;
					int yFrame = 0;

					var left = i == 1;
					var right = i == 0;
					var down = j != 0;
					var up = j != 1 - height;

					if ((up || down))
					{
						if (right)
							xFrame = 0;
						if (left)
							xFrame = 18;

						yFrame = (short)(treeRand.Next(3) * 18);

						if (treeRand.Next(3) == 1)
							xFrame += 18 * 2;
					}

					Rectangle frame = new Rectangle(xFrame, yFrame, 16, 16);

					drawPos += (new Vector2(i, j).RotatedBy(Projectile.rotation) * 16) + origin;

					Point lightingSample = ((drawPos + Main.screenPosition) / 16).ToPoint();
					var color = Lighting.GetColor(lightingSample);
					Main.spriteBatch.Draw(tex, drawPos, frame, color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);



					if (right && !up && down)
					{
						drawPos -= new Vector2((tex.Width / 2), tex.Height).RotatedBy(Projectile.rotation);
						if (direction == 1)
							drawPos += new Vector2(-32, 16).RotatedBy(Projectile.rotation);
						else
							drawPos += new Vector2(0, 16).RotatedBy(Projectile.rotation);
						Texture2D topTex = ModContent.Request<Texture2D>(Texture + "Top").Value;
						Main.spriteBatch.Draw(topTex, drawPos + new Vector2(50, 40).RotatedBy(Projectile.rotation), null, color.MultiplyRGB(Color.Gray), ThickTree.GetLeafSway(0, 0.05f, 0.01f) + Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
						Main.spriteBatch.Draw(topTex, drawPos + new Vector2(-30, 80).RotatedBy(Projectile.rotation), null, color.MultiplyRGB(Color.DarkGray), ThickTree.GetLeafSway(2, 0.025f, 0.012f) + Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
						Main.spriteBatch.Draw(topTex, drawPos, null, color, ThickTree.GetLeafSway(3, 0.05f, 0.008f) + Projectile.rotation, new Vector2(tex.Width / 2, tex.Height), 1, 0, 0);
					}
				}
			}

			return false;
		}

		public override void Kill(int timeLeft)
		{
			Point basePos = (Projectile.Center / 16).ToPoint();
			Core.Systems.CameraSystem.CameraSystem.shake += 15;
			Vector2 position = (basePos.ToVector2()) + new Vector2(0.5f, 0);

			Vector2 unitVector = (Projectile.rotation - 1.57f).ToRotationVector2();
			for (int i = 0; i < height; i++)
			{
				position += unitVector;
				Rectangle spawnRect = new Rectangle((int)(position.X * 16) - 8, (int)(position.Y * 16) - 8, 16, 16);

				for (int j = 0; j < 12; j++)
					Dust.NewDustPerfect((position * 16) + Main.rand.NextVector2Circular(16, 16), 7, Main.rand.NextVector2Circular(2, 2) - new Vector2(0, 3), 0, default, Main.rand.NextFloat(1, 1.6f));

				Item.NewItem(new EntitySource_DropAsItem(Projectile), spawnRect, ItemID.Wood, 5);
			}

			for (int k = 0; k < 600; k++)
			{
				Vector2 gorePos = (position * 16) + Main.rand.NextVector2Circular(128, 128);
				Tile tile = Main.tile[(int)gorePos.X / 16, (int)gorePos.Y / 16];
				if (!tile.HasTile || !Main.tileSolid[tile.TileType])
					Gore.NewGoreDirect(new EntitySource_DropAsItem(Projectile), gorePos, Main.rand.NextVector2Circular(3, 3), GoreID.TreeLeaf_Normal);
			}
		}
		private bool TouchingTile()
		{
			Point basePos = (Projectile.Center / 16).ToPoint();
			Vector2 position = (basePos.ToVector2()) + new Vector2(0.5f, 0);

			Vector2 unitVector = (Projectile.rotation - 1.57f).ToRotationVector2();
			for (int i = 0; i < height + 5; i++)
			{
				position += unitVector;

				Tile tile = Main.tile[(int)position.X, (int)position.Y];
				if (tile.HasTile && Main.tileSolid[tile.TileType])
					return true;
			}

			return false;
		}
	}
}