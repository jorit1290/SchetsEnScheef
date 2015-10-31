using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace SchetsEditor
{
    public interface ISchetsTool
    {
        void MuisVast(SchetsControl s, Point p);
        void MuisDrag(SchetsControl s, Point p);
        void MuisLos(SchetsControl s, Point p);
        void Letter(SchetsControl s, char c);
    }

    public static class Tools
    {
        public static Rectangle Punten2Rechthoek(Point p1, Point p2)
        {
            return new Rectangle(new Point(Math.Min(p1.X, p2.X), Math.Min(p1.Y, p2.Y))
                                , new Size(Math.Abs(p1.X - p2.X), Math.Abs(p1.Y - p2.Y))
                                );
        }
        public static Pen MaakPen(Brush b, int dikte)
        {
            Pen pen = new Pen(b, dikte);
            pen.StartCap = LineCap.Round;
            pen.EndCap = LineCap.Round;
            return pen;
        }
    }

    public abstract class StartpuntTool : ISchetsTool
    {
        protected Point startpunt;
        protected Color kleur;

        public virtual void MuisVast(SchetsControl s, Point p)
        {
            startpunt = p;
        }
        public virtual void MuisLos(SchetsControl s, Point p)
        {
            kleur = s.PenKleur;
        }
        public abstract void MuisDrag(SchetsControl s, Point p);
        public abstract void Letter(SchetsControl s, char c);
    }

    public class TekstTool : StartpuntTool
    {
        public override string ToString() { return "tekst"; }

        public override void MuisDrag(SchetsControl s, Point p) { }

        public override void Letter(SchetsControl s, char c)
        {
            if (char.IsControl(c)) return;

            var letter = new Letter(kleur, startpunt, c);
            s.Schets.Toevoegen(letter);
            s.Invalidate();

            startpunt.X += letter.box.Width;
        }
    }

    public abstract class TweepuntTool : StartpuntTool
    {
        public override void MuisVast(SchetsControl s, Point p)
        {
            base.MuisVast(s, p);
            kleur = Color.Gray;
        }
        public override void MuisDrag(SchetsControl s, Point p)
        {
            s.Refresh();
            this.Bezig(s.CreateGraphics(), this.startpunt, p);
        }
        public override void MuisLos(SchetsControl s, Point p)
        {
            base.MuisLos(s, p);
            this.Compleet(s.Schets, this.startpunt, p);
            s.Invalidate();
        }
        public override void Letter(SchetsControl s, char c)
        {
        }
        public abstract void Bezig(Graphics g, Point p1, Point p2);

        public abstract void Compleet(Schets s, Point p1, Point p2);
    }

    public class RechthoekTool : TweepuntTool
    {
        public override string ToString() { return "kader"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawRectangle(new Pen(kleur, 3), Tools.Punten2Rechthoek(p1, p2));
        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            Rechthoek rechthoek = new Rechthoek(kleur, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(rechthoek);
        }
    }


    public class VolRechthoekTool : RechthoekTool
    {
        public override string ToString() { return "vlak"; }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            VolRechthoek volrechthoek = new VolRechthoek(kleur, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(volrechthoek);
        }
    }

    public class CirkelTool : TweepuntTool
    {
        public override string ToString() { return "cirkel"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawEllipse(new Pen(kleur, 3), Tools.Punten2Rechthoek(p1, p2));
        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            Cirkel cirkel = new Cirkel(kleur, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(cirkel);
        }
    }

    public class VolCirkelTool : CirkelTool
    {
        public override string ToString() { return "rondje"; }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            VolCirkel volcirkel = new VolCirkel(kleur, Tools.Punten2Rechthoek(p1, p2));
            s.Toevoegen(volcirkel);
        }
    }

    public class LijnTool : TweepuntTool
    {
        public override string ToString() { return "lijn"; }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            g.DrawLine(new Pen(this.kleur, 3), p1, p2);
        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            Lijn lijn = new Lijn(kleur, p1, p2);
            s.Toevoegen(lijn);
        }
    }

    public class PenTool : TweepuntTool
    {
        protected Bitmap bitmap;
        protected Graphics BitmapGraphics;
        private int minX;
        private int minY;
        private int maxX;
        private int maxY;

        public override void MuisVast(SchetsControl s, Point p)
        {
            kleur = s.PenKleur;
            startpunt = p;

            bitmap = new Bitmap(s.Schets.bitmap.Width, s.Schets.bitmap.Width);
            BitmapGraphics = Graphics.FromImage(bitmap);
            minX = p.X;
            minY = p.Y;
            maxX = p.X;
            maxY = p.Y;
        }

        public override void MuisDrag(SchetsControl s, Point p)
        {
            Bezig(s.CreateGraphics(), startpunt, p);
        }

        public override string ToString()
        {
            return "pen";
        }

        public override void Bezig(Graphics g, Point p1, Point p2)
        {
            minX = Math.Min(minX, p2.X);
            minY = Math.Min(minY, p2.Y);
            maxX = Math.Max(maxX, p2.X);
            maxY = Math.Max(maxY, p2.Y);

            BitmapGraphics.DrawLine(new Pen(kleur, 3), p1, p2);
            g.DrawImage(bitmap, 0, 0);
            startpunt = p2;
        }

        public override void Compleet(Schets s, Point p1, Point p2)
        {
            int breedte = maxX - minX + 6;
            int hoogte = maxY - minY + 6;

            Point min = new Point(minX - 3, minY - 3);
            Point max = new Point(maxX + 3, maxY + 3);

            Bitmap minibitmap = new Bitmap(breedte, hoogte);
            Graphics g = Graphics.FromImage(minibitmap);

            g.DrawImage(bitmap, 0, 0, Tools.Punten2Rechthoek(min, max), GraphicsUnit.Pixel);

            Tekening tekening = new Tekening(kleur, minibitmap, min);
            s.Toevoegen(tekening);

            bitmap.Dispose();
            BitmapGraphics.Dispose();
        }
    }

    public class GumTool : StartpuntTool
    {
        public override void MuisLos(SchetsControl s, Point p)
        {
            base.MuisLos(s, p);

            var buffer = s.Schets.buffer;
            for (var i = buffer.Count - 1; i >= 0; i--)
            {
                var geometry = buffer[i];
                if (geometry.Bevat(p))
                {
                    buffer.RemoveAt(i);
                    s.Schets.Herteken();
                    s.Invalidate();
                    break;
                }
            }
        }



        public override void Letter(SchetsControl s, char c)
        {

        }

        public override void MuisDrag(SchetsControl s, Point p)
        {

        }

        public override string ToString() { return "gum"; }
    }
}
