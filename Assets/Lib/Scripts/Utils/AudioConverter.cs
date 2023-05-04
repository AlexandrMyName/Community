using System.IO;
using System;
using UnityEngine;

namespace Utils
{
    class CustomAudioConverter
    {
        public static AudioClip FromMp3Data(byte[] bytes, int outputRate = 24000)
        {
            float[] samplesArray = Mp3Data2Samples(bytes, outputRate: outputRate);
            var audioClip = AudioClip.Create("MySound", samplesArray.Length, 1, outputRate, false);
            audioClip.SetData(samplesArray, 0);
            return audioClip;
        }
        public static float[] Mp3Data2Samples(byte[] bytes, int outputRate = 24000)
        {
            MP3Sharp.MP3Stream mp3Stream = new MP3Sharp.MP3Stream(new MemoryStream(bytes));

            //Get the converted stream data
            MemoryStream convertedAudioDataStream = new MemoryStream();
            byte[] buffer = new byte[2048];
            int bytesReturned = -1;
            int totalBytesReturned = 0;

            while (bytesReturned != 0)
            {
                bytesReturned = mp3Stream.Read(buffer, 0, buffer.Length);
                convertedAudioDataStream.Write(buffer, 0, bytesReturned);
                totalBytesReturned += bytesReturned;
            }
            byte[] convertedAudioData = convertedAudioDataStream.ToArray();
            //Convert the byte converted byte data into float form in the range of 0.0-1.0
            float[] samplesArray = new float[convertedAudioData.Length / 2];            

            for (int i = 0; i < samplesArray.Length; i++)
                samplesArray[i] = (float)(BitConverter.ToInt16(convertedAudioData, i * 2) / 32768.0f);
            //Debug.Log("MP3 file has " + mp3Stream.ChannelCount + " channels with a frequency of " + mp3Stream.Frequency);
            return samplesArray;
        }
        public static byte[] getBytes(string bytes64)
        {
            if (bytes64 == null || bytes64.Length < 1)
            {
                FlutterUnityIntegration.UnityMessageManager.Instance.SendMessageToFlutter("DEBUG_TAG : Bytes Array is EMPTY");
                return new byte[0];
            }
            else return Convert.FromBase64String(bytes64);
        }
    }
}