﻿using System;

namespace StarlightRiver.Content.NPCs.Vitric.Gauntlet
{
	internal class GauntletSpawner : ConstructSpawner
	{
		public Vector2 targetPos;
		public Vector2 startPos;

		public int moveTimer;

		public override bool PreAI()
		{
			moveTimer++;

			gravity = false;
			Projectile.tileCollide = false;

			if (moveTimer < 60)
				Projectile.Center = Vector2.SmoothStep(startPos, targetPos, moveTimer / 60f) + new Vector2(0, -(float)Math.Sin(moveTimer / 60f * 3.14f) * 180);
			else if (moveTimer == 60)
				Timer = 2;

			return true;
		}
	}
}
