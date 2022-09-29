﻿//TODO:
//Clean up code
//Better Collision
//Fix bug with screen position while it'd thrown
//Better thrown hit cooldown
//Better sound effects
//Obtainment
//Balance
//Sellprice
//Rarity
//Make it look good when swinging to the left
//Less spritebatch restarts
//Description
//Less spritebatch restarts

//TODO on red rightclick:
//Make it not turn around if you have no enemy selected

//TODO on white rightclick:

//TODO on blue rightclick:

//TODO on greenrightclick:

//TODO on purple rightclick:

//TODO on yellow rightclick:

using Microsoft.Xna.Framework;
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
	public class LightsaberLoader : IOrderedLoadable
    {
		public float Priority => 1;

		public void Load()
		{
			var Mod = StarlightRiver.Instance;

			Mod.AddContent(new Lightsaber("Red", "Right click to pull in enemies, release to slash them for high damage \n'There is no escape'", ItemID.RedPhaseblade));
			Mod.AddContent(new Lightsaber("Blue", "Right click to parry oncoming projectiles \n'May the force be with you'", ItemID.BluePhaseblade));
			Mod.AddContent(new Lightsaber("Green", "Right click to flip in the air, spinning the blade \n'Do or do not. There is no try'", ItemID.GreenPhaseblade));
			Mod.AddContent(new Lightsaber("Purple", "Right click to spin the blade around you rapidly \n'The senate will decide your fate'", ItemID.PurplePhaseblade));
			Mod.AddContent(new Lightsaber("White", "Right click to dash towards the blade while it's thrown out\n'I am no Jedi.'", ItemID.WhitePhaseblade));
			Mod.AddContent(new Lightsaber("Yellow", "Right click to pull out a second blade\n'I've got a bad feeling about this'", ItemID.YellowPhaseblade));
		}

		public void Unload() { }
	}

	[Autoload(false)]
	public class Lightsaber : ModItem
	{
		public override string Texture => AssetDirectory.BreacherItem + Name;

		protected override bool CloneNewInstances => true;

		public override string Name => newName;

		public string ProjType;

		public string Description;

		public string ItemName;

		public int IngrediantID;

		public string newName;

		public Lightsaber() { }

		public Lightsaber(string colorName, string description, int ingrediantID)
        {
			newName = colorName + "Lightsaber";
			ItemName = colorName + " Lightsaber";
			ProjType = "LightsaberProj_" + colorName;
			Description = description;
			IngrediantID = ingrediantID;
        }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(ItemName);
			Tooltip.SetDefault("Left click to slash and throw \n" + Description);
		}

		public override void SetDefaults()
		{
			Item.damage = 22;
			Item.DamageType = DamageClass.Melee;
			Item.width = 36;
			Item.height = 44;
			Item.useTime = 12;
			Item.useAnimation = 12;
			Item.reuseDelay = 20;
			Item.channel = true;
			Item.useStyle = ItemUseStyleID.Shoot;
			Item.knockBack = 6.5f;
			Item.crit = 9;
			Item.shootSpeed = 14f;
			Item.autoReuse = false;
			Item.shoot = Mod.Find<ModProjectile>(ProjType).Type;
			Item.noUseGraphic = true;
			Item.noMelee = true;
			Item.autoReuse = false;
			Item.value = Item.sellPrice(0, 0, 20, 0);
            Item.rare = ItemRarityID.Blue;
        }

        public override bool AltFunctionUse(Player player)
        {
			return true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
			Projectile proj = Projectile.NewProjectileDirect(source, position, velocity, type, damage, knockback, player.whoAmI);
			(proj.ModProjectile as LightsaberProj).rightClicked = player.altFunctionUse == 2;
			return false;
        }
    }

	enum CurrentAttack : int
	{
		Slash1 = 0,
		Slash2 = 1,
		Slash3 = 2,
		Slash4 = 3,
		Throw = 4,
		Reset = 5
	}
	public abstract class LightsaberProj : ModProjectile
	{
		public override string Texture => AssetDirectory.BreacherItem + "LightsaberProj";


		private CurrentAttack currentAttack = CurrentAttack.Slash1;

		protected bool updatePoints = true;

		protected Vector2 oldScreenPos = Vector2.Zero;

		protected Vector2 anchorPoint = Vector2.Zero;

		protected int afterImageLength = 16;

		protected int throwTimer = 0;
		protected bool thrown = false;

		protected Vector2 thrownDirection = Vector2.Zero;

		public bool rightClicked = false;
		protected bool hide = true;
		protected bool canHit = false;

		protected bool initialized = false;

		protected int attackDuration = 0;

		protected float startRotation = 0f;

		protected float midRotation = 0f;
		protected float endMidRotation = 0f;
		protected float startMidRotation = 0f;

		protected float endRotation = 0f;

		protected bool facingRight;

		protected float squish = 1;

		protected float startSquish = 1;

		protected float endSquish = 1;

		protected float rotVel = 0f;

		protected int growCounter = 0;

		protected List<float> oldRotation = new List<float>();
		protected List<Vector2> oldPositionDrawing = new List<Vector2>();
		protected List<float> oldSquish = new List<float>();

		protected List<Vector2> oldPositionCollision = new List<Vector2>();

		protected List<NPC> hit = new List<NPC>();

		protected ref float nonEasedProgress => ref Projectile.ai[0];

		protected Player owner => Main.player[Projectile.owner];

		protected virtual Vector3 BladeColor => Color.Green.ToVector3();

		private bool FirstTickOfSwing
		{
			get => Projectile.ai[0] == 0;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lightsaber");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
			Main.projFrames[Projectile.type] = 1;
		}

		public override void SetDefaults()
		{
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.tileCollide = false;
			Projectile.Size = new Vector2(150, 150);
			Projectile.penetrate = 1;
			Projectile.ownerHitCheck = true;
			Projectile.extraUpdates = 5;
		}

		public override void AI()
		{
			if (!hide)
				Lighting.AddLight(Projectile.Center, BladeColor * 4.6f);
			if (rightClicked)
			{
				RightClickBehavior();
			}
			else
			{
				hide = false;
				canHit = true;
				if (thrown)
					ThrownBehavior();
				else
					HeldBehavior();
			}

			if (!updatePoints)
				return;

			oldSquish.Add(squish);
			oldRotation.Add(Projectile.rotation);
			oldPositionDrawing.Add(anchorPoint);
			oldPositionCollision.Add(Projectile.Center);

			if (oldRotation.Count > afterImageLength)
				oldRotation.RemoveAt(0);
			if (oldPositionDrawing.Count > afterImageLength)
				oldPositionDrawing.RemoveAt(0);
			if (oldSquish.Count > afterImageLength)
				oldSquish.RemoveAt(0);
			if (oldPositionCollision.Count > afterImageLength)
				oldPositionCollision.RemoveAt(0);

			if (thrown)
			{ 
				for (int i = 0; i < oldPositionDrawing.Count; i++)
				{
					oldPositionCollision[i] += Projectile.velocity;
					oldPositionDrawing[i] += Projectile.velocity;
					oldSquish[i] = squish;
				}
            }
			if (thrown && throwTimer % Projectile.extraUpdates == Projectile.extraUpdates - 1)
			{ 
				for (int i = 0; i < oldPositionDrawing.Count; i++)
				{
					oldPositionDrawing[i] += (oldScreenPos - Main.screenPosition);
				}
			}
			oldScreenPos = Main.screenPosition;
		}

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
		{
			float collisionPoint = 0f;

			for (int i = 0; i < oldPositionCollision.Count; i++) 
			{
				if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), oldPositionCollision[i], GetCollisionPoint(i) + oldPositionCollision[i], 40, ref collisionPoint))
					return true;
			}
			return false;
		}

		public override bool? CanHitNPC(NPC target)
		{
			if (hit.Contains(target) || !canHit)
				return false;
			return base.CanHitNPC(target);
		}
		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
			Projectile.penetrate++;
			hit.Add(target);
			//Helper.PlayPitched("Impacts/PanBonkSmall", 0.5f, Main.rand.NextFloat(-0.2f, 0.2f), target.Center);
			Core.Systems.CameraSystem.Shake += 2;
		}

		public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			hitDirection = Math.Sign(target.Center.X - owner.Center.X);
        }

        public override bool PreDraw(ref Color lightColor)
        {
			if (hide)
				return false;
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D whiteTex = ModContent.Request<Texture2D>(Texture + "_White").Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Texture2D smallGlowTex = ModContent.Request<Texture2D>(Texture + "_SmallGlow").Value;

			Vector2 scaleVec;
            Effect effect = Filters.Scene["3DSwing"].GetShader().Shader;
            effect.Parameters["color"].SetValue(Color.White.ToVector4());
			effect.Parameters["rotation"].SetValue(Projectile.rotation - midRotation);
			for (int i = 0; i < oldPositionDrawing.Count; i++) //disgusting amount of spritebatch restarts but I can't figure out another way to do it
			{
				scaleVec = new Vector2(1, oldSquish[i]) * 4;
				effect.Parameters["rotation"].SetValue(oldRotation[i] - midRotation);

				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(new Vector4(BladeColor * 40, 0) * MathHelper.Max(rotVel * 0.125f, 0.00125f));
				effect.Parameters["pommelToOriginPercent"].SetValue(-0.305f);
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(glowTex, oldPositionDrawing[i], null, Color.White, midRotation, (glowTex.Size() / 2f), scaleVec, SpriteEffects.None, 0f);


				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(new Vector4(BladeColor * 40, 0) * MathHelper.Max(rotVel * 0.25f, 0.0025f));
				effect.Parameters["pommelToOriginPercent"].SetValue(0.1f);
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(smallGlowTex, oldPositionDrawing[i], null, Color.White, midRotation, smallGlowTex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);

				Main.spriteBatch.End();
				effect.Parameters["color"].SetValue(Color.White.ToVector4());
				Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(whiteTex, oldPositionDrawing[i], null, Color.White, midRotation, whiteTex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);
			}
			scaleVec = new Vector2(1, squish) * 4;
			Main.spriteBatch.End();

			effect.Parameters["color"].SetValue(lightColor.ToVector4());
			effect.Parameters["rotation"].SetValue(Projectile.rotation - midRotation);

			Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, midRotation, tex.Size() / 2f, scaleVec, SpriteEffects.None, 0f);

			Main.spriteBatch.End();
			Main.spriteBatch.Begin(default, default, default, default, default, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

		private Vector2 GetCollisionPoint(int i)
        {
			float angleShift = oldRotation[i] - midRotation;

			//Get the coordinates of the angle shift.
			Vector2 anglePoint = angleShift.ToRotationVector2();

			//Squish the angle point's coordinate
			anglePoint.X *= 1;
			anglePoint.Y *= oldSquish[i];

			//And back into an angle
			angleShift = anglePoint.ToRotation();

			return ((angleShift + midRotation).ToRotationVector2() * 80);
		}

		private float EaseProgress(float input)
		{
			if (currentAttack == CurrentAttack.Throw)
				return EaseFunction.EaseQuadInOut.Ease(input);
			return EaseFunction.EaseCubicInOut.Ease(input);
		}

		private void ThrownBehavior()
        {
			rotVel = 0.04f;
			squish = MathHelper.Lerp(squish, 0.6f - (Projectile.velocity.Length() * 0.08f), 0.1f);
			anchorPoint = Projectile.Center - Main.screenPosition;
			Projectile.timeLeft = 50;
			throwTimer++;
			nonEasedProgress = (float)Math.Cos(throwTimer * 0.01f);
			float progress = EaseFunction.EaseQuadOut.Ease(Math.Abs(nonEasedProgress)) * Math.Sign(Math.Cos(throwTimer * 0.01f));
			bool goingBack = Math.Sign(progress) == -1;
			if (goingBack)
				thrownDirection = owner.DirectionTo(Projectile.Center);
			Projectile.velocity = (progress) * 5 * thrownDirection;
			midRotation = Projectile.velocity.ToRotation();

			Projectile.extraUpdates = 8;
			Projectile.rotation += 0.06f;

			if (Projectile.Distance(owner.Center) < 20 && goingBack)
				Projectile.active = false;
        }

		private void HeldBehavior()
        {
			Projectile.velocity = Vector2.Zero;
			Projectile.Center = owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.heldProj = Projectile.whoAmI;

			if (FirstTickOfSwing)
			{
				hit = new List<NPC>();
				if (owner.DirectionTo(Main.MouseWorld).X > 0)
					facingRight = true;
				else
					facingRight = false;

				float rot = owner.DirectionTo(Main.MouseWorld).ToRotation();
				if (!initialized)
				{
					anchorPoint = Vector2.Zero;
					initialized = true;
					endRotation = rot - (1f * owner.direction);

					oldRotation = new List<float>();
					oldPositionDrawing = new List<Vector2>();
					oldSquish = new List<float>();
					oldPositionCollision = new List<Vector2>();
				}
				else
				{
					currentAttack = (CurrentAttack)((int)currentAttack + 1);
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item15 with { Pitch = Main.rand.NextFloat(-0.1f, 0.1f) }, owner.Center);

				startRotation = endRotation;
				startSquish = 0.4f;
				endMidRotation = rot + Main.rand.NextFloat(-0.45f, 0.45f);
				startMidRotation = midRotation;

				switch (currentAttack)
				{
					case CurrentAttack.Slash1:
						endSquish = 0.3f;
						endRotation = rot + (3f * owner.direction);
						attackDuration = 125;
						break;
					case CurrentAttack.Slash2:
						endSquish = 0.2f;
						attackDuration = 129;
						endRotation = rot - (2f * owner.direction);
						break;
					case CurrentAttack.Slash3:
						endSquish = 0.45f;
						endRotation = rot + (3f * owner.direction);
						attackDuration = 125;
						break;
					case CurrentAttack.Slash4:
						endSquish = 0.15f;
						endRotation = rot - (3f * owner.direction);
						attackDuration = 125;
						break;
					case CurrentAttack.Throw:
						endSquish = 0.6f;
						attackDuration = 250;
						endRotation = rot + (10f * owner.direction);
						break;
					case CurrentAttack.Reset:
						endSquish = 0.6f;
						Projectile.active = false;
						break;
				}
				Projectile.ai[0] += 30f / attackDuration;
			}

			if (Projectile.ai[0] < 1)
			{
				Projectile.timeLeft = 50;
				Projectile.ai[0] += 1f / attackDuration;
				rotVel = Math.Abs(EaseProgress(Projectile.ai[0]) - EaseProgress(Projectile.ai[0] - (1f / attackDuration))) * 2;
			}
			else
			{
				rotVel = 0f;
				if (Main.mouseLeft)
				{
					Projectile.ai[0] = 0;
					return;
				}
			}

			float progress = EaseProgress(Projectile.ai[0]);

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
			float wrappedRotation = MathHelper.WrapAngle(Projectile.rotation);

			float throwingAngle = MathHelper.WrapAngle(owner.DirectionTo(Main.MouseWorld).ToRotation());
			if (currentAttack == CurrentAttack.Throw && Math.Abs(wrappedRotation - throwingAngle) < 0.2f && Projectile.ai[0] > 0.4f)
			{
				oldScreenPos = Main.screenPosition;
				thrown = true;
				thrownDirection = owner.DirectionTo(Main.MouseWorld);
				Projectile.velocity = thrownDirection * 1;
				afterImageLength = 50;
			}

			owner.ChangeDir(facingRight ? 1 : -1);

			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - 1.57f);
			owner.itemAnimation = owner.itemTime = 5;

			if (owner.direction != 1)
				Projectile.rotation += 0.78f;
		}
		protected virtual void RightClickBehavior()
		{

		}
	}
}