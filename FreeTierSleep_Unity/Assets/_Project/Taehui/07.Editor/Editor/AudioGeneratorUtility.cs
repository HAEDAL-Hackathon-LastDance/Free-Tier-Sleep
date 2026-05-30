using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Taehui.Editor
{
    [InitializeOnLoad]
    public static class AudioGeneratorUtility
    {
        private const string AudioDirectoryPath = "Assets/_Project/Taehui/06.Audio";

        static AudioGeneratorUtility()
        {
            // 컴파일 또는 에디터 로드 시 자동으로 음원 파일 존재 여부를 확인하고 없으면 생성합니다.
            EditorApplication.delayCall += () =>
            {
                if (!Directory.Exists(AudioDirectoryPath))
                {
                    Directory.CreateDirectory(AudioDirectoryPath);
                }

                string bgmPath = Path.Combine(AudioDirectoryPath, "Phase1_BGM.wav");
                if (!File.Exists(bgmPath))
                {
                    Debug.Log("[AudioGenerator] BGM 파일이 없으므로 자동으로 오디오 파일 생성 절차를 시작합니다.");
                    GenerateAllAudioFiles();
                }
            };
        }

        [MenuItem("Tools/Taehui/Generate All Procedural Audio Files")]
        public static void GenerateAllAudioFiles()
        {
            try
            {
                if (!Directory.Exists(AudioDirectoryPath))
                {
                    Directory.CreateDirectory(AudioDirectoryPath);
                }

                // 1. Phase 1 BGM 생성
                ExportClipToWav(ProceduralAudioHelper.CreatePhase1BGM(16f), "Phase1_BGM.wav");

                // 2. 효과음들 생성
                ExportClipToWav(ProceduralAudioHelper.CreateTypeSound(), "TypeSFX.wav");
                ExportClipToWav(ProceduralAudioHelper.CreateWarningSound(), "WarningSFX.wav");
                ExportClipToWav(ProceduralAudioHelper.CreateGlitchSound(2.0f), "GlitchSFX.wav");
                ExportClipToWav(ProceduralAudioHelper.CreateHoverSound(), "HoverSFX.wav");
                ExportClipToWav(ProceduralAudioHelper.CreateBeepSound(1200f, 0.5f), "SystemBeepSFX.wav");
                ExportClipToWav(ProceduralAudioHelper.CreateStaticNoiseSound(2.0f), "TVStaticSFX.wav");

                AssetDatabase.Refresh();
                Debug.Log("[AudioGenerator] 모든 음원 및 효과음 파일이 성공적으로 생성 및 임포트되었습니다! 위치: " + AudioDirectoryPath);
            }
            catch (Exception ex)
            {
                Debug.LogError("[AudioGenerator] 오디오 파일 생성 중 오류 발생: " + ex.Message);
            }
        }

        private static void ExportClipToWav(AudioClip clip, string fileName)
        {
            if (clip == null) return;

            string fullPath = Path.Combine(AudioDirectoryPath, fileName);
            float[] samples = new float[clip.samples * clip.channels];
            clip.GetData(samples, 0);

            byte[] wavBytes = ConvertToWav(samples, clip.frequency, clip.channels);
            File.WriteAllBytes(fullPath, wavBytes);
            Debug.Log($"[AudioGenerator] 생성 완료: {fileName} ({clip.samples} 샘플)");
        }

        private static byte[] ConvertToWav(float[] samples, int sampleRate, int channels)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(memoryStream))
                {
                    writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                    writer.Write(36 + samples.Length * 2);
                    writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                    writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                    writer.Write(16);
                    writer.Write((ushort)1); // AudioFormat: 1 (PCM)
                    writer.Write((ushort)channels); // NumChannels
                    writer.Write(sampleRate); // SampleRate
                    writer.Write(sampleRate * channels * 2); // ByteRate (SampleRate * channels * 2 bytes/sample)
                    writer.Write((ushort)(channels * 2)); // BlockAlign
                    writer.Write((ushort)16); // BitsPerSample: 16
                    writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                    writer.Write(samples.Length * 2);

                    foreach (var sample in samples)
                    {
                        float clamped = Mathf.Clamp(sample, -1f, 1f);
                        short intSample = (short)(clamped * 32767);
                        writer.Write(intSample);
                    }
                }
                return memoryStream.ToArray();
            }
        }
    }
}
