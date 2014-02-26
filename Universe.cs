using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using ConwaysLife.Utility;

namespace ConwaysLife
{
    public class CellItem
    {
        public long x { get; set; }
        public long y { get; set; }
        public int neighbourCount { get; set; }
    }

    [Serializable]
    public class Universe
    {
        public long UNIVERSE_SIZE_X = Int64.MaxValue;
        public long UNIVERSE_SIZE_Y = Int64.MaxValue;

        private List<CellItem> _liveItems;
        public List<CellItem> LiveItems
        {
            get
            {
                if (_liveItems == null) _liveItems = new List<CellItem>();
                return _liveItems;
            }
            set { _liveItems = value; }
        }

        public Universe()
        {

        }

        public Universe(Dimensions d)
        {
            UNIVERSE_SIZE_X = d.Width;
            UNIVERSE_SIZE_Y = d.Height;
        }


        public void SetInitialState(List<CellItem> itemList)
        {
            LiveItems = itemList;
        }

        public bool NextGeneration() //if smth was changed returns true flag
        {
            var newItems = new List<CellItem>();
            bool isChanged = false;
            foreach (var item in LiveItems)
            {
                item.neighbourCount = getNeighbourCount(item.x, item.y);
                //check for any possible new neighbours
                for (int i = item.x > 0 ? -1 : 0; item.x + i < UNIVERSE_SIZE_X && i < 2; i++)
                    for (int j = item.y > 0 ? -1 : 0; item.y + j < UNIVERSE_SIZE_Y && j < 2; j++)
                    {
                        var nCount = 0;
                        if (!LiveItems.Any(z=>z.x==item.x + i && z.y==item.y + j))
                            nCount = getNeighbourCount(item.x + i, item.y + j);
                        if (nCount == 3 && newItems.Count(ni => ni.x == item.x + i && ni.y == item.y + j) == 0)
                        {
                            newItems.Add(new CellItem() {x = item.x + i, y = item.y + j, neighbourCount = nCount});
                        }
                    }
            }

            //remove dead items
            var deadItems = LiveItems.Where(x => x.neighbourCount > 3 || x.neighbourCount < 2);

            if (deadItems.Count() > 0 || newItems.Count > 0)
                isChanged = true;

            LiveItems.RemoveAll(x => x.neighbourCount > 3 || x.neighbourCount < 2);
            //add new items
            LiveItems.AddRange(newItems);

            return isChanged;
        }

        private int getNeighbourCount(long x, long y)
        {
            return LiveItems.Count(i => Math.Abs(i.x - x) <= 1 && Math.Abs(i.y - y) <= 1 && Math.Abs(i.x - x) + Math.Abs(i.y - y) != 0);
        }

        public void Set (long x, long y, byte value)
        {
            if (value == 1)
                LiveItems.Add(new CellItem(){x=x, y=y, neighbourCount = 0});
            else
                LiveItems.RemoveAll(i => i.x == x && i.y == y);
        }

        public byte Get (long x, long y)
        {
            return LiveItems.Any(i => i.x == x && i.y == y) ? (byte)1 : (byte)0;
        }

        public void Clear ()
        {
            LiveItems.Clear();
        }

    }
}
