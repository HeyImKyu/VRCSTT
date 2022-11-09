namespace VRCSTT.Helper.KanaConverter
{
    /// <summary>
    /// Edited Version of https://github.com/pilotMike/KanaConverter
    /// </summary>
    public class KanaRomajiConverter : KanaConverter
    {
        public string Convert(string text)
        {
            return ConvertKanaToRomaji(text);
        }
    }
}
