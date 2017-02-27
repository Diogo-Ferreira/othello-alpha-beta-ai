using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AlphaBetaTeam11Library
{
    /// <summary>
    /// Board logic du jeu, gère les règles du jeu et ainsi que le score
    /// </summary>
    [Serializable()]
     public class LogicBoard : IPlayable.IPlayable, ISerializable

    {
        public static int WIDTH => 8;

        public static int HEIGHT => 8;

        ///<summary>
        /// Board logic du jeu, si case vide null
        /// </summary>
        public Pawn[,] Board { get; set; } = new Pawn[WIDTH, HEIGHT];

        /// <summary>
        ///  Directions prédéfinis des recherches
        /// </summary>
        readonly Pawn.Direction[] directions = new Pawn.Direction[8]{
            new Pawn.Direction( 0, -1), // North
            new Pawn.Direction( 0,  1), // South
            new Pawn.Direction(-1,  0), // west
            new Pawn.Direction( 1,  0), // east

            new Pawn.Direction(-1, -1), // north west
            new Pawn.Direction( 1,  1), // south east
            new Pawn.Direction( 1, -1), // north east
            new Pawn.Direction(-1,  1), // south west
        };

        /// <summary>
        /// Constructeur vide pour la sérialisation
        /// </summary>
        public LogicBoard(){}


        /// <summary>
        /// Ajoute un pion à la coord spécifié
        /// </summary>
        /// <param name="x">col</param>
        /// <param name="y">row</param>
        /// <param name="color">couleur</param>
        public void addPawn(int x, int y, Pawn.Colors color )
        {
            Board[y,x] = new Pawn(color, x, y);
        }

        /// <summary>
        /// Charge le board depuis un tableaux 2D
        /// </summary>
        public void fillBoard()
        {
            int[,] fake =
            {
                {0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0},
                {0,0,0,1,2,0,0,0},
                {0,0,0,2,1,0,0,0},
                {0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0},
                {0,0,0,0,0,0,0,0},
            };

            for (var i = 0; i < HEIGHT; i++)
            {
                for (var j = 0; j < WIDTH; j++)
                {
                    if (fake[i, j] != 0)
                    {
                        var color = fake[i, j] == 1 ? Pawn.Colors.White : Pawn.Colors.Black;
                        Board[i, j] = new Pawn(color, j, i);
                    }
                    else
                    {
                        Board[i, j] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Cherche dans la direction spécifiée un pion de la couleur
        ///  spécifié et tourne les pions croisés dans cette directions
        /// </summary>
        /// <param name="color">couleur à chercher</param>
        /// <param name="column">col</param>
        /// <param name="line">row</param>
        /// <param name="deltaX">directionX</param>
        /// <param name="deltaY">directionY</param>
        /// <param name="pawnsCrossed">liste des pions croisés (sortie)</param>
        /// <returns>True si pion même couleur trouvé, Fase Sinon</returns>
        private bool SearchInDirection(Pawn.Colors color,int column, int line, int deltaX, int deltaY, ICollection<Pawn> pawnsCrossed = null)
        {
            var hit = false;
            var stop = false;
            var atLeastOne = false;

            while (!stop)
            {
                line += deltaY;
                column += deltaX;

                if (line >= HEIGHT || line < 0 || column >= WIDTH || column < 0)
                {
                    hit = false;
                    stop = true;
                }
                else
                {
                    var currentPawn = Board[line, column];

                    if (currentPawn == null)
                    {
                        hit = false;
                        stop = true;
                    }

                    else if (currentPawn.Color == color)
                    {
                        hit = atLeastOne;
                        stop = true;
                    }
                    else
                    {
                        atLeastOne = true;
                    }


                    if (!stop)
                    {
                        pawnsCrossed?.Add(currentPawn);
                    }
                }
            }

            return hit;
        }
        
        public bool IsPlayable(int column, int line, bool isWhite)
        {
            //Position empty ?
            if (Board[line,column] != null)
                return false;

            var ourColor = isWhite ? Pawn.Colors.White : Pawn.Colors.Black;

            var currentPawn = new Pawn.Direction(x: column, y: line);

            var found = false;

            directions.ToList().ForEach(direction =>
            {
                if (!found && currentPawn.y < HEIGHT && currentPawn.y >= 0 && currentPawn.x < WIDTH && currentPawn.x >= 0)
                {
                    found = SearchInDirection(ourColor, currentPawn.x, currentPawn.y, direction.x, direction.y);
                }
            });

            return found;
        }

        public bool PlayMove(int column, int line, bool isWhite)
        {
            var color = isWhite ? Pawn.Colors.White : Pawn.Colors.Black;

            directions.ToList().ForEach(direction =>
            {   
                var pawnsCrossed = new List<Pawn>();

                var playable = SearchInDirection(color, column, line, direction.x, direction.y,pawnsCrossed);

                if(playable)
                    pawnsCrossed.ForEach(p => p.Flip());
            });

            addPawn(column, line, color);

            return false;
        }

        public Tuple<char, int> getNextMove(int[,] game, int level, bool whiteTurn)
        {
            throw new NotImplementedException();
        }
        
        public int GetWhiteScore() => (from pawn in Board.Cast<Pawn>()
                                        where pawn?.Color == Pawn.Colors.White
                                        select pawn).Count();
        

        public int GetBlackScore() => (from pawn in Board.Cast<Pawn>()
                                       where pawn?.Color == Pawn.Colors.Black
                                       select pawn).Count();

        /// <summary>
        /// Constructeur de la sérialisation
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public LogicBoard(SerializationInfo info, StreamingContext ctxt)
        {
            Board = (Pawn[,])info.GetValue("Board", typeof(Pawn[,]));

        }

        /// <summary>
        /// Méthode permettant la sérialisation, vient de l'interface ISerializable
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext ctxt)
        {
            info.AddValue("Board", Board);
        }

        public string GetName()
        {
            throw new NotImplementedException();
        }

        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {
            throw new NotImplementedException();
        }

        public int[,] GetBoard()
        {
            throw new NotImplementedException();
        }

        public class TreeNode
        {
            public int x;
            public int y;
            public LogicBoard board;

            public double eval()
            {
                double mobility = 0;

                int maxCoin = 0;//Les pions les plus présents->devient le maxPlayer
                int minCoin = 0;

                int maxMobility = 0;//Regarde combien de mouvement sont possibles.
                int minMobility = 0;

                if (board.GetBlackScore() > board.GetWhiteScore())//Black est max
                {
                    maxCoin = board.GetBlackScore();
                    minCoin = board.GetWhiteScore();

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (board.IsPlayable(i, j, false))//On regarde les noirs
                            {
                                maxMobility++;
                            }
                            else if (board.IsPlayable(i, j, true))
                            {
                                minMobility++;
                            }
                        }
                    }
                }
                else
                {
                    maxCoin = board.GetWhiteScore();
                    minCoin = board.GetBlackScore();

                    for (int i = 0; i < 8; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            if (board.IsPlayable(i, j, false))//On regarde les noirs
                            {
                                minMobility++;
                            }
                            else if (board.IsPlayable(i, j, true))
                            {
                                maxMobility++;
                            }
                        }
                    }
                }
                //Parity
                double parity = 100 * (maxCoin - minCoin) / (maxCoin - minCoin);
                //Mobility
                if (maxMobility + minMobility != 0)
                {
                    mobility = 100 * (maxMobility - minMobility) / (maxMobility + minMobility);
                }
                else
                {
                    mobility = 0;
                }
                //Corners captured
                //Stability
                //Score from : https://github.com/kartikkukreja/blog-codes/blob/master/src/Heuristic%20Function%20for%20Reversi%20(Othello).cpp
                double score = (10 * parity) + (78.922 * mobility);
                return score;
            }

            public bool final()
            {
                return false;
            }

            public TreeNode[] ops()
            {
                return null;
            }

            public TreeNode apply(TreeNode op)
            {
                return null;
            }
            
        }

        public Tuple<double, TreeNode> alphabeta(TreeNode root, int depth, double minOrMax, double parentValue)
        {
            if( depth == 0 || root.final())
                return new Tuple<double, TreeNode>(root.eval(), null);

            var optVal = minOrMax*-int.MaxValue;
            TreeNode optOp = null;

            foreach (var op in root.ops())
            {
                var newOp = root.apply(op);
                var val = alphabeta(newOp, depth - 1, -minOrMax, optVal).Item1;
                if (val*minOrMax > optVal*minOrMax)
                {
                    optVal = val;
                    optOp = op;

                    if (optVal*minOrMax > parentValue*minOrMax)
                        break;
                }
            }

            return new Tuple<double, TreeNode>(optVal,optOp);
        }
    }
}
