using Microsoft.Kinect;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Kinect_ing_Pepper.Business
{
    public class FrameParser
    {
        private const int MAP_DEPTH_TO_BYTE = 8000 / 256;

        /// <summary>
        /// Maximum value (as a float) that can be returned by the InfraredFrame
        /// </summary>
        private const float InfraredSourceValueMaximum = (float)ushort.MaxValue;

        /// <summary>
        /// The value by which the infrared source data will be scaled
        /// </summary>
        private const float InfraredSourceScale = 0.75f;

        /// <summary>
        /// Smallest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMinimum = 0.01f;

        /// <summary>
        /// Largest value to display when the infrared data is normalized
        /// </summary>
        private const float InfraredOutputValueMaximum = 1.0f;

        public WriteableBitmap ParseToWriteableBitmap(ColorFrame colorFrame)
        {
            WriteableBitmap colorBitmap = new WriteableBitmap(colorFrame.FrameDescription.Width, colorFrame.FrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
            {
                colorBitmap.Lock();

                colorFrame.CopyConvertedFrameDataToIntPtr(
                    colorBitmap.BackBuffer,
                    (uint)(colorFrame.FrameDescription.Width * colorFrame.FrameDescription.Height * 4),
                    ColorImageFormat.Bgra);

                colorBitmap.AddDirtyRect(new Int32Rect(0, 0, colorBitmap.PixelWidth, colorBitmap.PixelHeight));

                colorBitmap.Unlock();
            }

            return colorBitmap;
        }

        public WriteableBitmap ParseToWriteableBitmap(DepthFrame depthFrame)
        {
            WriteableBitmap depthBitmap = new WriteableBitmap(depthFrame.FrameDescription.Width, depthFrame.FrameDescription.Height, 96.0, 96.0, PixelFormats.Gray8, null);

            using (KinectBuffer depthBuffer = depthFrame.LockImageBuffer())
            {
                ushort maxDepth = ushort.MaxValue;
                //maxDepth = depthFrame.DepthMaxReliableDistance; //TODO: make this a setting?

                byte[] depthPixels = ConvertDepthFrameData(depthFrame.FrameDescription, depthBuffer.UnderlyingBuffer, depthBuffer.Size, depthFrame.DepthMinReliableDistance, maxDepth);

                depthBitmap.WritePixels(new Int32Rect(0, 0, depthBitmap.PixelWidth, depthBitmap.PixelHeight), depthPixels, depthBitmap.PixelWidth, 0);
            }

            return depthBitmap;
        }

        public Bitmap ParseToBitmap(WriteableBitmap writableBitmap)
        {
            Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder encoder = new BmpBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(writableBitmap));
                encoder.Save(outStream);

                bitmap = new Bitmap(outStream);
            }
            return bitmap;
        }

        private unsafe byte[] ConvertDepthFrameData(FrameDescription depthFrameDescription, IntPtr depthFrameData, uint depthFrameDataSize, ushort minDepth, ushort maxDepth)
        {
            // depth frame data is a 16 bit value
            ushort* frameData = (ushort*)depthFrameData;
            byte[] depthPixels = new byte[depthFrameDescription.Width * depthFrameDescription.Height];

            // convert depth to a visual representation
            for (int i = 0; i < (int)(depthFrameDataSize / depthFrameDescription.BytesPerPixel); ++i)
            {
                // Get the depth for this pixel
                ushort depth = frameData[i];

                // To convert to a byte, we're mapping the depth value to the byte range.
                // Values outside the reliable depth range are mapped to 0 (black).
                depthPixels[i] = (byte)(depth >= minDepth && depth <= maxDepth ? (depth / MAP_DEPTH_TO_BYTE) : 0);
            }

            return depthPixels;
        }

        public WriteableBitmap ParseToWriteableBitmap(InfraredFrame infraredFrame)
        {
            WriteableBitmap infraredBitmap = new WriteableBitmap(infraredFrame.FrameDescription.Width, infraredFrame.FrameDescription.Height, 96.0, 96.0, PixelFormats.Gray32Float, null);

            using (Microsoft.Kinect.KinectBuffer infraredBuffer = infraredFrame.LockImageBuffer())
            {
                ConvertInfraredFrameData(infraredBitmap, infraredFrame.FrameDescription, infraredBuffer.UnderlyingBuffer, infraredBuffer.Size);
            }

            return infraredBitmap;
        }

        /// <summary>
        /// Directly accesses the underlying image buffer of the InfraredFrame to 
        /// create a displayable bitmap.
        /// This function requires the /unsafe compiler option as we make use of direct
        /// access to the native memory pointed to by the infraredFrameData pointer.
        /// </summary>
        /// <param name="infraredFrameData">Pointer to the InfraredFrame image data</param>
        /// <param name="infraredFrameDataSize">Size of the InfraredFrame image data</param>
        private unsafe void ConvertInfraredFrameData(WriteableBitmap infraredBitmap, FrameDescription infraredFrameDescription, IntPtr infraredFrameData, uint infraredFrameDataSize)
        {
            // infrared frame data is a 16 bit value
            ushort* frameData = (ushort*)infraredFrameData;

            // lock the target bitmap
            infraredBitmap.Lock();

            // get the pointer to the bitmap's back buffer
            float* backBuffer = (float*)infraredBitmap.BackBuffer;

            // process the infrared data
            for (int i = 0; i < (int)(infraredFrameDataSize / infraredFrameDescription.BytesPerPixel); ++i)
            {
                // since we are displaying the image as a normalized grey scale image, we need to convert from
                // the ushort data (as provided by the InfraredFrame) to a value from [InfraredOutputValueMinimum, InfraredOutputValueMaximum]
                backBuffer[i] = Math.Min(InfraredOutputValueMaximum, (((float)frameData[i] / InfraredSourceValueMaximum * InfraredSourceScale) * (1.0f - InfraredOutputValueMinimum)) + InfraredOutputValueMinimum);
            }

            // mark the entire bitmap as needing to be drawn
            infraredBitmap.AddDirtyRect(new Int32Rect(0, 0, infraredBitmap.PixelWidth, infraredBitmap.PixelHeight));

            // unlock the bitmap
            infraredBitmap.Unlock();
        }

        //If you get 'dllimport unknown'-, then add 'using System.Runtime.InteropServices;'
        [DllImport("gdi32.dll", EntryPoint = "DeleteObject")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DeleteObject([In] IntPtr hObject);

        public ImageSource ImageSourceForBitmap(System.Drawing.Bitmap bmp)
        {
            var handle = bmp.GetHbitmap();
            try
            {
                return Imaging.CreateBitmapSourceFromHBitmap(handle, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            }
            finally { DeleteObject(handle); }
        }
    }
}
