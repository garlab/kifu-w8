
namespace GoLib.SGF
{
    // TODO: move onto Kifu.Utils 
    public static class SgfHelper
    {
        public static string ToString(Goban goban)
        {
            return SgfWriter.ToSGF(goban);
        }

        public static Goban FromString(string sgf)
        {
            return SgfParser.SgfDecode(sgf);
        }
    }
}
