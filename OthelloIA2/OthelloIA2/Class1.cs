using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OthelloIA2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    // Tile states
    public enum tileState
    {
        WHITE,
        BLACK,
        EMPTY
    }

    [Serializable]
    public class OthelloBoard : IPlayable.IPlayable, ISerializable
    {
        private const int BOARDSIZE = 8;

        private tileState[,] board;     // X = Line     Y = Column
        private int[,] myBoard;
        private List<Tuple<int, int>> canMove;

        // stopwatch to measure time of each player
        // Offsets are for keeping the times when serialized
        private TimeSpan offset1;
        private TimeSpan offset2;
        private Stopwatch watch1;
        private Stopwatch watch2;

        #region CONSTRUCTOR
        public OthelloBoard()
        {
            board = new tileState[BOARDSIZE, BOARDSIZE];
            myBoard = new int[BOARDSIZE, BOARDSIZE];
            canMove = new List<Tuple<int, int>>();

            offset1 = new TimeSpan(0);
            offset2 = new TimeSpan(0);

            watch1 = new Stopwatch();
            watch2 = new Stopwatch();

            // Make the board empty
            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    board[i, j] = tileState.EMPTY;
                }
            }


            // Setup first pieces
            board[3, 3] = tileState.WHITE;
            board[3, 4] = tileState.BLACK;
            board[4, 4] = tileState.WHITE;
            board[4, 3] = tileState.BLACK;

            // Get the possible moves (black always start)
            possibleMoves(false);
        }
        #endregion

        /**
         *  Functions to return players elapsed time
         */
        public TimeSpan elapsedWatch1()
        {
            TimeSpan ts = watch1.Elapsed + offset1;
            return ts;
        }

        public TimeSpan elapsedWatch2()
        {
            TimeSpan ts = watch2.Elapsed + offset2;
            return ts;
        }


        #region IPlayable

        public string GetName()
        {
            return "01_Ceschin_Magnin";
        }
        public bool IsPlayable(int column, int line, bool isWhite)
        {
            Tuple<int, int> pos = new Tuple<int, int>(column, line);
            possibleMoves(isWhite);
            return canMove.Contains(pos);
        }

        public bool PlayMove(int column, int line, bool isWhite)
        {
            int x = column;
            int y = line;

            Tuple<int, int> pos = new Tuple<int, int>(x, y);

            if (isWhite)
            {
                possibleMoves(isWhite);
                if (canMove.Contains(pos))
                {
                    board[x, y] = tileState.WHITE;
                    turnPieces(x, y, isWhite);
                    possibleMoves(!isWhite);

                    //start stopwatch of black player
                    watch2.Start();
                    watch1.Stop();

                    return true;
                }
                else
                    return false;
            }
            else
            {
                possibleMoves(isWhite);
                if (canMove.Contains(pos))
                {
                    board[x, y] = tileState.BLACK;
                    turnPieces(x, y, isWhite);
                    possibleMoves(!isWhite);

                    //start stopwatch of white player
                    watch1.Start();
                    watch2.Stop();

                    return true;
                }
                else
                    return false;
            }
        }

        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            char[] colonnes = "ABCDEFGH".ToCharArray();
            possibleMoves(whiteTurn);
            if (canMove.Count == 0)
                return new Tuple<int, int>(-1, -1);
            return canMove.First();
/*            List<Tuple<char, int>> possibleMoves = GetPossibleMoves(whiteTurn);
            if (possibleMoves.Count == 0)
                return new Tuple<char, int>('P', 0);
            else
                return possibleMoves[rnd.Next(possibleMoves.Count)];
  */      }

        public int GetBlackScore()
        {
            return calculateScore(tileState.BLACK);
        }

        public int GetWhiteScore()
        {
            return calculateScore(tileState.WHITE);
        }

        public int[,] GetBoard()
        {
            for (int i=0; i< BOARDSIZE; i++)
            {
                for (int j=0; j< BOARDSIZE; j++)
                {
                    if (this.board[i, j] == tileState.BLACK)
                        myBoard[i, j] = 1;
                    else if (this.board[i, j] == tileState.WHITE)
                        myBoard[i, j] = 0;
                    else
                        myBoard[i, j] = -1;
                }   
            }
            return myBoard;
        }

        #endregion

        /*-------------------------------------------------------
         * ISerializable functions
         -------------------------------------------------------- */
        // Deserialize
        public OthelloBoard(SerializationInfo info, StreamingContext context)
        {
            board = (tileState[,])info.GetValue("board", typeof(tileState[,]));
            offset1 = (TimeSpan)info.GetValue("time1", typeof(TimeSpan));
            watch1 = new Stopwatch();
            offset2 = (TimeSpan)info.GetValue("time2", typeof(TimeSpan));
            watch2 = new Stopwatch();

            canMove = new List<Tuple<int, int>>();

        }

        // Serialize
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("board", board, typeof(tileState[,]));
            info.AddValue("time1", elapsedWatch1(), typeof(TimeSpan));
            info.AddValue("time2", elapsedWatch2(), typeof(TimeSpan));
        }


        /*-------------------------------------------------------
         * Class functions
         -------------------------------------------------------- */

        /// <summary>
        /// Play the move
        /// </summary>
        /// <param name="x">grid X value</param>
        /// <param name="y">grid Y value</param>
        /// <param name="isWhite">which player play</param>
        private void turnPieces(int x, int y, bool isWhite)
        {
            tileState color;
            tileState ennemyColor;

            int laX;
            int laY;
            bool wentTroughEnnemy;

            bool turnNorth = false;
            bool turnSouth = false;
            bool turnWest = false;
            bool turnEast = false;
            bool turnNorthEast = false;
            bool turnNorthWest = false;
            bool turnSouthEast = false;
            bool turnSouthWest = false;

            // Setup the colors we need to check
            if (!isWhite)
            {
                color = tileState.BLACK;
                ennemyColor = tileState.WHITE;
            }
            else
            {
                color = tileState.WHITE;
                ennemyColor = tileState.BLACK;
            }

            /*
             * CHECK WICH LINES TO TURN
             */

            //check NORTH
            laX = x - 1;
            wentTroughEnnemy = false;
            while (laX >= 0)
            {
                if (board[laX, y] == ennemyColor)
                {
                    // if its ennemy, look after
                    laX -= 1;
                    wentTroughEnnemy = true;
                }
                else if (board[laX, y] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnNorth = true;
                    break;
                }
                else
                {
                    //empty cell
                    break;
                }
            }

            // check SOUTH
            laX = x + 1;
            wentTroughEnnemy = false;
            while (laX < BOARDSIZE)
            {
                if (board[laX, y] == ennemyColor)
                {
                    // if its ennemy, look after
                    laX += 1;
                    wentTroughEnnemy = true;
                }
                else if (board[laX, y] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnSouth = true;
                    break;
                }
                else
                {
                    break;
                }
            }


            // check WEST
            laY = y - 1;
            wentTroughEnnemy = false;
            while (laY >= 0)
            {
                if (board[x, laY] == ennemyColor)
                {
                    // if its ennemy, look after
                    laY -= 1;
                    wentTroughEnnemy = true;
                }
                else if (board[x, laY] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnWest = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            // check EAST
            laY = y + 1;
            wentTroughEnnemy = false;
            while (laY < BOARDSIZE)
            {
                if (board[x, laY] == ennemyColor)
                {
                    // if its ennemy, look after
                    laY += 1;
                    wentTroughEnnemy = true;
                }
                else if (board[x, laY] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnEast = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            // check NORTH EAST
            laX = x - 1;
            laY = y + 1;
            wentTroughEnnemy = false;
            while (laX >= 0 && laY < BOARDSIZE)
            {
                if (board[laX, laY] == ennemyColor)
                {
                    // if its ennemy, look after
                    laX -= 1;
                    laY += 1;
                    wentTroughEnnemy = true;
                }
                else if (board[laX, laY] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnNorthEast = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            // check NORTH WEST
            laX = x - 1;
            laY = y - 1;
            wentTroughEnnemy = false;
            while (laX >= 0 && laY >= 0)
            {
                if (board[laX, laY] == ennemyColor)
                {
                    // if its ennemy, look after
                    laX -= 1;
                    laY -= 1;
                    wentTroughEnnemy = true;
                }
                else if (board[laX, laY] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnNorthWest = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            // check SOUTH EAST
            laX = x + 1;
            laY = y + 1;
            wentTroughEnnemy = false;
            while (laX < BOARDSIZE && laY < BOARDSIZE)
            {
                if (board[laX, laY] == ennemyColor)
                {
                    // if its ennemy, look after
                    laX += 1;
                    laY += 1;
                    wentTroughEnnemy = true;
                }
                else if (board[laX, laY] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnSouthEast = true;
                    break;
                }
                else
                {
                    break;
                }
            }

            // check SOUTH WEST
            laX = x + 1;
            laY = y - 1;
            wentTroughEnnemy = false;
            while (laX < BOARDSIZE && laY >= 0)
            {
                if (board[laX, laY] == ennemyColor)
                {
                    // if its ennemy, look after
                    laX += 1;
                    laY -= 1;
                    wentTroughEnnemy = true;
                }
                else if (board[laX, laY] == color)
                {
                    // if its our color and we eat ennemies, we gonna turn the line
                    if (wentTroughEnnemy)
                        turnSouthWest = true;
                    break;
                }
                else
                {
                    break;
                }
            }


            /*
             *  TURN PIECES 
             */
            int tempx = x;
            int tempy = y;

            if (turnNorth)
            {
                tempx = x - 1;
                while (board[tempx, y] == ennemyColor)
                {
                    board[tempx, y] = color;
                    tempx -= 1;
                }
            }
            if (turnSouth)
            {
                tempx = x + 1;
                while (board[tempx, y] == ennemyColor)
                {
                    board[tempx, y] = color;
                    tempx += 1;
                }
            }
            if (turnEast)
            {
                tempy = y + 1;
                while (board[x, tempy] == ennemyColor)
                {
                    board[x, tempy] = color;
                    tempy += 1;
                }
            }
            if (turnWest)
            {
                tempy = y - 1;
                while (board[x, tempy] == ennemyColor)
                {
                    board[x, tempy] = color;
                    tempy -= 1;
                }
            }
            if (turnNorthEast)
            {
                tempx = x - 1;
                tempy = y + 1;
                while (board[tempx, tempy] == ennemyColor)
                {
                    board[tempx, tempy] = color;
                    tempx -= 1;
                    tempy += 1;
                }
            }
            if (turnNorthWest)
            {
                tempx = x - 1;
                tempy = y - 1;
                while (board[tempx, tempy] == ennemyColor)
                {
                    board[tempx, tempy] = color;
                    tempx -= 1;
                    tempy -= 1;
                }
            }
            if (turnSouthEast)
            {
                tempx = x + 1;
                tempy = y + 1;
                while (board[tempx, tempy] == ennemyColor)
                {
                    board[tempx, tempy] = color;
                    tempx += 1;
                    tempy += 1;
                }
            }
            if (turnSouthWest)
            {
                tempx = x + 1;
                tempy = y - 1;
                while (board[tempx, tempy] == ennemyColor)
                {
                    board[tempx, tempy] = color;
                    tempx += 1;
                    tempy -= 1;
                }
            }
        }

        /// <summary>
        /// Calculate what moves are available
        /// Available moves are in canMove field
        /// </summary>
        /// <param name="isWhite">which player play</param>
        public void possibleMoves(bool isWhite)
        {
            tileState color;
            tileState ennemyColor;
            List<Tuple<int, int>> colorList = new List<Tuple<int, int>>();

            // Reset the canMove list
            canMove.Clear();

            // Setup the colors we need to check
            if (!isWhite)
            {
                color = tileState.BLACK;
                ennemyColor = tileState.WHITE;
            }
            else
            {
                color = tileState.WHITE;
                ennemyColor = tileState.BLACK;
            }

            //Get all the color pieces on board
            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if (board[i, j] == color)
                    {
                        colorList.Add(new Tuple<int, int>(i, j));
                    }
                }
            }


            foreach (Tuple<int, int> pos in colorList)
            {
                int x = pos.Item1;
                int y = pos.Item2;
                int laX;
                int laY;
                bool wentTroughEnnemy;

                //check NORTH
                laX = x - 1;
                wentTroughEnnemy = false;
                while (laX >= 0)
                {
                    if (board[laX, y] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laX -= 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[laX, y] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(laX, y));
                        break;
                    }
                }

                // check SOUTH
                laX = x + 1;
                wentTroughEnnemy = false;
                while (laX < BOARDSIZE)
                {
                    if (board[laX, y] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laX += 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[laX, y] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(laX, y));
                        break;
                    }
                }


                // check WEST
                laY = y - 1;
                wentTroughEnnemy = false;
                while (laY >= 0)
                {
                    if (board[x, laY] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laY -= 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[x, laY] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(x, laY));
                        break;
                    }
                }

                // check EAST
                laY = y + 1;
                wentTroughEnnemy = false;
                while (laY < BOARDSIZE)
                {
                    if (board[x, laY] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laY += 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[x, laY] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(x, laY));
                        break;
                    }
                }

                // check NORTH EAST
                laX = x - 1;
                laY = y + 1;
                wentTroughEnnemy = false;
                while (laX >= 0 && laY < BOARDSIZE)
                {
                    if (board[laX, laY] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laX -= 1;
                        laY += 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[laX, laY] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(laX, laY));
                        break;
                    }
                }

                // check NORTH WEST
                laX = x - 1;
                laY = y - 1;
                wentTroughEnnemy = false;
                while (laX >= 0 && laY >= 0)
                {
                    if (board[laX, laY] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laX -= 1;
                        laY -= 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[laX, laY] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(laX, laY));
                        break;
                    }
                }

                // check SOUTH EAST
                laX = x + 1;
                laY = y + 1;
                wentTroughEnnemy = false;
                while (laX < BOARDSIZE && laY < BOARDSIZE)
                {
                    if (board[laX, laY] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laX += 1;
                        laY += 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[laX, laY] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(laX, laY));
                        break;
                    }
                }

                // check SOUTH WEST
                laX = x + 1;
                laY = y - 1;
                wentTroughEnnemy = false;
                while (laX < BOARDSIZE && laY >= 0)
                {
                    if (board[laX, laY] == ennemyColor)
                    {
                        // if its ennemy, look after
                        laX += 1;
                        laY -= 1;
                        wentTroughEnnemy = true;
                    }
                    else if (board[laX, laY] == color)
                    {
                        // if its our color, end
                        break;
                    }
                    else
                    {
                        // if its empty and we eat an enenmy, add it to canMove
                        if (wentTroughEnnemy)
                            canMove.Add(new Tuple<int, int>(laX, laY));
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Count how many pieces of the color are on the board
        /// </summary>
        /// <param name="tileColor">color of the pieces</param>
        /// <returns></returns>
        private int calculateScore(tileState tileColor)
        {
            int score = 0;

            for (int i = 0; i < BOARDSIZE; i++)
            {
                for (int j = 0; j < BOARDSIZE; j++)
                {
                    if (board[i, j] == tileColor)
                        score++;
                }
            }

            return score;
        }

        /*-------------------------------------------------------
         * Getters and Setters
         -------------------------------------------------------- */
        public tileState[,] getState()
        {
            return board;
        }

        public List<Tuple<int, int>> getCanMove()
        {
            return canMove;
        }

    }


    public class OthelloAI 
    {

        public string GetName()
        {
            return "Goldorak";
        }

        #region IPlayable interface
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetWhiteScore()
        {
            return 32;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public int GetBlackScore()
        {
            return 32;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool PlayMove(int column, int line, bool isWhite)
        {
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="column"></param>
        /// <param name="line"></param>
        /// <param name="isWhite"></param>
        /// <returns></returns>
        public bool IsPlayable(int column, int line, bool isWhite)
        {
            return true;
        }

        public Tuple<int, int> GetNextMove(int[,] game, int level, bool isWhiteTurn)
        {
            return new Tuple<int, int>(-1, -1);
        }

        #endregion
    }
}
