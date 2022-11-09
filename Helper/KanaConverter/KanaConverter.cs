using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VRCSTT.Helper.KanaConverter
{
    /// <summary>
    /// Edited Version of https://github.com/pilotMike/KanaConverter
    /// </summary>
    public abstract class KanaConverter
    {
        // Notes: the difference between hiragana and
        // katakana is 96, with katakana being lower.
        // Hiragana range is 0x3040 - 0x3096  // Kyu: btw. is it not until 307F?
        // Katakana range is 0x30A1 - 0x30FA

        protected static List<RomajiKana> Diphthongs;
        protected static List<RomajiKana> RomajiKanas;
        private const string DiphthongsUri = "Helper/KanaConverter/Diphthongs.txt";
        private const string KanaUri = "Helper/KanaConverter/RomajiKana.txt";

        protected KanaConverter()
        {
            if (Diphthongs == null)
                Diphthongs = GetDiphthongs().ToList();
            if (RomajiKanas == null)
                RomajiKanas = GetNormalConversion().ToList();
        }

        private void ReplaceCharacters(StringBuilder sb, bool toHiragana)
        {
            Diphthongs.ForEach(d => sb.Replace(d.Romaji, toHiragana ? d.Hiragana : d.Katakana));
            RomajiKanas.ForEach(rk => sb.Replace(rk.Romaji, toHiragana ? rk.Hiragana : rk.Katakana));
        }

        protected string ConvertKanaToRomaji(string text)
        {
            StringBuilder sb = new StringBuilder(text.ToLower());
            Diphthongs.ForEach(d => sb.Replace(d.Hiragana, d.Romaji));
            RomajiKanas.ForEach(rk => sb.Replace(rk.Hiragana, rk.Romaji));
            Diphthongs.ForEach(d => sb.Replace(d.Katakana, d.Romaji));
            RomajiKanas.ForEach(rk => sb.Replace(rk.Katakana, rk.Romaji));

            string s = sb.ToString();
            sb.Clear();

            // replace any leftover small 'tsu'
            ReplaceTsuWithNextCharacter(s, sb);
            return sb.ToString();
        }

        private void ReplaceTsuWithNextCharacter(string s, StringBuilder sb)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == 'っ' || s[i] == 'ッ' && i < s.Length - 1)
                    sb.Append(s[i + 1]);
                else
                    sb.Append(s[i]);
            }
        }

        #region StaticFunctions
        public static bool ContainsHiraganaCharacters(string text)
        {
            return text.Any(s => s > 0x3040 && s <= 0x307F);
        }

        public static bool ContainsKatakanaCharacters(string text)
        {
            return text.Any(c => c > 0x30A0 && c < 0x30FA);
        }

        public static KanaRomajiConverter GetKanaRomajiConverter()
        {
            return new KanaRomajiConverter();
        }

        #endregion StaticFunctions

        #region Initialize
        private IEnumerable<RomajiKana> GetDiphthongs()
        {
            return LoadRomajiKanas(DiphthongsUri);
        }

        private IEnumerable<RomajiKana> GetNormalConversion()
        {
            return LoadRomajiKanas(KanaUri);
        }

        private static IEnumerable<RomajiKana> LoadRomajiKanas(string file)
        {
            using (StreamReader sr = new StreamReader(file))
            {
                while (!sr.EndOfStream)
                {
                    var splitMe = sr.ReadLine().Split('@');
                    RomajiKana rk = new RomajiKana
                    {
                        Romaji = splitMe[0],
                        Hiragana = splitMe[1],
                        Katakana = splitMe[2]
                    };
                    yield return rk;
                }
            }
        }
        #endregion
    }

    public struct RomajiKana
    {
        public string Romaji;
        public string Hiragana;
        public string Katakana;
    }
}