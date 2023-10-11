using System;
using System.Collections.Generic;
using Godot;

namespace Weave;

public class Grid
{
    private readonly float _cellHeight;
    private readonly IList<IList<Cell>> _cells = new List<IList<Cell>>();
    private readonly float _cellWidth;
    private readonly int _height;
    private readonly int _nrCols;
    private readonly int _nrRows;
    private readonly int _width;

    public Grid(int nrRows, int nrCols, int width, int height)
    {
        _nrRows = nrRows;
        _nrCols = nrCols;
        _width = width;
        _height = height;

        _cellWidth = (float)width / nrRows;
        _cellHeight = (float)height / nrCols;

        // Populate grid with empty cells
        for (var rowIndex = 0; rowIndex < _nrRows; rowIndex++)
        {
            List<Cell> rowList = new();
            for (var colIndex = 0; colIndex < _nrCols; colIndex++)
            {
                var position = new Vector2(colIndex * _cellWidth, rowIndex * _cellHeight);
                var rect = new Rect2(position, new Vector2(_cellWidth, _cellHeight));
                var cell = new Cell(rect);
                rowList.Add(cell);
            }

            _cells.Add(rowList);
        }
    }

    public IEnumerable<SegmentShape2D> GetSegmentsFromPlayerPosition(
        Vector2 playerPosition,
        float playerRadius
    )
    {
        var playerSegments = new HashSet<SegmentShape2D>();

        for (var i = 0; i < _nrCols; i++)
        {
            for (var j = 0; j < _nrRows; j++)
            {
                if (IsCircleIntersectingRectangle(playerPosition, playerRadius, _cells[i][j].Rect))
                    playerSegments.UnionWith(_cells[i][j].Segments);
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
            return true;

        var closestX = Mathf.Clamp(
            circleCenter.X,
            rectangle.Position.X,
            rectangle.Position.X + rectangle.Size.X
        );

        var closestY = Mathf.Clamp(
            circleCenter.Y,
            rectangle.Position.Y,
            rectangle.Position.Y + rectangle.Size.Y
        );

        var closestPoint = new Vector2(closestX, closestY);
        var distance = circleCenter.DistanceTo(closestPoint);

        return distance <= circleRadius;
    }

    private bool IsPointOutsideBounds(Vector2 point)
    {
        return point.X < 0 || point.X > _width || point.Y < 0 || point.Y > _height;
    }

    private readonly struct Cell
    {
        public Rect2 Rect { get; }
        public ISet<SegmentShape2D> Segments { get; } = new HashSet<SegmentShape2D>();

        public Cell(Rect2 rect2)
        {
            Rect = rect2;
        }
    }
}
