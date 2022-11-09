namespace VRCSTT.Helper.KanaConverter
{
    /// <summary>
    /// Edited Version of https://github.com/pilotMike/KanaConverter
    /// </summary>
    public class KanaKanaConverter : KanaConverter
    {
        public string Convert(string text)
        {
            return ContainsHiraganaCharacters(text) ?
                ConvertHiraganaToKatakana(text) :
                ConvertKatakanaToHiragana(text);
        }
    }
}
