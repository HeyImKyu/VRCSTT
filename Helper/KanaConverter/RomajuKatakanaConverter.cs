namespace VRCSTT.Helper.KanaConverter
{
    public class RomajiKatakanaConverter : KanaConverter
    {
        public string Convert(string text)
        {
            return ConvertRomajiToKatakana(text);
        }
    }
}
