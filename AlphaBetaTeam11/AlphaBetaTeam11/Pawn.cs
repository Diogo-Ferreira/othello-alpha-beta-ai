using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Othello
{
    /// <summary>
    /// Classe symbolisant les pions
    /// </summary>
    [Serializable()]
    public class Pawn: ISerializable
    {
        /// <summary>
        /// Structure pour les directions
        /// </summary>
        [Serializable()]
        public struct Direction: ISerializable
        {
            public int x, y;

            /// <summary>
            /// Constructeur de direction avec paramètres
            /// </summary>
            /// <param name="x"></param>
            /// <param name="y"></param>
            public Direction(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
            /// <summary>
            /// Contructeur de direction pour la serialization
            /// </summary>
            /// <param name="info"></param>
            /// <param name="ctxt"></param>
            public Direction(SerializationInfo info, StreamingContext ctxt)
            {
                x = (int)info.GetValue("X", typeof(int));
                y = (int)info.GetValue("Y", typeof(int));
            }
            /// <summary>
            /// Méthode permettant la sérialisation de la structure Direction, vient de ISerializable
            /// </summary>
            /// <param name="info"></param>
            /// <param name="context"></param>
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("X", x);
                info.AddValue("Y", y);
            }

            /// <summary>
            /// Redifinition de l'opérateur +
            /// </summary>
            /// <param name="d1"></param>
            /// <param name="d2"></param>
            /// <returns></returns>
            public static Direction operator +(Direction d1, Direction d2)
            {
                var tmp = new Direction(x: d1.x, y: d1.y);
                tmp.x += d2.x;
                tmp.y += d2.y;
                return tmp;
            }
        }

        public Direction pos;

        public enum Colors {White, Black};

        Colors color;
        
        /// <summary>
        /// Constructeur avec paramètres
        /// </summary>
        /// <param name="color"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public Pawn(Colors color, int x, int y)
        {
            this.color = color;
            pos = new Direction(x:x,y:y);
        }

        public Colors Color => color;

        public bool IsWhite => color == Colors.White;

        public void Flip() => color = color == Colors.White ? Colors.Black : Colors.White;

        /// <summary>
        /// Redéfinition de la méthode toString
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"{color} {pos.x} {pos.y}";
        }

        /// <summary>
        /// Constructeur de sérialisation
        /// </summary>
        /// <param name="info"></param>
        /// <param name="ctxt"></param>
        public Pawn(SerializationInfo info, StreamingContext ctxt)
        {
            pos = (Direction)info.GetValue("Direction", typeof(Direction));
            color = (Colors)info.GetValue("Colors", typeof(Colors));
        }
        /// <summary>
        /// Méthode permettant la sérialisation d'un pion, vient de ISerializable
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Direction", pos);
            info.AddValue("Colors", color);
        }
    }
}
