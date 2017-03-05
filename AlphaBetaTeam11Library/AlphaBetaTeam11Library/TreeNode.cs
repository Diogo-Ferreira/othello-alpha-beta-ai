using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaBetaTeam11Library
{
    public class TreeNode
    {
        public Pawn move; // Coup à joué
        public LogicBoard NodeBoard; // Tableau de jeu courant du noeud
        public bool IsWhite;
        public bool IsRoot;

        readonly int[,] theMatrix = {
                { 100, -10, 11, 6, 6, 11, -10, 100 },
                {-10, -20,  1, 2, 2,  1, -20, -10},
                { 10,   1,  5, 4, 4,  5,   1,  10},
                {  6,   2,  4, 2, 2,  4,   2,   6},
                {  6,   2,  4, 2, 2,  4,   2,   6},
                { 10,   1,  5, 4, 4,  5,   1,  10},
                {-10, -20,  1, 2, 2,  1, -20, -10},
                { 100, -10, 11, 6, 6, 11, -10,  100}
            };

       
        /// <summary>
        /// Evaluation du jeu basé sur une matrice
        /// </summary>
        /// <returns></returns>
        public double Eval()
        {
            return  (move == null ? 0 :theMatrix[move.pos.y, move.pos.x]);
        }


        /// <summary>
        /// Vérifie si le jeu courant du noeud est final
        /// </summary>
        /// <returns></returns>
        public bool Final()
        {
            var possibleMoves = PossibleMoves(IsWhite).Count;
            return possibleMoves == 0;
        }


        /// <summary>
        /// Récupère les coups possible pour le joueur courant
        /// </summary>
        /// <param name="IsWhite">Couleur du joueur courant</param>
        /// <returns>Liste des coups</returns>
        public List<Pawn> PossibleMoves(bool IsWhite)
        {
            var possiblePawns = new List<Pawn>();

            for (var i = 0; i < LogicBoard.HEIGHT; i++)
            {
                for (var j = 0; j < LogicBoard.WIDTH; j++)
                {
                    if (NodeBoard.IsPlayable(j, i, IsWhite))
                    {
                        possiblePawns.Add(new Pawn
                        {
                            pos = new Pawn.Direction
                            {
                                x = j,
                                y = i
                            },
                            color = IsWhite ? Pawn.Colors.White : Pawn.Colors.Black
                        });
                    }
                }
            }
            return possiblePawns;
        }

        /// <summary>
        /// Retourne les coups possibles à jouer pour l'état courant
        /// </summary>
        /// <returns>Tableau de treenode</returns>
        public TreeNode[] Ops()
        {

            //On récupère les coups possibles
            var possibleMoves = PossibleMoves(IsWhite);

            var ops = new TreeNode[possibleMoves.Count];
            var i = 0;

            //Pour chaque coup, on créer un treenode
            possibleMoves.ForEach(p =>
            {
                var treeNode = new TreeNode
                {
                    IsWhite = IsWhite,
                    move = p,
                    NodeBoard = NodeBoard,
                    IsRoot = false
                };
                ops[i] = treeNode;
                i++;
            });

            return ops;

        }

        /// <summary>
        /// Applique le noeud passé en paramètre
        /// </summary>
        /// <param name="op">Noeud à appliquer</param>
        /// <returns>Un nouveau noeud avec le coup joué</returns>
        public TreeNode Apply(TreeNode op)
        {
            var newOp = new TreeNode
            {
                IsRoot = false,
                IsWhite = IsWhite,
                move = new Pawn
                {
                    pos = new Pawn.Direction
                    {
                        x = op.move.pos.x,
                        y = op.move.pos.y
                    },
                    color = op.move.color
                },
                NodeBoard = new LogicBoard(op)
            };

            newOp.NodeBoard.PlayMove(op.move.pos.x, op.move.pos.y, IsWhite);

            return newOp;

        }
    }
}
