using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Serialization;
using System.IO;
using System.Drawing.Imaging;

namespace SchetsEditor
{
    [Serializable]
    public abstract class Geometry : ISerializable
    {
        protected Color kleur;

        public abstract void Teken(Graphics g);

        public abstract bool Bevat(Point pt);

        protected Geometry(Color kleur)
        {
            this.kleur = kleur;
        }

        protected Geometry(SerializationInfo info, StreamingContext context)
        {
            kleur = Color.FromArgb(info.GetInt32("color"));
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("color", kleur.ToArgb(), typeof(int));
        }
    }
    [Serializable]
    public class Rechthoek : Geometry
    {
        protected Rectangle box;

        public Rechthoek(Color kleur, Rectangle box) : base(kleur)
        {
            this.box = box;
        }

        public Rechthoek(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int x = info.GetInt32("x");
            int y = info.GetInt32("y");
            int breedte = info.GetInt32("breedte");
            int hoogte = info.GetInt32("hoogte");

            box = new Rectangle(x, y, breedte, hoogte);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("x", box.X, typeof(int));
            info.AddValue("y", box.Y, typeof(int));
            info.AddValue("breedte", box.Width, typeof(int));
            info.AddValue("hoogte", box.Height, typeof(int));

        }

        public override void Teken(Graphics g)
        {
            g.DrawRectangle(new Pen(kleur, 3), box);

        }

        public override bool Bevat(Point pt)
        {
            Rectangle buiten = Rectangle.Inflate(box, 5, 5);
            Rectangle binnen = Rectangle.Inflate(box, -5, -5);

            return buiten.Contains(pt) && !binnen.Contains(pt);
        }

    }
    [Serializable]
    public class VolRechthoek : Rechthoek
    {

        public VolRechthoek(Color kleur, Rectangle box) : base(kleur, box)
        {
        }

        public VolRechthoek(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Teken(Graphics g)
        {
            g.FillRectangle(new SolidBrush(kleur), box);
        }

        public override bool Bevat(Point pt)
        {
            return box.Contains(pt);
        }



    }
    [Serializable]
    public class Cirkel : Rechthoek
    {

        public Cirkel(Color kleur, Rectangle box) : base(kleur, box)
        {
        }

        public Cirkel(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Teken(Graphics g)
        {
            g.DrawEllipse(new Pen(kleur, 3), box);

        }

        protected bool CirkelBevat(Rectangle box, Point pt)
        {
            double xradius = box.Width / 2;
            double yradius = box.Height / 2;

            Point midden = new Point(box.X + (int)xradius, box.Y + (int)yradius);

            Point normalized = new Point(pt.X - midden.X, pt.Y - midden.Y);

            return ((double)(normalized.X * normalized.X) / (xradius * xradius)) + ((double)(normalized.Y * normalized.Y) / (yradius * yradius)) <= 1;
        }

        public override bool Bevat(Point pt)
        {
            Rectangle buiten = Rectangle.Inflate(box, 5, 5);
            Rectangle binnen = Rectangle.Inflate(box, -5, -5);

            return CirkelBevat(buiten, pt) && !CirkelBevat(binnen, pt);
        }

    }

    [Serializable]
    public class VolCirkel : Cirkel
    {

        public VolCirkel(Color kleur, Rectangle box) : base(kleur, box)
        {
        }

        public VolCirkel(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public override void Teken(Graphics g)
        {
            g.FillEllipse(new SolidBrush(kleur), box);
        }

        public override bool Bevat(Point pt)
        {
            return CirkelBevat(box, pt);
        }

    }

    [Serializable]
    public class Lijn : Geometry
    {
        Point a, b;

        public Lijn(Color kleur, Point p1, Point p2) : base(kleur)
        {
            a = p1;
            b = p2;
        }

        public Lijn(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int ax = info.GetInt32("ax");
            int ay = info.GetInt32("ay");
            int bx = info.GetInt32("bx");
            int by = info.GetInt32("by");

            a = new Point(ax, ay);
            b = new Point(bx, by);
        }

        public override void Teken(Graphics g)
        {
            g.DrawLine(new Pen(kleur, 3), a, b);
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("ax", a.X, typeof(int));
            info.AddValue("ay", a.Y, typeof(int));
            info.AddValue("bx", b.X, typeof(int));
            info.AddValue("by", b.Y, typeof(int));
        }

    }

    [Serializable]
    public class Letter : Geometry
    {
        protected string tekst;
        protected Point positie;
        public Rectangle box { get; protected set; }

        public Letter(Color kleur, Point positie, char c) : base(kleur)
        {
            this.positie = positie;
            tekst = c.ToString();
            box = new Rectangle();
        }

        public Letter(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            tekst = info.GetString("tekst");
            var x = info.GetInt32("x");
            var y = info.GetInt32("y");

            positie = new Point(x, y);
            box = new Rectangle();
        }

        public override void Teken(Graphics g)
        {
            var lettertype = new Font("Tahoma", 40);
            var size = g.MeasureString(tekst, lettertype, positie, StringFormat.GenericTypographic);

            box = new System.Drawing.Rectangle(positie.X, positie.Y, (int)size.Width, (int)size.Height);

            g.DrawString(tekst, lettertype, new SolidBrush(kleur), positie, StringFormat.GenericTypographic);
        }

        public override bool Bevat(Point pt)
        {
            return box.Contains(pt);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("x", positie.X, typeof(int));
            info.AddValue("y", positie.Y, typeof(int));
            info.AddValue("tekst", tekst, typeof(string));
        }
    }

    [Serializable]
    public class Tekening : Geometry
    {
        protected Bitmap bitmap;
        protected Point positie;

        public Tekening(Color kleur, Bitmap bitmap, Point positie) : base(kleur)
        {
            this.bitmap = bitmap;
            this.positie = positie;
        }

        public Tekening(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int x = info.GetInt32("x");
            int y = info.GetInt32("y");

            positie = new Point(x, y);

            byte[] data = (byte[])info.GetValue("image", typeof(byte[]));
            MemoryStream stream = new MemoryStream(data);
            bitmap = new Bitmap(stream);
            stream.Close();
        }

        public override void Teken(Graphics g)
        {
            g.DrawImage(bitmap, positie);
        }

        public override bool Bevat(Point pt)
        {
            if (pt.X < positie.X || pt.Y < positie.Y || pt.X >= positie.X + bitmap.Width || pt.Y >= positie.Y + bitmap.Height) return false;

            int x = pt.X - positie.X;
            int y = pt.Y - positie.Y;

            Color pixel = bitmap.GetPixel(x, y);

            return pixel.Name != "0";
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            info.AddValue("x", positie.X, typeof(int));
            info.AddValue("y", positie.Y, typeof(int));

            MemoryStream stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);
            byte[] data = stream.ToArray();

            info.AddValue("image", data, typeof(byte[]));

            stream.Close();
        }
    }
}
