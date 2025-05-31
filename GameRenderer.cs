using Silk.NET.SDL;
using Silk.NET.Maths;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using TheAdventure.Models;
using Point = Silk.NET.SDL.Point;

namespace TheAdventure;

public unsafe class GameRenderer
{
    private readonly Sdl _sdl;
    private readonly Renderer* _renderer;
    private readonly GameWindow _window;
    private readonly Camera _camera;

    private readonly Dictionary<int, IntPtr> _texturePointers = new();
    private readonly Dictionary<int, TextureData> _textureData = new();
    private int _textureId;

    public GameRenderer(Sdl sdl, GameWindow window)
    {
        _sdl = sdl;
        _window = window;

        _renderer = (Renderer*)_window.CreateRenderer();
        _sdl.SetRenderDrawBlendMode(_renderer, BlendMode.Blend);

        var windowSize = _window.Size;
        _camera = new Camera(windowSize.Width, windowSize.Height);
    }

    public void SetWorldBounds(Rectangle<int> bounds)
    {
        _camera.SetWorldBounds(bounds);
    }

    public void CameraLookAt(int x, int y)
    {
        _camera.LookAt(x, y);
    }

    public int LoadTexture(string fileName, out TextureData textureInfo)
    {
        using var fStream = new FileStream(fileName, FileMode.Open);
        var image = Image.Load<Rgba32>(fStream);
        textureInfo = new TextureData
        {
            Width = image.Width,
            Height = image.Height
        };

        var rawData = new byte[textureInfo.Width * textureInfo.Height * 4];
        image.CopyPixelDataTo(rawData.AsSpan());

        fixed (byte* data = rawData)
        {
            var surface = _sdl.CreateRGBSurfaceWithFormatFrom(
                data, textureInfo.Width, textureInfo.Height,
                32, textureInfo.Width * 4, (uint)PixelFormatEnum.Rgba32);

            if (surface == null)
                throw new Exception("Failed to create surface.");

            var texture = _sdl.CreateTextureFromSurface(_renderer, surface);
            if (texture == null)
            {
                _sdl.FreeSurface(surface);
                throw new Exception("Failed to create texture.");
            }

            _sdl.FreeSurface(surface);
            _textureData[_textureId] = textureInfo;
            _texturePointers[_textureId] = (IntPtr)texture;
        }

        return _textureId++;
    }

    public void RenderTexture(int textureId, Rectangle<int> src, Rectangle<int> dst,
        RendererFlip flip = RendererFlip.None, double angle = 0.0, Point center = default)
    {
        if (_texturePointers.TryGetValue(textureId, out var texture))
        {
            var screenDst = _camera.ToScreenCoordinates(dst);
            _sdl.RenderCopyEx(_renderer, (Texture*)texture, in src, in screenDst, angle, in center, flip);
        }
    }

    public Vector2D<int> ToWorldCoordinates(int x, int y)
    {
        return _camera.ToWorldCoordinates(new Vector2D<int>(x, y));
    }

    public void SetDrawColor(byte r, byte g, byte b, byte a)
    {
        _sdl.SetRenderDrawColor(_renderer, r, g, b, a);
    }

    public void ClearScreen()
    {
        _sdl.RenderClear(_renderer);
    }

    public void PresentFrame()
    {
        _sdl.RenderPresent(_renderer);
    }

    // ✅ Used by MiniMapRenderer
    public Sdl GetSdl()
    {
        return _sdl;
    }

    public Renderer* GetRenderer()
    {
        return _renderer;
    }

    public Vector2D<int> GetWindowSize()
    {
        var size = _window.Size;
        return new Vector2D<int>(size.Width, size.Height);
    }

    public void DrawPixel(int x, int y, byte r, byte g, byte b, byte a = 255)
    {
        SetDrawColor(r, g, b, a);
        _sdl.RenderDrawPoint(_renderer, x, y);
    }
}
