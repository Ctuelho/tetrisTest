/// <summary>
/// The classes, structs, functions and logic were based on the official Tetris SRS
/// Super Rotation System
/// https://tetris.fandom.com/wiki/SRS
/// </summary>
namespace Tetris
{
    using System.Collections.Generic;

    public enum TetrominoType { I, J, L, O, S, T, Z }

    public enum TetrominoOrientation { UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3 }

    public enum HorizontalTranslationDirection { RIGHT, LEFT }

    public enum VerticalTranslationDirection { DOWN, UP }

    /// <summary>
    /// A virtual piece that the player can control
    /// This clas has just spatial information about the tetromino
    /// </summary>
    public class Tetromino
    {
        public TetrominoType TetrominoType { get; private set; }

        public TetrominoOrientation Orientation { get; private set; } = TetrominoOrientation.UP;

        public bool[,] Matrix { get; private set; }

        public int MatrixRows { get; private set; } = 0;
        public int MatrixColumns { get; private set; } = 0;

        public int PosI { get; private set; } = 0;
        public int PosJ { get; private set; } = 0;

        public bool this[int i, int j]
        {
            get => Matrix[i, j];
            private set => Matrix[i, j] = value;
        }

        public Tetromino(TetrominoType tetrominoType, bool[,] matrix)
        {
            TetrominoType = tetrominoType;
            Matrix = matrix;
            MatrixRows = matrix.GetLength(0);
            MatrixColumns = matrix.GetLength(1);
        }

        public void SetOrientation(TetrominoOrientation orientation)
        {
            Orientation = orientation;
        }

        public void SetMatrix(bool[,] matrix)
        {
            Matrix = matrix;
        }

        public void SetPosition(int i, int j)
        {
            PosI = i;
            PosJ = j;
        }

        public int[] GetOffsets()
        {
            //we have to offset the blocks because of SRS centralization, so they match the grid
            return new int[] { 1, 1 };
        }
    }

    /// <summary>
    /// The information about the space of the game and blocks
    /// The grid can calculate if it's possible to rotate or move a block.
    /// By default, it is a 20 rows x 10 columns matrix
    /// With additional 3 rows of non playable area to allow the rotation
    /// and translation of tetrominos outside the view when they spawn
    /// The minimum acceptable is 4x4, or the blocks will never move/rotate 
    /// </summary>
    public class Grid
    {
        public const int SPAWN_ZONE = 3;

        private int m_rows = 20;

        public int Rows
        {
            get => (m_rows + SPAWN_ZONE);
            private set { m_rows = value; }
        }

        public int Columns { get; private set; } = 10;

        public bool[,] Matrix { get; private set; }

        public bool this[int i, int j]
        {
            get
            {
                return Matrix[i, j];
            }
            private set => Matrix[i, j] = value;
        }

        public Grid()
        {
            Matrix = new bool[Rows, Columns];
        }

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Matrix = new bool[Rows, Columns];
        }

        public bool CheckIfIsOverfilled()
        {
            //if any block in the spawn zone is filled, it's a game over
            for (int i = SPAWN_ZONE - 1; i >= 0; i--)
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (this[i, j])
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if there is a row to be consumed
        /// </summary>
        /// <returns>An integer with the index of the row in the matrix. -1 if no row was consumed</returns>
        public int ConsumeRow()
        {
            int rowConsumed = -1;

            int currentRow = Rows - 1;

            bool formedRow = false;

            do
            {
                for (int j = 0; j < Columns; j++)
                {
                    if (!this[currentRow, j])
                    {
                        formedRow = false;
                        break;
                    }
                    else
                    {
                        formedRow = true;
                    }
                }

                if (!formedRow)
                    currentRow--;

            } while (currentRow >= 0 && !formedRow);

            if (formedRow)
            {
                rowConsumed = currentRow;

                //using the naive method
                for (int i = currentRow; i >= 0; i--)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        if (i == 0)
                        {
                            this[i, j] = false;
                        }
                        else
                        {
                            this[i, j] = this[i - 1, j];
                        }
                    }
                }
            }

            return rowConsumed;
        }

        /// <summary>
        /// Tests if a tetromino can be horizontally translated and, if possible, translates it
        /// </summary>
        /// <param name="tetromino">The tetromino to be tested/translated</param>
        /// <param name="horizontalTranslationDirection">The direction of the horizontal translation</param>
        /// <returns>True if the tetromino was successfully translated, else false</returns>
        public bool TryTranslateTetrominoHorizontally(
            Tetromino tetromino,
            HorizontalTranslationDirection horizontalTranslationDirection)
        {
            //+1 means translating to the right, -1 to the left
            int translation = horizontalTranslationDirection == HorizontalTranslationDirection.RIGHT ? 1 : -1;

            int[] offsets = tetromino.GetOffsets();

            for (int i = 0; i < tetromino.MatrixRows; i++)
            {
                for (int j = 0; j < tetromino.MatrixColumns; j++)
                {
                    int iIndexOnTheGrid = i + tetromino.PosI - offsets[1];
                    int jIndexOnTheGrid = j + translation + tetromino.PosJ - offsets[1];

                    //we test only for actual blocks in the tetromino matrix
                    if (tetromino[i, j])
                    {
                        if (jIndexOnTheGrid > Columns - 1 || jIndexOnTheGrid < 0)
                        {
                            return false;
                        }
                        //test if it collides
                        else if (tetromino[i, j] && this[iIndexOnTheGrid, jIndexOnTheGrid])
                        {
                            return false;
                        }
                    }
                }
            }

            tetromino.SetPosition(tetromino.PosI, tetromino.PosJ + translation);

            return true;
        }

        /// <summary>
        /// Tests if a tetromino can be vertically translated and, if possible, translates it
        /// </summary>
        /// <param name="tetromino">The tetromino to be tested/translated</param>
        /// <param name="verticalTranslationDirection">The direction of the vertical translation</param>
        /// <returns>True if the tetromino was successfully translated, else false</returns>
        public bool TryTranslateTetrominoVertically(
            Tetromino tetromino,
            VerticalTranslationDirection verticalTranslationDirection)
        {
            //+1 means translating down, -1 up
            int translation = verticalTranslationDirection == VerticalTranslationDirection.DOWN ? 1 : -1;

            int[] offsets = tetromino.GetOffsets();

            for (int i = 0; i < tetromino.MatrixRows; i++)
            {
                for (int j = 0; j < tetromino.MatrixColumns; j++)
                {
                    int iIndexOnTheGrid = i + translation + tetromino.PosI - offsets[0];
                    int jIndexOnTheGrid = j + tetromino.PosJ - offsets[1];

                    //we test only for actual blocks in the tetromino matrix
                    if (tetromino[i, j])
                    {
                        //tests if the i index on the grid is outside the grid's top and bottom "walls"
                        if (iIndexOnTheGrid > Rows - 1 || iIndexOnTheGrid < 0)
                        {
                            return false;
                        }
                        //test if it collides
                        else if (tetromino[i, j] && this[iIndexOnTheGrid, jIndexOnTheGrid])
                        {
                            return false;
                        }
                    }
                }
            }

            tetromino.SetPosition(tetromino.PosI + translation, tetromino.PosJ);
            return true;
        }

        /// <summary>
        /// Attachs a tetromino to the grid. It's virtually attached
        /// because the tetromino is still independent from the grid
        /// Only it's blocks positions on the grid matrix are used
        /// to occupy the grid matrix at the same i,j.
        /// </summary>
        /// <param name="tetromino">The tetromino to be attached</param>
        public void AddTetrominoToGrid(Tetromino tetromino)
        {
            int[] offsets = tetromino.GetOffsets();

            //now fill the matrix with the blocks
            for (int i = 0; i < tetromino.MatrixRows; i++)
            {
                for (int j = 0; j < tetromino.MatrixColumns; j++)
                {
                    int iIndexOnTheGrid = i + tetromino.PosI - offsets[0];
                    int jIndexOnTheGrid = j + tetromino.PosJ - offsets[1];

                    if (tetromino[i, j])
                    {
                        this[iIndexOnTheGrid, jIndexOnTheGrid] = true;
                    }
                }
            }
        }

        /// <summary>
        /// Tests if the grid can fit the tetro's matrix without colliding with the floor or walls
        /// This is based on the official SRS(Super Rotation System) of Tetris
        /// 5 rotation attempts are tested consecutively
        /// In case all 5 attempts fail, the rotation does not occurs.
        /// The tetromino will be kicked up and left/right (i, j) positions
        /// depending on the tetromino's type, position and rotation, if the rotation is possible.
        /// </summary>
        /// <param name="testData">The information about the rotation to be performed</param>
        /// <returns>A kick data</returns>
        public KickData TryRotatingTetromino(GridTestData testData)
        {
            KickData result = new KickData { JumpI = 0, JumpJ = 0 };

            bool[,] rotatedTetroMatrix =
                TetrominoUtils.GetRotatedSquareBlockMatrix(
                    testData.tetromino.Matrix, testData.RotationDirection);

            List<int[]> testValues =new List<int[]>();

            //O tetrominoes don't need to be tested
            if (testData.tetromino.TetrominoType == TetrominoType.O)
            {
                result.Succeeded = false;
                return result;
            }
            else
            {
                //up -> right
                if (testData.FromRotation == TetrominoOrientation.UP &&
                   testData.RotationDirection == RotationDirection.CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] {-2, 0});
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { -2, 1 });
                        testValues.Add(new int[] { 1, -2 });
                    }
                    else
                    {
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { -1, -1 });
                        testValues.Add(new int[] { 0, 2 });
                        testValues.Add(new int[] { -1, 2 });
                    } 
                }
                //right -> up
                else if (testData.FromRotation == TetrominoOrientation.RIGHT &&
                         testData.RotationDirection == RotationDirection.COUNTER_CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] { 2, 0 });
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { 2, -1 });
                        testValues.Add(new int[] { -1, 2 });
                    }
                    else
                    {
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { 1, 1 });
                        testValues.Add(new int[] { 0, -2 });
                        testValues.Add(new int[] { 1, -2 });
                    }
                }
                //right -> down
                else if (testData.FromRotation == TetrominoOrientation.RIGHT &&
                         testData.RotationDirection == RotationDirection.CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { 2, 0 });
                        testValues.Add(new int[] { -1, -2 });
                        testValues.Add(new int[] { 2, 1 });
                    }
                    else
                    {
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { 1, 1 });
                        testValues.Add(new int[] { 0, -2 });
                        testValues.Add(new int[] { 1, -2 });
                    }
                }
                //down -> right
                else if (testData.FromRotation == TetrominoOrientation.DOWN &&
                         testData.RotationDirection == RotationDirection.COUNTER_CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { -2, 0 });
                        testValues.Add(new int[] { 1, 2 });
                        testValues.Add(new int[] { -2, 1 });
                    }
                    else
                    {
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { -1, -1 });
                        testValues.Add(new int[] { 0, 2 });
                        testValues.Add(new int[] { -1, 2 });
                    }
                }
                //down -> left
                else if (testData.FromRotation == TetrominoOrientation.DOWN &&
                         testData.RotationDirection == RotationDirection.CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] { 2, 0 });
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { 2, -1 });
                        testValues.Add(new int[] { -1, 2 });
                    }
                    else
                    {
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { 1, -1 });
                        testValues.Add(new int[] { 0, 2 });
                        testValues.Add(new int[] { 1, 2 });
                    }
                }
                //left -> down
                else if (testData.FromRotation == TetrominoOrientation.LEFT &&
                         testData.RotationDirection == RotationDirection.COUNTER_CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] { -2, 0 });
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { -2, 1 });
                        testValues.Add(new int[] { 1, -2 });
                    }
                    else
                    {
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { -1, 1 });
                        testValues.Add(new int[] { 0, -2 });
                        testValues.Add(new int[] { -1, -2 });
                    }
                }
                //left -> up
                else if (testData.FromRotation == TetrominoOrientation.LEFT &&
                         testData.RotationDirection == RotationDirection.CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { -2, 0 });
                        testValues.Add(new int[] { 1, 2 });
                        testValues.Add(new int[] { -2, -1 });
                    }
                    else
                    {
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { -1, 1 });
                        testValues.Add(new int[] { 0, -2 });
                        testValues.Add(new int[] { -1, -2 });
                    }
                }
                //up -> left
                else if (testData.FromRotation == TetrominoOrientation.UP &&
                         testData.RotationDirection == RotationDirection.COUNTER_CLOCKWISE)
                {
                    testValues = new List<int[]>();
                    testValues.Add(new int[] { 0, 0 });
                    if (testData.tetromino.TetrominoType == TetrominoType.I)
                    {
                        testValues.Add(new int[] { -1, 0 });
                        testValues.Add(new int[] { 2, 0 });
                        testValues.Add(new int[] { -1, -2 });
                        testValues.Add(new int[] { 2, 1 });
                    }
                    else
                    {
                        testValues.Add(new int[] { 1, 0 });
                        testValues.Add(new int[] { 1, -1 });
                        testValues.Add(new int[] { 0, 2 });
                        testValues.Add(new int[] { 1, 2 });
                    }
                }

                foreach (int[] values in testValues)
                {
                    if (!PerformCollisionTest(
                        values[0],
                        values[1],
                        rotatedTetroMatrix,
                        testData.tetromino))
                    {
                        result.JumpJ = values[0];
                        result.JumpI = values[1];
                        result.Succeeded = true;
                        return result;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Simulates a collision test beetwen a tetromino and it's possible rotation
        /// </summary>
        /// <param name="jOffset">J offset</param>
        /// <param name="iOffset">I offset</param>
        /// <param name="matrix">The simulation matrix to be tested</param>
        /// <param name="tetromino">The tetromino containing the spatial data</param>
        /// <returns>Boolean, true for collision, false if not</returns>
        private bool PerformCollisionTest(int jOffset, int iOffset, bool[,] matrix, Tetromino tetromino)
        {
            int[] offsets = tetromino.GetOffsets();
            //int[] offsets = { 0, 0 };

            for (int i = 0; i < tetromino.MatrixRows; i++)
            {
                for (int j = 0; j < tetromino.MatrixColumns; j++)
                {
                    int iIndexOnTheGrid = i + iOffset + tetromino.PosI - offsets[0];
                    int jIndexOnTheGrid = j + jOffset + tetromino.PosJ - offsets[1];

                    //we test only for actual blocks in the tetromino matrix
                    if (matrix[i, j])
                    {
                        //tests if the i index on the grid is outside the grid's top and bottom "walls"
                        if (iIndexOnTheGrid > Rows - 1 || iIndexOnTheGrid < 0)
                        {
                            return true;
                        }
                        //tests if the j index on the grid is outside the grid's left and right "walls"
                        else if (jIndexOnTheGrid > Columns - 1 || jIndexOnTheGrid < 0)
                        {
                            return true;
                        }
                        //test if it collides
                        else if (matrix[i, j] && this[iIndexOnTheGrid, jIndexOnTheGrid])
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }



    /// <summary>
    /// The data needed to perform a test if the tetromino would fit the grid.
    /// Matrix is the matrix to be tested.
    /// PosX and PosY are the tetromino's position in the grid.
    /// FromRotation is the current orientation of the tetromino, while
    /// toRotation is the target orientation. Ex: rotatiing from UP to RIGHT.
    /// TetrominoType is the type of the tetromino.
    /// </summary>
    public struct GridTestData
    {
        public Tetromino tetromino;
        public TetrominoOrientation FromRotation;
        public RotationDirection RotationDirection;
    }

    /// <summary>
    /// Contais a floor/wall kick data from a grid fit test.
    /// Succeeded being true means that the tetromino would fit the grid,
    /// false means that it failed to fit at all.
    /// JumpI and JumpJ means how much the tetromino should be translated
    /// horizontally and vertically
    /// </summary>
    public struct KickData
    {
        public bool Succeeded;
        public int JumpJ;
        public int JumpI;
    }
}