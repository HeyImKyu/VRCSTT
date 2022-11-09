namespace VRCSTT.Helper.KanaConverter
{
    public class KanaRomajiConverter : KanaConverter
    {
        public string Convert(string text)
        {
            return ConvertKanaToRomaji(text);
        }
    }
}
