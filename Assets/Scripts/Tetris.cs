namespace Tetris
{
    using System;

    public enum TetrominoType { I, J, L, O, S, T, Z }

    public enum TetrominoOrientation:int { UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3 }

    /// <summary>
    /// A virtual piece that the playr can control
    /// This clas has information about the tetromino's shape and type
    /// </summary>
    public class Tetromino
    {
        public TetrominoType TetrominoType { get; private set; }

        //the orientation the tetromino is locket at the momment
        public TetrominoOrientation Orientation { get; private set; } = TetrominoOrientation.UP;

        //the virtual information of the blocks this tetromino is occupying
        public bool[,] Matrix { get; private set; }

        public bool this[int i, int j]
        {
            get => Matrix[i, j];
            set => Matrix[i, j] = value;
        }

        public Tetromino(TetrominoType tetrominoType, bool[,] matrix)
        {
            TetrominoType = tetrominoType;
            Matrix = matrix;
        }

        public void SetOrientation(TetrominoOrientation orientation)
        {
            Orientation = orientation;
        }

        public void SetMatrix(bool[,] matrix)
        {
            Matrix = matrix;
        }
    }

    /// <summary>
    /// The information about the space of the game and blocks
    /// The grid can calculate if it's possible to rotate or move a block
    /// By default, it is a 20 lines x 10 columns matrix
    /// The minimum acceptable is 4x4, or the blocks will never move/rotate 
    /// </summary>
    public class Grid 
    {
        public int Lines { get; private set; } = 20;
        public int Columns { get; private set; } = 10;

        public bool[,] Matrix { get; private set; }

        public bool this[int i, int j]
        {
            get => Matrix[i, j];
            set => Matrix[i, j] = value;
        }

        public Grid()
        {
            Matrix = new bool[Lines, Columns];
        }

        public Grid(int lines, int columns)
        {
            Lines = lines;
            Columns = columns;
            Matrix = new bool[Lines, Columns]; 
        }

        public void UpdateMatrixPosition(int i, int j, bool status)
        {
            Matrix[i, j] = status;
        }
    }
}