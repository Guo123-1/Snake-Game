using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 蛇和食物的画面。
/// 它只知道"给我一串格子坐标，我把方块摆上去"，不知道任何规则：
/// 不知道为什么死、不知道哪格能吃、不知道下一步往哪走。
/// 想换表现（贴图、精灵、颜色、动画），只改这个文件。
/// </summary>
public class SnakeView : MonoBehaviour
{
    List<GameObject> segments = new List<GameObject>();
    GameObject foodObj;
    float cellSize = 1f;
    bool died;                       // 变红只需要做一次

    /// <summary>
    /// 建棋盘和食物。由 Snake 在 Start 里调，尺寸从它那边传过来，
    /// 避免两个脚本各配一份 boardSize 然后对不上。
    /// </summary>
    public void Build(int boardSize, float cellSize)
    {
        this.cellSize = cellSize;

        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Board";
        floor.transform.position = new Vector3(-0.5f, -0.05f, -0.5f);
        floor.transform.localScale = new Vector3(boardSize, 0.1f, boardSize);
        floor.GetComponent<Renderer>().material.color = new Color(0.15f, 0.15f, 0.18f);

        foodObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        foodObj.name = "Food";
        foodObj.transform.localScale = Vector3.one * 0.6f;
        foodObj.GetComponent<Renderer>().material.color = new Color(1f, 0.25f, 0.2f);
    }

    /// <summary>
    /// 把当前数据画出来。每走一格调一次。
    /// 传进来的是快照，不是引用 —— View 不回头改数据。
    /// </summary>
    public void Render(List<Vector2Int> body, Vector2Int food, bool isDead)
    {
        // 第一次画的时候顺手建头，之后补齐的都是身体
        if (segments.Count == 0)
            segments.Add(CreateSegment(true));

        while (segments.Count < body.Count)
            segments.Add(CreateSegment(false));

        for (int i = 0; i < body.Count; i++)
            segments[i].transform.position = CellToWorld(body[i]);

        foodObj.transform.position = CellToWorld(food);

        if (isDead && !died)
        {
            died = true;
            foreach (GameObject go in segments)
                go.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    GameObject CreateSegment(bool isHead)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = isHead ? "Head" : "Body";
        go.transform.localScale = Vector3.one * 0.9f;
        go.GetComponent<Renderer>().material.color = isHead ? Color.green : Color.white;
        return go;
    }

    Vector3 CellToWorld(Vector2Int cell)
    {
        return new Vector3(cell.x * cellSize, 0.5f, cell.y * cellSize);
    }
}
