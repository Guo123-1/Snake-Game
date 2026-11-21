using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Snake : MonoBehaviour
{
    public SnakeView view;           // 画面。Inspector 里拖，规则这边不关心它怎么画

    public int startLength = 4;
    public float interval = 0.3f;
    public float cellSize = 1f;
    public int boardSize = 20;

    List<Vector2Int> body = new List<Vector2Int>();
    Vector2Int dir = Vector2Int.up;
    List<Vector2Int> pending = new List<Vector2Int>();
    public int maxPending = 2;


    Vector2Int food;
    int score;
    public int Score => score;
    bool isDead;
    public bool IsDead => isDead;

    const string BestKey = "Best";    // 存/读共用，只写一次
    int best;
    public int Best => best;          // 给 HUD 读，不让外面改



    void Awake()
    {
        best = PlayerPrefs.GetInt(BestKey, 0);
    }

    void Start()
    {
        if (view == null)
        {
            Debug.LogError("[Snake] view 没有指定，去 Inspector 里把 SnakeView 拖进来", this);
            return;
        }

        view.Build(boardSize, cellSize);

        for (int i = 0; i < startLength; i++)
            body.Add(-dir * i);

        SpawnFood();
        view.Render(body, food, isDead);
        StartCoroutine(TickLoop());
    }

    void Update()
    {
        if (isDead)
        {
            if (Input.GetKeyDown(KeyCode.R))
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow)) TryTurn(Vector2Int.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) TryTurn(Vector2Int.down);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) TryTurn(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) TryTurn(Vector2Int.right);
    }

    IEnumerator TickLoop()
    {
        while (!isDead)
        {
            yield return new WaitForSeconds(interval);

            if (pending.Count > 0)
            {
                dir = pending[0];
                pending.RemoveAt(0);
            }

            Move(dir);
            view.Render(body, food, isDead);
        }
    }

    // —— 规则 ——

    void TryTurn(Vector2Int newDir)
    {
        if (pending.Count >= maxPending) return;      // 队列满了，这一下丢掉

        Vector2Int last = pending.Count > 0 ? pending[pending.Count - 1] : dir;
        if (newDir == -last) return;                  // 不能 180 度掉头

        pending.Add(newDir);
    }

    void Move(Vector2Int d)
    {
        Vector2Int newHead = body[0] + d;

        // eats 必须在判生死之前算：它决定了尾巴这一格算不算障碍
        bool eats = (newHead == food);
        if (!CanEnter(newHead, eats)) { Die(); return; }

        if (eats) body.Add(body[body.Count - 1]);

        for (int i = body.Count - 1; i >= 1; i--)
            body[i] = body[i - 1];

        body[0] = newHead;

        if (eats)
        {
            score++;
            Debug.Log($"得分：{score}");
            SpawnFood();
        }
    }
    // —— 规则 ——

    /// <summary>
    /// 这一格能不能进。
    /// eats 决定尾巴算不算障碍：不吃的时候，尾巴和头同步前移，会把它原来的格子让出来；
    /// 吃到的时候身体整体加长，尾巴留在原地，那一格仍然是实心的。
    /// </summary>
    bool CanEnter(Vector2Int cell, bool eats)
    {
        int half = boardSize / 2;
        if (cell.x < -half || cell.x >= half) return false;
        if (cell.y < -half || cell.y >= half) return false;

        int limit = eats ? body.Count : body.Count - 1;   // ← 唯一的一行差别
        for (int i = 0; i < limit; i++)
            if (body[i] == cell) return false;
        return true;
    }

    // 注意：这个函数不排除尾巴，因为 SpawnFood 要用它 —— 食物不能刷在尾巴上。
    bool IsOnBody(Vector2Int cell)
    {
        for (int i = 0; i < body.Count; i++)
            if (body[i] == cell) return true;
        return false;
    }

    void SpawnFood()
    {
        int half = boardSize / 2;
        List<Vector2Int> free = new List<Vector2Int>();

        for (int x = -half; x < half; x++)
            for (int y = -half; y < half; y++)
            {
                Vector2Int c = new Vector2Int(x, y);
                if (!IsOnBody(c)) free.Add(c);
            }

        if (free.Count == 0) return;      // 满屏，理论上不可能
        food = free[Random.Range(0, free.Count)];
    }

    /// <summary>
    /// 死了。所有"死亡之后要做的事"都收在这里：
    /// 现在只有存最高分，将来加特效、音效、成就，也往这里加。
    /// </summary>
    void Die()
    {
        if (isDead) return;      // 幂等：死了就不要再死一次

        isDead = true;

        if (score > best)
        {
            best = score;
            PlayerPrefs.SetInt(BestKey, best);
            PlayerPrefs.Save();      // 只在真正破纪录时落盘，一次
        }
    }

}


