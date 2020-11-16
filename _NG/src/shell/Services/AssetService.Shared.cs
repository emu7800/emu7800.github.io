// © Mike Murphy

using System.Collections.Generic;
using EMU7800.Services.Dto;

namespace EMU7800.Services
{
    public partial class AssetService
    {
        static readonly object _locker = new object();
        static readonly IDictionary<Asset, byte[]> _resourceCache = new Dictionary<Asset, byte[]>();
        static readonly IDictionary<Asset, string> _assetToFilenameMapping = new Dictionary<Asset, string>
        {
            { Asset.appbar_transport_pause_rest,                "appbar.transport.pause.rest.png" },
            { Asset.appbar_transport_pause_rest_inverted,       "appbar.transport.pause.rest_inverted.png" },
            { Asset.appbar_transport_play_rest,                 "appbar.transport.play.rest.png" },
            { Asset.appbar_transport_play_rest_inverted,        "appbar.transport.play.rest_inverted.png" },
            { Asset.appbar_transport_playdown_rest,             "appbar.transport.playdown.rest.png" },
            { Asset.appbar_transport_playdown_rest_inverted,    "appbar.transport.playdown.rest_inverted.png" },
            { Asset.appbar_transport_playup_rest,               "appbar.transport.playup.rest.png" },
            { Asset.appbar_transport_playup_rest_inverted,      "appbar.transport.playup.rest_inverted.png" },
            { Asset.appbar_transport_playleft_rest,             "appbar.transport.playleft.rest.png" },
            { Asset.appbar_transport_playleft_rest_inverted,    "appbar.transport.playleft.rest_inverted.png" },
            { Asset.appbar_back_rest,                           "appbar.back.rest.png" },
            { Asset.appbar_back_rest_inverted,                  "appbar.back.rest_inverted.png" },
            { Asset.appbar_basecircle_rest,                     "appbar.basecircle.rest.png" },
            { Asset.appbar_basecircle_rest_inverted,            "appbar.basecircle.rest_inverted.png" },
            { Asset.appbar_cancel_rest,                         "appbar.cancel.rest.png" },
            { Asset.appbar_cancel_rest_inverted,                "appbar.cancel.rest_inverted.png" },
            { Asset.appbar_check_rest,                          "appbar.check.rest.png" },
            { Asset.appbar_check_rest_inverted,                 "appbar.check.rest_inverted.png" },
            { Asset.appbar_feature_search_rest,                 "appbar.feature.search.rest.png" },
            { Asset.appbar_feature_search_rest_inverted,        "appbar.feature.search.rest_inverted.png" },
            { Asset.appbar_feature_settings_rest,               "appbar.feature.settings.rest.png" },
            { Asset.appbar_feature_settings_rest_inverted,      "appbar.feature.settings.rest_inverted.png" },
            { Asset.appbar_next_rest,                           "appbar.next.rest.png" },
            { Asset.appbar_next_rest_inverted,                  "appbar.next.rest_inverted.png" },
            { Asset.appbar_questionmark_rest,                   "appbar.questionmark.rest.png" },
            { Asset.appbar_questionmark_rest_inverted,          "appbar.questionmark.rest_inverted.png" },
            { Asset.appbar_add_rest,                            "appbar.add.rest.png" },
            { Asset.appbar_add_rest_inverted,                   "appbar.add.rest_inverted.png" },
            { Asset.appbar_minus_rest,                          "appbar.minus.rest.png" },
            { Asset.appbar_minus_rest_inverted,                 "appbar.minus.rest_inverted.png" },
            { Asset.appicon_128x128,                            "appicon_128x128.png" },
            { Asset.about,                                      "about.txt" },
            { Asset.romimport,                                  "romimport.txt" }
        };
    }
}