using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;
using static UnityEngine.ParticleSystem;

public class GameManager : MonoBehaviour
{
    // Static instance for the singleton
    public static GameManager Instance { get; private set; }

    public int GameID = 0;
    public GameObject Information;
    public GameObject GameOverScreen, GameWinScreen, InfoScreen;
    public GameObject GameOverOtherScreen;
    public bool GameState = false;
    public BasePlayer Player;

    public Text ScoreText;
    private int currentScore;

    private ScoreObj Score;

    public Text TimerText;  // Assign this in the Unity Inspector
    private float timer = 10f; // Timer starts at 60 seconds
    private bool isTimerRunning = false;

    public Slider progressBar;

    private float lastTapTime = 0f; // Time since the last tap
    private float tapDecayDelay = 0f; // Time before value starts decreasing
    public float decayRate = 0.1f;

    public GameObject[] peopleSad;
    public GameObject[] peopleHappy;
    //public GameObject[] ;
     public Animator Playeranimators;
    public AudioSource[] PlayerAudioSource;


    // Add these fields at the class level:
    private float scoreDecreaseTimer = 0f;
    public float scoreDecreaseInterval = 1f;  // Subtract score once every second
    public int scoreDecreaseAmount = 10;

    public GameObject[] rocks;

    public GameObject slimePrefab;
    public ParticleSystem particle;

    public bool isPlaying = false;

    public GameObject obstaclePrefab;
    public GameObject obstaclePointCenter;
    public float spawnDelay = 1f;
    public float Bulletspeed = 5f;


    public GameObject Enemy;
    private Vector3 enemyInitialPosition;

    public float enemySpeed = 2f;
    private float leftLimit = -2.4f;
    private float rightLimit = 2.4f;
    private int enemyDirection = 1;

    private Vector3 targetPosition;

    [SerializeField] private List<GameObject> Bears;
    private int currentBearIndex = 0;
    private float targetY = -0.43f;
    [SerializeField] private float waitTimeBeforeReturn = 2.0f;
    [SerializeField] private float speed = 1.0f;

    public AudioSource UISound;
    public AudioSource Tap;

    [DllImport("__Internal")]
    private static extern void SendScore(int score, int game);

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
           // Debug.LogWarning("Another instance of GameManager already exists. Destroying this instance.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    private void Start()
    {
        InfoScreen.SetActive(true);
        enemyInitialPosition = Enemy.transform.position;

        StartCoroutine(MoveBear(Bears[currentBearIndex]));

    }

    void Update()
    {
        if (!GameState)
            return;

        if (!isPlaying)
        {
            StartCoroutine(RandomToggleRocks());
        }
       

        if (progressBar.value == 0)
        {
            GameOverOtherScreen .SetActive(true);
            GameOver();
        }
        MoveEnemy();

        //  Playeranimators.SetFloat("Val", progressBar.value / progressBar.maxValue);

        if (Input.GetMouseButtonDown(0))
        {
           
            /*  Vector2 spawnPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
              SpawnSlimeAt(spawnPos);

              IncreaseSliderValue(0.4f);
              PlayerAudioSource[2].Play();*/

            /*        Vector2 worldClick = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    SpawnObstacle(worldClick);

                    PlayerAudioSource[2].Play();*/
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began) // Detects only the initial tap, ignoring holds
            {
                Vector2 spawnPos = Camera.main.ScreenToWorldPoint(touch.position);
             //   SpawnSlimeAt(spawnPos);

              //  IncreaseSliderValue(0.1f);
               // PlayerAudioSource[2].Play();
            }
        }



        if (Time.time - lastTapTime > tapDecayDelay)
        {
            DecreaseSliderValue();
            
        }

        /*if (Time.time - lastTapTime > tapDecayDelay)
        {
            scoreDecreaseTimer += Time.deltaTime;
            if (scoreDecreaseTimer >= scoreDecreaseInterval)
            {
                SubtractScore(scoreDecreaseAmount);
                scoreDecreaseTimer = 0f;
            }
        }*/
        //GAME LOGIC

    }

    private void MoveEnemy()
    {
        if (Enemy != null)
        {
            // Move the enemy left and right
            Enemy.transform.position += Vector3.right * enemyDirection * enemySpeed * Time.deltaTime;

            // Change direction at boundaries
            if (Enemy.transform.position.x >= rightLimit)
            {
                enemyDirection = -1; // Move left
            }
            else if (Enemy.transform.position.x <= leftLimit)
            {
                enemyDirection = 1; // Move right
            }
        }
    }
    private void SpawnObstacle(Vector2 clickWorldPos)
    {
        // ignore clickWorldPos.y here – we'll always go up to y = 10
        Vector3 startPos = obstaclePointCenter.transform.position;
        GameObject obstacle = Instantiate(obstaclePrefab, startPos, Quaternion.identity);
        StartCoroutine(MoveObstacle(obstacle, clickWorldPos.x));
    }

    private IEnumerator MoveObstacle(GameObject obj, float targetX)
    {
        Vector2 startPos = obstaclePointCenter.transform.position;
        Vector2 endPos = new Vector2(targetX, 10f);
        float duration = 2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            obj.transform.position = Vector2.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = endPos;
        Destroy(obj, 1.5f);
    }


    public void SpawnSlimeAt(Vector2 position)
    {
        GameObject slime = Instantiate(slimePrefab, position, Quaternion.identity);
        StartCoroutine(DeactivateAfterDelay(slime, 0.1f)); // Deactivate after 0.5 seconds
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj); // Use Destroy if instantiating, or obj.SetActive(false) if reusing
    }

    private IEnumerator MoveBear(GameObject bear)
    {


        Vector3 startPosition = new Vector3(bear.transform.position.x, -3.19f, bear.transform.position.z);
        bear.transform.position = startPosition;
        targetPosition = new Vector3(bear.transform.position.x, targetY, bear.transform.position.z);

        Vector3 halfwayPosition = Vector3.Lerp(startPosition, targetPosition, 0.5f);

        while (bear.transform.position != targetPosition)
        {

            bear.transform.position = Vector3.MoveTowards(bear.transform.position, targetPosition, speed * Time.deltaTime);

            if (bear.transform.position.y >= halfwayPosition.y)
            {
                bear.GetComponent<Collider2D>().enabled = true;
            }
            yield return null;
        }


        yield return new WaitForSeconds(waitTimeBeforeReturn);

        // Move the bear back down
        while (bear.transform.position != startPosition)
        {
            bear.transform.position = Vector3.MoveTowards(bear.transform.position, startPosition, speed * Time.deltaTime);
            bear.GetComponent<Collider2D>().enabled = false;
            yield return null;
        }

        yield return new WaitForSeconds(5);

        StartCoroutine(MoveBear(GetRandomBear()));
    }



    private GameObject GetRandomBear()
    {
        int randomIndex = Random.Range(0, Bears.Count);
        return Bears[randomIndex];
    }

    public void IncreaseSliderValue(float increment)
    {
        if (progressBar != null)
        {
            float previousValue = progressBar.value;
            progressBar.value += increment; // Increase slider value
            progressBar.value = Mathf.Clamp(progressBar.value, 0, progressBar.maxValue);
         

            float quarterWay = progressBar.maxValue * 0.25f;
            float halfway = progressBar.maxValue * 0.5f;
            float threeQuarterWay = progressBar.maxValue * 0.75f;
           

            if (previousValue < quarterWay && progressBar.value >= quarterWay)
            {
              
                //Debug.Log("Slider reached 25% (Quarter Way)");
            }
            if (previousValue < halfway && progressBar.value >= halfway)
            {
               
               
                //  Debug.Log("Slider reached 50% (Halfway)");
            }
            if (previousValue < threeQuarterWay && progressBar.value >= threeQuarterWay)
            {
               // activePeople();
                //  Debug.Log("Slider reached 75% (Three-Quarter Way)");
            }

            if (progressBar.value >= progressBar.maxValue)
            {
              //  PlayerAudioSource[1].Play();
                GameWin();
                
            }

        }
        lastTapTime = Time.time;
    }
    private void DecreaseSliderValue()
    {
        float previousValue = progressBar.value;
        if (progressBar != null && progressBar.value > 0)
        {
            progressBar.value -= decayRate * Time.deltaTime; // Decrease over time
          
            progressBar.value = Mathf.Clamp(progressBar.value, 0, progressBar.maxValue);
           

            float quarterWay = progressBar.maxValue * 0.25f;
            float halfway = progressBar.maxValue * 0.5f;
            float threeQuarterWay = progressBar.maxValue * 0.75f;

            if (previousValue >= threeQuarterWay && progressBar.value < threeQuarterWay)
            {

               // deactivePeople();
                // Debug.Log("Slider dropped below 75%");
            }
            if (previousValue >= halfway && progressBar.value < halfway)
            {
          

              

                // Debug.Log("Slider dropped below 50%");
            }
            if (previousValue >= quarterWay && progressBar.value < quarterWay)
            {
               

                // Debug.Log("Slider dropped below 25%");
            }


            
        }
    }
   
    private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 10);
        int seconds = Mathf.FloorToInt(timer % 10);
        TimerText.text = $"{minutes:00}:{seconds:00}";
    }
    public void GameWin()
    {
        particle.Play();
        GameState = false;
        GameWinScreen.SetActive(true);
        Debug.Log(currentScore);
        SendScore(currentScore, 124);
    }

    public void GameOver()
    {
       
        GameState = false;
        Debug.Log(currentScore);
        GameOverScreen.SetActive(true);
        SendScore(currentScore, 124);
        
    }
    public void AddScore()
    {


        if (int.TryParse(ScoreText.text, out currentScore))
        {
            currentScore += 10;
            ScoreText.text = currentScore.ToString();
        }
        else
        {

            ScoreText.text = "0";
        }
    }

    public void PlayGame()
    {

        Time.timeScale = 1;
        Information.SetActive(false);
    }
    public void PauseGame()
    {

        Information.SetActive(true);
        StartCoroutine(Pause());
    }

    public void uiSound()
    {
        UISound.Play();
    }

    IEnumerator Pause()
    {


        // Wait for a specified duration (adjust the delay as needed)
        yield return new WaitForSeconds(0.2f);
        Time.timeScale = 0;


    }

    public void SubtractScore(int amount)
    {
        if (int.TryParse(ScoreText.text, out currentScore))
        {
            // Subtract 'amount' points but ensure the score doesn't drop below 0.
            currentScore = Mathf.Max(currentScore - amount, 0);
            ScoreText.text = currentScore.ToString();
        }
        else
        {
            ScoreText.text = "0";
        }
    }

    public void GameResetScreen()
    {
        GameOverOtherScreen.SetActive(false);
        isPlaying = false;
        timer = 10f;
        GameState = true;
        isTimerRunning = true;
        ScoreText.text = "0";
        Score.score = 0;
        currentScore = 0;
      //  deactivePeople();
        UpdateTimerUI();
        Playeranimators.SetBool("isEnd", false);
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        GameState = true;
        if (progressBar != null)
            progressBar.value = 2;

        //Player.Reset();
    }

    public void AddScore(float f)
    {
        Score.score += f;
    }



    //HELPER FUNTION TO GET SPAWN POINT
    public Vector2 GetRandomPointInsideSprite(SpriteRenderer SpawnBounds)
    {
        if (SpawnBounds == null || SpawnBounds.sprite == null)
        {
            Debug.LogWarning("Invalid sprite renderer or sprite.");
            return Vector2.zero;
        }

        Bounds bounds = SpawnBounds.sprite.bounds;
        Vector2 randomPoint = new Vector2(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y)
        );

        // Transform local point to world space
        return SpawnBounds.transform.TransformPoint(randomPoint);
    }

    private IEnumerator RandomToggleRocks()
    {
        // GameState = true;
         isPlaying = true;
        while (GameState)  // Loop continuously as long as the game is active
        {
            if (rocks != null && rocks.Length > 0)
            {
                // First, ensure all rocks are deactivated
                foreach (GameObject rock in rocks)
                {
                    rock.SetActive(false);
                }

                // Wait for a brief pause before activating a rock
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));

                // Pick a random rock and activate it
                int randomIndex = Random.Range(0, rocks.Length);
                GameObject selectedRock = rocks[randomIndex];
                selectedRock.SetActive(true);

                // Wait while the rock stays active
                yield return new WaitForSeconds(Random.Range(1f, 2f));

                // Deactivate the rock
                selectedRock.SetActive(false);

                // Optional: wait before starting the next cycle
                yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            }
            else
            {
                yield return null;
            }

        }
        
    }


    public struct ScoreObj
    {
        public float score;
    }
}