using GoLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GoLib.SGF
{
    enum Tokens
    {
        None = 0,
        CurlyOpen,
        CurlyClose,
        SemiColon,
        SquaredOpen,
        SquaredClose,
        UcLetter
    }

    delegate void MoveAction(Move move, char[] sgf, ref int index, ref bool success);
    delegate void GameInfoAction(GameInfo gameInfo, char[] sgf, ref int index, ref bool success);

    static class SgfParser
    {
        public static Goban SgfDecode(string sgf)
        {
            bool success = true;
            return SgfDecode(sgf, ref success);
        }

        public static Goban SgfDecode(string sgf, ref bool success)
        {
            success = true;
            if (sgf != null)
            {
                char[] charArray = sgf.ToCharArray();
                int index = 0;
                return ParseGameTree(charArray, ref index, ref success);
            }
            return null;
        }

        private static Goban ParseGameTree(char[] sgf, ref int index, ref bool success)
        {
            if (LookAhead(sgf, index) != Tokens.CurlyOpen)
            {
                success = false;
                return null;
            }

            NextToken(sgf, ref index);
            var gameInfo = ParseGameInfoProperties(sgf, ref index, ref success);

            if (!success)
                return null;

            var token = LookAhead(sgf, index);

            if (token == Tokens.CurlyClose)
            {
                return new Goban(gameInfo);
            }
            else
            {
                var moves = ParseMoves(sgf, ref index, ref success);
                if (success)
                {
                    if (LookAhead(sgf, index) == Tokens.CurlyClose)
                    {
                        var goban = new Goban(gameInfo);
                        // TODO: Add moves to goban
                        return goban;
                    }
                    else
                    {
                        success = false;
                        return null;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        private static GameInfo ParseGameInfoProperties(char[] sgf, ref int index, ref bool success)
        {
            if (LookAhead(sgf, index) != Tokens.SemiColon)
            {
                success = false;
                return null;
            }

            NextToken(sgf, ref index);
            var gameInfo = new GameInfo();

            while (LookAhead(sgf, index) == Tokens.UcLetter)
            {
                var ident = ParsePropIdent(sgf, ref index, ref success);
                if (success)
                {
                    AddGameInfoProperty(gameInfo, ident, sgf, ref index, ref success);
                    if (!success)
                        return null;
                }
                else
                {
                    return null;
                }
            }
            return gameInfo; // TODO: check GameInfo consistency (perhaps some mandatory field like size are not set)
        }

        private static Tree<Move> ParseMoves(char[] sgf, ref int index, ref bool success)
        {
            var moves = new Tree<Move>();
            var current = moves;

            if (LookAhead(sgf, index) == Tokens.SemiColon)
            {
                while (LookAhead(sgf, index) == Tokens.SemiColon)
                {
                    var move = ParseMove(sgf, ref index, ref success);
                    if (success)
                    {
                        current.Add(move);
                        current = current.Next;
                    }
                    else
                    {
                        return null;
                    }
                }

                if (LookAhead(sgf, index) != Tokens.CurlyOpen)
                {
                    return moves;
                }
            }

            if (LookAhead(sgf, index) == Tokens.CurlyOpen)
            {
                while (LookAhead(sgf, index) == Tokens.CurlyOpen)
                {
                    var variation = ParseVariation(sgf, ref index, ref success);
                    if (success)
                        current.AddTree(variation);
                    else
                        return null;
                }
                return moves;
            }
            else
            {
                success = false;
                return null;
            }
        }

        private static Tree<Move> ParseVariation(char[] sgf, ref int index, ref bool success)
        {
            NextToken(sgf, ref index); // token : (

            if (LookAhead(sgf, index) != Tokens.SemiColon)
            {
                success = false;
                return null;
            }

            var moves = ParseMoves(sgf, ref index, ref success);
            if (!success)
                return null;

            if (LookAhead(sgf, index) == Tokens.CurlyClose)
            {
                NextToken(sgf, ref index); // token : )
                return moves;
            }
            else
            {
                success = false;
                return null;
            }
        }

        private static Move ParseMove(char[] sgf, ref int index, ref bool success)
        {
            NextToken(sgf, ref index); // token : ;
            var move = new Move();

            while (LookAhead(sgf, index) == Tokens.UcLetter)
            {
                var ident = ParsePropIdent(sgf, ref index, ref success);
                if (!success)
                    return null;
                AddMoveProperty(move, ident, sgf, ref index, ref success);
                if (!success)
                    return null;
            }
            return move;
        }

        private static int ParseNumber(char[] sgf, ref int index, ref bool success)
        {
            EatWhitespace(sgf, ref index);

            int lastIndex = LastIndex(sgf, index);
            int length = (lastIndex - index) + 1;
            int number = number = int.Parse(new string(sgf, index, length));
            index += length;

            return number;
        }

        private static double ParseReal(char[] sgf, ref int index, ref bool success)
        {
            EatWhitespace(sgf, ref index);

            int lastIndex = LastIndex(sgf, index);
            int length = (lastIndex - index) + 1;
            double real = real = double.Parse(new string(sgf, index, length));
            index += length;

            return real;
        }

        private static int ParseDouble(char[] sgf, ref int index, ref bool success)
        {
            EatWhitespace(sgf, ref index);

            char c = sgf[index];
            if (c == '1' || c == '2')
            {
                index++;
                return (int)(c - '0');
            }
            else
            {
                success = false;
                return 0;
            }
        }

        private static Colour ParseColor(char[] sgf, ref int index, ref bool success)
        {
            EatWhitespace(sgf, ref index);

            char c = sgf[index];
            if (c == 'B')
            {
                index++;
                return Colour.Black;
            }
            else if (c == 'W')
            {
                index++;
                return Colour.White;
            }
            else
            {
                success = false;
                return Colour.None;
            }
        }

        private static Point ParsePoint(char[] sgf, ref int index, ref bool success)
        {
            EatWhitespace(sgf, ref index);

            if (sgf.Length - index < 2)
            {
                success = false;
                return null;
            }

            char x = sgf[index];
            char y = sgf[index + 1];
            Point point = Point.Empty;

            try
            {
                point = Point.Parse(x, y);
                index += 2;
            }
            catch (FormatException)
            {
                success = false;
            }

            return point; // TODO: check limits
        }

        private static string ParseText(char[] sgf, ref int index, ref bool success)
        {
            var s = new StringBuilder();

            for (; index < sgf.Length; ++index)
            {
                char c = sgf[index];
                if (c == ']')
                {
                    break;
                }
                else if (c == '\\')
                {
                    if (index == sgf.Length)
                    {
                        success = false;
                        return null;
                    }
                    s.Append(sgf[++index]);
                    // TODO: soft line breaks
                }
                else
                {
                    // TODO: convert whitespaces
                    s.Append(c);
                }
            }

            return s.ToString();
        }

        private static string ParsePropIdent(char[] sgf, ref int index, ref bool success)
        {
            EatWhitespace(sgf, ref index);

            char c1 = sgf[index++];
            if (LookAhead(sgf, index) == Tokens.UcLetter)
            {
                char c2 = sgf[index++];
                return c1.ToString() + c2.ToString();
            }
            else
            {
                return c1.ToString();
            }
        }

        private static Tokens LookAhead(char[] sgf, int index)
        {
            int saveIndex = index;
            return NextToken(sgf, ref saveIndex);
        }

        private static Tokens NextToken(char[] sgf, ref int index)
        {
            EatWhitespace(sgf, ref index);

            if (index >= sgf.Length)
            {
                return Tokens.None;
            }

            char c = sgf[index++];
            switch (c)
            {
                case '(':
                    return Tokens.CurlyOpen;
                case ')':
                    return Tokens.CurlyClose;
                case ';':
                    return Tokens.SemiColon;
                case '[':
                    return Tokens.SquaredOpen;
                case ']':
                    return Tokens.SquaredClose;
            }

            if (char.IsUpper(c))
                return Tokens.UcLetter;

            return Tokens.None;
        }

        private static void EatWhitespace(char[] sgf, ref int index)
        {
            while (index < sgf.Length && " \t\n\r".IndexOf(sgf[index]) != -1)
            {
                ++index;
            }
        }

        private static int LastIndex(char[] sgf, int index)
        {
            int lastIndex = index;

            while (lastIndex < sgf.Length && "0123456789+-.".IndexOf(sgf[lastIndex]) != -1)
            {
                ++lastIndex;
            }
            return lastIndex - 1;
        }

        private static void ThrowError(char[] sgf, int index, string message)
        {
            int line, column;
            CursorPosition(sgf, index, out line, out column);
            throw new Exception(message + " line : " + line + " column " + column);
        }

        private static void CursorPosition(char[] sgf, int index, out int line, out int column)
        {
            line = 1;
            column = 1;
            for (int i = 0; i <= index; ++i)
            {
                char c = sgf[i];
                if (c == '\n')
                {
                    line++;
                    column = 1;
                    if (i <= index && sgf[i + 1] == '\r')
                    {
                        i++;
                    }
                }
                else if (c == '\r')
                {
                    line++;
                    column = 1;
                    if (i <= index && sgf[i + 1] == '\n')
                    {
                        i++;
                    }
                }
                else
                {
                    column++;
                }
            }
        }

        #region Properties

        private static Dictionary<string, MoveAction> _moveActions;
        private static Dictionary<string, GameInfoAction> _gameInfoActions;

        private static void AddGameInfoProperty(GameInfo gameInfo, string ident, char[] sgf, ref int index, ref bool success)
        {
            if (LookAhead(sgf, index) == Tokens.SquaredOpen)
            {
                NextToken(sgf, ref index);
                var action = _gameInfoActions[ident];
                if (action != null)
                {
                    action(gameInfo, sgf, ref index, ref success);
                }
                else
                {
                    string str = ParseText(sgf, ref index, ref success);
                    // TODO: Add to an internal dictionary and raise a warning
                }
                if (LookAhead(sgf, index) == Tokens.SquaredClose)
                {
                    NextToken(sgf, ref index);
                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }
        }

        private static void AddMoveProperty(Move move, string ident, char[] sgf, ref int index, ref bool success)
        {
            if (LookAhead(sgf, index) == Tokens.SquaredOpen)
            {
                NextToken(sgf, ref index);
                var action = _moveActions[ident];
                if (action != null)
                {
                    action(move, sgf, ref index, ref success);
                }
                else
                {
                    string str = ParseText(sgf, ref index, ref success);
                    // TODO: Add to an internal dictionary and raise a warning "unknow property : XX"
                }
                if (LookAhead(sgf, index) == Tokens.SquaredClose)
                {
                    NextToken(sgf, ref index);
                }
                else
                {
                    success = false;
                }
            }
            else
            {
                success = false;
            }
        }

        static SgfParser()
        {
            _gameInfoActions = new Dictionary<string, GameInfoAction>();
            _moveActions = new Dictionary<string, MoveAction>();

            // Root properties
            _gameInfoActions["SZ"] = ParseSize;
            _gameInfoActions["GM"] = ParseGameId;
            _gameInfoActions["FF"] = ParseFileFormat;
            _gameInfoActions["CA"] = ParseCharset;
            _gameInfoActions["AP"] = ParseApplication;
            _gameInfoActions["ST"] = ParseStyle;

            // Game-Info properties
            _gameInfoActions["RU"] = ParseRules;
            _gameInfoActions["RE"] = ParseResult;
            _gameInfoActions["HA"] = ParseHandicap;
            _gameInfoActions["KM"] = ParseKomi;

            _gameInfoActions["AB"] = ParseHandicapBlack;
            _gameInfoActions["AW"] = ParseHandicapWhite;

            // Move properties
            _moveActions["B"] = ParseBlackMove;
            _moveActions["W"] = ParseWhiteMove;
            _moveActions["MN"] = null; // Move number
            _moveActions["KO"] = null; // Ko

            // Setup-Properties
            _moveActions["AB"] = ParseAddBlack;
            _moveActions["AW"] = ParseAddWhite;
            _moveActions["AE"] = ParseAddEmpty;
            _moveActions["PL"] = null; // Player to play

            // Node-Annotations
            _moveActions["TB"] = null; // Territories Black
            _moveActions["TW"] = null; // Territories White
            _moveActions["C"] = null; // Comments
            _moveActions["DM"] = null; // Position is even
            _moveActions["GB"] = null; // Good for Black
            _moveActions["GW"] = null; // Good for White
            _moveActions["HO"] = null; // Hotspot
            _moveActions["N"] = null; // Node Name
            _moveActions["UC"] = null; // Unclear
            _moveActions["V"] = null; // Value

            // Markup Properties
            _moveActions["CR"] = null; // Circle
            _moveActions["LB"] = null; // Label
            _moveActions["MA"] = null; // Mark
            _moveActions["SQ"] = null; // Square
            _moveActions["TR"] = null; // Triangle
        }

        #endregion

        #region GameInfo properties

        private static void ParseRules(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            string value = ParseText(sgf, ref index, ref success);
            if (success)
            {
                try
                {
                    Rules rule = (Rules)Enum.Parse(typeof(Rules), value, true);
                    gameInfo.Rule = rule;
                }
                catch (Exception)
                {
                    success = false;
                }
            }
        }

        // simpletext
        private static void ParseResult(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {

        }

        private static void ParseHandicap(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            int handicap = ParseNumber(sgf, ref index, ref success);
            if (success)
            {
                if (0 <= handicap && handicap <= 9)
                {
                    gameInfo.Handicap = handicap;
                }
                else
                {
                    success = false;
                }
            }
        }

        private static void ParseKomi(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            double komi = ParseReal(sgf, ref index, ref success);
            if (success)
            {
                if (komi == Math.Floor(komi) || komi - 0.5 == Math.Floor(komi))
                {
                    gameInfo.Komi = komi;
                }
                else
                {
                    success = false;
                }
            }
        }

        // list of point
        private static void ParseHandicapBlack(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
        }

        // list of point
        private static void ParseHandicapWhite(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
        }

        #endregion

        #region Root properties

        private static void ParseSize(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            int size = ParseNumber(sgf, ref index, ref success);
            if (success)
            {
                if (size == 9 || size == 13 || size == 19)
                {
                    gameInfo.Size = size;
                }
                else
                {
                    ThrowError(sgf, index, "Supported sizes: 9x9, 13x13 or 19x19");
                }
            }
        }

        private static void ParseGameId(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            int gameId = ParseNumber(sgf, ref index, ref success);
            if (gameId != 1)
            {
                ThrowError(sgf, index, "GM property must be set to 1 (Go Game)");
            }
        }

        private static void ParseFileFormat(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            int ff = ParseNumber(sgf, ref index, ref success);
            if (ff != 4)
            {
                ThrowError(sgf, index, "FF property must be set to '4'");
            }
        }

        private static void ParseCharset(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            string ca = ParseText(sgf, ref index, ref success);
            // TODO: Convertir les commentaires et autres selon ce format (ou pas ?)
        }

        private static void ParseApplication(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            string ap = ParseText(sgf, ref index, ref success);
            // Useless, will be overwriten by "Kifu" once game saved
        }

        private static void ParseStyle(GameInfo gameInfo, char[] sgf, ref int index, ref bool success)
        {
            int st = ParseNumber(sgf, ref index, ref success);
            // Useless, will be onverwriten by "4" once game saved
        }

        #endregion

        #region Move properties

        // point
        private static void ParseBlackMove(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        // point
        private static void ParseWhiteMove(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        #endregion

        #region setup properties

        private static void ParseAddBlack(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        private static void ParseAddWhite(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        private static void ParseAddEmpty(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        #endregion

        #region other properties

        // créer ces methodes pour moves aussi ?
        // -> imbriguer un move dans GameInfo

        private static void ParseComment(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        private static void ParseLabel(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        private static void ParseCircle(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        private static void ParseSquare(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        private static void ParseTriangle(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        private static void ParseMark(Move move, char[] sgf, ref int index, ref bool success)
        {
        }

        #endregion
    }
}
