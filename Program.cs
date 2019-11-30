using System;
using System.Linq;
using System.Collections.Generic;
// using System.Random;
using System.Threading;

// TODO: make set from snake point
// to generate fruit faster

namespace snake
{
    class Program
    {
        static void Main(string[] args)
        {
            var point = new Point(1, 1);
            var snake = new Snake(point);
            var field = new Field(snake, 30);
            var ui = new UI(field);
            var keypress = new KeyPress();

            snake.Notify += field.EvalField;

            keypress.LeftArrow += snake.ToLeft;
            keypress.RightArrow += snake.ToRight;
            keypress.UpArrow += snake.ToTop;
            keypress.DownArrow += snake.ToBot;

            while (true)
            {
                ui.Draw();
                keypress.ReadKeyPress();
                snake.move();
                Thread.Sleep(150);
            }
        }
    }

    class KeyPress
    {
        public delegate void LeftMoveHandler();
        public event LeftMoveHandler LeftArrow;

        public delegate void RightMoveHandler();
        public event RightMoveHandler RightArrow;

        public delegate void TopMoveHandler();
        public event TopMoveHandler UpArrow;

        public delegate void BotMoveHandler();
        public event BotMoveHandler DownArrow;

        public void ReadKeyPress()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        UpArrow?.Invoke();
                        break;
                    case ConsoleKey.DownArrow:
                        DownArrow?.Invoke();
                        break;
                    case ConsoleKey.LeftArrow:
                        LeftArrow?.Invoke();
                        break;
                    case ConsoleKey.RightArrow:
                        RightArrow?.Invoke();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    class Field
    {
        // █
        // *
        // we need to check for boundaries
        // and if snake did not bite herself
        // as well as if she hit the fruit
        // our UI is a matrix of chars with
        // field substitution based on field type

        // at each step we need to find 
        // if snake hit fruit
        // and keep fruit number at 1
        private Point fruit;
        private Snake snake;
        private Random random;

        public int side {get; private set;}
        public List<List<char>> matrix {get; private set;}

        public Field(Snake snake, int side)
        {
            this.snake = snake;
            this.side = side;
            this.random = new Random();

            // TODO: change to better
            var row = Enumerable.Repeat<char>(' ', this.side).ToList();
            this.matrix = Enumerable.Repeat<List<char>>(row, this.side).ToList();

            this.GenerateFruit();
            this.PutEmpty();
            this.PutFruit();
            this.PutSnake();
        }

        public void EvalField()
        {
            this.CheckColision();
            this.CheckFruit();
            this.PutEmpty();
            this.PutFruit();
            this.PutSnake();
        }

        public void CheckColision()
        {
            if (this.snake.HeadBodyColision() || 
                this.snake.body.Any(part =>
                    (part.x < 0 || part.x >= this.side ||
                     part.y < 0 || part.y >= this.side)))
            {
                Console.Clear();
                Console.WriteLine("You lost!");
                System.Environment.Exit(0); 
            }
        }

        private void CheckFruit()
        {
            if (this.snake.body.Find(this.fruit) != null)
            {
                this.snake.GrowBody();
                this.GenerateFruit();
            }
        }

        private void PutEmpty()
        {
            for (int y = 0; y < this.side; y++)
            {
                var row = Enumerable.Repeat<char>(' ', this.side).ToList();
                this.matrix[y] = row;
            }
        }

        private void PutSnake()
        {
            foreach (Point part in this.snake.body)
            {
                this.matrix[part.y][part.x] = '█';
            }
        }

        private void GenerateFruit()
        {
            var possible_locs = new List<Point>();
            for (int y = 0; y < this.side; y++)
            {
                for (int x = 0; x < this.side; x++)
                {
                    var curr_pos = new Point(y, x);
                    if (!this.snake.body.Contains(curr_pos))
                    {
                        possible_locs.Add(curr_pos);
                    }
                }
            }

            this.fruit = possible_locs[this.random.Next(0, possible_locs.Count)];
        }

        private void PutFruit()
        {
            this.matrix[this.fruit.y][this.fruit.x] = '*';
        }
    }

    class UI {
        public Field field {get; private set;}

        public UI(Field field) {
            this.field = field;
        }

        public void Draw() {
            // first we need to clear console
            // and then put our matrix there
            Console.Clear();

            var border = String.Concat(Enumerable.Repeat<char>('═', this.field.side).ToList());
            Console.WriteLine("╔" + border + "╗");
            foreach (List<char> row in this.field.matrix)
            {
                var row_str = String.Concat(row);
                Console.WriteLine("║" + row_str + "║");
            }
            Console.WriteLine("╚" + border + "╝");
        }
    }

    class Point : IEquatable<Point>
    {
        public int x, y;
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public Point()
        {
            this.x = 0;
            this.y = 0;
        }

        public static Point operator +(Point self, Point other) => new Point(self.x + other.x, self.y + other.y);

        bool System.IEquatable<Point>.Equals(Point other) {
            return this.x == other.x && this.y == other.y;
        }

        public String Debug() {
            String s = "x: " + this.x.ToString() + " y: " + this.y.ToString();
            return s;
        }

    }

    class Snake
    {
        // TODO: implement enum for movement
        public Point direction {get; private set;}
        public LinkedList<Point> body {get; private set;}

        public delegate void MoveHandler();
        public event MoveHandler Notify;
        
        public Snake(Point head)
        {
            this.direction = new Point(1, 0);
            this.body = new LinkedList<Point>();
            this.body.AddFirst(head);
        }

        public bool HeadBodyColision()
        {
            // check if our head point is the same
            // as any other body part
            // because we need to know if snake
            // bites herself
            if (this.body.Find(this.body.First.Value) ==
                this.body.FindLast(this.body.First.Value))
            {
                return false;
            }
            return true;
        }

        public void GrowBody() {
            this.body.AddLast(this.body.Last.Value);
        }

        public void move() {
            Notify?.Invoke();
            Point head = this.body.First.Value;
            this.body.AddFirst(head + this.direction);
            this.body.RemoveLast();
        }

        public void ToTop()
        {
            if (this.direction.y != +1)
            {
                this.direction = new Point(0, -1);
            }
        }

        public void ToBot()
        {
            if (this.direction.y != -1)
            {
                this.direction = new Point(0, +1);
            }
        }

        public void ToLeft()
        {
            if (this.direction.x != +1)
            {
                this.direction = new Point(-1, 0);
            }
        }

        public void ToRight()
        {
            if (this.direction.x != -1)
            {
                this.direction = new Point(+1, 0);
            }
        }
    }
}
