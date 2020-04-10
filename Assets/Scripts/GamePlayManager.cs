using UnityEngine;
using System.Collections;

public class GamePlayManager : MonoBehaviour
{
    //the world camera
    public Camera GamePlayCamera;

    //prefabs of tetrominoes
    public GameObject ITetrominoPrefab;
    public GameObject JTetrominoPrefab;
    public GameObject LTetrominoPrefab;
    public GameObject OTetrominoPrefab;
    public GameObject STetrominoPrefab;
    public GameObject TTetrominoPrefab;
    public GameObject ZTetrominoPrefab;

    //prefab of block
    public GameObject BlockPrefab;

    //transform that will hold the blocks
    public Transform BlocksHolder;

    //the grid information and state
    private Tetris.Grid m_grid;

    //the current tetromino the player is holding
    private Tetris.Tetromino m_tetromino;

    //the visual representation of the current tetromino
    private GameObject m_visualTetromino;

    public GameObject TetrominoVisualPosition;

    //the visual blocks grid
    private GameObject[,] m_blocksVisualGrid;

    //the coroutine for moving the blocks down
    private Coroutine m_blockMoverCoroutine;

    private float m_timeCounter = 0;

    private bool m_accelerated;

    const float TIME_TO_MOVE_BLOCKS = 1;

    private void Awake()
    {
        //initialize the grid with default values
        m_grid = new Tetris.Grid();
        m_blocksVisualGrid = new GameObject[m_grid.Rows, m_grid.Columns];

        //create a tetromino
        GetRandomTetromino();

        //start the block move down
        m_blockMoverCoroutine = StartCoroutine(MoveBlocks());
    }

    IEnumerator MoveBlocks()
    {
        while(m_timeCounter < TIME_TO_MOVE_BLOCKS)
        {
            m_timeCounter += (Time.deltaTime * (m_accelerated ? 10 : 1));
            yield return null;
        }

        m_timeCounter = 0;

        //test if the block can move down
        //if false, it means the block has settled
        TranslateTetrominoVertically(Tetris.VerticalTranslationDirection.DOWN);

        m_blockMoverCoroutine = StartCoroutine(MoveBlocks());
    } 

    private void GetRandomTetromino()
    {
        //m_tetromino = Tetris.TetrominoUtils.GetTetromino(Tetris.TetrominoType.O);
        m_tetromino = Tetris.TetrominoUtils.GetRandomTetromino();
        
        //clear the old visual tetromino
        if (m_visualTetromino != null)
        {
            Destroy(m_visualTetromino);
        }

        //create the visual of the tetromino and define it's initial offset
        switch (m_tetromino.TetrominoType)
        {
            case Tetris.TetrominoType.I:
                m_visualTetromino = Instantiate(ITetrominoPrefab);
                break;
            case Tetris.TetrominoType.J:
                m_visualTetromino = Instantiate(JTetrominoPrefab);
                break;
            case Tetris.TetrominoType.L:
                m_visualTetromino = Instantiate(LTetrominoPrefab);
                break;
            case Tetris.TetrominoType.O:
                m_visualTetromino = Instantiate(OTetrominoPrefab);
                break;
            case Tetris.TetrominoType.S:
                m_visualTetromino = Instantiate(STetrominoPrefab);
                break;
            case Tetris.TetrominoType.T:
                m_visualTetromino = Instantiate(TTetrominoPrefab);
                break;
            case Tetris.TetrominoType.Z:
                m_visualTetromino = Instantiate(ZTetrominoPrefab);
                break;
        }
        m_visualTetromino.transform.SetParent(transform);

        //position it at the horizontal center of the grid
        //we translate it additionally to keep up with the SRS spawning positions
        Vector2Int tetrominoOffset = new Vector2Int(-1, -1);
        //add the spawn zone to the translation
        tetrominoOffset.x += Tetris.Grid.SPAWN_ZONE;
        m_tetromino.SetPosition(0 + tetrominoOffset.x, (m_grid.Columns / 2) + tetrominoOffset.y);
        UpdateVisualTetrominoPosition();
    }

    private void Update()
    {
        //rotates the tetromino clockwise
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {          
            UpdateTetrominoRotation(Tetris.RotationDirection.CLOCKWISE);
        }
        
        //horizontal/vertical movment of the tetromino
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            m_grid.TryTranslateTetrominoHorizontally(
                m_tetromino,
                Tetris.HorizontalTranslationDirection.RIGHT);
            UpdateVisualTetrominoPosition();
        }
        else if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            m_grid.TryTranslateTetrominoHorizontally(
                m_tetromino,
                Tetris.HorizontalTranslationDirection.LEFT);
            UpdateVisualTetrominoPosition();
        }
        
        //accelerates the descending
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_accelerated = true;
        }
        if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            m_accelerated = false;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            GetRandomTetromino();
    }

    private void TranslateTetrominoVertically(Tetris.VerticalTranslationDirection direction)
    {
        if (m_grid.TryTranslateTetrominoVertically(
                m_tetromino,
                Tetris.VerticalTranslationDirection.DOWN))
        {
            //the tetromino was translated
            UpdateVisualTetrominoPosition();
        }
        else
        {
            m_grid.AddTetrominoToGrid(m_tetromino);

            //the tetromino was added to the grid, so we need to update the visuals

            //create blocks to fill the visual grid based on the tetromino's matrix and position
            //save the material
            Material mat =
                m_visualTetromino.transform.GetComponentInChildren<Renderer>().material;

            int jOffset = 1;
            float positionXoffset = - 0.5f;
            float positionYoffset = 1.5f;
            
            for (int i = 0; i < m_tetromino.MatrixRows; i++)
            {
                for (int j = 0; j < m_tetromino.MatrixColumns; j++)
                {
                    if (m_tetromino[i, j])
                    {
                        //create a block
                        GameObject block = Instantiate(BlockPrefab, BlocksHolder);
                        block.GetComponent<Renderer>().material = mat;

                        //position the block, transposed because unity's orientation is different from matrix orientation
                        int blockPositionX = m_tetromino.PosJ + j - jOffset;
                        int blockPositionY = m_grid.Rows - 1 - m_tetromino.PosI - i;

                        block.transform.position =
                            new Vector3(
                                blockPositionX + positionXoffset,
                                blockPositionY + positionYoffset,
                                0);
                    }
                }
            }

            //try to consume row
            int row = m_grid.ConsumeRow();
            if (row != -1)
            {
                DropBlocks(row);
                StartCoroutine(WaitRowConsumptionAnimation());
            }
        }
    }

    IEnumerator WaitRowConsumptionAnimation()
    {
        yield return new WaitForSeconds(0.5f);

        int row = m_grid.ConsumeRow();
        if (row != -1)
        {
            DropBlocks(row);
            StartCoroutine(WaitRowConsumptionAnimation());
        }
        else
        {
            //get a new tetromino
            GetRandomTetromino();
        }
    }

    private void DropBlocks(int row)
    {

    }

    private void UpdateVisualTetrominoPosition()
    {      
        //we have to translate the visual tetromino because how they
        //are centralized and the 3D model's pivot
        Vector2 tetrominoOffset = new Vector2(-0.5f, 0.5f);
        if (m_tetromino.TetrominoType == Tetris.TetrominoType.O) 
        {
            tetrominoOffset = Vector2.up;
        }
        else if(m_tetromino.TetrominoType == Tetris.TetrominoType.I)
        {
            tetrominoOffset = Vector2.zero;
        }
    
        m_visualTetromino.transform.position =
            new Vector3(
                m_tetromino.PosJ + tetrominoOffset.x,
                m_grid.Rows - 1 - m_tetromino.PosI + tetrominoOffset.y,
                0);

        //REMOVE THIS LATERRRRR ----------------------------------------------------------------------------------------------------------
        //$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$
        TetrominoVisualPosition.transform.position =
            new Vector3(
                m_tetromino.PosJ,
                m_grid.Rows - 1 - m_tetromino.PosI,
                0);
    }

    private void UpdateTetrominoRotation(Tetris.RotationDirection direction)
    {
        //no point in rotating an O tetromino
        if (m_tetromino.TetrominoType == Tetris.TetrominoType.O)
            return;

        //the new rotation
        bool[,] matrix =
            Tetris.TetrominoUtils.GetRotatedSquareBlockMatrix(m_tetromino.Matrix, direction);

        m_tetromino.SetMatrix(matrix);

        int orientationIncrement = direction == Tetris.RotationDirection.CLOCKWISE ? 1 : -1;
        int finalOrientation = (int)m_tetromino.Orientation + orientationIncrement;
        //rotated counter clockwise from up
        if (finalOrientation < 0)
            finalOrientation = 3;
        //rotated clockwise from left
        if (finalOrientation > 3)
            finalOrientation = 0;

        m_tetromino.SetOrientation(
            (Tetris.TetrominoOrientation)finalOrientation);

        //set the visual tetromino orientation accordingly
        switch (m_tetromino.Orientation)
        {
            case Tetris.TetrominoOrientation.UP:
                //up is the default orientation
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.zero);
                break;
            case Tetris.TetrominoOrientation.RIGHT:
                //rotated 90 degrees right
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.forward * -90);
                break;
            case Tetris.TetrominoOrientation.DOWN:
                //rotated 180 degrees right
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.forward * -180);
                break;
            case Tetris.TetrominoOrientation.LEFT:
                //rotated 270 degrees right
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.forward * -270);
                break;
        }
    }
}
