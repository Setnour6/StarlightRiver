﻿using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Breacher;
using StarlightRiver.Content.Items.Magnet;
using StarlightRiver.Core.Systems.LightingSystem;
using StarlightRiver.Core.Systems.MetaballSystem;
using System.Linq;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Metaballs
{
	internal class GrayGooMetaballs : MetaballActor
	{
		public RenderTarget2D touchingNPCs;

		public override bool Active => Main.dust.Any(x => x.active && x.type == DustType);

		public override Color OutlineColor => Color.DarkGray;

		public virtual Color InteriorColor => Color.Gray;

		public virtual int DustType => ModContent.DustType<GrayGooDust>();

		public override bool OverEnemies => true;

		public override void DrawShapes(SpriteBatch spriteBatch)
		{
			Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowVerySoft").Value;

			if (borderNoise is null)
				return;

			borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
			borderNoise.CurrentTechnique.Passes[0].Apply();

			foreach (Dust dust in Main.dust)
			{
				if (dust.active && dust.type == DustType)
				{
					borderNoise.Parameters["offset"].SetValue(dust.rotation);
					spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, Color.White * 0.9f, dust.rotation, tex.Size() / 2, dust.scale * 0.25f, SpriteEffects.None, 0);
				}
			}

			spriteBatch.End();
            spriteBatch.Begin();
        }

		public override bool PreDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			if (GrayGooProj.NPCTarget == null)
				return false;
			
            Effect effect = Filters.Scene["GrayGooShader"].GetShader().Shader;
            effect.Parameters["NPCTarget"].SetValue(GrayGooProj.NPCTarget);
            effect.Parameters["threshhold"].SetValue(0.99f);

            spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, effect);

            spriteBatch.Draw(Target, position: Vector2.Zero, color: Color.White);

            spriteBatch.End();
            spriteBatch.Begin();
            return false;
		}

		public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
		{
			var sourceRect = new Rectangle(0, 0, target.Width, target.Height);
			LightingBufferRenderer.DrawWithLighting(sourceRect, target, sourceRect, InteriorColor, new Vector2(2, 2));
			return false;
		}
	}
}
