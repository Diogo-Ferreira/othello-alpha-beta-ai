﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace AlphaBetaTeam11Library
{
    /// <summary>
    /// Board logic du jeu, gère les règles du jeu et ainsi que le score
    /// ATTENTION : L'IA est lente car le board est un board d'objet Pawn et non de int,
    /// ce qui est plus lent à cloner et à convertir à chaque fois.
    /// 
    /// C'est une erreur de conception qui vient du projet précédent.
    /// </summary>
    [Serializable()]
     public class LogicBoard : IPlayable.IPlayable, ISerializable

    {
        public static int WIDTH => 8;

        public static int HEIGHT => 8;


        //For debug
        List<string> history = new List<string>();

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
        public LogicBoard()
        {
            fillBoard();
        }

        /// <summary>
        /// Créer un board suivant un noeud (Deep Copy)
        /// </summary>
        /// <param name="boardToCopy"></param>
        public LogicBoard(TreeNode boardToCopy)
        {
            for(var i = 0; i < LogicBoard.HEIGHT; i++)
            {
                for (var j = 0; j < LogicBoard.WIDTH; j++)
                {

                    if (boardToCopy.NodeBoard.Board[j, i] != null)
                    {

                        var element = boardToCopy.NodeBoard.Board[j, i];

                        Board[j, i] = new Pawn
                        {
                            pos = new Pawn.Direction
                            {
                                x = element.pos.x,
                                y = element.pos.x
                            },
                            color = element.Color
                        };
                    }
                    else
                    {
                        Board[j, i] = null;
                    }
                }
            }
        }


        /// <summary>
        /// Ajoute un pion à la coord spécifié
        /// </summary>
        /// <param name="x">col</param>
        /// <param name="y">row</param>
        /// <param name="color">couleur</param>
        /// 
        public void addPawn(int col, int line, Pawn.Colors color )
        {
            Board[col,line] = new Pawn(color, col, line);
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
                    var currentPawn = Board[column, line];

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
            if (Board[column, line] != null)
                return false;

            var ourColor = isWhite ? Pawn.Colors.White : Pawn.Colors.Black;

            var currentPawn = new Pawn.Direction() {x = column, y = line};

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
            return "Debrot_Ferreira";
        }

        public Tuple<int, int> GetNextMove(int[,] game, int level, bool whiteTurn)
        {

            // On créer le premier noeud
            var root = new TreeNode()
            {
                NodeBoard = Clone(this),
                IsWhite = whiteTurn,
                IsRoot = true
            };


            // Algo 
            var result = Alphabeta(root, level, 1, int.MaxValue);

            if (result.Item2 != null)
                history.Add(result.Item2.move.ToString());
            

            return result.Item2 != null ? new Tuple<int, int>(result.Item2.move.pos.x, result.Item2.move.pos.y) : new Tuple<int, int>(-1,-1);
           
           
        }

        public int[,] GetBoard()
        {

            //Conversion du board en pawn en board de int
            var outBoard = new int[WIDTH, HEIGHT];

            for (var i = 0; i < HEIGHT; i++)
            {
                for (var j = 0; j < WIDTH; j++)
                {
                    outBoard[j, i] = Board[j, i] == null ? -1 : Board[j, i].IsWhite ? 0 : 1;
                }
            }

            return outBoard;
        }


        /// <summary>
        /// Algo alphabeta
        /// </summary>
        /// <param name="root"></param>
        /// <param name="depth"></param>
        /// <param name="minOrMax"></param>
        /// <param name="parentValue"></param>
        /// <returns></returns>
        private Tuple<double, TreeNode> Alphabeta(TreeNode root, int depth, double minOrMax, double parentValue)
        {

            if (depth == 0 || root.Final())
                return new Tuple<double, TreeNode>(root.Eval(), null);

            var optVal = minOrMax * -int.MaxValue;
            TreeNode optOp = null;
            foreach (var op in root.Ops())
            {
                var newOp = root.Apply(op);
                var val = Alphabeta(newOp, depth - 1, -minOrMax, optVal).Item1;

                if (val * minOrMax > optVal * minOrMax)
                {
                    optVal = val;
                    optOp = op;
                    
                    if (optVal * minOrMax > parentValue * minOrMax)
                        break;
                }
            }

            return new Tuple<double, TreeNode>(optVal, optOp);
        }

        /// <summary>
        /// Permet de cloner un objet, attention -->  lent !
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static T Clone<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }
    }
    
}
