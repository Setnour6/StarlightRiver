﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.Breacher
{
	public class LightsaberProj_Blue : LightsaberProj
	{
		protected override Vector3 BladeColor => new Vector3(0, 0.1f, 0.255f);
	}

	public class LightsaberProj_Green : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Green.ToVector3();

		bool jumped = false;

		private int soundTimer = 0;
        protected override void RightClickBehavior()
        {
			if (!jumped)
            {
				jumped = true;
				owner.velocity = owner.DirectionTo(Main.MouseWorld) * 20;
				owner.GetModPlayer<LightsaberPlayer>().jumping = true;
            }

			if (soundTimer++ % 100 == 0)
			{
				hit = new List<NPC>();
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, owner.Center);
			}
			owner.itemTime = owner.itemAnimation = 2;
			Projectile.timeLeft = 200;
			afterImageLength = 30;
			Projectile.rotation += 0.06f * owner.direction;
			rotVel = 0.02f;
			midRotation = owner.velocity.ToRotation();
			squish = 0.7f;
			hide = false;
			canHit = true;
			anchorPoint = Projectile.Center - Main.screenPosition;
			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.heldProj = Projectile.whoAmI;

			if (!owner.GetModPlayer<LightsaberPlayer>().jumping)
			{
				Core.Systems.CameraSystem.Shake += 10;
				for (int i = 0; i < 30; i++)
					Dust.NewDustPerfect(owner.Bottom, ModContent.DustType<LightsaberGlow>(), Main.rand.NextVector2Circular(10, 10), 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(1.95f, 2.35f));
				Projectile.active = false;
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightsaberProj_GreenExplosion>(), Projectile.damage * 2, 0, owner.whoAmI);
				Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), owner.Bottom, Vector2.Normalize(owner.GetModPlayer<LightsaberPlayer>().jumpVelocity) * 1.2f, ModContent.ProjectileType<Vitric.IgnitionGauntletsImpactRing>(), 0, 0, owner.whoAmI, Main.rand.Next(60, 90), owner.GetModPlayer<LightsaberPlayer>().jumpVelocity.ToRotation());
				(proj.ModProjectile as Vitric.IgnitionGauntletsImpactRing).outerColor = new Color(BladeColor.X, BladeColor.Y, BladeColor.Z);
				(proj.ModProjectile as Vitric.IgnitionGauntletsImpactRing).ringWidth = 40;
				(proj.ModProjectile as Vitric.IgnitionGauntletsImpactRing).timeLeftStart = 40;
				proj.timeLeft = 40;
			}
		}
    }

	public class LightsaberProj_Purple : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Purple.ToVector3();
	}

	public class LightsaberProj_Red : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.DarkRed.ToVector3() * 1.3f;

		bool releasedRight = false;

		int pullTimer = 0;

		NPC pullTarget;

		private bool targetNoGrav = false;

		private Vector2 pullDirection = Vector2.Zero;

		private int pauseTime = 0;

		private Vector2 launchVector = Vector2.Zero;
		protected override void RightClickBehavior()
        {
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.heldProj = Projectile.whoAmI;
			if (!releasedRight && Main.mouseRight)
			{
				Projectile.timeLeft = 30;
				hide = true;
				canHit = false;
				if (pullTimer == 0)
					pullTarget = Main.npc.Where(x => x.active && x.knockBackResist > 0 && !x.boss && !x.townNPC && x.Distance(Main.MouseWorld) < 200).OrderBy(x => x.Distance(Main.MouseWorld)).FirstOrDefault();

				if (pullTarget != default)
				{
					if (pullTimer == 0)
					{
						targetNoGrav = pullTarget.noGravity;
						pullTarget.noGravity = true;
					}
					pullDirection = owner.DirectionTo(pullTarget.Center);
					pullTarget.velocity = -pullDirection * EaseFunction.EaseQuinticIn.Ease(MathHelper.Clamp(pullTimer / 150f, 0, 1)) * 12;
					Projectile.rotation = pullDirection.ToRotation();

					if (pullTarget.Distance(owner.Center) < 5)
						releasedRight = true;

					Vector2 dustVel = pullDirection.RotatedByRandom(0.8f) * Main.rand.NextFloat();
					Dust.NewDustPerfect(pullTarget.Center - (dustVel * 45), ModContent.DustType<Dusts.Glow>(), dustVel * 3, 0, new Color(BladeColor.X, BladeColor.Y, BladeColor.Z), Main.rand.NextFloat(0.25f, 0.45f));
				}
				else
					Projectile.rotation = owner.DirectionTo(Main.MouseWorld).ToRotation();
				pullTimer++;
			}
			else
			{
				if (pullTarget != default)
                {
					pullTarget.noGravity = targetNoGrav;
                }

				if (!releasedRight)
				{
					float rot = Projectile.rotation;
					if (owner.direction == 1)
						facingRight = true;
					else
						facingRight = false;

					midRotation = rot;
					canHit = true;
					releasedRight = true;
					hide = false;

					anchorPoint = Vector2.Zero;
					endRotation = rot - (2f * owner.direction);

					oldRotation = new List<float>();
					oldPositionDrawing = new List<Vector2>();
					oldSquish = new List<float>();
					oldPositionCollision = new List<Vector2>();

					Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, owner.Center);

					startRotation = endRotation;
					startSquish = endSquish;
					endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
					startMidRotation = midRotation;
					endSquish = 0.3f;
					endRotation = rot + (3f * owner.direction);
					attackDuration = 65;
					//Projectile.ai[0] += 30f / attackDuration;
				}

				if (Projectile.ai[0] < 1)
				{
					Projectile.timeLeft = 50;
					if (pauseTime-- <= 0)
						Projectile.ai[0] += 1f / attackDuration;
					rotVel = Math.Abs(EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]) - EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0] - (1f / attackDuration))) * 2;
				}
				else
					rotVel = 0f;

				float progress = EaseFunction.EaseQuadInOut.Ease(Projectile.ai[0]);

				/*if (Main.netMode != NetmodeID.Server)
				{
					if (trailCounter % 5 == 0 || (progress > 0.1f && progress < 0.9f))
					{
						ManageCaches();
						ManageTrail();
					}
				}*/

				Projectile.scale = MathHelper.Min(MathHelper.Min(growCounter++ / 30f, 1 + (rotVel * 4)), 1.3f);

				Projectile.rotation = MathHelper.Lerp(startRotation, endRotation, progress);
				midRotation = MathHelper.Lerp(startMidRotation, endMidRotation, progress);
				squish = MathHelper.Lerp(startSquish, endSquish, progress) + (0.35f * (float)Math.Sin(3.14f * progress));
				anchorPoint = Projectile.Center - Main.screenPosition;

				owner.ChangeDir(facingRight ? 1 : -1);

				owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
				owner.itemAnimation = owner.itemTime = 5;

				if (owner.direction != 1)
					Projectile.rotation += 0.78f;

				updatePoints = pauseTime <= 0;

				if (pullTarget != null && pullTarget.active)
				{
					if (pauseTime > 0)
					{
						pullTarget.velocity = Vector2.Zero;
					}
					else if (pauseTime == 0)
                    {
						pullTarget.velocity = launchVector * 8 * pullTarget.knockBackResist;
                    }
				}
			}
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (target == pullTarget)
            {
				Core.Systems.CameraSystem.Shake += 5;
				launchVector = pullTarget.DirectionTo(Main.MouseWorld);
				damage = (int)(damage * 2.5f);
				target.velocity = Vector2.Zero;
				pauseTime = 40;
            }
        }
    }

	public class LightsaberProj_White : LightsaberProj
	{
		protected override Vector3 BladeColor => new Color(200, 200, 255).ToVector3();
    }

	public class LightsaberProj_Yellow : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Yellow.ToVector3() * 0.8f * fade;

		private bool dashing = false;

		private bool caughtUp = false;

		private float fade = 1f;

        protected override void RightClickBehavior()
        {
			hide = true;
			canHit = false;
			Projectile.active = false;
        }

        protected override void SafeLeftClickBehavior()
        {
			if (!thrown)
				return;

			if (Main.mouseRight && !dashing)
			{
				dashing = true;
				Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<LightsaberProj_YellowDash>(), Projectile.damage * 2, 0, owner.whoAmI);
				owner.GetModPlayer<LightsaberPlayer>().dashing = true;
			}

			if (dashing)
            {
				Projectile.velocity = Vector2.Zero;
				if (owner.Distance(Projectile.Center) < 80 || !owner.GetModPlayer<LightsaberPlayer>().dashing && !caughtUp)
                {
					owner.Center = Projectile.Center;
					owner.velocity = Vector2.Zero;
					owner.GetModPlayer<LightsaberPlayer>().dashing = false;
					Projectile.active = true;
					caughtUp = true;
				}

				if (caughtUp)
                {
					Projectile.active = true;
					fade -= 0.01f;
					if (fade <= 0)
						Projectile.active = false;
                }
				else
					owner.velocity = owner.DirectionTo(Projectile.Center) * 60;
			}
        }
    }

	public class LightsaberProj_YellowDash : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults() => DisplayName.SetDefault("Lightsaber");

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 120;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.Center = owner.Center;

			if (!owner.GetModPlayer<LightsaberPlayer>().dashing)
				Projectile.active = false;
		}
	}

	public class LightsaberProj_GreenExplosion : ModProjectile
	{
		public override string Texture => AssetDirectory.Invisible;

		private Player owner => Main.player[Projectile.owner];

		public override void SetStaticDefaults() => DisplayName.SetDefault("Lightsaber");

		public override void SetDefaults()
		{
			Projectile.width = Projectile.height = 170;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.aiStyle = -1;
			Projectile.friendly = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 10;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}

		public override void AI()
		{
			Projectile.Center = owner.Center;
		}
	}

	public class LightsaberPlayer : ModPlayer
    {
		public bool dashing = false;

		public bool jumping = false;
		public Vector2 jumpVelocity = Vector2.Zero;

		public float storedBodyRotation = 0f;

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
			if (dashing)
				return false;
            return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override void PreUpdate()
        {
            if (dashing || jumping)
				Player.maxFallSpeed = 2000f;
		}

        public override void PostUpdate()
        {
			if (jumping)
            {
				storedBodyRotation += 0.3f * Player.direction;
				Player.fullRotation = storedBodyRotation;
				Player.fullRotationOrigin = Player.Size / 2;
			}
			if (Player.velocity.X == 0 || Player.velocity.Y == 0)
				dashing = false;
			if (Player.velocity.Y == 0)
            {
				storedBodyRotation = 0;
				Player.fullRotation = 0;
				jumping = false;
			}
			else
            {
				jumpVelocity = Player.velocity;
            }
        }
    }
}