using Silk.NET.SDL;
using Silk.NET.Maths;
using System.Collections.Generic;
using TheAdventure.Models;

namespace TheAdventure;

public static unsafe class MiniMapRenderer
{
    private const int MiniMapWidth = 150;
    private const int MiniMapHeight = 150;
    private const int Margin = 10;

    public static void Render(
        Sdl sdl,
        Renderer* renderer,
        List<GameObject> gameObjects,
        GameObject player,
        int windowWidth,
        int windowHeight,
        int worldWidth,
        int worldHeight)
    {
        if (gameObjects == null || gameObjects.Count == 0 || player == null)
            return;

        int mapX = windowWidth - MiniMapWidth - Margin;
        int mapY = Margin;

        var backgroundRect = new Rectangle<int>(mapX, mapY, MiniMapWidth, MiniMapHeight);
        sdl.SetRenderDrawColor(renderer, 64, 64, 64, 255);
        sdl.RenderFillRect(renderer, &backgroundRect);

        foreach (var obj in gameObjects)
        {
            if (obj is not RenderableGameObject renderable) continue;

            var pos = renderable.Position;

            float normX = pos.X / (float)worldWidth;
            float normY = pos.Y / (float)worldHeight;

            normX = Math.Clamp(normX, 0f, 1f);
            normY = Math.Clamp(normY, 0f, 1f);

            int px = mapX + (int)(normX * MiniMapWidth);
            int py = mapY + (int)(normY * MiniMapHeight);

            var (r, g, b) = GetColor(obj, player);
            sdl.SetRenderDrawColor(renderer, r, g, b, 255);
            sdl.RenderDrawPoint(renderer, px, py);
        }
    }

    private static (byte R, byte G, byte B) GetColor(GameObject obj, GameObject player)
    {
        if (obj == player) return (0, 255, 0); // Green for player

        string type = obj.GetType().Name.ToLower();

        if (type.Contains("enemy")) return (255, 0, 0); // Red
        if (type.Contains("npc")) return (0, 0, 255);   // Blue
        if (obj is TemporaryGameObject) return (255, 255, 0); // Yellow

        return (255, 255, 255); // White fallback
    }
}
