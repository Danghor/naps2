using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NAPS2.Config;
using NAPS2.Scan.Images;
using NAPS2.Util;

namespace NAPS2.WinForms
{
    public partial class ThumbnailList : DragScrollListView
    {
        private ThumbnailCache thumbnails;

        public ThumbnailList()
        {
            InitializeComponent();
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
            LargeImageList = ilThumbnailList;
        }

        public ThumbnailRenderer ThumbnailRenderer
        {
            set => thumbnails = new ThumbnailCache(value);
        }

        public Size ThumbnailSize
        {
            get => ilThumbnailList.ImageSize;
            set => ilThumbnailList.ImageSize = value;
        }

        public void UpdateImages(List<ScannedImage> images, List<int> selection = null)
        {
            if (images.Count == 0)
            {
                // Fast case
                Items.Clear();
                ilThumbnailList.Images.Clear();
            }
            else
            {
                int delta = images.Count - Items.Count;
                for (int i = 0; i < delta; i++)
                {
                    Items.Add("", i);
                    Debug.Assert(selection == null);
                }
                for (int i = 0; i < -delta; i++)
                {
                    Items.RemoveAt(Items.Count - 1);
                    ilThumbnailList.Images.RemoveAt(ilThumbnailList.Images.Count - 1);
                    Debug.Assert(selection == null);
                }
            }

            // Determine the smallest range that contains all images in the selection
            int min = selection == null || !selection.Any() ? 0 : selection.Min();
            int max = selection == null || !selection.Any() ? images.Count : selection.Max() + 1;

            for (int i = min; i < max; i++)
            {
                if (i >= ilThumbnailList.Images.Count)
                {
                    ilThumbnailList.Images.Add(thumbnails[images[i]]);
                    Debug.Assert(selection == null);
                }
                else
                {
                    ilThumbnailList.Images[i] = thumbnails[images[i]];
                }
            }

            thumbnails.TrimCache(images);
            Invalidate();
        }

        public void AppendImage(ScannedImage img)
        {
            ilThumbnailList.Images.Add(thumbnails[img]);
            string text = Platform.Compat.UseEmptyStringInListViewItems ? "" : " ";
            Items.Add(text, ilThumbnailList.Images.Count - 1);
        }

        public void ReplaceThumbnail(int index, ScannedImage img)
        {
            ilThumbnailList.Images[index] = thumbnails[img];
            Invalidate(Items[index].Bounds);
        }

        public void RegenerateThumbnailList(List<ScannedImage> images)
        {
            if (ilThumbnailList.Images.Count > 0)
            {
                ilThumbnailList.Images.Clear();
            }
            var thumbnailArray = images.Select(x => (Image)thumbnails[x]).ToArray();
            ilThumbnailList.Images.AddRange(thumbnailArray);
        }
    }
}
