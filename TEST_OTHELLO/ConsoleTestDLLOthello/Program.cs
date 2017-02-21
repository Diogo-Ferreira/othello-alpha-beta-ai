/*
 * Projet TEST pour tournoi d'IA du jeu Othello
 * 
 * Le programme référence deux IA implémentées dans un Assembly  IA1.dll et IA2.dll
 * Les IA et le projet TESTS implémentent une interface commune définie par l'assemblage IPlayable.dll
 * Les IA doivent avoir un constructeur par défaut et la classe qui implémente IPlayable contenir "Board" dans son nom
 * 
 * + Make GetNextMove calls async and set a time limit for timeout ?
  * */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleTestDLLOthello
{
    class Program
    {
        public const int PASS = -1;
        /// <summary>
        /// Displays in the console the board given as argument with players scores
        /// </summary>
        /// <param name="board"></param>
        static public void DrawBoard(int[,] board, string name, int blackScore, int whiteScore)
        {
            Console.WriteLine(name + "\tBLACK [X]:" + blackScore + "\tWHITE [O]:" + whiteScore);
            Console.WriteLine("  A B C D E F G H");
            for (int line = 0; line < 8; line++)
            {
                Console.Write($"{(line + 1)}");
                for (int col = 0; col < 8; col++)
                {
                    Console.Write((board[col, line] == (int)OthelloLib.EtatCase.Empty) ? " -" : (board[col, line] == (int)OthelloLib.EtatCase.White) ? " O" : " X");
                }
                Console.Write("\n");
            }
            Console.WriteLine();
            Console.WriteLine();
        }

        /// <summary>
        /// verify if both boards given as 2D int arrays are equal
        /// </summary>
        /// <param name="b1">First player board game status</param>
        /// <param name="b2">Second player board game status</param>
        /// <returns>Boolean comparison result (what did you think?)</returns>
        public static bool boardCompare(int[,] b1, int[,] b2)
        {
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    if (b1[i, j] != b2[i, j])
                        return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Console application that plays an othello game with 2 AIs implementing IPlayable interface
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {

            #region LOAD_IAs

            //find all DLLs with name starting with "OthelloIA" in the executable folder
            List<Assembly> IAPlayers = new List<Assembly>();
            string path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            foreach (string dll in Directory.GetFiles(path, "OthelloIA*.dll"))
                IAPlayers.Add(Assembly.LoadFile(dll));
            foreach (Assembly assembly in IAPlayers)
                Assembly.Load(assembly.FullName);
            #endregion

            // 1. Connect two players by either loading a)dynamically or b)statically the teams assemblies
            IPlayable.IPlayable player1 = null, player2 = null, serverController = null;

            //a)Recover the types from the DLL assemblies using reflection for the 2 players
            if (IAPlayers.Count >= 2)
            {
                Type[] T1 = IAPlayers[0].GetTypes();
                for (int i = 0; i < T1.Count(); i++)
                {
                    if (T1[i].Name.Contains("Board"))       // the IA's class that implements IPlayable must have "Board" in its name. E.g OthelloBoard, TheBoard, MyBoard, ...
                        player1 = (IPlayable.IPlayable)Activator.CreateInstance(T1[i]);  // requires a default constructore
                }
                if (player1 == null)
                    player1 = new OthelloIA2.OthelloBoard();
                Type[] T2 = IAPlayers[1].GetTypes();        //or    GetType ("OthelloIA2.OthelloBoard");
                for (int i = 0; i < T2.Count(); i++)
                {
                    if (T2[i].Name.Contains("Board"))       // the IA's class that implements IPlayable must have "Board" in its name. E.g OthelloBoard, TheBoard, MyBoard, ...
                        player2 = (IPlayable.IPlayable)Activator.CreateInstance(T2[i]);  // requires a default constructore
                }
                if (player2 == null)
                    player2 = new OthelloIA2.OthelloBoard();
            }
            else // b) add a reference to your class assembly in the project and instantiate it 
            {
                player1 = new OthelloIA2.OthelloBoard();   // for example
                player2 = new OthelloIA2.OthelloBoard();   // for example
            }
            // The Game controller
            serverController = new OthelloLib.Board();             //reference interne au projet

            // Initialize the board for initial display
            int[,] theBoard = (serverController as OthelloLib.Board).GetBoard();

            //UI PART draw the 3 boards: reference, player1, player2 
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine("REFEREE : " + serverController.GetName());
            Console.WriteLine("TEAM 1  : " + player1.GetName());
            Console.WriteLine("TEAM 2  : " + player2.GetName());
            (serverController as OthelloLib.Board).DrawBoard();
            //DrawBoard(player1.GetBoard(), player1.GetName(), player1.GetBlackScore(), player1.GetWhiteScore());
            //DrawBoard(player2.GetBoard(), player2.GetName(), player2.GetBlackScore(), player2.GetWhiteScore());

            #region GAMELOOP
            //GAME LOOP
            int[,] refBoard = (serverController as OthelloLib.Board).GetBoard();
            int[,] board1 = player1.GetBoard();
            int[,] board2 = player2.GetBoard();

            Tuple<int, int> playerMove = null;
            int passCount = 0;
            bool testPlayer1, testPlayer2;
            bool whitePlays = false;
            IPlayable.IPlayable activePlayer = player1;     // player 1 begins playing black

            while (boardCompare(refBoard, board1) && boardCompare(refBoard, board2) && (passCount<2))
            {
                int totalScore = serverController.GetBlackScore() + serverController.GetWhiteScore();
                Console.Clear();
                try
                {
                    playerMove = activePlayer.GetNextMove(theBoard, 5, whitePlays);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message + "\n\n" + e.StackTrace);
                }
                Console.Write(whitePlays ? "\n[O]" : "\n[X]");
                // check move validity
                if ((playerMove.Item1 == PASS) && (playerMove.Item1 == PASS))
                {
                    Console.WriteLine("PASS");
                    passCount++;
                    ///TODO verify if no move was possible (no playable move otherwise display a msg or throw something
                    if ((serverController as OthelloLib.Board).GetPossibleMoves(whitePlays).Count!=0)
                        throw new Exception($"you shall NOT PASS {activePlayer}!");
                }
                else
                {
                    passCount = 0;
                    Console.WriteLine($"{activePlayer.GetName()}: {(char)('A' + playerMove.Item1)}{1 + playerMove.Item2}");
                    // check validity
                    if ((serverController as OthelloLib.Board).IsPlayable(playerMove, whitePlays))
                    
                    {
                        Console.WriteLine("Coup valide");
                        // play the move for both players and referee
                        serverController.PlayMove(playerMove.Item1, playerMove.Item2, whitePlays);
                        testPlayer1 = player1.PlayMove(playerMove.Item1, playerMove.Item2, whitePlays);  // no verification here
                        testPlayer1 = player2.PlayMove(playerMove.Item1, playerMove.Item2, whitePlays);  // no verification here

                        // compare boards for validity
                        refBoard = (serverController as OthelloLib.Board).GetBoard();
                        board1 = player1.GetBoard();
                        board2 = player2.GetBoard();
                        testPlayer1 = boardCompare(refBoard, board1);
                        testPlayer2 = boardCompare(refBoard, board2);
                        if (testPlayer1 && testPlayer2)  // we only need to draw one board now
                        {
                            (serverController as OthelloLib.Board).DrawBoard();
                        }
                        else if (!testPlayer1)
                            throw new Exception("Board state of player 1 is incorrect");
                        else if (!testPlayer2)
                            throw new Exception("Board state of player 2 is incorrect");
                    }
                }
                // SWAP players and color
                whitePlays = !whitePlays;
                activePlayer = (activePlayer == player1) ? player2 : player1;
                
                System.Threading.Thread.Sleep(200); // slow down game speed or //Console.ReadKey();
            } // end of GAMELOOP
            #endregion

            if (serverController.GetBlackScore() > serverController.GetWhiteScore())
                Console.WriteLine($"{player1.GetName()} WINS!" + player1.GetBlackScore() + " - " + player1.GetWhiteScore());
            else
                Console.WriteLine($"{player2.GetName()} WINS!" + player1.GetWhiteScore() + " - " + player1.GetBlackScore());
            Console.WriteLine("END OF GAME, FAREWELL!");
            Console.ReadKey();
        }
    }
}