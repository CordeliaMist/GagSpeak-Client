using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Dalamud;
using Dalamud.Interface.Internal;
using ImGuiNET;
using Lumina.Data.Files;
using Dalamud.Game.Config;
using Dalamud.Utility;
using Action = Lumina.Excel.GeneratedSheets.Action;
using Dalamud.Plugin.Services;
using Dalamud.Interface;
using OtterGui.Services;

namespace GagSpeak.Utility;
public class IconManager : IDisposable
{
    private readonly IDataManager _data;
    private readonly UiBuilder _uiBuilder;
    private readonly ITextureProvider _textureProvider;
    private readonly IGameConfig _gameConfig;
    private bool disposed;
    private readonly Dictionary<(int, bool), IDalamudTextureWrap> iconTextures = new();
    private readonly Dictionary<uint, ushort> actionCustomIcons = new() {};

    public GraphicFont? FontIcons { get; private set; }

    public class GraphicFont : IDisposable {
        private readonly ITextureProvider _textureProvider;
        private readonly IGameConfig _gameConfig;
        public string Identifier = "gftd";
        public string Version = "0100";
        public GraphicFontIcon[] Icons = Array.Empty<GraphicFontIcon>();
        
        private static IDalamudTextureWrap? _textureWrap;
        private static uint _padIconMode;
        private static string[] _texturePath = {
            "common/font/fonticon_xinput.tex",
            "common/font/fonticon_ps3.tex",
            "common/font/fonticon_ps4.tex",
            "common/font/fonticon_ps5.tex"
        };
        
        public class GraphicFontIcon {
            private readonly ITextureProvider _textureProvider;
            private readonly IGameConfig _gameConfig;
            public ushort ID;
            public Vector2 Position;
            public Vector2 Size;
            public byte[] Remainder;
        
            public GraphicFontIcon(BinaryReader reader, ITextureProvider textureProvider, IGameConfig gameConfig) {
                _gameConfig = gameConfig;
                _textureProvider = textureProvider;
                ID = reader.ReadUInt16();
                Position = new Vector2(reader.ReadUInt16(), reader.ReadUInt16());
                Size = new Vector2(reader.ReadUInt16(), reader.ReadUInt16());
                Remainder = reader.ReadBytes(6);
            }

            public bool IsValid() {
                return Size is { X: >= 1, Y: >= 1 };
            }



            public void DrawScaled(Vector2 scale, bool highQuality = true) {
                Draw(Size * (highQuality ? 2 : 1) * scale, highQuality);
            }
            
            public void Draw(bool highQuality) => Draw(null, highQuality);
            public void Draw(Vector2? drawSize = null, bool highQuality = true) {
                if (!IsValid()) return;
                var scale = new Vector2(highQuality ? 2 : 1);
                var size = Size * scale;
                if (_textureWrap == null) {
                    if (_gameConfig.TryGet(SystemConfigOption.PadSelectButtonIcon, out _padIconMode) && _padIconMode < _texturePath.Length) {
                        _textureWrap = _textureProvider.GetTextureFromGame(_texturePath[_padIconMode]);
                    }
                    ImGui.Dummy(drawSize ?? size);
                    return;
                }

                var textureSize = new Vector2(_textureWrap.Width, _textureWrap.Height);
                var basePosition = new Vector2(0, highQuality ? 1/3f : 0);
                
                var uv0 = basePosition + (Position * scale) / textureSize;
                var uv1 = uv0 + size / textureSize;

                ImGui.Image(_textureWrap.ImGuiHandle, drawSize ?? size, uv0, uv1);
            }
        }
        
        public GraphicFont(ITextureProvider textureProvider, IGameConfig gameConfig) {
            _textureProvider = textureProvider;
            _gameConfig = gameConfig;
            
        }
        
        public GraphicFont(byte[] data, ITextureProvider textureProvider, IGameConfig gameConfig) {
            _textureProvider = textureProvider;
            _gameConfig = gameConfig;
            if (data.Length < 16) {
                throw new Exception("Missing Header");
            }
            using var mStream = new MemoryStream(data);
            using var reader = new BinaryReader(mStream);

            Identifier = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (Identifier != "gftd") {
                throw new Exception("Not a GFTD");
            }
            Version = Encoding.ASCII.GetString(reader.ReadBytes(4));
            if (Version != "0100") {
                throw new Exception($"GFTD Version {Version} not supported.");
            }

            Icons = new GraphicFontIcon[reader.ReadInt32()];
            reader.ReadUInt32();

            if (data.Length != 16 * (Icons.Length + 1)) {
                throw new Exception("incorrect data size");
            }

            for (var i = 0; i < Icons.Length; i++) {
                Icons[i] = new GraphicFontIcon(reader, _textureProvider, _gameConfig);
            }
        }

        public void Dispose() {
            _textureWrap?.Dispose();
            _textureWrap = null;
            Icons = Array.Empty<GraphicFontIcon>();
        }
    }
    
    
    public IconManager(IDataManager data, UiBuilder uiBuilder, ITextureProvider textureProvider, IGameConfig gameConfig) {
        _data = data;
        _uiBuilder = uiBuilder;
        _textureProvider = textureProvider;
        _gameConfig = gameConfig;

        var gfd = _data.GetFile("common/font/gfdata.gfd");
        if (gfd != null) {
            FontIcons = new GraphicFont(gfd.Data, textureProvider, gameConfig);
        }
    }

    public void Dispose() {
        disposed = true;
        var c = 0;
        GSLogger.LogType.Debug("Disposing icon textures");
        foreach (var texture in iconTextures.Values.Where(texture => texture != null)) {
            c++;
            texture.Dispose();
        }

        GSLogger.LogType.Debug($"Disposed {c} icon textures.");
        iconTextures.Clear();
        
        FontIcons?.Dispose();
        
    }
        
    private void LoadIconTexture(int iconId, bool hq = false) {
        Task.Run(() => {
            try {
                var iconTex = GetIcon(iconId, hq);

                var tex = _uiBuilder.LoadImageRaw(iconTex.GetRgbaImageData(), iconTex.Header.Width, iconTex.Header.Height, 4);

                if (tex.ImGuiHandle != IntPtr.Zero) {
                    this.iconTextures[(iconId, hq)] = tex;
                } else {
                    tex.Dispose();
                }
            } catch (Exception ex) {
                GSLogger.LogType.Error($"Failed loading texture for icon {iconId} - {ex.Message}");
            }
        });
    }
        
    public TexFile GetIcon(int iconId, bool hq = false) => this.GetIcon(_data.Language, iconId, hq);

    /// <summary>
    /// Get a <see cref="T:Lumina.Data.Files.TexFile" /> containing the icon with the given ID, of the given language.
    /// </summary>
    /// <param name="iconLanguage">The requested language.</param>
    /// <param name="iconId">The icon ID.</param>
    /// <returns>The <see cref="T:Lumina.Data.Files.TexFile" /> containing the icon.</returns>
    public TexFile GetIcon(ClientLanguage iconLanguage, int iconId, bool hq = false)
    {
        string type;
        switch (iconLanguage)
        {
            case ClientLanguage.Japanese:
                type = "ja/";
                break;
            case ClientLanguage.English:
                type = "en/";
                break;
            case ClientLanguage.German:
                type = "de/";
                break;
            case ClientLanguage.French:
                type = "fr/";
                break;
            default:
                throw new ArgumentOutOfRangeException("Language", "Unknown Language: " + _data.Language.ToString());
        }
        return this.GetIcon(type, iconId, hq);
    }
        
    public TexFile GetIcon(string type, int iconId, bool hq = false)
    {
        if (type == null)
            type = string.Empty;
        if (type.Length > 0 && !type.EndsWith("/"))
            type += "/";
            
        var formatStr = $"ui/icon/{{0:D3}}000/{(hq?"hq/":"")}{{1}}{{2:D6}}.tex";
        // ignore the possible null reference remarks
        #pragma warning disable CS8600, CS8603

        TexFile file = _data.GetFile<TexFile>(string.Format(formatStr, (object) (iconId / 1000), (object) type, (object) iconId));
        return file != null || type.Length <= 0 ? file : _data.GetFile<TexFile>(string.Format(formatStr, (object) (iconId / 1000), (object) string.Empty, (object) iconId));

        #pragma warning restore CS8600, CS8603
    }
        

    public IDalamudTextureWrap GetActionIcon(Action action) {
        return GetIconTexture(actionCustomIcons.ContainsKey(action.RowId) ? actionCustomIcons[action.RowId] : action.Icon);
    }

    public ushort GetActionIconId(Action action) {
        return actionCustomIcons.ContainsKey(action.RowId) ? actionCustomIcons[action.RowId] : action.Icon;
    }

    public IDalamudTextureWrap GetIconTexture(int iconId, bool hq = false) {
        if (this.disposed) return null!;
        if (this.iconTextures.ContainsKey((iconId, hq))) return this.iconTextures[(iconId, hq)];
        this.iconTextures.Add((iconId, hq), null!);
        try{
        LoadIconTexture(iconId, hq);
        } catch (Exception ex) {
            GSLogger.LogType.Error($"Failed loading texture for icon {iconId} - {ex.Message}");
        }
        return this.iconTextures[(iconId, hq)];
    }
}