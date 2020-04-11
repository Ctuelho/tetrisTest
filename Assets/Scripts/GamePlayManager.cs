using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using DG.Tweening;

public class GamePlayManager : MonoBehaviour
{ 
    public static GamePlayManager Instance = null;

    const float TIME_TO_MOVE_BLOCKS = 1;

    const float NUMBER_OF_KICKS = 5;

    //prefabs of tetrominoes
    public GameObject ITetrominoPrefab;
    public GameObject JTetrominoPrefab;
    public GameObject LTetrominoPrefab;
    public GameObject OTetrominoPrefab;
    public GameObject STetrominoPrefab;
    public GameObject TTetrominoPrefab;
    public GameObject ZTetrominoPrefab;

    //the explsion prefab
    public GameObject PopPrefab;

    //prefab of a single block
    public GameObject BlockPrefab;

    //transform that will hold the blocks
    public Transform BlocksHolder;

    //the grid information and state
    private Tetris.Grid m_grid;

    //the current tetromino the player is holding
    private Tetris.Tetromino m_tetromino;

    //the visual representation of the current tetromino
    private GameObject m_visualTetromino;

    //the visual blocks grid
    private GameObject[,] m_blocksVisualGrid;

    //the coroutine for moving the blocks down
    private Coroutine m_blockMoverCoroutine;
    private float m_timeCounter = 0;
    private bool m_accelerated;

    //current number of kicks done with this tetromino
    private int m_kicksDone;

    //random tetrominoes pool
    private List<Tetris.TetrominoType> m_tetrominoPool;

    public GameObject WorldGrid;
    private Vector3 m_originalWorldGridPosition;

    public Transform NextetrominoPos;

    private GameObject m_nextTetromino;

    private float m_score;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        m_originalWorldGridPosition = WorldGrid.transform.position;
    }

    public void StartGame()
    {
        m_grid = new Tetris.Grid();
        m_blocksVisualGrid = new GameObject[m_grid.Rows, m_grid.Columns];

        m_score = 0;

        //create a tetromino
        GetRandomTetromino();
    }

    public void AssembleGamePlay()
    {
        //move the grid down
        WorldGrid.SetActive(true);
        WorldGrid.transform.position = m_originalWorldGridPosition + Vector3.up * 30;
        WorldGrid.transform.DOMoveY(m_originalWorldGridPosition.y, 2.5f);
    }

    private void GameOver()
    {
        m_tetromino = null;

        if (m_blockMoverCoroutine != null)
            StopCoroutine(m_blockMoverCoroutine);

        StopCoroutine("PopBlocks");

        UI.Instance.DropDefeatMenu();
    }
   
    public void ClearTheTable(bool moveTheGrid)
    {
        m_grid = null;

        m_tetromino = null;

        if (m_visualTetromino != null)
            Destroy(m_visualTetromino);

        //clear the blocks
        int blocksChildCount = BlocksHolder.childCount;
        for (int i = blocksChildCount -1; i >= 0; i--)
        {
            Destroy(BlocksHolder.GetChild(i).gameObject);
        }

        if(m_nextTetromino)
            Destroy(m_nextTetromino);

        m_blocksVisualGrid = null;

        //move the grid up
        if (moveTheGrid)
        {
            WorldGrid.transform.DOMoveY(m_originalWorldGridPosition.y + 30, 2f).
                OnComplete(() => WorldGrid.SetActive(false));
        }

        if (m_blockMoverCoroutine != null)
            StopCoroutine(m_blockMoverCoroutine);

        StopCoroutine("PopBlocks");
    }

    private void AutoMove()
    {
        if (m_blockMoverCoroutine != null) 
        {
            StopCoroutine(m_blockMoverCoroutine);
          
            if (m_tetromino != null)
                TranslateTetrominoVertically(Tetris.VerticalTranslationDirection.DOWN);
        }

        m_timeCounter = 0;

        m_blockMoverCoroutine = StartCoroutine(MoveBlock());
    }

    IEnumerator MoveBlock()
    {

        while (m_timeCounter < TIME_TO_MOVE_BLOCKS)
        {
            float boost = 1 + (m_score / 25f);
            m_timeCounter += (Time.deltaTime * boost * (m_accelerated ? 10 : 1));
            yield return null;
        }

        AutoMove();
    }

    private void Update()
    {
        if (m_tetromino != null)
        {
            //try to rotate the tetromino clockwise
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                Tetris.GridTestData testData = new Tetris.GridTestData
                {
                    tetromino = m_tetromino,
                    FromRotation = m_tetromino.Orientation,
                    RotationDirection = Tetris.RotationDirection.CLOCKWISE
                };
                Tetris.KickData result = m_grid.TryRotatingTetromino(testData);
                if (result.Succeeded)
                {
                    //if it was a kick, it will only occur if it has not reached the limit of kicks
                    if(result.JumpI > 0 || result.JumpJ > 0)
                    {
                        if (m_kicksDone < NUMBER_OF_KICKS)
                        {
                            m_kicksDone++;

                            //rotate and kick the tetromino
                            Tetris.TetrominoUtils.RotateTetromino(
                                m_tetromino, Tetris.RotationDirection.CLOCKWISE);

                            m_tetromino.SetPosition(
                                m_tetromino.PosI + result.JumpI,
                                m_tetromino.PosJ + result.JumpJ);

                            UpdateVisualTetrominoPosition();
                            UpdateVisualTetrominoRotation();

                            if(m_blockMoverCoroutine != null)
                            {
                                StopCoroutine(m_blockMoverCoroutine);
                                m_blockMoverCoroutine = StartCoroutine(MoveBlock());
                            }
                        }
                    }
                    else
                    {
                        Tetris.TetrominoUtils.RotateTetromino(
                                m_tetromino, Tetris.RotationDirection.CLOCKWISE);

                        m_tetromino.SetPosition(
                            m_tetromino.PosI + result.JumpI,
                            m_tetromino.PosJ + result.JumpJ);

                        UpdateVisualTetrominoPosition();
                        UpdateVisualTetrominoRotation();

                        if (m_blockMoverCoroutine != null)
                        {
                            StopCoroutine(m_blockMoverCoroutine);
                            m_blockMoverCoroutine = StartCoroutine(MoveBlock());
                        }
                    }
                }
            }

            //horizontal/vertical movment of the tetromino
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                m_grid.TryTranslateTetrominoHorizontally(
                    m_tetromino,
                    Tetris.HorizontalTranslationDirection.RIGHT);
                UpdateVisualTetrominoPosition();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                m_grid.TryTranslateTetrominoHorizontally(
                    m_tetromino,
                    Tetris.HorizontalTranslationDirection.LEFT);
                UpdateVisualTetrominoPosition();
            }
        }

        //accelerates the descending
        m_accelerated = Input.GetKey(KeyCode.DownArrow);
    }

    private void TranslateTetrominoVertically(Tetris.VerticalTranslationDirection direction)
    {
        //test if the block can move down
        //if false, it means the block has settled
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

                        //position the block, transposed because unity's orientation is different from
                        //matrix orientation
                        int blockPositionX = m_tetromino.PosJ + j - jOffset;
                        int blockPositionY = m_grid.Rows - 1 - m_tetromino.PosI - i;

                        block.transform.position =
                            new Vector3(
                                blockPositionX + positionXoffset,
                                blockPositionY + positionYoffset,
                                0);

                        int iIndexOnTheGrid = i + m_tetromino.PosI - m_tetromino.GetOffsets()[0];
                        int jIndexOnTheGrid = j + m_tetromino.PosJ - m_tetromino.GetOffsets()[0];
                        m_blocksVisualGrid[iIndexOnTheGrid, jIndexOnTheGrid] = block;
                    }
                }
            }

            if (m_grid.CheckIfIsOverfilled())
            {
                GameOver();
            }
            else
            {
                m_tetromino = null;
                Destroy(m_visualTetromino);

                StartCoroutine(PopBlocks());
            }
        }
    }

    IEnumerator PopBlocks()
    {
        int consecutiveRows = 0;

        //try to consume row
        int row = m_grid.ConsumeRow();
        while (row != -1)
        {
            consecutiveRows++;

            m_score += consecutiveRows;

            //first, clear the blocks row
            for (int j = 0; j < m_grid.Columns; j++)
            {
                GameObject pop = Instantiate(PopPrefab);
                pop.transform.position = m_blocksVisualGrid[row, j].transform.position;
                ParticleSystem.MainModule main =
                    pop.GetComponent<ParticleSystem>().main;
                main.startColor = 
                    m_blocksVisualGrid[row, j].GetComponent<Renderer>().material.color;
                Destroy(m_blocksVisualGrid[row, j]);
                m_blocksVisualGrid[row, j] = null;
            }

            AudioManager.PlaySfx(3);

            yield return new WaitForSeconds(0.25f);
            DropBlocks(row);
            UI.Instance.IncreaseScore(consecutiveRows);
            yield return new WaitForSeconds(0.25f);

            row = m_grid.ConsumeRow();
        }

        GetRandomTetromino();
    }

    private void DropBlocks(int row)
    {
        //using the naive method
        for (int i = row; i > 0; i--)
        {
            for (int j = 0; j < m_grid.Columns; j++)
            {
                m_blocksVisualGrid[i, j] = m_blocksVisualGrid[i - 1, j];
                //drop the block 1 unity down
                if(m_blocksVisualGrid[i, j] != null)
                {
                    m_blocksVisualGrid[i, j].transform.position =
                    new Vector3(m_blocksVisualGrid[i, j].transform.position.x,
                                m_blocksVisualGrid[i, j].transform.position.y - 1,
                                0);
                }
            }
        }
    }
    private void TryFillTetrominoPool()
    {
        //prepare the tetromino pool
        if (m_tetrominoPool == null || m_tetrominoPool.Count == 0)
        {
            m_tetrominoPool = new List<Tetris.TetrominoType>();

            System.Array tetrominoTypeValues =
                System.Enum.GetValues(typeof(Tetris.TetrominoType));
            List<Tetris.TetrominoType> temp = new List<Tetris.TetrominoType>();
            for (int i = 0; i < tetrominoTypeValues.Length; i++)
            {
                temp.Add((Tetris.TetrominoType)tetrominoTypeValues.GetValue(i));
            }

            while (temp.Count > 0)
            {
                int index = Random.Range(0, temp.Count);
                m_tetrominoPool.Add(temp[index]);
                temp.RemoveAt(index);
            }
        }
    }

    private void GetRandomTetromino()
    {
        TryFillTetrominoPool();

        m_tetromino = Tetris.TetrominoUtils.GetTetromino(m_tetrominoPool[0]);
        m_tetrominoPool.RemoveAt(0);

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
        Vector2Int tetrominoOffset = new Vector2Int(0, -1);
        m_tetromino.SetPosition(0 + tetrominoOffset.x, (m_grid.Columns / 2) + tetrominoOffset.y);

        UpdateVisualTetrominoPosition();

        //display the next one
        TryFillTetrominoPool();
        if (m_nextTetromino != null)
            Destroy(m_nextTetromino);
        Tetris.TetrominoType nextTetrominoType = m_tetrominoPool[0];
        switch (nextTetrominoType)
        {
            case Tetris.TetrominoType.I:
                m_nextTetromino = Instantiate(ITetrominoPrefab);
                break;
            case Tetris.TetrominoType.J:
                m_nextTetromino = Instantiate(JTetrominoPrefab);
                break;
            case Tetris.TetrominoType.L:
                m_nextTetromino = Instantiate(LTetrominoPrefab);
                break;
            case Tetris.TetrominoType.O:
                m_nextTetromino = Instantiate(OTetrominoPrefab);
                break;
            case Tetris.TetrominoType.S:
                m_nextTetromino = Instantiate(STetrominoPrefab);
                break;
            case Tetris.TetrominoType.T:
                m_nextTetromino = Instantiate(TTetrominoPrefab);
                break;
            case Tetris.TetrominoType.Z:
                m_nextTetromino = Instantiate(ZTetrominoPrefab);
                break;
        }
        m_nextTetromino.transform.position = NextetrominoPos.position;

        //start the tetromino auto move down
        if (m_blockMoverCoroutine != null)
            StopCoroutine(m_blockMoverCoroutine);
        m_blockMoverCoroutine = StartCoroutine(MoveBlock());

        //reset kicks
        m_kicksDone = 0;
    }

    private void UpdateVisualTetrominoPosition()
    {      
        //we have to translate the visual tetromino because how they
        //are centralized and because of the 3D model's pivot
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
    }

    private void UpdateVisualTetrominoRotation()
    {
        //no point in rotating an O tetromino
        if (m_tetromino.TetrominoType == Tetris.TetrominoType.O)
            return;

        //set the visual tetromino orientation accordingly
        switch (m_tetromino.Orientation)
        {
            case Tetris.TetrominoOrientation.UP:
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.zero);
                break;
            case Tetris.TetrominoOrientation.RIGHT:
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.forward * -90);
                break;
            case Tetris.TetrominoOrientation.DOWN:
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.forward * -180);
                break;
            case Tetris.TetrominoOrientation.LEFT:
                m_visualTetromino.transform.rotation =
                    Quaternion.Euler(Vector3.forward * -270);
                break;
        }
    }
}
