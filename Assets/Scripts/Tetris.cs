/// <summary>
/// The classes, structs, functions and logic were based on the official Tetris SRS
/// Super Rotation System
/// https://tetris.fandom.com/wiki/SRS
/// </summary>
namespace Tetris
{
    public enum TetrominoType { I, J, L, O, S, T, Z }

    public enum TetrominoOrientation { UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3 }

    public enum HorizontalTranslationDirection { RIGHT, LEFT }

    public enum VerticalTranslationDirection { DOWN, UP }

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
            //we have to offset the blocks because of SRS centralization
            return new int[] { 1, 1 };
        }
    }

    /// <summary>
    /// The information about the space of the game and blocks
    /// The grid can calculate if it's possible to rotate or move a block
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
            private set { }
        }

        public int Columns { get; private set; } = 10;

        public bool[,] Matrix { get; private set; }

        public bool this[int i, int j]
        {
            get => Matrix[i, j];
            private set => Matrix[i, j] = value;
        }

        public Grid()
        {
            Matrix = new bool[Rows + SPAWN_ZONE, Columns];
        }

        public Grid(int rows, int columns)
        {
            Rows = rows;
            Columns = columns;
            Matrix = new bool[Rows+SPAWN_ZONE, Columns];
        }

        /// <summary>
        /// Checks if there is a row to be consumed
        /// </summary>
        /// <returns>An integer with the index of the row in the matrix. -1 if no row was consumed</returns>
        public int ConsumeRow()
        {
            int rowConsumed = -1;

            //read the rows from bottom upwards, ignore the spawnzone
            //starts at the last row
            int currentRow = Rows - SPAWN_ZONE - 1;

            bool formedRow = false;

            do
            {
                for (int i = currentRow; i > 0; i--)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        if (!this[i, j])
                        {
                            formedRow = false;
                            break;
                        }
                        else
                        {
                            formedRow = true;
                        }

                    }
                }

                if (formedRow)
                {
                    rowConsumed = currentRow;
                    
                    //found a full row
                    //consume it and drop the rows above 1 unity

                }

            } while (currentRow < Rows - 1 || !formedRow);

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

            //UnityEngine.Debug.Log("Testing for a matrix nXm " + "[" + tetromino.MatrixLines + " X " + tetromino.MatrixColumns + "]");

            //UnityEngine.Debug.Log("the tetropos: " + tetromino.PosJ);

            for (int i = 0; i < tetromino.MatrixRows; i++)
            {
                //for (int j = tetrominoMatrix.GetLength(1) - 1; j >= 0; j--)
                for (int j = 0; j < tetromino.MatrixColumns; j++)
                {
                    int iIndexOnTheGrid = i + tetromino.PosI - offsets[1];
                    int jIndexOnTheGrid = j + translation + tetromino.PosJ - offsets[1];

                    //we test only for actual blocks in the tetromino matrix
                    if (tetromino[i, j])
                    {
                        //UnityEngine.Debug.Log("Testing for [i,j] on the tetromino matrix " + "[" + i + "," + j + "]");

                        //UnityEngine.Debug.Log("jIndexOnTheGrid / grid width " + jIndexOnTheGrid + " / " + (Columns - 1));

                        //tests if the j index on the grid is outside the grid's right or left "walls"
                        if (jIndexOnTheGrid > Columns - 1 || jIndexOnTheGrid < 0)
                        {
                            //UnityEngine.Debug.Log("result false");
                            return false;
                        }
                        //test if it collides
                        else if (tetromino[i, j] && this[iIndexOnTheGrid, jIndexOnTheGrid])
                        {
                            //UnityEngine.Debug.Log("result false");
                            return false;
                        }
                    }
                }
            }

            //UnityEngine.Debug.Log("result true");

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

            //UnityEngine.Debug.Log("Testing for a matrix nXm " + "[" + tetromino.MatrixLines + " X " + tetromino.MatrixColumns);

            //UnityEngine.Debug.Log("the tetropos: " + tetromino.PosI);

            for (int i = 0; i < tetromino.MatrixRows; i++)
            {
                for (int j = 0; j < tetromino.MatrixColumns; j++)
                {
                    int iIndexOnTheGrid = i + translation + tetromino.PosI - offsets[0];
                    int jIndexOnTheGrid = j + tetromino.PosJ - offsets[1];

                    //we test only for actual blocks in the tetromino matrix
                    if (tetromino[i, j])
                    {
                        //UnityEngine.Debug.Log("Testing for [i,j] on the tetromino matrix " + "[" + i + "," + j + "]");

                        //UnityEngine.Debug.Log("jIndexOnTheGrid / grid height " + iIndexOnTheGrid + " / " + (Lines - 1));

                        //tests if the j index on the grid is outside the grid's top and bottom "walls"
                        if (iIndexOnTheGrid > Rows - 1 || iIndexOnTheGrid < 0)
                        {
                            //UnityEngine.Debug.Log("result false");
                            return false;
                        }
                        //test if it collides
                        else if (tetromino[i, j] && this[iIndexOnTheGrid, jIndexOnTheGrid])
                        {
                            //UnityEngine.Debug.Log("result false");
                            return false;
                        }
                    }
                }
            }

            //UnityEngine.Debug.Log("result true");

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
        /// Tests if a matrix can fit the grid without colliding with the floor or walls
        /// The test is made based on the matrix, the tetromino's position and type, and the rotatation
        /// for example: from up to right, from up to left, etc
        /// This is based on the official SRS(Super Rotation System) of Tetris
        /// 5 rotation attempts are tested consecutively
        /// In case all 5 attempts fail, the rotation does not occurs.
        /// The tetromino will be kicked up and left/right (n, m) positions
        /// depending on the tetromino's type, position and rotation if
        /// the rotation fits the grid.
        /// </summary>
        /// <param name="testData">The information about the rotation to be performed</param>
        /// <returns>A kick data</returns>
        public KickData TryRotatingtetromino(GridTestData testData)
        {
            KickData result = new KickData { JumpX = 0, JumpY = 0 };

            //O tetrominoes don't need to be tested
            if (testData.tetromino.TetrominoType == TetrominoType.O)
            {
                result.Succeeded = true;
                return result;
            }
            //I tetrominoes have specific test parameters due to it's length
            else if (testData.tetromino.TetrominoType == TetrominoType.I)
            {

            }
            //all other tetrominoes share the same test parameters
            else
            {

            }

            return result;
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
        public TetrominoOrientation ToRotation;
    }

    /// <summary>
    /// Contais a floor/wall kick data from a grid fit test.
    /// Succeeded being true means that the tetromino would fit the grid,
    /// false means that it failed to fit at all.
    /// Jumpx and Jumpy means how much the tetromino should be translated
    /// horizontally and vertically
    /// ex: (1, -2) would mean 1 positive in x and 2 negative in y
    /// </summary>
    public struct KickData
    {
        public bool Succeeded;
        public int JumpX;
        public int JumpY;
    }
}