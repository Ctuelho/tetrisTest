using UnityEngine;
using System.Collections;

public class GamePlayManager : MonoBehaviour
{
    //the world camera
    public Camera GamePlayCamera;

    //the prefab of the block
    public GameObject BlockPrefab;

    //the grid information and state
    private Tetris.Grid m_grid;

    //the visual grid of blocks
    private Transform[,] m_blocks;

    //the current tetromino the player is holding
    private Tetris.Tetromino m_tetromino;

    //the current center of the tetromino
    private Vector2 m_tetrominoPosition;

    //the last position of the tetromino
    private Vector2 m_tetrominoLastPosition;

    //the last visual info of the tetromino
    private bool[,] m_tetrominoLastVisual;

    //if grid needs to be updated
    private bool m_updateGrid;

    //the coroutine for moving the blocks down
    private Coroutine m_blockMoverCoroutine;

    const float TIME_TO_MOVE_BLOCKS = 1;

    private void Awake()
    {
        //initialize the grid with default values
        m_grid = new Tetris.Grid();

        //create the blocks in the world
        m_blocks = new Transform[m_grid.Lines, m_grid.Columns];
        //for(int i = 0; i < m_grid.Lines; i++)
        //{
        //    for(int j = 0; j < m_grid.Columns; j++)
        //    {
        for (int i = 0; i < m_grid.Columns; i++)
        {
            for (int j = 0; j < m_grid.Lines; j++)
            {
                GameObject block = Instantiate(BlockPrefab);

                //block.SetActive(false);

                //block.transform.position = new Vector3(m_grid.Lines - i - 1, m_grid.Columns + j, 0);
                //block.transform.position = new Vector3(j, i, 0);

                //m_blocks[i, j] = block.transform;

                //test here
                block.transform.position = new Vector3(i, j, 0);

                //m_blocks[j, i] = block.transform;
                m_blocks[m_grid.Lines - j - 1, i] = block.transform;

                block.GetComponentInChildren<TMPro.TextMeshPro>().text = "("+i.ToString() + "," + (m_grid.Lines - j - 1).ToString() + ")";
            }
        }

        //create a tetromino
        GetRandomTetromino();

        //setting the tetromino at the middle of grid
        //m_tetrominoPosition = new Vector2(m_grid.Lines/2, m_grid.Columns/2);
        m_tetrominoPosition = Vector2.zero;

        //start the block move down
        m_blockMoverCoroutine = StartCoroutine(MoveBlocks(TIME_TO_MOVE_BLOCKS));

        m_updateGrid = true;
        UpdateGrid();
    }

    IEnumerator MoveBlocks(float time)
    {
        yield return new WaitForSeconds(time);

        //for testing
        //UpdateTetrominoRotation(Tetris.RotationDirection.CLOCKWISE);

        m_updateGrid = true;

        m_blockMoverCoroutine = StartCoroutine(MoveBlocks(TIME_TO_MOVE_BLOCKS));

    } 

    private void GetRandomTetromino()
    {
        if(m_tetromino == null)
        {
            m_tetromino = Tetris.TetrominoManager.GetTetromino(Tetris.TetrominoType.L);
            //m_tetromino = Tetris.TetrominoManager.GetRandomTetromino();
            //set the last data to be current's
            m_tetrominoLastPosition = m_tetrominoPosition;
            m_tetrominoLastVisual = m_tetromino.Matrix;
        }
        else
        {
            m_tetromino = Tetris.TetrominoManager.GetRandomTetromino();
        }

        Debug.Log(m_tetromino.TetrominoType + " / " + m_tetromino.Orientation);

        m_updateGrid = true;

        UpdateGrid();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            
            UpdateTetrominoRotation(Tetris.RotationDirection.CLOCKWISE);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            UpdateTetrominoRotation(Tetris.RotationDirection.COUNTER_CLOCKWISE);
        }

        if (Input.GetKeyDown(KeyCode.Space))
            GetRandomTetromino();

        UpdateGrid();
    }

    private void UpdateTetrominoPosition(Vector2 positon)
    {
        m_tetrominoLastPosition = m_tetrominoPosition;
        m_tetrominoPosition = positon;

        m_updateGrid = true;
    }

    private void UpdateTetrominoRotation(Tetris.RotationDirection direction)
    {
        //no point in rotating a O block
        if (m_tetromino.TetrominoType == Tetris.TetrominoType.O)
            return;

        m_tetrominoLastVisual = m_tetromino.Matrix;

        //the new rotation
        bool[,] matrix =
            Tetris.TetrominoManager.GetRotatedSquareBlockMatrix(m_tetromino.Matrix, direction);

        m_tetromino.SetMatrix(matrix);

        int orientationIncrement = direction == Tetris.RotationDirection.CLOCKWISE ? 1 : -1;
        int finalOrientation = (int)m_tetromino.Orientation + orientationIncrement;
        //ratated counter clockwise from up
        if (finalOrientation < 0)
            finalOrientation = 3;
        //rotated clockwise from left
        if (finalOrientation > 3)
            finalOrientation = 0;

        m_tetromino.SetOrientation((Tetris.TetrominoOrientation)finalOrientation);

        m_updateGrid = true;
    }

    private void UpdateGrid()
    {
        if (!m_updateGrid)
            return;

        //update the grid visuals
        m_updateGrid = false;

        //erase the last visual data from the grid
        int lastVisualHeight = m_tetrominoLastVisual.GetLength(0) + (int)m_tetrominoLastPosition.x;
        int lastVisualWidth = m_tetrominoLastVisual.GetLength(1) + (int)m_tetrominoLastPosition.y;
        for (int lastVisualH = (int)m_tetrominoLastPosition.x; lastVisualH < lastVisualHeight; lastVisualH++)
        {
            for (int lastVisualW = (int)m_tetrominoLastPosition.y; lastVisualW < lastVisualWidth; lastVisualW++)
            {
                m_grid.UpdateMatrixPosition(
                    lastVisualH,
                    lastVisualW,
                    false);
            }
        }

        //add the new visual data to the grid
        int VisualHeight = m_tetromino.Matrix.GetLength(0) + (int)m_tetrominoPosition.x;
        int VisualWidth = m_tetromino.Matrix.GetLength(1) + (int)m_tetrominoPosition.y;
        for (int VisualH = (int)m_tetrominoPosition.x; VisualH < VisualHeight; VisualH++)
        {
            for (int visualW = (int)m_tetrominoPosition.y; visualW < VisualWidth; visualW++)
            {
                m_grid.UpdateMatrixPosition(
                    VisualH,
                    visualW,
                    m_tetromino[
                        VisualH - (int)m_tetrominoPosition.x,
                        visualW - (int)m_tetrominoPosition.y]);
            }
        }

        //update the visual of blocks
        for(int i = 0; i < m_grid.Lines; i++)
        {
            for(int j = 0; j < m_grid.Columns; j++)
            {
                //m_blocks[i, j].gameObject.SetActive(m_grid[i, j]);
                m_blocks[i, j].gameObject.SetActive(true);

                if (m_grid[i, j])
                    m_blocks[i, j].GetComponent<Renderer>().material.color = Color.yellow;
                else
                    m_blocks[i, j].GetComponent<Renderer>().material.color = Color.white;
            }
        }
    }
}
