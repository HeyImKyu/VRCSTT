namespace VRCSTT.Helper.KanaConverter
{
    public class RomajiHiraganaConverter : KanaConverter
    {
        public string Convert(string text)
        {
            return ConvertRomajiToHiragana(text);
        }
    }
}
