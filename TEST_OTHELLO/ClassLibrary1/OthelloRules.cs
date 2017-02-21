using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloLib
{

    public enum EtatCase
    {
        Empty = -1,
        White = 0,
        Black = 1
    }

    public class Board : IPlayable.IPlayable
    {
        int[,] theBoard = new int[8, 8];
        int whiteScore = 0;
        int blackScore = 0;
        public bool GameFinish { get; set; }

        private Random rnd = new Random();

        public Board()
        {
            initBoard();
        }


        public void DrawBoard()
        {
            Console.WriteLine("REFERENCE" + "\tBLACK [X]:" + blackScore + "\tWHITE [O]:" + whiteScore);
            Console.WriteLine("  A B C D E F G H");
            for (int line = 0; line < 8; line++)
            {
                Console.Write($"{(line + 1)}");
                for (int col = 0; col < 8; col++)
                {
                    Console.Write((theBoard[col, line] == (int)OthelloLib.EtatCase.Empty) ? " -" : (theBoard[col, line] == (int)OthelloLib.EtatCase.White) ? " O" : " X");
                }
                Console.Write("\n");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// Returns the board game as a 2D array of int
        /// with following values
        /// -1: empty
        ///  0: white
        ///  1: black
        /// </summary>
        /// <returns></returns>
        public int[,] GetBoard()
        {
            return (int[,])theBoard;
        }

        #region IPlayable
        public int GetWhiteScore() { return whiteScore; }
        public int GetBlackScore() { return blackScore; }
        public string GetName() { return "OHU implementation"; }

        /// <summary>
        /// plays randomly amon the possible moves
        /// </summary>
        /// <param name="game"></param>
        /// <param name="level"></param>
        /// <param name="whiteTurn"></param>
        /// <returns>The move it will play, will return {P,0} if it has to PASS its turn (no move is possible)</returns>
        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            List<Tuple<char, int>> possibleMoves = GetPossibleMoves(whiteTurn);
            if (possibleMoves.Count == 0)
                return new Tuple<int, int>(-1, -1);
            else
                return new Tuple<int, int>(-1, -1);//possibleMoves[rnd.Next(possibleMoves.Count)];
        }

        public bool PlayMove(int column, int line, bool isWhite)
        {
            //0. Verify if indices are valid
            if ((column < 0) || (column > 7) || (line < 0) || (line > 7))
                return false;
            //1. Verify if it is playable
            if (IsPlayable(column, line, isWhite) == false)
                return false;

            //2. Create a list of directions {dx,dy,length} where tiles are flipped
            int c = column, l = line;
            bool playable = false;
            EtatCase opponent = isWhite ? EtatCase.Black : EtatCase.White;
            EtatCase ownColor = (!isWhite) ? EtatCase.Black : EtatCase.White;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();

            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c <= 7) && (c >= 0) && (l <= 7) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) <= 7) && (c + dCol >= 0) &&
                                  ((l + dLine) <= 7) && ((l + dLine >= 0))
                                   && (theBoard[c, l] == (int)opponent) ) // pour éviter les trous
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                theBoard[column, line] = (int)ownColor;
                                catchDirections.Add(new Tuple<int, int, int>(dCol, dLine, counter));
                            }
                        }
                    }
                }
            }
            // 3. Flip ennemy tiles
            foreach (var v in catchDirections)
            {
                int counter = 0;
                l = line;
                c = column;
                while (counter++ < v.Item3)
                {
                    c += v.Item1;
                    l += v.Item2;
                    theBoard[c, l] = (int)ownColor;
                }
            }
            //Console.WriteLine("CATCH DIRECTIONS:" + catchDirections.Count);
            computeScore();
            return playable;
        }

        /// <summary>
        /// More convenient overload to verify if a move is possible
        /// </summary>
        /// <param name=""></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool IsPlayable(Tuple<int,int> move, bool isWhite)
        {
            return IsPlayable(move.Item1, move.Item2, isWhite);
        }

        public bool IsPlayable(int column, int line, bool isWhite)
        {
            //1. Verify if the tile is empty !
            if (theBoard[column, line] != (int)EtatCase.Empty)
                return false;
            //2. Verify if at least one adjacent tile has an opponent tile
            EtatCase opponent = isWhite ? EtatCase.Black : EtatCase.White;
            EtatCase ownColor = (!isWhite) ? EtatCase.Black : EtatCase.White;
            int c = column, l = line;
            bool playable = false;
            List<Tuple<int, int, int>> catchDirections = new List<Tuple<int, int, int>>();
            for (int dLine = -1; dLine <= 1; dLine++)
            {
                for (int dCol = -1; dCol <= 1; dCol++)
                {
                    c = column + dCol;
                    l = line + dLine;
                    if ((c <= 7) && (c >= 0) && (l <= 7) && (l >= 0)
                        && (theBoard[c, l] == (int)opponent))
                    // Verify if there is a friendly tile to "pinch" and return ennemy tiles in this direction
                    {
                        int counter = 0;
                        while (((c + dCol) <= 7) && (c + dCol >= 0) &&
                                  ((l + dLine) <= 7) && ((l + dLine >= 0)))
                        {
                            c += dCol;
                            l += dLine;
                            counter++;
                            if (theBoard[c, l] == (int)ownColor)
                            {
                                playable = true;
                                break;
                            }
                            else if (theBoard[c, l] == (int)opponent)
                                continue;
                            else if (theBoard[c, l] == (int)EtatCase.Empty)
                                break;  //empty slot ends the search
                        }
                    }
                }
            }
            return playable;
        }
        #endregion

        /// <summary>
        /// Returns all the playable moves
        /// </summary>
        /// <param name="v"></param>
        /// <param name="whiteTurn"></param>
        /// <returns></returns>
        public List<Tuple<char, int>> GetPossibleMoves(bool whiteTurn, bool show = false)
        {
            char[] colonnes = "ABCDEFGH".ToCharArray();
            List<Tuple<char, int>> possibleMoves = new List<Tuple<char, int>>();
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                {
                    if (IsPlayable(i, j, whiteTurn))
                    {
                        possibleMoves.Add(new Tuple<char, int>(colonnes[i], j + 1));
                        if (show == true)
                            Console.Write((colonnes[i]).ToString() + (j + 1).ToString() + ", ");
                    }
                }
            return possibleMoves;
        }

        private void initBoard()
        {
            for (int i = 0; i < 8; i++)
                for (int j = 0; j < 8; j++)
                    theBoard[i, j] = (int)EtatCase.Empty;

            theBoard[3, 3] = (int)EtatCase.White;
            theBoard[4, 4] = (int)EtatCase.White;
            theBoard[3, 4] = (int)EtatCase.Black;
            theBoard[4, 3] = (int)EtatCase.Black;

            computeScore();
        }

        private void computeScore()
        {
            whiteScore = 0;
            blackScore = 0;
            foreach (var v in theBoard)
            {
                if (v == (int)EtatCase.White)
                    whiteScore++;
                else if (v == (int)EtatCase.Black)
                    blackScore++;
            }
            GameFinish = ((whiteScore == 0) || (blackScore == 0) ||
                        (whiteScore + blackScore == 64));
        }
    }
}
