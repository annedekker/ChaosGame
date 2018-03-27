using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChaosGame
{
    class XYC
    {
        public int x; // x coordinate
        public int y; // y coordinate
        public int c; // corner

        public XYC(int x, int y, int c)
        {
            this.x = x;
            this.y = y;
            this.c = c;
        }
    }

    class ChaosCode
    {
        Random rnd = new Random();

        // Corners & Related variables

        public int RenderSize
        {
            get { return renderSize; }
            set
            {
                if (value < 2) renderSize = 2;
                else if (value > 32768) renderSize = 32768;
                else renderSize = value;

                if (autoCorners) MakeAutoCorners();
            }
        }
        public int ShapeSize
        {
            get { return shapeSize; }
            set
            {
                if (value < 2) shapeSize = 2;
                else if (value > 65536) shapeSize = 65536;
                else shapeSize = value;

                if (autoCorners) MakeAutoCorners();
            }
        }
        public bool AutoCorners
        {
            get { return autoCorners; }
            set
            {
                autoCorners = value;

                if (autoCorners) MakeAutoCorners();
            }
        }
        public int CornerCount
        {
            get { return cornerCount; }
            set
            {
                if (value < 3) cornerCount = 3;
                else if (value > 64) cornerCount = 64;
                else cornerCount = value;

                // update corner list
                if (autoCorners) MakeAutoCorners();
                else
                {
                    XYC[] newcorners = new XYC[cornerCount];
                    for (int i = 0; i < cornerCount; i++)
                    {
                        if (i < corners.Count) newcorners[i] = corners[i];
                        else newcorners[i] = new XYC(0, 0, i);
                    }
                    corners = newcorners.ToList();
                }

                // remove invalid indexes from excludeindexes
                for (int i = 0; i < excludeIndexes.Count; i++)
                {
                    if (excludeIndexes[i] >= cornerCount)
                    {
                        excludeIndexes.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
        public List<XYC> Corners { get { return corners; } }

        int renderSize = 2048;
        int shapeSize = 2000;
        int cornerCount = 3;
        bool autoCorners = true;

        List<XYC> corners = new List<XYC>();

        // Distance & Corner exclusion variables

        public double DistanceMoved
        {
            get { return distanceMoved; }
            set
            {
                if (value < 0.0) distanceMoved = 0.0;
                else if (value > 10.0) distanceMoved = 10.0;
                else distanceMoved = value;
            }
        }
        public int ExcludeFromLast
        {
            get { return excludeFromLast; }
            set
            {
                if (value < 0) excludeFromLast = 0;
                else if (value > 4) excludeFromLast = 4;
                else excludeFromLast = value;
            }
        }
        public List<int> ExcludeIndexes
        {
            get { return excludeIndexes; }
            set { excludeIndexes = value; }
        }
        public bool ExcludeClockwise
        {
            get { return excludeClockwise; }
            set { excludeClockwise = value; }
        }

        double distanceMoved = 0.5;
        int excludeFromLast = 0;
        List<int> excludeIndexes = new List<int>();
        bool excludeClockwise = true;

        // The point & last corners for regular generation

        XYC chaosPoint = new XYC(1, 1, 0);
        Queue<int> lastCorners = new Queue<int>();

        // Init

        public ChaosCode()
        {
            MakeAutoCorners();
        }

        // Moving the point

        public List<XYC> GetChaosPoints(int count)
        {
            List<XYC> points = new List<XYC>();

            for (int i = 0; i < count; i++)
            {
                UpdateChaosPoint();
                points.Add(new XYC(chaosPoint.x, chaosPoint.y, chaosPoint.c));
            }

            return points;
        }

        private void UpdateChaosPoint()
        {
            lastCorners.Enqueue(chaosPoint.c);
            if (lastCorners.Count > excludeFromLast) lastCorners.Dequeue();

            List<int> validCorners = GetFilteredCorners(lastCorners).ToList();
            if (validCorners.Count < 1) return;

            int chosenOne = validCorners[rnd.Next(0, validCorners.Count)];
            XYC chosenCorner = corners.Single(i => i.c == chosenOne);

            // (origin) + ((corner - origin) * distance moved)
            int x = (Int32)(chaosPoint.x + (chosenCorner.x - chaosPoint.x) * distanceMoved);
            int y = (Int32)(chaosPoint.y + (chosenCorner.y - chaosPoint.y) * distanceMoved);
            chaosPoint = new XYC(x, y, chosenCorner.c);
        }

        private HashSet<int> GetFilteredCorners(Queue<int> lastcorners)
        {
            HashSet<int> filtered = new HashSet<int>();
            for (int i = 0; i < cornerCount; i++) filtered.Add(i);

            if (lastcorners.Count < 1 || excludeIndexes.Count < 1) return filtered;

            foreach (int m in lastcorners)
            {
                foreach (int n in excludeIndexes)
                {
                    if (excludeClockwise)
                    {
                        int c = m + n;
                        if (c >= cornerCount) c -= cornerCount;
                        filtered.Remove(c);
                    }
                    else
                    {
                        int c = m - n;
                        if (c < 0) c += cornerCount;
                        filtered.Remove(c);
                    }
                }
            }
            return filtered;
        }

        // Corners

        public void SetCornerCoordinate(int index, bool isX, int value)
        {
            if (index < 0 || index >= corners.Count) return;

            if (isX) corners[index].x = value;
            else corners[index].y = value;
        }

        private void MakeAutoCorners()
        {
            corners.Clear();

            double angleRadians = (360.0 / cornerCount) * Math.PI / 180.0;

            for (int i = 0; i < cornerCount; i++)
            {
                // coordinate = (center) + (radius * cos or sin (angle in radians + correction))
                int x = (Int32)((renderSize / 2) + ((shapeSize / 2) * Math.Cos(i * angleRadians + (270 * Math.PI / 180.0))));
                int y = (Int32)((renderSize / 2) + ((shapeSize / 2) * Math.Sin(i * angleRadians + (270 * Math.PI / 180.0))));

                corners.Add(new XYC(x, y, i));
            }

            lastCorners.Clear();
            for (int i = 0; i < 16; i++) UpdateChaosPoint();
        }
    }

}
