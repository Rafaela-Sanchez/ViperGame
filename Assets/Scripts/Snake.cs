using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Snake : MonoBehaviour
{
    public const int Width = 24;
    public const int Height = 16;

    public Transform segmentPrefab;
    public Transform food;
    public Sprite headSprite, bodySprite, cornerSprite, tailSprite;
    public float startSpeed = 5f, speedStep = 0.2f, maxSpeed = 15f;

    public event Action AteFood, HitSelf, Won;
    public bool Waiting { get; private set; } = true;

    readonly List<Transform> segments = new();
    Vector2Int direction = Vector2Int.right, nextDirection = Vector2Int.right, lastTailCell;
    float speed, timer;

    void Awake() => segments.Add(transform);

    void Update()
    {
        var input = ReadInput();
        if (input != Vector2Int.zero && input != -direction)
        {
            nextDirection = input;
            Waiting = false;
        }
        if (Waiting) return;
        timer += Time.deltaTime;
        if (timer < 1f / speed) return;
        timer -= 1f / speed;
        Move();
    }

    public void NewGame()
    {
        speed = startSpeed;
        Spawn(3);
        PlaceFood();
    }

    public void Respawn() => Spawn(segments.Count);

    void Spawn(int length)
    {
        while (segments.Count > length)
        {
            Destroy(segments[^1].gameObject);
            segments.RemoveAt(segments.Count - 1);
        }
        while (segments.Count < length) segments.Add(Instantiate(segmentPrefab, transform.parent));
        var cells = Layout(length);
        for (int i = 0; i < length; i++) segments[i].position = (Vector2)cells[i];
        direction = nextDirection = Vector2Int.right;
        timer = 0f;
        Waiting = true;
        Render();
        if (cells.Contains(Cell(food))) PlaceFood();
    }

    public static List<Vector2Int> Layout(int count)
    {
        var cells = new List<Vector2Int>();
        int cx = Width / 2, cy = Height / 2;
        for (int r = 0; r < Height; r++)
        {
            int y = (cy - r + Height) % Height;
            for (int i = 0; i < Width && cells.Count < count; i++)
            {
                int x = r == 0 ? cx - i : r % 2 == 1 ? i : Width - 1 - i;
                if (x < 0) break;
                cells.Add(new Vector2Int(x, y));
            }
        }
        for (int x = Width - 1; x > cx && cells.Count < count; x--) cells.Add(new Vector2Int(x, cy));
        return cells;
    }

    void Move()
    {
        direction = nextDirection;
        lastTailCell = Cell(segments[^1]);
        for (int i = segments.Count - 1; i > 0; i--) segments[i].position = segments[i - 1].position;
        var head = Cell(transform) + direction;
        transform.position = new Vector3((head.x + Width) % Width, (head.y + Height) % Height);
        Render();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform != food)
        {
            HitSelf?.Invoke();
            return;
        }
        segments.Add(Instantiate(segmentPrefab, (Vector2)lastTailCell, Quaternion.identity, transform.parent));
        speed = Mathf.Min(speed + speedStep, maxSpeed);
        Render();
        if (segments.Count == Width * Height)
        {
            Won?.Invoke();
            return;
        }
        PlaceFood();
        AteFood?.Invoke();
    }

    void PlaceFood()
    {
        var occupied = new HashSet<Vector2Int>();
        foreach (var s in segments) occupied.Add(Cell(s));
        var free = new List<Vector2Int>();
        for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                if (!occupied.Contains(new Vector2Int(x, y))) free.Add(new Vector2Int(x, y));
        food.position = (Vector2)free[UnityEngine.Random.Range(0, free.Count)];
    }

    void Render()
    {
        int last = segments.Count - 1;
        for (int i = 0; i <= last; i++)
        {
            var cell = Cell(segments[i]);
            var sr = segments[i].GetComponent<SpriteRenderer>();
            if (i == 0)
            {
                sr.sprite = headSprite;
                Point(segments[i], -Delta(cell, Cell(segments[1])));
            }
            else if (i == last)
            {
                sr.sprite = tailSprite;
                Point(segments[i], Delta(cell, Cell(segments[i - 1])));
            }
            else
            {
                var a = Delta(cell, Cell(segments[i - 1]));
                var b = Delta(cell, Cell(segments[i + 1]));
                sr.sprite = a == -b ? bodySprite : cornerSprite;
                segments[i].rotation = Quaternion.Euler(0f, 0f, a == -b ? Angle(a) : CornerAngle(a, b));
            }
        }
    }

    static float CornerAngle(Vector2Int a, Vector2Int b)
    {
        Vector2Int r = Vector2Int.right, u = Vector2Int.up;
        for (int k = 0; k < 4; k++)
        {
            if ((r == a && u == b) || (r == b && u == a)) return 90f * k;
            r = new Vector2Int(-r.y, r.x);
            u = new Vector2Int(-u.y, u.x);
        }
        return 0f;
    }

    static void Point(Transform t, Vector2Int dir) => t.rotation = Quaternion.Euler(0f, 0f, Angle(dir));

    static float Angle(Vector2Int d) => Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;

    static Vector2Int Delta(Vector2Int from, Vector2Int to)
    {
        var d = to - from;
        if (Mathf.Abs(d.x) > 1) d.x = -Math.Sign(d.x);
        if (Mathf.Abs(d.y) > 1) d.y = -Math.Sign(d.y);
        return d;
    }

    static Vector2Int Cell(Transform t) => Vector2Int.RoundToInt(t.position);

    static Vector2Int ReadInput()
    {
        var k = Keyboard.current;
        if (k == null) return Vector2Int.zero;
        if (k.upArrowKey.wasPressedThisFrame || k.wKey.wasPressedThisFrame) return Vector2Int.up;
        if (k.downArrowKey.wasPressedThisFrame || k.sKey.wasPressedThisFrame) return Vector2Int.down;
        if (k.leftArrowKey.wasPressedThisFrame || k.aKey.wasPressedThisFrame) return Vector2Int.left;
        if (k.rightArrowKey.wasPressedThisFrame || k.dKey.wasPressedThisFrame) return Vector2Int.right;
        return Vector2Int.zero;
    }
}
