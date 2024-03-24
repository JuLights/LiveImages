using System;
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using OpenCvSharp;
using NAudio.Wave;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;
using NAudio.Wave.Compression;

namespace Worker
{
    class Program
    {
        public static int count = 0;
        private static Client _client;
        private static Stopwatch _watch = new Stopwatch();

        static void Main(string[] args)
        {
            //_WorkerTh = new Thread(MainWorker);
            _client = new Client();
            //CameraRecorder();
            AudioRecorder();
            //Thread.Sleep(1500);
            //_WorkerTh.Start();
        }

        public static void AudioRecorder()
        {

            //byte[] buffer = null;
            using (var audioCapture = new WaveInEvent())
            {
                var byteArr = new byte[5120000];
                audioCapture.WaveFormat = new WaveFormat(44100, 16, 1); // Mono, 44.1 kHz, 16-bit
                int i = 0;
                audioCapture.DataAvailable += async (s, e) =>
                {
                    byte[] buffer = new byte[e.BytesRecorded];
                    //Debug.WriteLine(buffer[0]);
                    Buffer.BlockCopy(e.Buffer, 0, byteArr, i*buffer.Length, e.BytesRecorded);
                    Console.WriteLine(i*buffer.Length);
                    i++;
                    if (_watch.ElapsedMilliseconds > 10000)
                    {
                        i = 0;
                        if (buffer != null)
                        {
                            //using(var strm = new MemoryStream())
                            //using (var writer = new WaveFileWriter(strm, audioCapture.WaveFormat))
                            //{
                            //    writer.Write(byteArr);
                            //    string base64Audio = Convert.ToBase64String(strm.ToArray());
                            //    _client.AudioTransferAsync(base64Audio).GetAwaiter().GetResult();
                            //}

                            var arr = SaveAudioWithInterval(byteArr); //properly write wav file to memory Stream
                            string base64Audio = Convert.ToBase64String(arr);
                            _client.AudioTransferAsync(base64Audio).GetAwaiter().GetResult();
                        }
                        Console.WriteLine(_watch.ElapsedMilliseconds);
                        Console.WriteLine($"Sent size: {buffer.Length}");
                        Console.WriteLine($"BytesRecorded: {e.BytesRecorded}");
                        _watch.Reset();
                        _watch.Start();
                    }
                };

                audioCapture.StartRecording();
                _watch.Start();



                Console.WriteLine("Streaming audio to server. Press any key to stop.");
                Console.ReadKey();

                audioCapture.StopRecording();
            }
            Thread.Sleep(1);

        }
        public static int cnt = 0;
        //public static MemoryStream memStream;

        public static byte [] SaveAudioInMemoryStream(byte[] data)
        {

            // Define WAV file format parameters
            short channels = 2; // Stereo
            int sampleRate = 44100; // 44.1 kHz
            short bitsPerSample = 16; // 16-bit PCM

            // Calculate the size of the audio data chunk
            int dataSize = 100 * data.Length;

            // Calculate the size of the entire file
            int fileSize = 36 + dataSize; // 36 bytes for the WAV header

            // Write WAV file header
            var memStream = new MemoryStream();
            using (var writer = new BinaryWriter(memStream))
            {
                // Write the 'RIFF' chunk descriptor
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(fileSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                // Write the 'fmt ' sub-chunk
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Sub-chunk size
                writer.Write((short)1); // Audio format (PCM)
                writer.Write(channels); // Number of channels
                writer.Write(sampleRate); // Sample rate
                writer.Write(sampleRate * channels * (bitsPerSample / 8)); // Byte rate
                writer.Write((short)(channels * (bitsPerSample / 8))); // Block align
                writer.Write(bitsPerSample); // Bits per sample

                // Write the 'data' sub-chunk
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize); // Sub-chunk size

                // Write audio data
                writer.Write(data);
            }
            cnt++;
            return memStream.ToArray();
        }
        public static byte [] SaveAudioWithInterval(byte[] data)
        {
            
            // Define WAV file format parameters
            short channels = 1; // Mono
            int sampleRate = 44100; // 44.1 kHz
            short bitsPerSample = 16; // 16-bit PCM

            // Calculate the size of the audio data chunk
            int dataSize = 1000 * data.Length;

            // Calculate the size of the entire file
            int fileSize = 36 + dataSize; // 36 bytes for the WAV header

            // Write WAV file header
            using (var fileStream = new FileStream($"C:\\Users\\Davy\\Desktop\\Audio\\asd{cnt}.wav", FileMode.Create))
            using (var writer = new BinaryWriter(fileStream))
            {
                // Write the 'RIFF' chunk descriptor
                writer.Write(Encoding.ASCII.GetBytes("RIFF"));
                writer.Write(fileSize);
                writer.Write(Encoding.ASCII.GetBytes("WAVE"));

                // Write the 'fmt ' sub-chunk
                writer.Write(Encoding.ASCII.GetBytes("fmt "));
                writer.Write(16); // Sub-chunk size
                writer.Write((short)1); // Audio format (PCM)
                writer.Write(channels); // Number of channels
                writer.Write(sampleRate); // Sample rate
                writer.Write(sampleRate * channels * (bitsPerSample / 8)); // Byte rate
                writer.Write((short)(channels * (bitsPerSample / 8))); // Block align
                writer.Write(bitsPerSample); // Bits per sample

                // Write the 'data' sub-chunk
                writer.Write(Encoding.ASCII.GetBytes("data"));
                writer.Write(dataSize); // Sub-chunk size

                // Write audio data
                writer.Write(data);
            }
            cnt++;

            return data;
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