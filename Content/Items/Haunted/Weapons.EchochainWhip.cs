﻿using ReLogic.Content;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.Enums;
using Terraria.GameContent.Creative;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Haunted
{
	public class EchochainWhip : ModItem
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Echochain");
			Tooltip.SetDefault("Chains enemies together, sharing summon damage between them\nHold and release <right> to chain enemies near you to the ground, chaining all effected enemies");
			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.DefaultToWhip(ModContent.ProjectileType<EchochainWhipProjectile>(), 15, 3f, 3.75f, 40);
			Item.SetShopValues(ItemRarityColor.Green2, Item.sellPrice(gold: 1));
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.GoldBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();

			recipe.AddIngredient(ItemID.PlatinumBar, 15);
			recipe.AddIngredient<VengefulSpirit>(10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	public class EchochainWhipProjectile : BaseWhip
	{
		public List<Vector2> tipPositions = new();
		public List<float> tipRotations = new();

		public List<NPC> hitTargets = new();
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public EchochainWhipProjectile() : base("Echochain", 40, 1.25f, new Color(150, 255, 20) * 0.5f) { }

		public override void ArcAI()
		{
			if (Projectile.ai[0] > flyTime * 0.45f)
			{
				var points = new List<Vector2>();
				points.Clear();
				SetPoints(points);

				Vector2 tipPos = points[39];

				tipPositions.Add(tipPos);
				if (tipPositions.Count > 15)
					tipPositions.RemoveAt(0);

				Vector2 difference = points[39] - points[38];
				float rotation = difference.ToRotation() - MathHelper.PiOver2;

				tipRotations.Add(rotation);
				if (tipRotations.Count > 15)
					tipRotations.RemoveAt(0);

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1f, 1f), 0, new Color(100, 200, 10), 0.5f);

				Dust.NewDustPerfect(tipPos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(2f, 2f), 0, new Color(150, 255, 25), 0.35f);
			}
		}

		public override void DrawBehindWhip(ref Color lightColor)
		{
			Texture2D texBlur = ModContent.Request<Texture2D>(Texture + "_TipBlur").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(Texture + "_TipGlow").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;

			Asset<Texture2D> texture = ModContent.Request<Texture2D>(Texture);
			Rectangle whipFrame = texture.Frame(1, 5, 0, 0);
			int height = whipFrame.Height;

			float fadeOut = 1f;
			if (Projectile.ai[0] > flyTime * 0.4f && Projectile.ai[0] < flyTime * 0.9f)
				fadeOut *= 1f - (Projectile.ai[0] - flyTime * 0.4f) / (flyTime * 0.5f);
			else if (Projectile.ai[0] >= flyTime * 0.9f)
				fadeOut = 0f;

			for (int i = 15; i > 0; i--)
			{
				float fade = 1 - (15f - i) / 15f;

				if (i > 0 && i < tipPositions.Count)
				{
					whipFrame.Y = height * 4;
					var color = Color.Lerp(new Color(20, 135, 15, 0), new Color(100, 200, 10, 0), fade);
					Main.EntitySpriteDraw(texture.Value, tipPositions[i] - Main.screenPosition, whipFrame, Color.White * fade * fadeOut, tipRotations[i], whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

					Main.spriteBatch.Draw(texBlur, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut, tipRotations[i], texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, color * fade * fadeOut * 0.5f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

					Main.spriteBatch.Draw(bloomTex, tipPositions[i] - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut * 0.6f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);

					if (i < 15 && i + 1 < tipPositions.Count)
					{
						var newPosition = Vector2.Lerp(tipPositions[i], tipPositions[i + 1], 0.5f);
						float newRotation = MathHelper.Lerp(tipRotations[i], tipRotations[i + 1], 0.5f);

						Main.EntitySpriteDraw(texture.Value, newPosition - Main.screenPosition, whipFrame, Color.White * fade * fadeOut, newRotation, whipFrame.Size() * 0.5f, Projectile.scale, SpriteEffects.None, 0);

						Main.spriteBatch.Draw(texBlur, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut, newRotation, texBlur.Size() / 2f, Projectile.scale, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut, 0f, bloomTex.Size() / 2f, 1f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, color * fade * fadeOut * 0.5f, 0f, bloomTex.Size() / 2f, 1.5f * fade, 0f, 0f);

						Main.spriteBatch.Draw(bloomTex, newPosition - Main.screenPosition, null, Color.White with { A = 0 } * fade * fadeOut * 0.6f, 0f, bloomTex.Size() / 2f, 0.8f * fade, 0f, 0f);
					}
				}
			}
		}

		public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
		{
			hitTargets.Add(target);

			var points = new List<Vector2>();
			points.Clear();
			SetPoints(points);

			Vector2 tipPos = points[39];

			for (int i = 0; i < 5; i++)
			{
				Dust.NewDustPerfect(target.Center, ModContent.DustType<Dusts.GlowFastDecelerate>(), target.Center.DirectionTo(tipPos).RotatedByRandom(0.5f) * Main.rand.NextFloat(5f, 10f), 0, new Color(100, 200, 20), 0.5f);
			}
		}

		public override void Kill(int timeLeft)
		{
			if (hitTargets.Count >= 2)
				EchochainSystem.AddNPCS(hitTargets);

			hitTargets.Clear();
		}
	}

	/// <summary>
	/// This system acts as a handler for the echo chain damage graph
	/// </summary>
	class EchochainSystem : ModSystem
	{
		// These represent the graph for the echo chain's connection. Essentially
		// every NPC acts as a node, and are connected by edges which are created
		// when NPCs are struck by the weapon. Damage sharing is handled via a
		// graph traversal, and chain expiration happens as a simple iteration over
		// the edges collection and removing expired edges.
		public static List<EchochainNode> nodes = new();
		public static List<EchochainEdge> edges = new();

		public const int MAX_EDGE_TIMER = 300; // the maximum timer for each edge.

		public override void Load()
		{
			StarlightNPC.OnHitByItemEvent += OnHitChains;
			StarlightNPC.OnHitByProjectileEvent += OnHitChainsProj;

			On_Main.DrawNPCs += DrawEdges;
		}

		/// <summary>
		/// Calls the DrawEdge() method for all active edges, in the Main.DrawNPCs() hook
		/// </summary>
		/// <param name="orig"></param>
		/// <param name="self"></param>
		/// <param name="behindTiles"></param>
		/// <exception cref="NotImplementedException"></exception>
		private void DrawEdges(On_Main.orig_DrawNPCs orig, Main self, bool behindTiles)
		{
			foreach (EchochainEdge edge in edges)
			{
				edge.DrawEdge(Main.spriteBatch);
			}

			orig(self, behindTiles);
		}

		/// <summary>
		/// Triggers a traversal over the graph to damage all enemies connected to the hit enemy
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <param name="hit"></param>
		/// <param name="damageDone"></param>
		private void OnHitChains(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			ResetTimers(npc);

			if (!item.CountsAsClass(DamageClass.Summon))
				return;

			Traverse(npc, (n) => n.SimpleStrikeNPC((int)(damageDone * 0.25f), 0, false), true);
		}

		/// <summary>
		/// Triggers a traversal over the graph to damage all enemies connected to the hit enemy
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="projectile"></param>
		/// <param name="hit"></param>
		/// <param name="damageDone"></param>
		private void OnHitChainsProj(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			ResetTimers(npc);

			if (ProjectileID.Sets.IsAWhip[projectile.type] || !projectile.CountsAsClass(DamageClass.Summon)) // only minions should proc the chain hits
				return;

			Traverse(npc, (n) => n.SimpleStrikeNPC((int)(damageDone * 0.25f), 0, false), true);
		}

		public override void PostUpdateEverything()
		{
			List<EchochainEdge> edgesToRemove = new();

			foreach (EchochainEdge edge in edges)
			{
				edge.UpdateEdge();
				// removes ALL chains in which theyre timer has ran out, one of their npcs is inactive, one of their npcs is too far away from another, or somehow both of the edges npcs are the same.
				if (edge.timer <= 0 || !edge.end.npc.active || edge.end == null || !edge.start.npc.active || edge.start == null || Vector2.Distance(edge.start.npc.Center, edge.end.npc.Center) > 500f || edge.start == edge.end)
				{
					edgesToRemove.Add(edge);
				}
			}

			// this prevents "Collection was modified, enumeration may not execute"
			foreach (EchochainEdge edge in edgesToRemove)
			{
				edges.Remove(edge);
				edge.DestroyEdge();
			}
		}

		/// <summary>
		/// This function will create appropriate nodes and edges for NPCs when they are
		/// struck by the weapon. All NPCs passed not currently connected will have connections
		/// added to them.
		/// </summary>
		/// <param name="npcsToAdd"></param>
		public static void AddNPCS(List<NPC> npcsToAdd)
		{
			List<EchochainNode> list = new();

			for (int i = 0; i < npcsToAdd.Count; i++)
			{
				NPC npc = npcsToAdd[i];

				if (npc.active) // don't add dead npcs to the list silly!
				{
					// First we check if a node already exists for this NPC, and if not, add it
					EchochainNode node = nodes.FirstOrDefault(n => n.npc == npc);

					if (node == null)
					{
						node = new EchochainNode(new List<EchochainEdge>(), npc);
						nodes.Add(node);
					}

					list.Add(node);
				}
			}

			// Now we check all unordered permutations of 2 NPCs, and generate chains between
			// them if they do not already exist
			for (int i = 0; i < list.Count; i++)
			{
				for (int j = i; j < list.Count; j++)
				{
					// This is likely very slightly inefficient over performing the equality
					// check here, but this is sparsely run and this is far more readable

					int[] frames = new int[24];
					for (int a = 0; a < frames.Length; a++) // populate array
					{
						frames[a] = Main.rand.Next(3);
					}

					var potential = new EchochainEdge(list[i], list[j], MAX_EDGE_TIMER, frames);

					if (!edges.Any(n => n.Equals(potential)) && list[i] != list[j]) // check if it already exists, and if we are trying to add an edge that consists of the same npc, twice
					{
						edges.Add(potential);

						// We append the refferences to this edge to the nodes it is connecting aswell
						nodes.First(n => n == list[i]).edges.Add(potential);
						nodes.First(n => n == list[j]).edges.Add(potential);

						for (int d = 0; d < 20; d++)
						{
							Dust.NewDustPerfect(Vector2.Lerp(list[i].npc.Center, list[j].npc.Center, d / 20f), ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(3f, 3f), 0, new Color(100, 200, 20), 1f);
						}
					}
				}
			}
		}

		/// <summary>
		/// Triggers a DFS traversal of the graph starting with the node containing the given
		/// NPC, and performing the given action on any NPC connected.
		/// </summary>
		/// <param name="start">The NPC where the traversal should start from. If they are not present in the node colleciton, nothing happens</param>
		/// <param name="action">The action to be performed on every connected NPC</param>
		/// <param name="ignoreFirst">If the NPC passed should ignore the action passed or not</param>
		public static void Traverse(NPC start, Action<NPC> action, bool ignoreFirst = false)
		{
			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == start);

			// If the start isnt in the graph, give up
			if (startNode is null)
				return;

			Stack<EchochainNode> stack = new();
			HashSet<EchochainNode> visited = new();

			stack.Push(startNode);

			while (stack.Count > 0)
			{
				EchochainNode node = stack.Pop();

				if (!visited.Contains(node))
				{
					if (!(ignoreFirst && node == startNode))
						action(node.npc);

					foreach (EchochainEdge edge in node.edges)
					{
						EchochainNode adjNode = edge.start == node ? edge.end : edge.start;

						if (!visited.Contains(adjNode))
							stack.Push(adjNode);
					}

					visited.Add(node);
				}
			}
		}

		/// <summary>
		/// Helper Method which performs a DFS traversal of the graph, starting at start.
		/// </summary>
		/// <param name="start">The NPC where the traversal should start from. If they are not present in the node collection, nothing happens</param>
		public static void ResetTimers(NPC start)
		{
			EchochainNode startNode = nodes.FirstOrDefault(n => n.npc == start);

			// If the start isnt in the graph, give up
			if (startNode is null)
				return;

			Stack<EchochainNode> stack = new();
			HashSet<EchochainNode> visited = new();

			stack.Push(startNode);

			while (stack.Count > 0)
			{
				EchochainNode node = stack.Pop();

				if (!visited.Contains(node))
				{
					foreach (EchochainEdge edge in node.edges)
					{
						edge.timer = MAX_EDGE_TIMER; // reset the timer

						EchochainNode adjNode = edge.start == node ? edge.end : edge.start;

						if (!visited.Contains(adjNode))
							stack.Push(adjNode);
					}

					visited.Add(node);
				}
			}
		}

		/// <summary>
		/// Removes an NPC from the node list, and disconnects all edges connected to it.
		/// </summary>
		/// <param name="toRemove">The NPC to be removed</param>
		public static void RemoveNPC(NPC toRemove)
		{
			nodes.RemoveAll(n => n.npc == toRemove);
			edges.RemoveAll(n => n.start.npc == toRemove || n.end.npc == toRemove);
		}
	}

	/// <summary>
	/// Represents an edge in the echo chain graph, essentially a chain between two NPCs
	/// </summary>
	class EchochainEdge
	{
		public EchochainNode start;
		public EchochainNode end;
		public int timer;

		public int[] chainFrames; // frames for the chain link are randomized upon creation

		internal List<Vector2> cache;
		internal Trail trail;
		public EchochainEdge(EchochainNode start, EchochainNode end, int timer, int[] chainFrames)
		{
			this.start = start;
			this.end = end;
			this.timer = timer;
			this.chainFrames = chainFrames;
		}

		public void UpdateEdge()
		{
			timer--;
			if (!Main.dedServ)
			{
				ManageCaches();
				ManageTrail();
			}
		}

		public void DestroyEdge()
		{
			start.edges.Remove(this);
			end.edges.Remove(this);
		}

		public void DrawEdge(SpriteBatch spriteBatch)
		{
			if (!start.npc.active || !end.npc.active || start.npc == null || end.npc == null)
				return;

			DrawPrimitives(spriteBatch);

			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain").Value;
			Texture2D texGlow = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Glow").Value;
			Texture2D texBlur = ModContent.Request<Texture2D>(AssetDirectory.HauntedItem + "EchochainWhipChain_Blur").Value;
			Texture2D bloomTex = ModContent.Request<Texture2D>(AssetDirectory.Keys + "GlowAlpha").Value;
			Vector2 chainStart = start.npc.Center;
			Vector2 chainEnd = end.npc.Center;

			float rotation = chainStart.DirectionTo(chainEnd).ToRotation() + MathHelper.PiOver2;

			float distance = Vector2.Distance(chainStart, chainEnd);

			for (int i = 0; i < (distance / 22); i += 1)
			{
				var pos = Vector2.Lerp(chainStart, chainEnd, i * 22 / distance);

				spriteBatch.Draw(bloomTex, pos - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.2f, 0f, bloomTex.Size() / 2f, 0.5f, 0f, 0f);

				Rectangle frame = tex.Frame(verticalFrames: 5, frameY: chainFrames[i]);	
				spriteBatch.Draw(tex, pos - Main.screenPosition, frame, Color.White, rotation, frame.Size() / 2f, 1f, 0f, 0f);

				frame = texBlur.Frame(verticalFrames: 5, frameY: chainFrames[i]);
				spriteBatch.Draw(texBlur, pos - Main.screenPosition, frame, Color.White with { A = 0 } * 0.5f, rotation, frame.Size() / 2f, 1f, 0f, 0f);
			}

			spriteBatch.Draw(bloomTex, chainStart - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.15f, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);
			spriteBatch.Draw(bloomTex, chainEnd - Main.screenPosition, null, new Color(100, 200, 10, 0) * 0.15f, 0f, bloomTex.Size() / 2f, 0.45f, 0f, 0f);
		}

		#region Primitive Drawing
		private void ManageCaches()
		{
			cache = new List<Vector2>();

			for (int i = 0; i < 10; i++)
			{
				cache.Add(Vector2.Lerp(start.npc.Center, end.npc.Center, i / 10f));
			}
		}

		private void ManageTrail()
		{
			trail ??= new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(0), factor => 12.5f, factor =>
			{
				if (factor.X >= 0.85f)
					return Color.Transparent;

				return new Color(100, 200, 10) * 0.3f * factor.X;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = cache[9];
		}
		public void DrawPrimitives(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

			var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
			Matrix view = Main.GameViewMatrix.ZoomMatrix;
			var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.025f);
			effect.Parameters["repeats"].SetValue(1f);
			effect.Parameters["transformMatrix"].SetValue(world * view * projection);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/FireTrail").Value);

			trail?.Render(effect);

			effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
			effect.Parameters["repeats"].SetValue(2f);
			effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

			trail?.Render(effect);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
		}
		#endregion Primitive Drawing


		/// <summary>
		/// Checks the equality of two edges. Since this graph is undirected, if the order is inversed
		/// it should still count as equal to that edge.
		/// </summary>
		/// <param name="obj">The other edge to compare to</param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj is EchochainEdge edge)
			{
				return
					start == edge.start && end == edge.end ||
					start == edge.end && end == edge.start;
			}

			return false;	
		}
	}

	/// <summary>
	/// Represents a node in the echo chain graph, essentially a wrapper around an NPC
	/// </summary>
	class EchochainNode
	{
		public List<EchochainEdge> edges;
		public NPC npc;

		public EchochainNode(List<EchochainEdge> edges, NPC npc)
		{
			this.edges = edges;
			this.npc = npc;
		}
	}
}