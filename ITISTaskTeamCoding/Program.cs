using System;

namespace MazeSolver
{
    public class Maze
    {
        public char[,] grid;
        public int rows;
        public int cols;
        public bool[,] visited;
        public int startRow, startCol;
        public int endRow, endCol;
        public int visitedCount;

        public Maze(string[] input, int rows, int cols)
        {
            this.rows = rows;
            this.cols = cols;
            grid = new char[rows, cols];
            visited = new bool[rows, cols];
            visitedCount = 0;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    grid[i, j] = input[i][j];
                    if (grid[i, j] == 'S')
                    {
                        startRow = i;
                        startCol = j;
                    }
                    else if (grid[i, j] == 'E')
                    {
                        endRow = i;
                        endCol = j;
                    }
                }
            }
        }

        public void PrintMaze()
        {
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(grid[i, j]);
                }
                Console.WriteLine();
            }
        }

        public bool IsValidMove(int row, int col)
        {
            if (row < 0 || row >= rows || col < 0 || col >= cols)
                return false;
            if (grid[row, col] == '#')
                return false;
            if (visited[row, col])
                return false;
            return true;
        }
    }

    public class MazeSolver
    {
        private Maze maze;
        private bool pathFound = false;
        private int pathLength = 0;

        public MazeSolver(Maze maze)
        {
            this.maze = maze;
        }

        public bool FindPath()
        {
            pathFound = RecursiveDFS(maze.startRow, maze.startCol);
            return pathFound;
        }

        private bool RecursiveDFS(int row, int col)
        {
            if (!maze.IsValidMove(row, col))
                return false;

            maze.visited[row, col] = true;
            maze.visitedCount++;

            if (row == maze.endRow && col == maze.endCol)
            {
                pathFound = true;
                pathLength++;
                return true;
            }

            bool found = false;

            // Порядок: вверх, вправо, вниз, влево
            int[] dRow = { -1, 0, 1, 0 };
            int[] dCol = { 0, 1, 0, -1 };

            for (int i = 0; i < 4; i++)
            {
                int newRow = row + dRow[i];
                int newCol = col + dCol[i];

                if (RecursiveDFS(newRow, newCol))
                {
                    found = true;
                    if (!(row == maze.startRow && col == maze.startCol) && !(newRow == maze.endRow && newCol == maze.endCol))
                    {
                        maze.grid[row, col] = '*';
                    }
                    pathLength++;
                    break;
                }
            }

            return found;
        }

        public void PrintResult()
        {
            Console.WriteLine("Найденный путь:");
            maze.PrintMaze();
            Console.WriteLine($"Длина пути: {pathLength}");
            Console.WriteLine($"Статус: {(pathFound ? "Путь найден" : "Путь не найден")}");
            Console.WriteLine($"Посещено клеток: {maze.visitedCount}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("Введите размер лабиринта (строки столбцы):");
                string[] sizeInput = Console.ReadLine().Split();
                if (sizeInput.Length != 2)
                {
                    Console.WriteLine("Ошибка: Неверный формат ввода");
                    return;
                }

                int rows = int.Parse(sizeInput[0]);
                int cols = int.Parse(sizeInput[1]);

                string[] mazeInput = new string[rows];
                Console.WriteLine($"Введите лабиринт ({rows} строк по {cols} символов):");
                for (int i = 0; i < rows; i++)
                {
                    mazeInput[i] = Console.ReadLine();
                    if (mazeInput[i].Length != cols)
                    {
                        Console.WriteLine("Ошибка: Неверная длина строки");
                        return;
                    }
                }

                Maze maze = new Maze(mazeInput, rows, cols);

                if (maze.grid[maze.startRow, maze.startCol] != 'S')
                {
                    Console.WriteLine("Ошибка: Начальная точка не найдена");
                    return;
                }

                if (maze.grid[maze.endRow, maze.endCol] != 'E')
                {
                    Console.WriteLine("Ошибка: Конечная точка не найдена");
                    return;
                }

                MazeSolver solver = new MazeSolver(maze);
                bool found = solver.FindPath();
                solver.PrintResult();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}