namespace VRCSTT.Helper.KanaConverter
{
    public class RomajiHiraganaConverter : KanaConverter
    {
        /// <summary>
        /// Edited Version of https://github.com/pilotMike/KanaConverter
        /// </summary>
        public string Convert(string text)
        {
            return ConvertRomajiToHiragana(text);
        }
    }
}
