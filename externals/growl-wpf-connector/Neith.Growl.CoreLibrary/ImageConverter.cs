using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace Neith.Growl.CoreLibrary
{
    /// <summary>
    /// Converts Image objects to byte arrays, as well as converting byte arrays and
    /// url references into Images.
    /// </summary>
    public static class ImageConverter
    {
        /// <summary>
        /// Converts the specified <see cref="BitmapImage"/> into an array of bytes
        /// </summary>
        /// <param name="image"><see cref="BitmapImage"/></param>
        /// <returns>Array of bytes</returns>
        public static byte[] ImageToBytes(BitmapImage image)
        {
            var stream = image.StreamSource;
            Byte[] buffer = null;
            if (stream != null && stream.Length > 0) {
                using (BinaryReader br = new BinaryReader(stream)) {
                    buffer = br.ReadBytes((Int32)stream.Length);
                }
            }

            return buffer;
        }

        /// <summary>
        /// Converts an array of bytes into an <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="bytes">The array of bytes</param>
        /// <returns>The resulting <see cref="BitmapImage"/></returns>
        public static BitmapImage ImageFromBytes(byte[] bytes)
        {
            return ImageFromStream(new MemoryStream(bytes));
        }

        /// <summary>
        /// Converts a url (filesystem or web) into an <see cref="BitmapImage"/>
        /// </summary>
        /// <param name="url">The url path to the image</param>
        /// <returns>The resulting <see cref="BitmapImage"/></returns>
        public static BitmapImage ImageFromUrl(string url)
        {
            var image = new BitmapImage();
            image.BeginInit();
            try {
                image.UriSource = new Uri(url);
                return image;
            }
            finally {
                image.EndInit();
            }
        }

        private static BitmapImage ImageFromStream(Stream stream)
        {
            var image = new BitmapImage();
            image.BeginInit();
            try {
                image.StreamSource = stream;
                return image;
            }
            finally {
                image.EndInit();
            }
        }


        /* I AM JUST SAVING THIS FOR NOW
        private byte[] ConvertToBytes2(Bitmap bmp)
        {
            System.Drawing.Imaging.BitmapData bData = bmp.LockBits(new Rectangle(new Point(), bmp.Size),
            System.Drawing.Imaging.ImageLockMode.ReadOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            // number of bytes in the bitmap
            int byteCount = bData.Stride * bmp.Height;
            byte[] bmpBytes = new byte[byteCount];

            // Copy the locked bytes from memory
            System.Runtime.InteropServices.Marshal.Copy(bData.Scan0, bmpBytes, 0, byteCount);

            // don't forget to unlock the bitmap!!
            bmp.UnlockBits(bData);

            return bmpBytes;
        }
         * */
    }
}
