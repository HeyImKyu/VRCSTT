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
        // Hiragana range is 0x3040 - 0x3096
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

        protected string ConvertRomajiToHiragana(string text)
        {
            // first replace all diphthongs
            StringBuilder sb = new StringBuilder(text.ToLower());
            ReplaceCharacters(sb, true);
            // now, replace any lingering consonants (hopefully that's all that's left) with a small 'tsu'
            string result = Regex.Replace(sb.ToString(), "[a-zA-Z]", "っ");
            return result;
        }

        protected string ConvertRomajiToKatakana(string text)
        {
            StringBuilder sb = new StringBuilder(text.ToLower());
            ReplaceCharacters(sb, false);
            // now, replace any lingering consonants (hopefully that's all that's left) with a small 'tsu'
            string result = Regex.Replace(sb.ToString(), "[a-zA-Z]", "ッ");
            return result;
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

        protected string ConvertHiraganaToKatakana(string text)
        {
            // change characters from ぁ to small ke, leave the rest the same
            var katEnum = text.Select(c => (char)(c > 0x3040 && c < 0x30F7 ? c + 96 : c));
            return string.Join("", katEnum);
        }

        protected string ConvertKatakanaToHiragana(string text)
        {
            // shift all katakana characters down to the hiragana equivalent,
            // up until 'vu', which doesn't have an equivalent.
            var hirEnum = text.Select(c => (char)(c > 0x30A0 && c < 0x30F4 ? c - 96 : c));
            return string.Join("", hirEnum);
        }

        #region StaticFunctions
        public static bool ContainsRomajiCharacters(string text)
        {
            return text.Any(s => s >= 'A' && s <= 'z');
        }

        public static bool ContainsHiraganaCharacters(string text)
        {
            return text.Any(s => s > 0x3040 && s <= 0x307F);
        }

        public static bool ContainsKatakanaCharacters(string text)
        {
            return text.Any(c => c > 0x30A0 && c < 0x30FA);
        }

        /// <summary>
        /// Returns the first instance of the type of characters found.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static CharacterType GetCharacterType(string text)
        {
            if (ContainsHiraganaCharacters(text))
                return CharacterType.Hiragana;
            if (ContainsKatakanaCharacters(text))
                return CharacterType.Katakana;
            if (ContainsRomajiCharacters(text))
                return CharacterType.Romaji;
            return CharacterType.Unknown;
        }

        /// <summary>
        /// Determines whether the text is kana or romaji and returns
        /// a converter to go in the other direction. Assumes Romaji -> Hiragana.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static KanaConverter GetConverter(string text)
        {
            if (ContainsHiraganaCharacters(text) || ContainsKatakanaCharacters(text))
                return new KanaRomajiConverter();
            if (ContainsRomajiCharacters(text))
                return new RomajiHiraganaConverter();
            throw new Exception("string does not contain any Kana or Romaji characters");
        }

        public static KanaRomajiConverter GetKanaRomajiConverter()
        {
            return new KanaRomajiConverter();
        }

        /// <summary>
        /// Determines whether the text is kana or romaji and returns
        /// a converter to go in the other direction. Use this to specify
        /// Romaji -> Katakana or Kana -> Kana.
        /// Note: this currently doesn't allow an attempt at Hiragana to Hiragana conversion,
        /// only from one to the other.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="destinationType">force romaji, hiragana, katakana</param>
        /// <returns></returns>
        public static KanaConverter GetConverter(string text, CharacterType destinationType)
        {
            // eeeewwww. this is ugly.
            if (ContainsHiraganaCharacters(text) || ContainsKatakanaCharacters(text))
                if (destinationType == CharacterType.Hiragana || destinationType == CharacterType.Katakana)
                    return new KanaKanaConverter();
                else
                    return new KanaRomajiConverter();
            if (ContainsRomajiCharacters(text))
                if (destinationType == CharacterType.Hiragana)
                    return new RomajiHiraganaConverter();
                else if (destinationType == CharacterType.Katakana)
                    return new RomajiKatakanaConverter();
                else
                    return new KanaRomajiConverter();// as a default
            throw new Exception("string does not contain any Kana or Romaji characters");
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

    public enum CharacterType
    {
        Romaji, Hiragana, Katakana, Unknown
    }
}