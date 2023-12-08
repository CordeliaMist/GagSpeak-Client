using System.Collections.Generic;
using System;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace GagSpeak.Data {

    public class Translator
    {
        public string Phoneme { get; set; }
        public string NancyDrew { get; set; }
        public string SweetGwen { get; set; }
        public string Gimp { get; set; }

        public Translator(string phoneme, string nancyDrew = "m", string sweetGwen = "m", string gimp = "m")
        {
            Phoneme = phoneme;
            NancyDrew = nancyDrew;
            SweetGwen = sweetGwen;
            Gimp = gimp;
        }

    }

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

        /// <summary>
        /// The list of all the phonemes that can be garbled
        /// </summary>
        public static List<Translator> MasterList = new List<Translator>(
        {
            { "æ", } // Ash
            { "ɐ", } // upside down a
            { "ɑ", } // open a
            { "ɒ", } // upsode down a (rounded a)
            { "α", } // alpha
            { "β", } // Beta
            { "ɓ", } // implosive b
            { "ç", } // c cedille
            { "č", } // c hachek
            { "ɕ", } // curled c (palatal fricative)
            { "ð", } // Eth
            { "ɖ", } // Retroflex d (IPA)
            { "ḍ", } // retroflex d (d dot)
            { "ɗ", } // implosive d
            { "ə", } // schwa
            { "ε", } // Greek epsilon
            { "ɛ", } // open e (IPA epsilon)
            { "ẹ", } // e dot
            { "ɜ", } // backwards epsilon
            { "ɚ", } // rhotic schwa
            { "ɘ", } // backwards e
            { "φ", } // Greek phi
            { "ɸ", } // Unicode IPA phi
            { "ɟ", } // upside-down f (voiced palatal stop)
            { "γ", } // Greek gamma
            { "ɣ", } // Unicode IPA gamma
            { "ɠ", } // implosive g
            { "ħ", } // barred h
            { "ɦ", } // curly top h
            { "h", } // superscript h
            { "ʰ", } // Unicode superscript h
            { "ɥ", } // upside down h (front-round glide)
            { "ḥ", } // h-dot
            { "ɧ", } // /sh-x/
            { "ɪ", } // Unicode lax I
            { "i", } // i barred
            { "ɨ", } // Unicode i barred
            { "ǰ", } // j hacheck
            { "ʝ", } // curled j (palatal fricative)
            { "ʲ", } // Unicode superscript j
            { "ɟ", } // crossed j (palatal stop)
            { "ɫ", } // velarized l
            { "l", } // l strike
            { "λ", } // Lambda
            { "ʎ", } // palatal l
            { "ɭ", } // IPA retroflex l
            { "ḷ", } // Retroflex l (l dot)
            { "ɬ", } // Welsh voiceless lateral fricative
            { "ɮ", } // Zulu voiced lateral fricative
            { "ɱ", } // labiodental nasal
            { "ɯ", } // upside down m (back unrounded vowel)
            { "ɰ", } // upside down m with tail (back unrounded glide)
            { "ñ", } // n-tilde
            { "ŋ", } // engma
            { "ɲ", } // IPA Palatal N
            { "ɳ", } // IPA Retroflex n
            { "ṇ", } // Retroflex n (n dot)
            { "ø", } // e -slash (front-rounded vowel)
            { "œ", } // o-e ligature (entity code)
            { "ö", } // umlaut
            { "ɔ", } // Open o
            { "ọ", } // O dot
            { "ɵ", } // rounded schwa (barred o)
            { "ɹ", } // upside down r
            { "ʁ", } // upside down capital r
            { "ř", } // r hacheck
            { "ɾ", } // r fishhook (flap)
            { "ɽ", } // IPA retroflex r trill
            { "ṛ", } // retroflex r (r dot)
            { "ɻ", } // IPA retroflex approximant (flap)
            { "š", } // s hachek
            { "ʃ", } // IPA long esh (Unicode)
            { "ʄ", } // bared esh
            { "ś", } // s with acute accent
            { "ṣ", } // Retroflex s (s-dot)
            { "ʂ", } // s cedille
            { "θ", } // Greek theta
            { "þ", } // Germanic thorn
            { "ʈ", } // IPA Retroflex t
            { "ṭ", } // Retroflex t (t-dot)
            { "ʊ", } // IPA Lax u (upside down omega)
            { "ü", } // u umlaut
            { "u", } // u barred
            { "ʉ", } // Unicode u barred
            { "ɞ", } // Sideways heart
            { "ʌ", } // central vowel carot
            { "ʋ", } // labial dental glide (script v)
            { "ʍ", } // upside-down w (voiceless)
            { "w", } // superscript w
            { "ʷ", } // Unicode superscript w
            { "ŵ", } // Welsh w circumflex
            { "ɯ", } // upside down m (back unrounded vowel)
            { "ɰ", } // upside down m with tail (back unrounded glide)
            { "χ", } // Chi (uvular fricative)
            { "ɥ", } // front-round glide
            { "ŷ", } // Welsh y circumflex
            { "y", } // superscript y
            { "ɰ", } // upside down m with tail (back unrounded glide)
            { "ž", } // z hachek
            { "ʒ", } // IPA long ezh
            { "ẓ", } // Retroflex z (z-dot)
            { "ʑ", } // z curl (palatal fricative)
            { "ʐ", } // z cedille
            { "ʔ", } // glottal stop
            { "ʕ", } // pharyngeal fricative
            { "ʡ", } // glottal bar
            { "ʢ", } // pharyngeal bar
            { "ʘ" }  // bilabial click
        )
    }

}