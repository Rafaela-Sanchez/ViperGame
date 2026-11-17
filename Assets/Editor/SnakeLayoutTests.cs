using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class SnakeLayoutTests
{
    [Test]
    public void LayoutIsContiguousAndUniqueForEveryLength()
    {
        for (int n = 1; n <= Snake.Width * Snake.Height; n++)
        {
            var cells = Snake.Layout(n);
            Assert.AreEqual(n, cells.Count);
            Assert.AreEqual(n, new HashSet<Vector2Int>(cells).Count);
            Assert.AreEqual(new Vector2Int(Snake.Width / 2, Snake.Height / 2), cells[0]);
            if (n > 1) Assert.AreEqual(cells[0] + Vector2Int.left, cells[1]);
            for (int i = 1; i < n; i++)
            {
                var d = cells[i] - cells[i - 1];
                int dx = Mathf.Abs(d.x) == Snake.Width - 1 ? 1 : Mathf.Abs(d.x);
                int dy = Mathf.Abs(d.y) == Snake.Height - 1 ? 1 : Mathf.Abs(d.y);
                Assert.AreEqual(1, dx + dy, $"n={n} i={i}");
            }
        }
    }
}
