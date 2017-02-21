using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloIA12
{
    public class Board : IPlayable.IPlayable
    {
        public enum TileState
        {
            Empty = 0,
            Black = -1,
            White = 1
        };

        public struct Vector2i
        {
            public Vector2i(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            public int x;
            public int y;

            public static Vector2i operator +(Vector2i v1, Vector2i v2)
            {
                return new Vector2i(v1.x + v2.x, v1.y + v2.y);
            }

            public bool IsValid(int size)
            {
                bool response = true;

                response &= (this.x >= 0 && this.x < size);
                response &= (this.y >= 0 && this.y < size);

                return response;
            }
        }

        private List<Vector2i> directions;
        public int[,] LogicBoard { get; set; }
        private bool isWhite;
        private int whiteScore;
        private Random rnd = new Random();
        private const string filename = "score.txt";
        public int WhiteScore
        {

            get
            {
                return whiteScore;
            }

            set
            {
                whiteScore = value;
                //NotifyPropertyChanged("WhiteScore");
            }
        }
        private int blackScore;
        public int BlackScore
        {
            get
            {
                return blackScore;
            }

            set
            {
                blackScore = value;
               // NotifyPropertyChanged("BlackScore");
            }
        }
        public int NumTiles { get; private set; }
        private int blackTime;
        public int BlackTime { get; set; }
        private int whiteTime;
        public int WhiteTime { get; set; }


        public Board() 
        {
            NumTiles = 8;
            Reset();

            directions = new List<Vector2i>();
            directions.Add(new Vector2i(0, 1));
            directions.Add(new Vector2i(0, -1));
            directions.Add(new Vector2i(1, 0));
            directions.Add(new Vector2i(1, 1));
            directions.Add(new Vector2i(1, -1));
            directions.Add(new Vector2i(-1, 0));
            directions.Add(new Vector2i(-1, 1));
            directions.Add(new Vector2i(-1, -1));
        }
        public Board(int numTiles = 8/*, MainWindow parent = null*/)
        {
            //main = parent;
            NumTiles = numTiles;
            Reset();

            directions = new List<Vector2i>();
            directions.Add(new Vector2i(0, 1));
            directions.Add(new Vector2i(0, -1));
            directions.Add(new Vector2i(1, 0));
            directions.Add(new Vector2i(1, 1));
            directions.Add(new Vector2i(1, -1));
            directions.Add(new Vector2i(-1, 0));
            directions.Add(new Vector2i(-1, 1));
            directions.Add(new Vector2i(-1, -1));
        }

        public void Reset()
        {
            isWhite = false;
            LogicBoard = new int[NumTiles, NumTiles];
            int half = NumTiles / 2;
            LogicBoard[half - 1, half - 1] = (int)TileState.White;
            LogicBoard[half, half] = (int)TileState.White;
            LogicBoard[half - 1, half] = (int)TileState.Black;
            LogicBoard[half, half - 1] = (int)TileState.Black;
            BlackScore = WhiteScore = 2;
        }


        private int CountValidPlay(bool isWhite)
        {
            int countValidPlay = 0;
            for (int i = 0; i < LogicBoard.GetLength(0) - 1; i++)
            {
                for (int j = 0; j < LogicBoard.GetLength(1) - 1; j++)
                {
                    if (IsPlayable(i, j, isWhite))
                    {
                        countValidPlay++;
                    }
                }
            }
            return countValidPlay;
        }



        public bool isTilePlayable(int column, int line)
        {
            return (IsPlayable(column, line, isWhite));

        }

        public string GetName()
        {
            return "12: Ruedin_Serex";
        }


        public int[,] GetBoard()
        {
            int[,] tournamentBoard = new int[8, 8];
            for (int i=0;i<8;i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (LogicBoard[i, j] == (int)TileState.Empty)
                        tournamentBoard[i, j] = -1;
                    else if (LogicBoard[i, j] == (int)TileState.White)
                        tournamentBoard[i, j] = 0;
                    else
                        tournamentBoard[i, j] = +1;
                }
            }

            return tournamentBoard;
        }

        public bool IsPlayable(int column, int line, bool isWhite)
        {
            if ((LogicBoard[column, line] != 0)) return false; // Tile must be empty

            int color = (int)TileState.Black;
            if (isWhite) color = (int)TileState.White;
            Vector2i pos = new Vector2i(column, line);

            foreach (var dir in directions)
            {
                Vector2i sideTile = pos + dir;
                if (sideTile.IsValid(NumTiles) &&
                        LogicBoard[sideTile.x, sideTile.y] != color &&
                        LogicBoard[sideTile.x, sideTile.y] != 0)
                {
                    Vector2i tile = sideTile + dir;
                    while (tile.IsValid(NumTiles) &&
                        LogicBoard[tile.x, tile.y] != color &&
                        LogicBoard[tile.x, tile.y] != 0)
                    {
                        tile = tile + dir;
                    }
                    if (tile.IsValid(NumTiles) && LogicBoard[tile.x, tile.y] == color)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool PlayMove(int column, int line, bool isWhite)
        {
            //Check if the move can be done
            if (IsPlayable(column, line, isWhite))
            {
                //Settling an integer for entering color in the logic state board.
                int color = (int)TileState.Black;
                if (isWhite) color = (int)TileState.White;

                //Settling the actual position for vectorial operations and a list of pawns to replace
                Vector2i pos = new Vector2i(column, line);
                List<Tuple<int, int>> pawnsToReplace = new List<Tuple<int, int>>();


                foreach (var dir in directions)
                {
                    List<Tuple<int, int>> tmp = new List<Tuple<int, int>>();
                    Vector2i tile = pos + dir;

                    //While we're in the board and we can find an opposite color pawn in a direction, we add positions to add a pawn to later, and we iterate until we reach 
                    //the end of the board or pawn of our color.
                    while (tile.IsValid(NumTiles) &&
                        LogicBoard[tile.x, tile.y] != color &&
                        LogicBoard[tile.x, tile.y] != 0)
                    {
                        tmp.Add(new Tuple<int, int>(tile.x, tile.y));
                        tile = tile + dir;
                    }

                    //If we actually ended up on a pawn of our color instead of finishing off the board, then it's a valid line of pawns to replace.
                    if (tile.IsValid(NumTiles) && LogicBoard[tile.x, tile.y] == color)
                    {
                        pawnsToReplace.Add(new Tuple<int, int>(column, line));
                        pawnsToReplace.AddRange(tmp);
                    }
                }


                //We then update the logic board based on the pawns to replace
                foreach (var pair in pawnsToReplace)
                {
                    LogicBoard[pair.Item1, pair.Item2] = color;
                   // main.UpdateBoard(pair, isWhite);
                }

                BlackScore = GetBlackScore();
                WhiteScore = GetWhiteScore();



                return true;
            }
            return false;
        }

        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            List<Tuple<int, int>> possibleMoves = new List<Tuple<int, int> >();
            for (int i=0; i< 8;i++)
            {
                for (int j=0; j<8;j++)
                {
                    if (IsPlayable(i, j, whiteTurn))
                        //return new Tuple<int, int>(i, j);
                        possibleMoves.Add(new Tuple<int, int>(i, j));
                }
            }
            //choose best move 
            // ... randomly
            if (possibleMoves.Count > 0)
                return possibleMoves[rnd.Next(possibleMoves.Count)];
            else
                return new Tuple<int, int>(-1, -1);
            //throw new NotImplementedException();
        }

        public int GetWhiteScore()
        {
            int score = 0;
            foreach (var i in LogicBoard)
            {
                if (i == 1)
                {
                    score++;
                }
            }
            return score;
        }

        public int GetBlackScore()
        {
            int score = 0;
            foreach (var i in LogicBoard)
            {
                if (i == -1)
                {
                    score++;
                }
            }
            return score;
        }

        public bool IsWhiteTurn()
        {
            return isWhite;
        }

    }

}
