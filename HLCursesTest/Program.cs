using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NGCurses;
using NGCurses.ConsoleObjects;

namespace NGCursesTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.Unicode;
            ConsoleManager cm = new ConsoleManager();
            //Canvas canvas = new Canvas(ConsoleColor.Gray);
            GridContainer container = new GridContainer();
            var SmallBox1 = new BoxObject(4, 4);
            var SmallBox2 = new BoxObject(4, 4);
            var SmallBox3 = new BoxObject(4, 4);
            var SmallBox4 = new BoxObject(15, 15);
            var BigBox1 = new BoxObject(40, 40) { BoxChars = BoxObject.Presets.BoxChars.DoubleLine };
            var BigBox2 = new BoxObject(40, 40) { BoxChars = BoxObject.Presets.BoxChars.DoubleHorizontalSingleVertical };
            var BigBox3 = new BoxObject(40, 40) { BoxChars = BoxObject.Presets.BoxChars.SingleHorizontalDoubleVertical };
            cm.AddObject(container);
            container.AddChild(SmallBox1);
            container.AddChild(SmallBox2);
            container.AddChild(SmallBox3);
            Console.ReadLine();
            for (int i = 0; i < 20; i++ )
                SmallBox2.UpdateSize = new Point(i, i);
            for (int i = 20; i >= 4; i--)
                SmallBox2.UpdateSize = new Point(i, i);
            Console.ReadLine();
            SmallBox2.UpdateSize = new Point(50, 50);
            GridContainer container2 = new GridContainer();
            Console.ReadLine();
            SmallBox2.AddChild(container2);
            Console.ReadLine();
            container2.AddChild(new TextObject("This is an embedded element."));
            Console.ReadLine();
            container2.AddChild(SmallBox1);
            Console.ReadLine();
            container2.AddChild(BigBox1);
            Console.ReadLine();
            container2.AddChild(BigBox2);
            Console.ReadLine();
            container2.AddChild(BigBox3);
            Console.ReadLine();
            BigBox1.AddChild(SmallBox4);
            Console.ReadLine();
            SmallBox4.AddChild(new TextObject("Hello World"));
            Console.ReadLine();
            container.RemoveChild(BigBox1);
            Console.ReadLine();
        }
    }
}
