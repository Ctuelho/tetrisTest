using System.Collections;
using UnityEngine;

using UnityEngine.UI;

using DG.Tweening;

public class UI : MonoBehaviour
{
    public static UI Instance = null;

    public Canvas MainMenuCanvas;
    public CanvasGroup MainMenuGroup;
    public RectTransform MainMenuPanell;

    public Canvas ScoreCanvas;
    public RectTransform ScorePannel;
    public Text ScoreValue;

    public Canvas InGameMenuCanvas;
    public CanvasGroup InGameMenuGroup;
    public RectTransform InGameMenuPannel;

    public Canvas PauseMenuCanvas;
    public CanvasGroup PauseMenuGroup;
    public RectTransform PauseMenuPannel;

    public Canvas DefeatMenuCanvas;
    public CanvasGroup DefeatMenuGroup;
    public RectTransform DefeatMenuPannel;

    public Canvas CountDownCanvas;
    public Text CountDownText;

    public Canvas NextTetrominoCanvas;
    public Text NextTetrominoTitle;

    const int COUNTDOWN_TIME = 4;
    private int m_countDownCounter;

    private int m_scoreCounter = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DropMainMenu();
    }

    private void PlayWaitingBGM()
    {
        AudioManager.PlayBGM(1);
    }

    private void PlayRandomBGM()
    {
        AudioManager.PlayBGM(Random.Range(2, 6));
    }

    private void StopBGM()
    {
        AudioManager.StopBGM();
    }

    private void PlayGetReadySFX()
    {
        AudioManager.PlaySfx(2000);
    }

    private void PlayPauseSFX()
    {
        AudioManager.PlaySfx(3000);
    }

    private void PlayConfirmSFX()
    {
        AudioManager.PlaySfx(3001);
    }

    private void PlayMenuHideSFX()
    {
        AudioManager.PlaySfx(3002);
    }

    private void PlayDefeatSfx()
    {
        AudioManager.PlaySfx(2001);
    }

    public void Pause()
    {
        Time.timeScale = 0;

        InGameMenuGroup.interactable = false;

        PauseMenuGroup.interactable = false;
        PauseMenuCanvas.gameObject.SetActive(true);
        PauseMenuPannel.anchoredPosition = new Vector2(0, -1000);
        PauseMenuPannel.DOAnchorPos(new Vector2(0, 0), 1.5f).
            SetUpdate(true).
            SetEase(Ease.OutBounce).
            OnComplete(() => {
                PauseMenuGroup.interactable = true;
            });          

        PlayPauseSFX();
    }

    public void Resume()
    {
        PauseMenuGroup.interactable = false;
        PauseMenuPannel.DOAnchorPos(new Vector2(0, -1000), 1.5f).
            SetUpdate(true).
            OnComplete(() => {
                PauseMenuCanvas.gameObject.SetActive(false);
                Time.timeScale = 1;
                InGameMenuGroup.interactable = true;
            });

        PlayMenuHideSFX();
    }

    public void BackToMenu(bool fromPause)
    {
        GamePlayManager.Instance.ClearTheTable(true);

        if (fromPause)
        {
            Time.timeScale = 1;
            PauseMenuGroup.interactable = false;
            PauseMenuPannel.DOAnchorPos(new Vector2(0, -1000), 1.5f).
                SetUpdate(true).
                OnComplete(() => {                    
                    DropMainMenu();
                    PauseMenuCanvas.gameObject.SetActive(false);
                });
        }
        else
        {
            DefeatMenuGroup.interactable = false;
            DefeatMenuPannel.DOAnchorPos(new Vector2(0, -1000), 1.5f).
                SetUpdate(true).
                OnComplete(() => {   
                    DropMainMenu();
                    DefeatMenuCanvas.gameObject.SetActive(false);
                });
        }

        InGameMenuGroup.interactable = false;
        InGameMenuPannel.DOScale(Vector3.zero, 0.5f).
            OnComplete(() => InGameMenuCanvas.gameObject.SetActive(false));

        ScorePannel.DOScale(Vector3.zero, 0.5f).
            OnComplete(() => ScoreCanvas.gameObject.SetActive(false));

        NextTetrominoTitle.transform.DOScale(0, 0.5f).
            OnComplete(() => NextTetrominoCanvas.gameObject.SetActive(false));

        StopBGM();
        PlayMenuHideSFX();
    }

    public void Replay()
    {
        GamePlayManager.Instance.ClearTheTable(false);

        DefeatMenuGroup.interactable = false;
        DefeatMenuPannel.DOAnchorPos(new Vector2(0, -1000), 1.5f).
            SetUpdate(true).
            OnComplete(() => {
                DefeatMenuCanvas.gameObject.SetActive(false);

                //startCountDown
                m_countDownCounter = COUNTDOWN_TIME;
                CountDownCanvas.gameObject.SetActive(true);
                CountDownText.text = COUNTDOWN_TIME.ToString();
                CountDownText.transform.localScale = Vector3.zero;
                CountDownText.transform.DOScale(1, 0.4f).
                    SetEase(Ease.OutElastic).
                    OnComplete(() => { StartCoroutine(CountDown()); });
            });

        InGameMenuGroup.interactable = false;
        InGameMenuPannel.DOScale(Vector3.zero, 0.5f).
            OnComplete(() => InGameMenuCanvas.gameObject.SetActive(false));

        ScorePannel.DOScale(Vector3.zero, 0.5f).
            OnComplete(() => ScoreCanvas.gameObject.SetActive(false));

        NextTetrominoTitle.transform.DOScale(0, 0.5f).
            OnComplete(() => NextTetrominoCanvas.gameObject.SetActive(false));

        PlayConfirmSFX();
    }

    public void DropMainMenu()
    {
        PlayWaitingBGM();

        MainMenuGroup.interactable = false;
        MainMenuCanvas.gameObject.SetActive(true);
        MainMenuPanell.anchoredPosition = new Vector2(0, 1000);
        MainMenuPanell.DOAnchorPos(new Vector2(0, 0), 1.5f).
            SetUpdate(true).
            OnComplete(() => {
                MainMenuGroup.interactable = true;
            });
    }

    public void DropDefeatMenu()
    {
        InGameMenuGroup.interactable = false;

        DefeatMenuGroup.interactable = false;
        DefeatMenuCanvas.gameObject.SetActive(true);
        DefeatMenuPannel.anchoredPosition = new Vector2(0, -1000);
        DefeatMenuPannel.DOAnchorPos(new Vector2(0, 0), 1.5f).
            SetUpdate(true).
            OnComplete(() => {
                DefeatMenuGroup.interactable = true;
            });

        PlayDefeatSfx();
        StopBGM();
    }

    public void ClickPlayButton()
    {
        MainMenuGroup.interactable = false;
        MainMenuPanell.DOAnchorPos(new Vector2(0, 1000), 1.5f).
            SetUpdate(true).
            OnComplete(() => {
                MainMenuGroup.interactable = false;
                MainMenuCanvas.gameObject.SetActive(false);
                GamePlayManager.Instance.AssembleGamePlay();

                //startCountDown
                m_countDownCounter = COUNTDOWN_TIME;
                CountDownCanvas.gameObject.SetActive(true);
                CountDownText.text = COUNTDOWN_TIME.ToString();
                CountDownText.transform.localScale = Vector3.zero;
                CountDownText.transform.DOScale(1, 0.4f).
                    SetEase(Ease.OutElastic).
                    OnComplete(() => { StartCoroutine(CountDown()); });
            });

        PlayConfirmSFX();
    }

    public void ClickQuitButton()
    {
        MainMenuGroup.interactable = false;
        MainMenuPanell.DOAnchorPos(new Vector2(0, 1000), 1.5f).
            SetUpdate(true).
            OnComplete(() => {
                MainMenuGroup.interactable = false;
                MainMenuCanvas.gameObject.SetActive(false);
                Application.Quit();
            });

        PlayMenuHideSFX();
    }

    IEnumerator CountDown()
    {
        StopBGM();
        PlayGetReadySFX();

        float timer = 0;
        while(m_countDownCounter > 0)
        {
            timer += Time.deltaTime;
            if(timer >= 1)
            {
                m_countDownCounter--;
                CountDownText.text = m_countDownCounter.ToString();
                timer = 0;
            }

            yield return null;
        }

        PrepareForGameStart();
    }

    private void PrepareForGameStart()
    {
        CountDownText.transform.localScale = Vector3.zero;
        CountDownText.transform.DOScale(0, 0.4f).
            SetEase(Ease.OutElastic).
            OnComplete(() => {

                CountDownCanvas.gameObject.SetActive(false);

                ScoreCanvas.gameObject.SetActive(true);
                m_scoreCounter = 0;
                ScoreValue.text = m_scoreCounter.ToString();
                ScorePannel.transform.localScale = Vector3.zero;
                ScorePannel.DOScale(1, 0.4f).
                    SetEase(Ease.OutElastic);

                InGameMenuCanvas.gameObject.SetActive(true);
                InGameMenuPannel.transform.localScale = Vector3.zero;
                InGameMenuPannel.DOScale(1, 0.4f).
                    SetEase(Ease.OutElastic).
                    OnComplete(() => InGameMenuGroup.interactable = true);

                NextTetrominoCanvas.gameObject.SetActive(true);
                NextTetrominoTitle.transform.localScale = Vector3.zero;
                NextTetrominoTitle.transform.DOScale(1, 0.4f).
                    SetEase(Ease.OutElastic);

                GamePlayManager.Instance.StartGame();

                PlayRandomBGM();
            });
    }

    public void IncreaseScore(int value)
    {
        m_scoreCounter += value;
        ScoreValue.transform.
            DOPunchScale(Vector3.one * 1.2f, 0.3f).
            OnComplete(() => ScoreValue.text = m_scoreCounter.ToString());
    }
}
