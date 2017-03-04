using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlphaBetaTeam11Library
{
    class TreeNode
    {
        public Pawn move;
        public LogicBoard NodeBoard;
        public bool IsWhite;
        public bool IsRoot = false;

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
                { 100, -10, 11, 6, 6, 11, -10, 100, },
                {-10, -20,  1, 2, 2,  1, -20, -10,},
                { 10,   1,  5, 4, 4,  5,   1,  10,},
                {  6,   2,  4, 2, 2,  4,   2,   6,},
                {  6,   2,  4, 2, 2,  4,   2,   6,},
                { 10,   1,  5, 4, 4,  5,   1,  10,},
                {-10, -20,  1, 2, 2,  1, -20, -10,},
                { 100, -10, 11, 6, 6, 11, -10,  100,}
            };
        public double Eval()
        {
            var earlyGame = (NodeBoard.GetBlackScore() + NodeBoard.GetWhiteScore() < 40);
            var countGoodness = 0.0;
            const int K1 = 2;
            const int K2 = 2;
            const int K3 = 4;

            var myMobility = PossibleMoves(IsWhite).Count;
            var hisMobility = PossibleMoves(!IsWhite).Count;

            var genMobility = 0;

            if(myMobility + hisMobility != 0)
                genMobility = 100 * (myMobility - hisMobility) / (myMobility + hisMobility);
     

            //double parity = 100 * (maxCoin - minCoin) / (minCoin + maxCoin);
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
                
           

            if (earlyGame)
            {
                // give-away in the early game
                countGoodness = K1 * ((IsWhite ? NodeBoard.GetBlackScore() : NodeBoard.GetWhiteScore()) - (IsWhite ? NodeBoard.GetWhiteScore() : NodeBoard.GetBlackScore()));
            }
            else
            {
                // take-back later in the game
                countGoodness = K2 * ((IsWhite ? NodeBoard.GetWhiteScore() : NodeBoard.GetBlackScore()) - (IsWhite ? NodeBoard.GetBlackScore() : NodeBoard.GetWhiteScore()));
            }
            var positionalGoodness = K3 * (move == null ? 0 :theMatrix[move.pos.y,move.pos.x]);
            //Console.WriteLine(countGoodness + positionalGoodness);
            //return new Random().Next(0, 100)*(move == null ? 0 : move.pos.x) +
              //     new Random().Next(0, 100)*(move == null ? 0 : move.pos.y);
            //return new Random().NextDouble() * countGoodness + positionalGoodness;

            var rnd = new Random().Next(0, 10) * (move == null ? 0 : move.pos.x) + new Random().Next(0, 10)*(move == null ? 0 : move.pos.y);
            return ((10 * parity) + (78.922 * rnd * genMobility) + (move == null ? 0 : 801.724 * theMatrix[move.pos.y, move.pos.x]));
            //return ((IsWhite ? NodeBoard.GetWhiteScore() : NodeBoard.GetBlackScore()) - (IsWhite ? NodeBoard.GetBlackScore() : NodeBoard.GetWhiteScore()));
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
                        possiblePawns.Add(new Pawn()
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
                var treeNode = new TreeNode()
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
            var newOp = new TreeNode()
            {
                IsRoot = false,
                IsWhite = IsWhite,
                move = new Pawn()
                {
                    pos = new Pawn.Direction()
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

                        newOp.NodeBoard.Board[j, i] = new Pawn()
                        {
                            pos = new Pawn.Direction()
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
