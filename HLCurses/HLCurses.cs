using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace NGCurses
{
    namespace ConsoleObjects
    {
        public abstract class ConsoleObject
        {
            public dynamic Parent;
            public readonly List<ConsoleObject> Children = new List<ConsoleObject>();
            public Tuple<int, int, int, int> Margins = new Tuple<int, int, int, int>(0, 0, 0, 0);
            protected Point offset;
            protected Point position;
            protected Point size;
            protected ConsoleColor? background;
            protected ConsoleColor? foreground;

            public Point Offset
            {
                get { return offset; }
                set { offset = value; }
            }

            /// <summary>
            /// Update offset and redraw
            /// </summary>
            public Point UpdateOffset
            {
                get { return offset; }
                set
                {
                    try
                    {
                        Clear();
                        offset = value;
                        Draw();
                    }
                    catch
                    {
                    }
                }
            }

            public Point Position
            {
                get { return position; }
                set { position = value; }
            }

            /// <summary>
            /// Update position and redraw
            /// </summary>
            public Point UpdatePosition
            {
                get { return position; }
                set
                {
                    try
                    {
                        Clear();
                        position = value;
                        Draw();
                    }
                    catch
                    {
                    }
                }
            }

            public Point Size
            {
                get { return size; }
                set { size = value; }
            }

            /// <summary>
            /// Update size and redraw
            /// </summary>
            public Point UpdateSize
            {
                get { return size; }
                set
                {
                    try
                    {
                        //Clear();
                        Parent.Clear();
                        size = value;
                        //Draw();
                        Parent.Draw();
                    }
                    catch
                    {
                    }
                }
            }

            public ConsoleColor? Background
            {
                get { return background ?? (Parent == null ? null : Parent.Background); }
                set { background = value; }
            }

            public ConsoleColor? Foreground
            {
                get { return foreground ?? (Parent == null ? null : Parent.Foreground); }
                set { foreground = value; }
            }

            /// <summary>
            /// Add and draw a child object (if object chain is valid)
            /// </summary>
            public void AddChild(ConsoleObject consoleObject)
            {
                Children.Add(consoleObject);
                consoleObject.Parent = this;
                try
                {
                    Draw();
                }
                catch
                {}
            }

            /// <summary>
            /// Remove a child (and all of its sub-children)
            /// </summary>
            public void RemoveChild(ConsoleObject consoleObject)
            {
                Children.Remove(consoleObject);
                try
                {
                    consoleObject.Clear();
                    consoleObject.Parent = null;
                    Draw();
                }
                catch
                {
                }
            }

            protected ConsoleObject(ConsoleColor? background, ConsoleColor? foreground)
            {
                this.Background = background;
                this.Foreground = foreground;
            }

            /// <summary>
            /// Draw the object, must be implemented and call base.Draw();
            /// </summary>
            public virtual void Draw()
            {
                VerifyChain();
                UpdateOffsets();
            }

            /// <summary>
            /// Clear the object from the console buffer
            /// </summary>
            public void Clear()
            {
                VerifyChain();
                ConsoleColor? clearBG = null;
                ConsoleColor? clearFG = null;
                if (Parent is ConsoleManager)
                    Console.ResetColor();
                else
                {
                    clearBG = Parent.Background;
                    clearFG = Parent.Foreground;
                }
                foreach (var consoleObject in Children)
                {
                    consoleObject.Clear();
                }
                for (int w = 0; w <= size.X; w++)
                {
                    for (int h = 0; h < size.Y; h++)
                    {
                        ConsoleManager.drawBlock(w + offset.X, h + offset.Y, " ", clearBG, clearFG);
                    }
                }
            }

            /// <summary>
            /// Determine object's position
            /// </summary>
            protected void UpdateOffsets()
            {
                if (Parent is ConsoleManager)
                {
                    offset.X = position.X;
                    offset.Y = position.Y;
                }
                else if (Parent is ConsoleObject)
                {
                    offset.X = Parent.Margins.Item1 + position.X + Parent.Offset.X;
                    offset.Y = Parent.Margins.Item2 + position.Y + Parent.Offset.Y;
                }
            }

            /// <summary>
            /// Ensure our object chain is valid and drawable
            /// </summary>
            protected void VerifyChain()
            {
                dynamic TopParent = Parent;
                while (TopParent.GetType() != typeof(ConsoleManager))
                {
                    if (TopParent == null)
                        throw new Exception("Orphaned object - No top-level parent!");
                    if (TopParent is ConsoleObject)
                        TopParent = TopParent.Parent;
                    else
                        throw new Exception("Parent of object isn't a ConsoleObject");
                }
            }
        }

        /// <summary>
        /// Simple grid container
        /// </summary>
        public class GridContainer : ConsoleObject
        {
            public GridContainer(ConsoleColor? background = null, ConsoleColor? foreground = null) : base(background, foreground)
            {
                
            }
            public override void Draw()
            {
                int curY = 0;
                int curX = 0;
                int maxY = 0;
                Offset = new Point(0,0);
                foreach (var consoleObject in Children)
                {
                    if (curX + consoleObject.Size.X >= Console.BufferWidth)
                    {
                        curY += maxY;
                        maxY = 0;
                        curX = 0;
                    }
                    maxY = Math.Max(maxY, consoleObject.Size.Y);
                    consoleObject.Offset = new Point(consoleObject.Position.X + curX, consoleObject.Position.Y + curY);
                    consoleObject.Draw();
                    curX += consoleObject.Size.X + 1;
                }
            }
        }

        /// <summary>
        /// Canvas element
        /// </summary>
        public class Canvas : ConsoleObject
        {
            public Canvas(ConsoleColor? background = null, ConsoleColor? foreground = null) : base(background, foreground) {}
            public override void Draw()
            {
                base.Draw();
                for (int y = 0; y < Console.BufferHeight; y++)
                    for (int x = 0; x < Console.BufferWidth; x++)
                    {
                        ConsoleManager.drawBlock(x,y,' ',background);
                    }
                foreach (var consoleObject in Children)
                {
                    consoleObject.Draw();
                }
            }
        }

        /// <summary>
        /// Basic text element
        /// </summary>
        public class TextObject : ConsoleObject
        {
            public string Text;
            public TextObject(string text, ConsoleColor? background = null, ConsoleColor? foreground = null) : base(background,foreground)
            {
                this.Text = text;
            }
            public override void Draw()
            {
                base.Draw();
                ConsoleManager.drawBlock(offset.X, offset.Y, Text, Background, Foreground);
            }
        }

        /// <summary>
        /// Create a simple, configurable box to host other elements
        /// </summary>
        public class BoxObject : ConsoleObject
        {
            /// <summary>
            /// Defaults to single line, presets available on NGCurses.BoxObject.Presets.BoxChars
            /// </summary>
            public Presets.BoxChars BoxChars;

            public class Presets
            {
                public class BoxChars
                {
                    public static readonly BoxChars SingleLine = new BoxChars
                                                                     {
                                                                         TopLeft = '\u250c',
                                                                         TopRight = '\u2510',
                                                                         BottomLeft = '\u2514',
                                                                         BottomRight = '\u2518',
                                                                         Horizontal = '\u2500',
                                                                         Vertical = '\u2502'
                                                                     };

                    public static readonly BoxChars DoubleLine = new BoxChars
                                                                     {
                                                                         TopLeft = '\u2554',
                                                                         TopRight = '\u2557',
                                                                         BottomLeft = '\u255a',
                                                                         BottomRight = '\u255d',
                                                                         Horizontal = '\u2550',
                                                                         Vertical = '\u2551'
                                                                     };

                    public static readonly BoxChars SingleHorizontalDoubleVertical = new BoxChars
                                                                                         {
                                                                                             TopLeft = '\u2553',
                                                                                             TopRight = '\u2556',
                                                                                             BottomLeft = '\u2559',
                                                                                             BottomRight = '\u255c',
                                                                                             Horizontal = '\u2500',
                                                                                             Vertical = '\u2551'
                                                                                         };

                    public static readonly BoxChars DoubleHorizontalSingleVertical = new BoxChars
                                                                                         {
                                                                                             TopLeft = '\u2552',
                                                                                             TopRight = '\u2555',
                                                                                             BottomLeft = '\u2558',
                                                                                             BottomRight = '\u255b',
                                                                                             Horizontal = '\u2550',
                                                                                             Vertical = '\u2502'
                                                                                         };

                    public char TopLeft;
                    public char TopRight;
                    public char BottomLeft;
                    public char BottomRight;
                    public char Horizontal;
                    public char Vertical;
                }
            }

            public BoxObject(int width, int height, ConsoleColor? background = null, ConsoleColor? foreground = null) : base(background,foreground)
            {
                size.X = width;
                size.Y = height;
                Margins = new Tuple<int, int, int, int>(1,1,1,1);
                if (BoxChars == null)
                    BoxChars = Presets.BoxChars.SingleLine;
            }

            public override void Draw()
            {
                base.Draw();
                for (int w = 0; w <= Size.X; w++)
                {
                    for (int h = 1; h < Size.Y - 1; h++)
                    {
                        ConsoleManager.drawBlock(w + offset.X, h + offset.Y, " ", Background, Foreground);
                    }
                    if (w == 0)
                    {
                        ConsoleManager.drawBlock(w + offset.X, offset.Y, BoxChars.TopLeft, Background, Foreground);
                        ConsoleManager.drawBlock(w + offset.X, offset.Y + Size.Y - 1, BoxChars.BottomLeft, Background, Foreground);
                    }
                    else if (w == Size.X)
                    {
                        ConsoleManager.drawBlock(w + offset.X, offset.Y, BoxChars.TopRight, Background, Foreground);
                        ConsoleManager.drawBlock(w + offset.X, offset.Y + Size.Y - 1, BoxChars.BottomRight, Background, Foreground);
                    }
                    else
                    {
                        ConsoleManager.drawBlock(w + offset.X, offset.Y, BoxChars.Horizontal, Background, Foreground);
                        ConsoleManager.drawBlock(w + offset.X, offset.Y + Size.Y - 1, BoxChars.Horizontal, Background, Foreground);
                    }
                }
                for (int h = 1; h < Size.Y - 1; h++)
                {
                    ConsoleManager.drawBlock(offset.X, h + offset.Y, BoxChars.Vertical, Background, Foreground);
                    ConsoleManager.drawBlock(offset.X + Size.X, h + offset.Y, BoxChars.Vertical, Background, Foreground);
                }
                foreach (var child in Children)
                {
                    child.Draw();
                }
            }
        }
    }
    public class ConsoleManager
    {
        public ConsoleColor? Background = ConsoleColor.Black;
        public ConsoleColor? Foreground = ConsoleColor.White;

        public static List<ConsoleObjects.ConsoleObject> ScreenObjects = new List<ConsoleObjects.ConsoleObject>();

        /// <summary>
        /// Add a (root) element to the console
        /// </summary>
        /// <param name="screenObject">ConsoleObject to add</param>
        public void AddObject(ConsoleObjects.ConsoleObject screenObject)
        {
            ScreenObjects.Add(screenObject);
            screenObject.Parent = this;
            screenObject.Draw();
        }

        /// <summary>
        /// Remove a (root) element from the console
        /// </summary>
        /// <param name="screenObject">ConsoleObject to remove</param>
        public void RemoveObject(ConsoleObjects.ConsoleObject screenObject)
        {
            ScreenObjects.Remove(screenObject);
            screenObject.Clear();
        }

        /// <summary>
        /// Console controller instantiator
        /// </summary>
        /// <param name="clearScreen">Clear the screen</param>
        public ConsoleManager(bool clearScreen = true)
        {
            try
            {
                Console.CursorVisible = false;
                Console.BufferWidth = Console.LargestWindowWidth > 175 ? 175 : Console.LargestWindowWidth;
                Console.BufferHeight = Console.LargestWindowHeight - 5;
                Console.SetWindowSize(Console.BufferWidth, Console.BufferHeight);
            }
            catch
            { }
            if (clearScreen)
                Console.Clear();
        }

        /// <summary>
        /// Redraw the entire screen
        /// </summary>
        public static void ReDraw()
        {
            foreach (var consoleObject in ScreenObjects)
            {
                consoleObject.Draw();
            }
        }

        /// <summary>
        /// Draw a block of characters on the console.  Currently calls the CLR implementation which is _EXTREMELY_ slow.
        /// </summary>
        /// <param name="x">X-pos</param>
        /// <param name="y">Y-pos</param>
        /// <param name="text">Text to draw</param>
        /// <param name="Background">Background color</param>
        /// <param name="Foreground">Foreground color</param>
        public static void drawBlock(int x, int y, string text, ConsoleColor? Background = ConsoleColor.Black, ConsoleColor? Foreground = ConsoleColor.White)
        {
            Console.SetCursorPosition(x, y);
            Console.BackgroundColor = Background != null ? (ConsoleColor) Background : ConsoleColor.Black;
            Console.ForegroundColor = Foreground != null ? (ConsoleColor)Foreground : ConsoleColor.White;
            Console.Write(text ?? " ");
        }

        /// <summary>
        /// Draw a character on the console.  Currently calls the CLR implementation which is _EXTREMELY_ slow.
        /// </summary>
        /// <param name="x">X-pos</param>
        /// <param name="y">Y-pos</param>
        /// <param name="text">Char to draw</param>
        /// <param name="Background">Background color</param>
        /// <param name="Foreground">Foreground color</param>
        public static void drawBlock(int x, int y, char text, ConsoleColor? Background = ConsoleColor.Black, ConsoleColor? Foreground = ConsoleColor.White)
        {
            drawBlock(x,y,text.ToString(),Background,Foreground);
        }
    }
}
