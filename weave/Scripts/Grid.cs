using System;
using System.Collections.Generic;
using Godot;

public class Grid
{
    private readonly int _nrRows;
    private readonly int _nrCols;
    private readonly int _width;
    private readonly int _height;
    private readonly float _cellWidth;
    private readonly float _cellHeight;
    private readonly List<List<Cell>> _cells = new();

    public class Cell
    {
        public Rect2 Rect2;
        public ISet<SegmentShape2D> Segments = new HashSet<SegmentShape2D>();

        public Cell(Rect2 rect2)
        {
            Rect2 = rect2;
        }
    }

    public Grid(int nrRows, int nrCols, int width, int height)
    {
        _nrRows = nrRows;
        _nrCols = nrCols;
        _width = width;
        _height = height;

        _cellWidth = width / nrRows;
        _cellHeight = height / nrCols;

        // Populate grid with empty cells
        for (var i = 0; i < _nrCols; i++)
        {
            List<Cell> rowList = new();
            for (var j = 0; j < _nrRows; j++)
            {
                var p1 = new Vector2(j * _cellWidth, i * _cellHeight);
                var p2 = new Vector2((j + 1) * _cellWidth, (i + 1) * _cellHeight);
                var rect = new Rect2(p1, p2);
                var cell = new Cell(rect);
                rowList.Add(cell);
            }
            _cells.Add(rowList);
        }
    }

    public ISet<SegmentShape2D> GetSegmentsFromPlayerPosition(
        Vector2 playerPosition,
        float playerRadius
    )
    {
        var playerSegments = new HashSet<SegmentShape2D>();

        for (var i = 0; i < _nrCols; i++)
        {
            for (var j = 0; j < _nrRows; j++)
            {
                if (IsCircleIntersectingRectangle(playerPosition, playerRadius, _cells[i][j].Rect2))
                {
                    playerSegments.UnionWith(_cells[i][j].Segments);
                }
            }
        }

        return playerSegments;
    }

    public void AddSegment(SegmentShape2D segment)
    {
        if (IsPointOutsideBounds(segment.A) || IsPointOutsideBounds(segment.B))
            return;

        var aXIndex = (int)Math.Floor(segment.A.X / _cellWidth);
        var aYIndex = (int)Math.Floor(segment.A.Y / _cellHeight);

        var bXIndex = (int)Math.Floor(segment.B.X / _cellWidth);
        var bYIndex = (int)Math.Floor(segment.B.Y / _cellHeight);

        _cells[aYIndex][aXIndex].Segments.Add(segment);
        _cells[bYIndex][bXIndex].Segments.Add(segment);
    }

    private static bool IsCircleIntersectingRectangle(
        Vector2 circleCenter,
        float circleRadius,
        Rect2 rectangle
    )
    {
        if (rectangle.HasPoint(circleCenter))
        {
            return true;
        }

        float closestX = Mathf.Clamp(
            circleCenter.X,
            rectangle.Position.X,
            rectangle.Position.X + rectangle.Size.X
        );
        float closestY = Mathf.Clamp(
            circleCenter.Y,
            rectangle.Position.Y,
            rectangle.Position.Y + rectangle.Size.Y
        );

        Vector2 closestPoint = new(closestX, closestY);
        float distance = circleCenter.DistanceTo(closestPoint);

        return distance <= circleRadius;
    }

    private bool IsPointOutsideBounds(Vector2 point)
    {
        return point.X < 0 || point.X > _width || point.Y < 0 || point.Y > _height;
    }
}
