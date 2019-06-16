using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameController : MonoBehaviour {

    private static GameController instance;

    public static GameController Instance()
    {
        if (instance == null)
            instance = GameObject.FindObjectOfType(typeof(GameController)) as GameController;
        return instance;
    }

    public Star star_blue;
    public Star star_green;
    public Star star_purple;
    public Star star_red;
    public Star star_yellow;

    public int score;

    public static int WIDTH = 10;
    public static int HEIGHT = 10;

    public static float STAR_SIZE = 64;

    public Star[,] stars;

    public bool start;
    public bool ableToContinue;

    public GameObject startPanel;
    public GameObject gamePanel;
    public GameObject failPanel;
    public GameObject scoreChartsPanel;
    public GameObject sureToStartGamePanel;
    public GameObject cannotContinuePanel;
    public GameObject pausePanel;

    public Text scoreText;
    public Text[] scoreChartsText;
    public Text finalScoreText;
    public Text newRecordText;

    void Start()
    {
        start = false;

        stars = new Star[WIDTH, HEIGHT];

        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        failPanel.SetActive(false);

        score = -1;
        if (Database.Instance().userData != null)
        {
            if (Database.Instance().userData.scoreBefore > 0)
                ableToContinue = true;
            else
                ableToContinue = false;
        }
    }

    void Update()
    {
        if (start)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y));

                int x = Mathf.FloorToInt((touchPosition.x + WIDTH / 2 * STAR_SIZE) / STAR_SIZE);
                int y = Mathf.FloorToInt((touchPosition.y + HEIGHT / 2 * STAR_SIZE) / STAR_SIZE);

                OnClick(x, y);
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (start)
                PauseButton();
            else
                Application.Quit();
        }
    }

    private void OnClick(int x, int y)
    {
        if (x < 0 || y < 0 || x >= WIDTH || y >= HEIGHT)
            return;

        Star star = stars[x, y];

        if (star == null)
            return;

        if (star.state == Star.StateType.STATE_NORMAL)
        {
            CancelAllOtherSelectedStars();

            SelectThisStar(x, y);
        }
        else
        {
            PopThisStar(x, y);

            Database.Instance().SaveStarInfo();

            CheckFailSituation();
        }
    }

    private void CancelAllOtherSelectedStars()
    {
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                if (stars[i, j] != null)
                    stars[i, j].state = Star.StateType.STATE_NORMAL;
    }

    private void SelectThisStar(int x, int y)
    {
        List<Index> starList = GetAdjacentStars(x, y);

        foreach (var index in starList)
            stars[index.row, index.col].state = Star.StateType.STATE_SELECTED;
    }

    private void PopThisStar(int x, int y)
    {
        List<Index> starList = GetAdjacentStars(x, y);

        if (starList.Count <= 1)
            return;

        score += 10 * starList.Count * starList.Count;
        scoreText.text = "Score : " + score;

        foreach (var index in starList)
        {
            Destroy(stars[index.row, index.col].gameObject);
            stars[index.row, index.col] = null;
        }

        bool collapseVertically = false;

        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                if (stars[i, j] != null)
                    continue;

                int k = j + 1;
                while (k < HEIGHT && stars[i, k] == null)
                    k++;

                if (k == HEIGHT)
                    break;

                float xPosition = STAR_SIZE * i - WIDTH / 2 * STAR_SIZE;
                float yPosition = STAR_SIZE * j - HEIGHT / 2 * STAR_SIZE;

                stars[i, j] = stars[i, k];
                stars[i, k] = null;

                iTween.MoveTo(stars[i, j].gameObject, new Vector3(xPosition, yPosition, 0), 1);

                collapseVertically = true;
            }

        for (int i = 0; i < WIDTH; i++)
        {
            if (stars[i, 0] != null)
                continue;

            int k = i + 1;
            while (k < WIDTH && stars[k, 0] == null)
                k++;

            if (k == WIDTH)
                break;

            for (int j = 0; j < HEIGHT; j++)
            {
                stars[i, j] = stars[k, j];
                stars[k, j] = null;

                if (stars[i, j] != null)
                {
                    float position = STAR_SIZE * i - WIDTH / 2 * STAR_SIZE;

                    Hashtable ht = new Hashtable();
                    ht.Add("x", position);
                    ht.Add("time", 1);

                    if (collapseVertically)
                        ht.Add("delay", 1);

                    iTween.MoveTo(stars[i, j].gameObject, ht);
                }
            }
        }
    }




    private Index[] GetNeighbours(Index current)
    {
        return new Index[]
        {
            new Index(current.row - 1, current.col),
            new Index(current.row, current.col - 1),
            new Index(current.row + 1, current.col),
            new Index(current.row, current.col + 1)
        };
    }

    private Star GetRandomStar()
    {
        switch (Random.Range(0, 5))
        {
            case 0:
                return star_blue;
            case 1:
                return star_green;
            case 2:
                return star_purple;
            case 3:
                return star_red;
            default:
                return star_yellow;
        }
    }

    public Star GetStar(int colorType)
    {
        switch (colorType)
        {
            case 0:
                return star_blue;
            case 1:
                return star_green;
            case 2:
                return star_purple;
            case 3:
                return star_red;
            default:
                return star_yellow;
        }
    }

    private List<Index> GetAdjacentStars(int x, int y)
    {
        bool[,] visited = new bool[WIDTH, HEIGHT];

        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                visited[i, j] = false;

        Star startStar = stars[x, y];

        List<Index> starList = new List<Index>();

        List<Index> dfsList = new List<Index>();
        Index startIndex = new Index(x, y);
        dfsList.Add(startIndex);
        visited[startIndex.row, startIndex.col] = true;

        while (dfsList.Count != 0)
        {
            Index current = dfsList[0];
            dfsList.RemoveAt(0);

            starList.Add(current);

            Index[] neighbours = GetNeighbours(current);

            foreach (var next in neighbours)
            {
                if (next.row >= 0 && next.row < WIDTH &&
                    next.col >= 0 && next.col < HEIGHT &&
                    !visited[next.row, next.col] &&
                    stars[next.row, next.col] != null &&
                    stars[next.row, next.col].colorType == startStar.colorType)
                {
                    dfsList.Add(next);
                    visited[next.row, next.col] = true;
                }
            }
        }

        return starList;
    }

    private void CheckFailSituation()
    {
        bool[,] visited = new bool[WIDTH, HEIGHT];

        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                visited[i, j] = false;

        int uniqueStarNumber = 0;
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                if (!visited[i, j] && stars[i, j] != null)
                {
                    uniqueStarNumber++;

                    List<Index> starList = GetAdjacentStars(i, j);

                    foreach (var index in starList)
                        visited[index.row, index.col] = true;
                }

        int leftStarNumber = 0;
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                if (stars[i, j] != null)
                    leftStarNumber++;

        if (uniqueStarNumber == leftStarNumber)
        {
            start = false;
            failPanel.SetActive(true);
            newRecordText.gameObject.SetActive(false);
            gamePanel.SetActive(false);
            finalScoreText.text = "Score : " + score;
            Database.Instance().AddScoreToCharts(score);
            Database.Instance().FinishGame();
            ableToContinue = false;
        }
    }

    private void ClearAll()
    {
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
                if (stars[i, j] != null)
                {
                    Destroy(stars[i, j].gameObject);
                    stars[i, j] = null;
                }
    }

    public void StartNew()
    {
        for (int i = 0; i < WIDTH; i++)
            for (int j = 0; j < HEIGHT; j++)
            {
                float x = STAR_SIZE * i - WIDTH / 2 * STAR_SIZE;
                float y = STAR_SIZE * j - HEIGHT / 2 * STAR_SIZE;

                var star = Instantiate(GetRandomStar(), new Vector3(x, y, 0), Quaternion.identity) as Star;

                stars[i, j] = star;
            }

        score = 0;
        scoreText.text = "Score : " + score;

        startPanel.SetActive(false);
        gamePanel.SetActive(true);
        sureToStartGamePanel.SetActive(false);

        start = true;
        Database.Instance().SaveStarInfo();
    }

    public void BackToHome()
    {
        ClearAll();
        start = false;

        failPanel.SetActive(false);
        startPanel.SetActive(true);
        gamePanel.SetActive(false);
        scoreChartsPanel.SetActive(false);
        sureToStartGamePanel.SetActive(false);
        cannotContinuePanel.SetActive(false);
        pausePanel.SetActive(false);

        Database.Instance().ReadData();
    }

    public void Retry()
    {
        ClearAll();
        StartNew();
        failPanel.SetActive(false);
    }

    public void ShowScoreCharts()
    {
        scoreChartsPanel.SetActive(true);
        for (int i = 0; i < Database.SCORES_NUMBER; i++)
        {
            if (Database.Instance().userData.scores[i] > 0)
                scoreChartsText[i].text = i+1 + ".    " + Database.Instance().userData.scores[i];
            else
                scoreChartsText[i].text = i+1 + ".    0";
        }
    }

    public void ContinueGame()
    {
        List<StarInfo> tempList = Database.Instance().userData.starInfos;
        while (tempList.Count != 0)
        {
            StarInfo current = tempList[0];
            tempList.RemoveAt(0);

            float x = STAR_SIZE * current._index.row - WIDTH / 2 * STAR_SIZE;
            float y = STAR_SIZE * current._index.col - HEIGHT / 2 * STAR_SIZE;

            var star = Instantiate(GetStar(current._colorType), new Vector3(x, y, 0), Quaternion.identity) as Star;

            stars[current._index.row, current._index.col] = star;
        }
        score = Database.Instance().userData.scoreBefore;
        scoreText.text = "Score : " + score;

        startPanel.SetActive(false);
        gamePanel.SetActive(true);

        start = true;
    }

    public void ContinueGameButton()
    {
        if (ableToContinue)
            ContinueGame();
        else
            cannotContinuePanel.SetActive(true);
    }

    public void StartGameButton()
    {
        if (ableToContinue)
            sureToStartGamePanel.SetActive(true);
        else
            StartNew();
    }

    public void PauseButton()
    {
        pausePanel.SetActive(true);
    }

    public void BackToGameButton()
    {
        pausePanel.SetActive(false);
    }
}
