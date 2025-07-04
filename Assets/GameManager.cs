using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static CartoonFX.CFXR_Effect;
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

/*    public Text TimerText;  // Assign this in the Unity Inspector
    private float timer = 60f; // Timer starts at 60 seconds
    private bool isTimerRunning = false;*/

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

   // [SerializeField] private List<GameObject> Bears;
    private int currentBearIndex = 0;
    private float targetY = -0.43f;
    [SerializeField] private float waitTimeBeforeReturn = 2.0f;
    [SerializeField] private float speed = 1.0f;

    public AudioSource UISound;
    public AudioSource Tap;
    public GameObject intro;
    public ParticleSystem tapParticlePrefab;
    public ParticleSystem WinParticlePrefab;


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

       // StartCoroutine(MoveBear(Bears[currentBearIndex]));

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

       /* if (isTimerRunning)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
               // CameraShake.IsShaking = false;
                GameState = false;
                timer = 0f;
                isTimerRunning = false;
                GameOver();
            }
            UpdateTimerUI();
        }*/


        if (Time.time - lastTapTime > tapDecayDelay)
        {
            DecreaseSliderValue();
            
        }

        if (Time.time - lastTapTime > tapDecayDelay)
        {
            scoreDecreaseTimer += Time.deltaTime;
            if (scoreDecreaseTimer >= scoreDecreaseInterval)
            {
                SubtractScore(scoreDecreaseAmount);
                scoreDecreaseTimer = 0f;
            }
        }
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
        // 1) Play tap particle at that spot
        if (tapParticlePrefab != null)
        {
            // Instantiate the particle system, it'll auto-play if set up that way
            ParticleSystem ps = Instantiate(tapParticlePrefab, position, Quaternion.identity);
            ps.Play();
        }

        // 2) Spawn the slime
        GameObject slime = Instantiate(slimePrefab, position, Quaternion.identity);
        StartCoroutine(DeactivateAfterDelay(slime, 0.1f)); // or however long you need
    }

    private IEnumerator DeactivateAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj); // Use Destroy if instantiating, or obj.SetActive(false) if reusing
    }





    /* private GameObject GetRandomBear()
     {
         int randomIndex = Random.Range(0, Bears.Count);
         return Bears[randomIndex];
     }*/

    public void IncreaseSliderValue(float baseIncrement)
    {
        if (progressBar == null) return;

        // compute how full the bar is, 0 to 1
        float fillRatio = progressBar.value / progressBar.maxValue;

        // choose strength multiplier based on the fillRatio buckets
        float strength;
        if (fillRatio < 0.3f)
        {
            strength = 0.8f;        // 0–30%
        }
        else if (fillRatio < 0.7f)
        {
            strength = 0.5f;      // 30–70%
        }
        else
        {
            strength = 0.3f;      // 70–100%
        }

        // scale your increment
        float scaledIncrement = baseIncrement * strength;

        // store previous for threshold checks
        float previousValue = progressBar.value;

        // apply and clamp
        progressBar.value = Mathf.Clamp(progressBar.value + scaledIncrement, 0f, progressBar.maxValue);

        // Optional: threshold checks (25%, 50%, 75%) here
        float quarterWay = progressBar.maxValue * 0.25f;
        float halfway = progressBar.maxValue * 0.5f;
        float threeQuarterWay = progressBar.maxValue * 0.75f;

        if (previousValue < quarterWay && progressBar.value >= quarterWay)
        {
            // reached 25%
        }
        if (previousValue < halfway && progressBar.value >= halfway)
        {
            // reached 50%
        }
        if (previousValue < threeQuarterWay && progressBar.value >= threeQuarterWay)
        {
            // reached 75%
        }

        // win condition
        if (progressBar.value >= progressBar.maxValue)
        {

            GameWin();
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
   
    /*private void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 10);
        int seconds = Mathf.FloorToInt(timer % 10);
        TimerText.text = $"{minutes:00}:{seconds:00}";
    }*/
    public void GameWin()
    {
        particle.Play();
        GameState = false;
        GameWinScreen.SetActive(true);
        Debug.Log(currentScore);
        SendScore(currentScore, 155);
    }

    public void GameOver()
    {
      
        GameState = false;
        Debug.Log(currentScore);
        GameOverScreen.SetActive(true);
        SendScore(currentScore, 155);
        
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
        intro.SetActive(true);
        GameOverOtherScreen.SetActive(false);
        isPlaying = false;
      //  timer = 20f;
        GameState = true;
      //  isTimerRunning = true;
        ScoreText.text = "0";
        Score.score = 0;
        currentScore = 0;
      //  deactivePeople();
      //  UpdateTimerUI();
        Playeranimators.SetBool("isEnd", false);
        InfoScreen.SetActive(false);
        GameOverScreen.SetActive(false);
        GameWinScreen.SetActive(false);
        GameState = true;
        if (progressBar != null)
            progressBar.value = 1;

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