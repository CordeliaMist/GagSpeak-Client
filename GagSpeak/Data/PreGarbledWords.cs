using System.Collections.Generic;
using System;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace GagSpeak.Data {
    public static class PreGarbledWords
    {
        /// <summary>
        /// Embedded dictionary of pre-garbled words for the nancy drew class
        /// </summary>
        public static Dictionary<string, string> NancyPremadeWords { get; } =  new(StringComparer.OrdinalIgnoreCase) {
            { "stop", "stah" },
            { "do", "oo" },
            { "its", "eth hs" },
            { "it's", "eth hs" },
            { "the", "thh" },
            { "let", "leh" },
            { "and", "'n" },
            { "i", "eh" },
            { "a", "ah" },
            { "of", "oph" },
            { "it", "eth" },
            { "that", "thah"},
            { "gag", "ghah"},
        };

        /// <summary>
        /// Embedded dictionary of pre-garbled words for the gwendoline class
        /// </summary>
        public static Dictionary<string, string> GwendolinePremadeWords { get; } = new(StringComparer.OrdinalIgnoreCase) {
            { "test", "megh"}
        };

        /// <summary>
        /// Embedded dictionary of pre-garbled words for the gimp drew class
        /// </summary>
        public static Dictionary<string, string> GimpPremadeWords { get; } = new(StringComparer.OrdinalIgnoreCase) {
            { "test", "megh"}
        };
    }

}