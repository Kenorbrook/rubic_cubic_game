using System;

/// <summary>
/// Model: Представляет одну грань кубика 3x3
/// </summary>
[Serializable]
public class CubeFaceModel
{
    // Грань представлена как массив 3x3
    public CubeColor[,] Cells { get; private set; }

    public CubeFaceModel(CubeColor initialColor)
    {
        Cells = new CubeColor[3, 3];
        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                Cells[row, col] = initialColor;
            }
        }
    }

    /// <summary>
    /// Получить цвет ячейки
    /// </summary>
    public CubeColor GetCell(int row, int col)
    {
        return Cells[row, col];
    }

    /// <summary>
    /// Установить цвет ячейки
    /// </summary>
    public void SetCell(int row, int col, CubeColor color)
    {
        Cells[row, col] = color;
    }

    /// <summary>
    /// Повернуть грань по часовой стрелке
    /// </summary>
    public void RotateClockwise()
    {
        CubeColor[,] temp = new CubeColor[3, 3];

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                temp[col, 2 - row] = Cells[row, col];
            }
        }

        Cells = temp;
    }

    /// <summary>
    /// Повернуть грань против часовой стрелки
    /// </summary>
    public void RotateCounterClockwise()
    {
        CubeColor[,] temp = new CubeColor[3, 3];

        for (int row = 0; row < 3; row++)
        {
            for (int col = 0; col < 3; col++)
            {
                temp[2 - col, row] = Cells[row, col];
            }
        }

        Cells = temp;
    }

    /// <summary>
    /// Получить строку (для обмена с соседними гранями)
    /// </summary>
    public CubeColor[] GetRow(int rowIndex)
    {
        CubeColor[] row = new CubeColor[3];
        for (int col = 0; col < 3; col++)
        {
            row[col] = Cells[rowIndex, col];
        }
        return row;
    }

    /// <summary>
    /// Получить столбец (для обмена с соседними гранями)
    /// </summary>
    public CubeColor[] GetColumn(int colIndex)
    {
        CubeColor[] column = new CubeColor[3];
        for (int row = 0; row < 3; row++)
        {
            column[row] = Cells[row, colIndex];
        }
        return column;
    }

    /// <summary>
    /// Установить строку
    /// </summary>
    public void SetRow(int rowIndex, CubeColor[] colors)
    {
        for (int col = 0; col < 3; col++)
        {
            Cells[rowIndex, col] = colors[col];
        }
    }

    /// <summary>
    /// Установить столбец
    /// </summary>
    public void SetColumn(int colIndex, CubeColor[] colors)
    {
        for (int row = 0; row < 3; row++)
        {
            Cells[row, colIndex] = colors[row];
        }
    }
}
