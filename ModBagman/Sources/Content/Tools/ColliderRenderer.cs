using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace ModBagman;

/// <summary>
/// Renders colliders in a level.
/// </summary>
public class ColliderRenderer : RenderComponent
{
    /// <summary>
    /// Should combat colliders be rendered?
    /// </summary>
    public bool RenderCombat { get; set; }

    /// <summary>
    /// Should level colliders be rendered?
    /// </summary>
    public bool RenderLevel { get; set; }

    /// <summary>
    /// Should movement colliders be rendered?
    /// </summary>
    public bool RenderMovement { get; set; }

    /// <summary>
    /// Creates a new collider renderer.
    /// </summary>
    public ColliderRenderer()
    {
        xTransform = new TransformComponent(Vector2.Zero);
    }

    /// <summary>
    /// Renders the colliders in the level based on current settings.
    /// </summary>
    /// <param name="spriteBatch">Spritebatch to use for rendering</param>
    public override void Render(SpriteBatch spriteBatch)
    {
        PlayerView localPlayer = Globals.Game.xLocalPlayer;
        CollisionMaster colliders = Globals.Game.xCollisionMaster;

        if (!localPlayer.bInitializedToServer || Globals.Game.xLevelMaster.xZoningHelper.IsZoning)
            return;

        if (RenderLevel)
        {
            foreach (Collider col in colliders.lxStaticColliders)
            {
                int iNine = 512;
                if ((iNine & col.ibitLayers) == 0 && (col.ibitLayers & localPlayer.xEntity.xCollisionComponent.ibitCurrentColliderLayer) != 0)
                {
                    col.Render(spriteBatch);
                }
            }
        }

        if (RenderCombat)
        {
            foreach (Collider col in colliders.lxAttackboxColliders)
            {
                col.Render(spriteBatch);
            }

            foreach (var pair in colliders.dexHitboxColliders)
            {
                if (pair.Key == Collider.ColliderLayers.HeighDifferenceIntercept)
                    continue;

                foreach (Collider col in pair.Value)
                {
                    col.Render(spriteBatch);
                }
            }
        }

        if (RenderMovement)
        {
            foreach (Collider col in colliders.lxMovementColliders)
            {
                col.Render(spriteBatch);
            }
        }
    }
}
