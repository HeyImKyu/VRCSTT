namespace VRCSTT.Helper.KanaConverter
{
    /// <summary>
    /// Edited Version of https://github.com/pilotMike/KanaConverter
    /// </summary>
    public class RomajiKatakanaConverter : KanaConverter
    {
        public string Convert(string text)
        {
            return ConvertRomajiToKatakana(text);
        }
    }
}
