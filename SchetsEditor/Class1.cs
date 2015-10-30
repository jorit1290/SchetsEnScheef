using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace SchetsEditor
{
    public abstract class Geometry
    {
        protected Brush kwast;

        public abstract void Teken(Graphics g);

        public abstract bool Bevat(Point pt);

        public abstract string[] Opslaan();

        protected Geometry(Brush kwast)
        {
            this.kwast = kwast;
        }

        protected Geometry (string[] data)
        {

        }
    }

    public class Rechthoek : Geometry
    {
        protected Rectangle box;

        public Rechthoek(Brush kwast, Rectangle box) : base(kwast)
        {
            this.box = box;
        }

        public Rechthoek(string[] data) : base(data)
        {
            Color color = Color.FromName(data[1]);
            int x = int.Parse(data[2]);
            int y = int.Parse(data[3]);
            int w = int.Parse(data[4]);
            int h = int.Parse(data[5]);

            kwast = new SolidBrush(color);
            box = new Rectangle(x, y, w, h);
        }

        public override void Teken(Graphics g)
        {
            g.DrawRectangle(Tools.MaakPen(kwast, 3), box);

        }

        public override bool Bevat(Point pt)
        {
            return box.Contains(pt);
        }

        public override string[] Opslaan()
        {
            string[] data = new string[6];

            data[0] = "rechthoek";
            data[1] = Tools.MaakPen(kwast,0).Color.ToString();
            data[2] = box.X.ToString();
            data[3] = box.Y.ToString();
            data[4] = box.Width.ToString();
            data[5] = box.Height.ToString();

            return data;
        }
    }

    public class VolRechthoek : Rechthoek
    {

        public VolRechthoek(Brush kwast, Rectangle box) : base(kwast, box)
        {
        }

        public override void Teken(Graphics g)
        {
            g.FillRectangle(kwast, box);
        }

        public override bool Bevat(Point pt)
        {
            return box.Contains(pt);
        }



    }
    public class Cirkel : Geometry
    {
        protected Rectangle box;

        public Cirkel(Brush kwast, Rectangle box) : base(kwast)
        {
            this.box = box;
        }

        public override void Teken(Graphics g)
        {
            g.DrawEllipse(Tools.MaakPen(kwast, 3), box);

        }

        public override bool Bevat(Point pt)
        {
            return box.Contains(pt);
        }

        public override string[] Opslaan()
        {
            return new string[0];
        }
    }

    public class VolCirkel : Rechthoek
    {

        public VolCirkel(Brush kwast, Rectangle box) : base(kwast, box)
        {
        }

        public override void Teken(Graphics g)
        {
            g.FillEllipse(kwast, box);
        }

        public override bool Bevat(Point pt)
        {
            return box.Contains(pt);
        }

    }

    public class Lijn : Geometry
    {
        Point a, b;

        public Lijn(Brush kwast, Point p1, Point p2) : base(kwast)
        {
            a = p1;
            b = p2;
        }

        public override void Teken(Graphics g)
        {
            g.DrawLine(Tools.MaakPen(kwast, 3), a, b);
        }

        public override bool Bevat(Point pt)
        {
            return Afstand(pt) < 5;
        }

        private double Afstand(Point pt)
        {
            var d = Math.Sqrt(Math.Pow(b.Y - a.Y, 2) + Math.Pow(b.X - a.X, 2));
            return Math.Abs((b.Y - a.Y) * pt.X - (b.X - a.X) * pt.Y + b.X * a.Y - b.Y * a.X) / d;
        }

        public override string[] Opslaan()
        {
            return new string[0];
        }
    }

    public class Letter : Geometry
    {
        protected char c;
        protected Point positie;
        public Rectangle box { get; protected set; }

        public Letter(Brush kwast, Point positie, char c) : base(kwast)
        {
            this.positie = positie;
            this.c = c;
            this.box = new Rectangle();
        }

        public override void Teken(Graphics g)
        {
            var lettertype = new Font("Tahoma", 40);
            var text = c.ToString();
            var size = g.MeasureString(text, lettertype, positie, StringFormat.GenericTypographic);

            box = new System.Drawing.Rectangle(positie.X, positie.Y, (int)size.Width, (int)size.Height);

            g.DrawString(text, lettertype, kwast, positie, StringFormat.GenericTypographic);
        }

        public override bool Bevat(Point pt)
        {
            return box.Contains(pt);
        }

        public override string[] Opslaan()
        {
            return new string[0];
        }
    }
}
