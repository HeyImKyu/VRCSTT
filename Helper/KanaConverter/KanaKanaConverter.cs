namespace VRCSTT.Helper.KanaConverter
{
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
