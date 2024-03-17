using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using OpenCvSharp;

namespace Worker
{
    class Program
    {
        public static int count = 0;
        private static Client _client;

        static void Main(string[] args)
        {
            //_WorkerTh = new Thread(MainWorker);
            _client = new Client();
            CameraRecorder();
            //Thread.Sleep(1500);
            //_WorkerTh.Start();
        }

        public static void CameraRecorder()
        {
            // Open the default camera (index 0)
            using var capture = new VideoCapture(0);
            // Set the desired frame width and height
            int frameWidth = 1200;
            int frameHeight = 800;
            capture.Set(VideoCaptureProperties.FrameWidth, frameWidth);
            capture.Set(VideoCaptureProperties.FrameHeight, frameHeight);

            // Check if the camera opened successfully
            if (!capture.IsOpened())
            {
                Console.WriteLine("Failed to open camera!");
                return;
            }

            // Create a Mat object to hold the captured frame
            var frame = new Mat();

            // Create a MemoryStream to hold the image data
            using var memoryStream = new MemoryStream();

            // Capture frames continuously
            while (true)
            {
                // Capture a frame from the camera
                capture.Read(frame);

                // Check if the frame is empty
                if (frame.Empty())
                {
                    Console.WriteLine("Failed to capture frame!");
                    break;
                }

                // Convert the frame to JPEG format and save it to the MemoryStream
                
                var imageBuffer = frame.ImEncode(".jpg");
                

                memoryStream.SetLength(0); // Clear the MemoryStream
                memoryStream.Write(imageBuffer, 0, imageBuffer.Length);

                // You can now use the MemoryStream as needed, such as sending it over a network or processing it further

                // Reset the position of the MemoryStream to the beginning
                memoryStream.Position = 0;

                var base64 = Convert.ToBase64String(memoryStream.ToArray());
                _client.ConnectAsync(base64).GetAwaiter().GetResult();

                // Display the frame
                Cv2.ImShow("Frame", frame);
                //frame.Dispose();
                Cv2.WaitKey(1); // Adjust the delay as needed (milliseconds)
            }
        }


    }
}