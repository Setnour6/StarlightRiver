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

namespace StarlightRiver.Content.Items.Lightsaber
{
	public class LightsaberProj_Orange : LightsaberProj
	{
		protected override Vector3 BladeColor => Color.Orange.ToVector3();

		private bool released = false;

		private int hitTimer = 0;

		private int releaseTimer = 0;

		private Vector2 mouseStart = Vector2.Zero;
		private Vector2 playerStart = Vector2.Zero;
		protected override void ThrownBehavior()
		{

			float progressMult = 0.0075f;

			owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, owner.DirectionTo(Projectile.Center).ToRotation() - 1.57f);
			if (hitTimer++ % ((Projectile.extraUpdates + 1) * 6) == 0)
				hit = new List<NPC>();
			if (throwTimer == 0)
			{
				Vector2 mouseOffset = Main.MouseWorld - owner.Center;
				int max = 400 + (int)MathHelper.Max(0, (mouseOffset.Length() - 400) * 0.2f);
				mouseStart = owner.Center + (Vector2.Normalize(mouseOffset) * MathHelper.Min(mouseOffset.Length(), max));
				playerStart = owner.Center;
				midRotation = Projectile.DirectionTo(mouseStart).ToRotation();
			}

			rotVel = 0.04f;
			squish = MathHelper.Lerp(squish, 0.8f - (Projectile.velocity.Length() * 0.04f), 0.1f);
			anchorPoint = Projectile.Center - Main.screenPosition;
			Projectile.timeLeft = 50;
			owner.itemTime = owner.itemAnimation = 2;

			if ((!Main.mouseLeft || releaseTimer++ > 1600) && !released && Math.Cos((throwTimer + 50) * progressMult) <= 0)
			{
				throwTimer = (int)(1.57f / progressMult);
				mouseStart = Projectile.Center;
				released = true;
			}

			if (released)
			{
				playerStart = owner.Center;
				midRotation = Projectile.DirectionTo(playerStart).ToRotation();
			}
			else
			{
				Vector2 mouseOffset = Main.MouseWorld - owner.Center;
				int max = 400 + (int)MathHelper.Max(0, (mouseOffset.Length() - 400) * 0.2f);
				mouseStart = owner.Center + (Vector2.Normalize(mouseOffset) * MathHelper.Min(mouseOffset.Length(), max));
			}

			float progress = EaseFunction.EaseQuadOut.Ease((float)Math.Sin(throwTimer * progressMult));
			float nextProgress = EaseFunction.EaseQuadOut.Ease((float)Math.Sin((throwTimer + 1) * progressMult));

			Vector2 currentPoint = Vector2.Lerp(playerStart, mouseStart, progress);
			Vector2 nextPoint = Vector2.Lerp(playerStart, mouseStart, nextProgress);
			Projectile.velocity = nextPoint - currentPoint;

			if (Math.Cos((throwTimer + 50) * progressMult) > 0 || released)
				throwTimer++;
			else
			{
				Vector2 mouseOffset = Main.MouseWorld - owner.Center;

				int max = 400 + (int)MathHelper.Max(0, (mouseOffset.Length() - 400) * 0.2f);
				Vector2 positionToBe = owner.Center + (Vector2.Normalize(mouseOffset) * MathHelper.Min(mouseOffset.Length(), max));
				Projectile.velocity = Projectile.DirectionTo(positionToBe) * 0.45f * MathHelper.Clamp((float)Math.Pow((Projectile.Center - positionToBe).Length(), 0.6f), 1, 30);
				midRotation = Projectile.velocity.ToRotation();
			}

			Projectile.extraUpdates = 8;
			Projectile.rotation += 0.06f;

			if ((Projectile.Distance(owner.Center) < 20 || Math.Sin(throwTimer * progressMult) < 0) && released)
				Projectile.active = false;

			updatePoints = true;
			canHit = true;
			hide = false;
		}
	}
}