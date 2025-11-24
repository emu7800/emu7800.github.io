// © Mike Murphy

namespace EMU7800.Assets;

using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public static class AssetService
{
    static readonly Dictionary<Asset, string> _assetToFilenameMapping = new()
    {
        { Asset.appbar_transport_pause_rest,                "images.appbar.transport.pause.rest.png" },
        { Asset.appbar_transport_pause_rest_inverted,       "images.appbar.transport.pause.rest_inverted.png" },
        { Asset.appbar_transport_play_rest,                 "images.appbar.transport.play.rest.png" },
        { Asset.appbar_transport_play_rest_inverted,        "images.appbar.transport.play.rest_inverted.png" },
        { Asset.appbar_transport_playdown_rest,             "images.appbar.transport.playdown.rest.png" },
        { Asset.appbar_transport_playdown_rest_inverted,    "images.appbar.transport.playdown.rest_inverted.png" },
        { Asset.appbar_transport_playup_rest,               "images.appbar.transport.playup.rest.png" },
        { Asset.appbar_transport_playup_rest_inverted,      "images.appbar.transport.playup.rest_inverted.png" },
        { Asset.appbar_transport_playleft_rest,             "images.appbar.transport.playleft.rest.png" },
        { Asset.appbar_transport_playleft_rest_inverted,    "images.appbar.transport.playleft.rest_inverted.png" },
        { Asset.appbar_back_rest,                           "images.appbar.back.rest.png" },
        { Asset.appbar_back_rest_inverted,                  "images.appbar.back.rest_inverted.png" },
        { Asset.appbar_basecircle_rest,                     "images.appbar.basecircle.rest.png" },
        { Asset.appbar_basecircle_rest_inverted,            "images.appbar.basecircle.rest_inverted.png" },
        { Asset.appbar_cancel_rest,                         "images.appbar.cancel.rest.png" },
        { Asset.appbar_cancel_rest_inverted,                "images.appbar.cancel.rest_inverted.png" },
        { Asset.appbar_check_rest,                          "images.appbar.check.rest.png" },
        { Asset.appbar_check_rest_inverted,                 "images.appbar.check.rest_inverted.png" },
        { Asset.appbar_feature_search_rest,                 "images.appbar.feature.search.rest.png" },
        { Asset.appbar_feature_search_rest_inverted,        "images.appbar.feature.search.rest_inverted.png" },
        { Asset.appbar_feature_settings_rest,               "images.appbar.feature.settings.rest.png" },
        { Asset.appbar_feature_settings_rest_inverted,      "images.appbar.feature.settings.rest_inverted.png" },
        { Asset.appbar_next_rest,                           "images.appbar.next.rest.png" },
        { Asset.appbar_next_rest_inverted,                  "images.appbar.next.rest_inverted.png" },
        { Asset.appbar_questionmark_rest,                   "images.appbar.questionmark.rest.png" },
        { Asset.appbar_questionmark_rest_inverted,          "images.appbar.questionmark.rest_inverted.png" },
        { Asset.appbar_add_rest,                            "images.appbar.add.rest.png" },
        { Asset.appbar_add_rest_inverted,                   "images.appbar.add.rest_inverted.png" },
        { Asset.appbar_minus_rest,                          "images.appbar.minus.rest.png" },
        { Asset.appbar_minus_rest_inverted,                 "images.appbar.minus.rest_inverted.png" },
        { Asset.appicon_128x128,                            "images.appicon_128x128.png" },
        { Asset.about,                                      "about.txt" },
        { Asset.ROMProperties,                              "ROMProperties.csv" }
    };

    public static async Task<ReadOnlyMemory<byte>> GetAssetBytesAsync(Asset asset)
    {
        await using var assetStream = GetAssetStream(asset);
        using var output = new MemoryStream();
        await assetStream.CopyToAsync(output);
        return output.ToArray();
    }

    public static IEnumerable<string> GetAssetByLines(Asset asset)
    {
        using var assetStream = GetAssetStream(asset);
        using var reader = new StreamReader(assetStream);
        while (!reader.EndOfStream)
        {
            yield return reader.ReadLine() ?? string.Empty;
        }
    }

    static Stream GetAssetStream(Asset asset)
    {
        var thisAssembly = System.Reflection.Assembly.GetAssembly(typeof(AssetService));
        var thisAssemblyName = thisAssembly?.GetName();
        var path = $"{thisAssemblyName?.Name}.{_assetToFilenameMapping[asset]}";
        return thisAssembly?.GetManifestResourceStream(path) ?? Stream.Null;
    }
}