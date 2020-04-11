namespace Tetris
{
    using System;

    public enum RotationDirection { CLOCKWISE, COUNTER_CLOCKWISE }

    /// <summary>
    /// Responsible for creating tetrominoes
    /// </summary>
    public static class TetrominoUtils
    {
        /// <summary>
        /// Gets a new tetromino of the chosen type
        /// </summary>
        /// <param name="tetrominoType">The type of the tetromino</param>
        /// <returns>A new tetromino</returns>
        public static Tetromino GetTetromino(TetrominoType tetrominoType)
        {
            return CreateTetromino(tetrominoType);
        }

        /// <summary>
        /// Gets a random type tetromino
        /// </summary>
        /// <returns>A new tetromino with a random type</returns>
        public static Tetromino GetRandomTetromino()
        {
            Array tetrominoTypeValues =
                Enum.GetValues(typeof(TetrominoType));

            Random random = new Random();

            TetrominoType tetrominoType =
                (TetrominoType)tetrominoTypeValues.GetValue(random.Next(tetrominoTypeValues.Length));

            return CreateTetromino(tetrominoType);
        }

        /// <summary>
        /// Creates a tetromino
        /// </summary>
        /// <param name="tetrominoType">The type of the tetromino</param>
        /// <returns>A new tetromino</returns>
        private static Tetromino CreateTetromino(TetrominoType tetrominoType)
        {
            Tetromino tetromino = new Tetromino(tetrominoType, GetTetrominoMatrix(tetrominoType));

            return tetromino;
        }

        /// <summary>
        /// Generates a block matrix that represents the shape of the tetromino based on the type
        /// This is based on the SRS models
        /// </summary>
        /// <param name="tetrominoType">The type of the tetromino</param>
        /// <returns>A new matrix with the shape representation of the tetromino</returns>
        private static bool[,] GetTetrominoMatrix(TetrominoType tetrominoType)
        {
            //the tetromino's "bounds"
            //all the tetromino's blocks are contained inside a n x m matrix
            //and are all centered vertically and horizontally
            //except for the I and O types that are shifted up 1 unity
            bool[,] matrix;

            //fill the matrix accordingly to the tetromino's type
            switch (tetrominoType)
            {
                case TetrominoType.I:
                    matrix = new bool[4, 4];
                    matrix[1, 0] = matrix[1, 1] = matrix[1, 2] = matrix[1, 3] = true;
                    break;
                case TetrominoType.J:
                    matrix = new bool[3, 3];
                    matrix[0, 0] = matrix[1, 0] = matrix[1, 1] = matrix[1, 2] = true;
                    break;
                case TetrominoType.L:
                    matrix = new bool[3, 3];
                    matrix[0, 2] = matrix[1, 0] = matrix[1, 1] = matrix[1, 2] = true;
                    break;
                case TetrominoType.O:
                    matrix = new bool[3, 4];
                    matrix[0, 1] = matrix[0, 2] = matrix[1, 1] = matrix[1, 2] = true;
                    break;
                case TetrominoType.S:
                    matrix = new bool[3, 3];
                    matrix[0, 1] = matrix[0, 2] = matrix[1, 0] = matrix[1, 1] = true;
                    break;
                case TetrominoType.T:
                    matrix = new bool[3, 3];
                    matrix[0, 1] = matrix[1, 0] = matrix[1, 1] = matrix[1, 2] = true;
                    break;
                case TetrominoType.Z:
                    matrix = new bool[3, 3];
                    matrix[0, 0] = matrix[0, 1] = matrix[1, 1] = matrix[1, 2] = true;
                    break;
                default:
                    //falls in the O case
                    matrix = new bool[3, 4];
                    matrix[0, 1] = matrix[0, 2] = matrix[1, 1] = matrix[1, 2] = true;
                    break;
            }

            return matrix;
        }

        /// <summary>
        /// Rotates a tetromino matrix in the direction
        /// </summary>
        /// <param name="tetromino">The tetromino to be rotated</param>
        /// <param name="direction">The direction</param>
        public static void RotateTetromino(Tetromino tetromino, RotationDirection direction)
        {
            tetromino.SetMatrix(GetRotatedSquareBlockMatrix(tetromino.Matrix, direction));

            int orientationIncrement = direction == RotationDirection.CLOCKWISE ? 1 : -1;
            int finalOrientation = (int)tetromino.Orientation + orientationIncrement;
            //rotated counter clockwise from up
            if (finalOrientation < 0)
                finalOrientation = 3;
            //rotated clockwise from left
            if (finalOrientation > 3)
                finalOrientation = 0;

            tetromino.SetOrientation((TetrominoOrientation)finalOrientation);
        }

        /// <summary>
        /// Calcualtes the rotation of a square block matrix in the given direction
        /// The rotation is 90 deegrees for clockwise and -90 deegrees for counter-clockwise
        /// </summary>
        /// <param name="matrix">The base matrix to calc the rotation</param>
        /// <param name="direction">The diection to rotate the matrix</param>
        /// <returns>A new matrix that is the rotation of the base matrix</returns>
        public static bool[,] GetRotatedSquareBlockMatrix(bool [,] matrix, RotationDirection direction)
        {
            int n = matrix.GetLength(0);

            bool[,] result = new bool[n, n];

            if(direction == RotationDirection.CLOCKWISE)
            {
                //the first row becomes the last column of the result matrix, and so on
                for(int i = 0; i < n; i++)
                {
                    //the result's column index for this row
                    int w = n - i - 1;

                    for (int j = 0; j < n; j++)
                    {
                        //tanspose the row's elements into the result's column
                        result[j, w] = matrix[i, j];
                    }
                }
            }
            else
            {
                //the last colum becomes the first row of the result matrix
                for (int i = n - 1; i >= 0; i--)
                {
                    //the result's line index for this column
                    int k = n - i - 1;

                    for (int j = 0; j < n; j++)
                    {
                        //tanspose the column's elements into the result;s row
                        result[k, j] = matrix[j, i];
                    }
                }
            }          

            return result;
        }
    }
}
