using System;
using System.Collections.Generic;
using System.Linq;

namespace AlphaBetaTeam11Library
{
    class TreeNode
    {
        public Pawn move;
        public LogicBoard NodeBoard;
        public bool IsWhite;
        public bool IsRoot;

        /*readonly int[,] theMatrix = {
            { 30, -25, 10, 5, 5, 10, -25, 30, },
            {-25, -25,  1, 1, 1,  1, -25, -25,},
            { 10,   1,  5, 2, 2,  5,   1,  10,},
            {  5,   1,  2, 1, 1,  2,   1,   5,},
            {  5,   1,  2, 1, 1,  2,   1,   5,},
            { 10,   1,  5, 2, 2,  5,   1,  10,},
            {-25, -25,  1, 1, 1,  1, -25, -25,},
            { 30, -25, 10, 5, 5, 10, -25,  30,}
        };*/

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

        private readonly Tuple<int,int>[] importantCorners =
        {
            new Tuple<int, int>(0,0),
            new Tuple<int, int>(0,7),
            new Tuple<int, int>(7,0),
            new Tuple<int, int>(7,7),
        };
        public double Eval()
        {

            // Mobility
            var myMobility = PossibleMoves(IsWhite).Count;
            var hisMobility = PossibleMoves(!IsWhite).Count;
            var genMobility = 0;
            if(myMobility + hisMobility != 0)
                genMobility = 100 * (myMobility - hisMobility) / (myMobility + hisMobility);
     
            
            // Parity
            var parity = 0;
            var blackScore = NodeBoard.GetBlackScore();
            var whiteScore = NodeBoard.GetWhiteScore();

            var parityDenum = ((IsWhite ? whiteScore : blackScore) +
                               (IsWhite ? blackScore : whiteScore));

            if (parityDenum != 0)
            {
                parity = 100 *
                             ((IsWhite ? whiteScore : blackScore) -
                              (IsWhite ? blackScore : whiteScore))
                             /
                             parityDenum;
            }
                

            //Important corner occupance
            var myCorners = 0;
            var hisCorners = 0;
            foreach (var corner in importantCorners)
            {
                if (NodeBoard.Board[corner.Item1, corner.Item2] == null) break;

                if (NodeBoard.Board[corner.Item1, corner.Item2].IsWhite == IsWhite)
                {
                    myCorners++;
                }
                else
                {
                    hisCorners++;
                }

            }

            var cornerOccupance = 25*(myCorners - hisCorners);
           
            //var rnd = new Random().Next(0, 10) * (move == null ? 0 : move.pos.x) + new Random().Next(0, 10)*(move == null ? 0 : move.pos.y);


            return ((10 * parity) + (78.922 * genMobility) + (move == null ? 0 : 382.026 * theMatrix[move.pos.y, move.pos.x]) + (801.724 * cornerOccupance));
        }

        public bool Final()
        {
            var possibleMoves = PossibleMoves(IsWhite).Count;
            return possibleMoves == 0;
        }

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

        public TreeNode[] Ops()
        {


            var possibleMoves = PossibleMoves(IsWhite);

            var ops = new TreeNode[possibleMoves.Count];
            var i = 0;
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
                NodeBoard = new LogicBoard()
            };

            // TODO: create constructor for this
            for (var i = 0; i < LogicBoard.HEIGHT; i++)
            {
                for (var j = 0; j < LogicBoard.WIDTH; j++)
                {

                    if (op.NodeBoard.Board[j, i] != null)
                    {

                        newOp.NodeBoard.Board[j, i] = new Pawn
                        {
                            pos = new Pawn.Direction
                            {
                                x = op.NodeBoard.Board[j, i].pos.x,
                                y = op.NodeBoard.Board[j, i].pos.x
                            },
                            color = op.move.color
                        };
                    }
                    else
                    {
                        newOp.NodeBoard.Board[j, i] = null;
                    }
                }
            }

            newOp.NodeBoard.PlayMove(op.move.pos.x, op.move.pos.y, IsWhite);

            return newOp;

        }
    }
}
