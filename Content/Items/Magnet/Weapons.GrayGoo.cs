using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Content.Dusts;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Magnet
{
	public class GrayGooDustData //Has to be a class so we can pass by reference
	{
		public int x;

		public int y;

		public Projectile proj;

		public float speed;

		public float lerp;
		public GrayGooDustData(int x, int y, Projectile proj, float lerp, float speed)
		{
			this.x = x;
			this.y = y;
			this.proj = proj;
			this.lerp = lerp;
			this.speed = speed;
		}
	}

	public class GrayGoo : ModItem
	{
		public override string Texture => AssetDirectory.MagnetItem + "GrayGoo";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gray Goo");
			Tooltip.SetDefault("Summons a swarm of nanomachines to devour enemies");
			ItemID.Sets.GamepadWholeScreenUseRange[Item.type] = true; // This lets the Player target anywhere on the whole screen while using a controller.
			ItemID.Sets.LockOnIgnoresCollision[Item.type] = true;
		}

		public override void SetDefaults()
		{
			Item.damage = 20;
			Item.mana = 10;
			Item.width = 32;
			Item.height = 32;
			Item.useTime = 36;
			Item.useAnimation = 36;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(0, 2, 0, 0);
			Item.rare = ItemRarityID.Orange;
			Item.UseSound = SoundID.Item44;

			Item.noMelee = true;
			Item.DamageType = DamageClass.Summon;
			Item.buffType = BuffType<GrayGooSummonBuff>();
			Item.shoot = ProjectileType<GrayGooProj>();
			Item.knockBack = 0;
			Item.noUseGraphic = true;
		}

		public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			// This is needed so the buff that keeps your minion alive and allows you to despawn it properly applies
			player.AddBuff(Item.buffType, 2);

			// Here you can change where the minion is spawned. Most vanilla minions spawn at the cursor position.
			position = Main.MouseWorld;

			var proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			proj.originalDamage = damage;
			return false;
		}
	}

	public class GrayGooProj : ModProjectile
	{
		public const int maxMinionChaseRange = 2000;

		public float lerper;

		public float oldLerper;

		public static RenderTarget2D NPCTarget;

		public bool foundTarget;

		public float oldEnemyWhoAmI;

		public ref float EnemyWhoAmI => ref Projectile.ai[1];

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Gray Goo");

			Main.projPet[Projectile.type] = true; // Denotes that this Projectile is a pet or minion
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; // This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true; // This is needed so your minion can properly spawn when summoned and replaced when other minions are summoned			 
			ProjectileID.Sets.CultistIsResistantTo[Projectile.type] = true; // Don't mistake this with "if this is true, then it will automatically home". It is just for damage reduction for certain NPCs
		}

		public override void Load()
		{
			if (Main.dedServ)
				return;

			ResizeTarget();

			On.Terraria.Main.CheckMonoliths += DrawNPCtarget;
		}

		public static void ResizeTarget()
		{
			Main.QueueMainThreadAction(() => NPCTarget = new RenderTarget2D(Main.graphics.GraphicsDevice, Main.screenWidth, Main.screenHeight));
		}

		public override void SetDefaults()
		{
			Projectile.width = 16;
			Projectile.height = 16;

			Projectile.tileCollide = false;

			Projectile.minion = true;
			Projectile.minionSlots = 1;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 5000;
			Projectile.friendly = true;
			Projectile.usesLocalNPCImmunity = true;
			Projectile.localNPCHitCooldown = 15;
		}

		public override bool? CanCutTiles()
		{
			return false;
		}

		public override bool MinionContactDamage()
		{
			return true;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (target.whoAmI != (int)EnemyWhoAmI)
				return false;
			return base.CanHitNPC(target);
		}

		public override void AI()
		{
			#region Active check
			if (Owner.dead || !Owner.active) // This is the "active check", makes sure the minion is alive while the Player is alive, and despawns if not
				Owner.ClearBuff(BuffType<GrayGooSummonBuff>());

			if (Owner.HasBuff(BuffType<GrayGooSummonBuff>()))
				Projectile.timeLeft = 2;
			#endregion

			#region Find target
			// Starting search distance
			Vector2 targetCenter = Projectile.Center;
			foundTarget = EnemyWhoAmI >= 0;

			var alreadyTargetted = new List<NPC>();
			var goos = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<GrayGooProj>() && n != Projectile && (n.ModProjectile as GrayGooProj).foundTarget).ToList();
			goos.ForEach(n => alreadyTargetted.Add(Main.npc[(int)(n.ModProjectile as GrayGooProj).EnemyWhoAmI]));


			// This code is required if your minion weapon has the targeting feature
			if (Owner.HasMinionAttackTargetNPC)
			{
				NPC NPC = Main.npc[Owner.MinionAttackTargetNPC];
				float between = Vector2.Distance(NPC.Center, Projectile.Center);
				// Reasonable distance away so it doesn't target across multiple screens
				if (between < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
					EnemyWhoAmI = NPC.whoAmI;
					foundTarget = true;
				}
			}
			else if (foundTarget)
			{
				NPC NPC = Main.npc[(int)EnemyWhoAmI];
				float betweenPlayer = Vector2.Distance(NPC.Center, Owner.Center);

				if (NPC.active && NPC.CanBeChasedBy() && betweenPlayer < maxMinionChaseRange)
				{
					targetCenter = NPC.Center;
				}
				else
				{
					EnemyWhoAmI = -1;
					foundTarget = false;
				}
			}

			else if (!Owner.HasMinionAttackTargetNPC)
			{
				NPC target = Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < maxMinionChaseRange && Collision.CanHitLine(Projectile.position, 0, 0, n.position, 0, 0) && !alreadyTargetted.Contains(n)).OrderBy(n => Vector2.Distance(n.Center, Projectile.Center)).FirstOrDefault();
				if (target != default)
				{
					targetCenter = target.Center;
					EnemyWhoAmI = target.whoAmI;
					foundTarget = true;
				}
				else
				{
					EnemyWhoAmI = 0;
					foundTarget = false;
				}
			}

			if (EnemyWhoAmI != oldEnemyWhoAmI)
			{
				if (foundTarget)
					ReadjustDust();
				oldEnemyWhoAmI = EnemyWhoAmI;
			}

			#endregion

			if (!Main.dedServ && lerper < 1)
			{
				if (lerper == 0)
					KillDust();
				for (int i = 0; i < 5; i++)
				{
					Vector2 startPos = Projectile.Center + Main.rand.NextVector2Circular(20, 20);
					Vector2 offset = Main.rand.NextVector2Circular(20, 20);
					var dust = Dust.NewDustPerfect(startPos, ModContent.DustType<GrayGooDust>(), Vector2.Zero, 0, Color.Transparent, 1);
					dust.customData = new GrayGooDustData((int)offset.X, (int)offset.Y, Projectile, Main.rand.NextFloat(0.07f, 0.17f), Main.rand.NextFloat(9, 20));
				}

				lerper += 0.1f;
			}

			if (foundTarget)
			{
				NPC actualTarget = Main.npc[(int)EnemyWhoAmI];

				if (!actualTarget.active)
				{
					EnemyWhoAmI = -1;
					foundTarget = false;
					Projectile.velocity = Vector2.Zero;
				}

				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(actualTarget.Center) * 16, 0.07f);
			}
			else
			{
				Projectile.velocity = Vector2.Lerp(Projectile.velocity, Projectile.DirectionTo(Owner.Center) * 16, 0.02f);
				EnemyWhoAmI = -1;
			}
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			float scale = 1;
			int amount = 5;

			if (target.life <= 0)
			{
				amount = 12;
				scale = 1.5f;
			}

			for (int i = 0; i < amount; i++)
			{
				Vector2 dir = Main.rand.NextVector2Circular(6, 6);
				Dust.NewDustPerfect(target.Center + dir, ModContent.DustType<GrayGooSplashDust>(), dir, default, default, scale);
			}
		}

		private void ReadjustDust()
		{
			NPC target = Main.npc[(int)EnemyWhoAmI];
			foreach (Dust dust in Main.dust)
			{
				if (dust.type == ModContent.DustType<GrayGooDust>() && dust.customData is GrayGooDustData data && data.proj == Projectile)
				{
					Vector2 offset = Main.rand.NextVector2Circular(target.width / 2, target.height / 2);
					data.x = (int)offset.X;
					data.y = (int)offset.Y;
				}
			}
		}

		private void KillDust()
		{
			foreach (Dust dust in Main.dust)
			{
				if (dust.type == ModContent.DustType<GrayGooDust>() && dust.customData is GrayGooDustData data && data.proj == Projectile)
				{
					dust.active = false;
				}
			}
		}

		private static void DrawGooTarget(Projectile goo, SpriteBatch spriteBatch)
		{
			if (goo == default)
				return;

			var modproj = goo.ModProjectile as GrayGooProj;

			if (!modproj.foundTarget)
				return;

			NPC NPC = Main.npc[(int)modproj.EnemyWhoAmI];

			if (!NPC.active)
				return;

			if (NPC.active)
			{
				if (NPC.ModNPC != null)
				{
					if (NPC.ModNPC is ModNPC ModNPC)
					{
						if (ModNPC.PreDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White)))
							Main.instance.DrawNPC((int)modproj.EnemyWhoAmI, false);

						ModNPC.PostDraw(spriteBatch, Main.screenPosition, NPC.GetAlpha(Color.White));
					}
				}
				else
				{
					Main.instance.DrawNPC((int)modproj.EnemyWhoAmI, false);
				}
			}
		}
		private void DrawNPCtarget(On.Terraria.Main.orig_CheckMonoliths orig)
		{
			orig();

			var goos = Main.projectile.Where(n => n.active && n.type == ModContent.ProjectileType<GrayGooProj>()).ToList();
			if (goos.Count == 0)
				return;

			GraphicsDevice gD = Main.graphics.GraphicsDevice;
			SpriteBatch spriteBatch = Main.spriteBatch;

			if (Main.gameMenu || Main.dedServ || spriteBatch is null || NPCTarget is null || gD is null)
				return;

			RenderTargetBinding[] bindings = gD.GetRenderTargets();
			gD.SetRenderTarget(NPCTarget);
			gD.Clear(Color.Transparent);

			spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.ZoomMatrix);

			goos.ForEach(n => DrawGooTarget(n, spriteBatch));
			spriteBatch.End();
			gD.SetRenderTargets(bindings);
		}
	}
	public class GrayGooSplashDust : ModDust
	{
		public override string Texture => AssetDirectory.Invisible;

		public override void OnSpawn(Dust dust)
		{
			dust.noGravity = false;
			dust.noLight = false;
		}

		public override bool Update(Dust dust)
		{
			dust.position += dust.velocity;
			dust.velocity.Y += 0.2f;
			if (Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].HasTile && Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].BlockType == Terraria.ID.BlockType.Solid && Main.tileSolid[Main.tile[(int)dust.position.X / 16, (int)dust.position.Y / 16].TileType])
				dust.active = false;

			dust.rotation = dust.velocity.ToRotation();
			dust.scale *= 0.99f;
			if (dust.scale < 0.5f)
				dust.active = false;
			return false;
		}
	}
	public class GrayGooDust : Glow
	{
		public override bool Update(Dust dust)
		{
			var data = (GrayGooDustData)dust.customData;
			if (!data.proj.active)
			{
				dust.active = false;
				return false;
			}

			var MP = data.proj.ModProjectile as GrayGooProj;
			float lerper = data.lerp / 3;

			Vector2 entityCenter = MP.Owner.Center;
			if (MP.foundTarget)
			{
				NPC npc = Main.npc[(int)MP.EnemyWhoAmI];

				if (npc.active)
				{
					entityCenter = npc.Center;
					lerper *= 3;
				}
			}

			Vector2 posToBe = entityCenter + new Vector2(data.x, data.y);
			var unused = dust.shader.UseColor(dust.color);

			if ((posToBe - dust.position).Length() < 5)
			{
				dust.position = posToBe;
				dust.velocity = Vector2.Zero;
				return false;
			}

			Vector2 direction = dust.position.DirectionTo(posToBe);

			if (posToBe.Distance(dust.position) > 20)
				dust.velocity = Vector2.Lerp(dust.velocity, direction * data.speed, lerper);
			dust.position += dust.velocity;
			return false;
		}
	}
}