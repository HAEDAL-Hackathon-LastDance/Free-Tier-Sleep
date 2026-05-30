using UnityEngine;

namespace Taehui
{
    /// <summary>
    /// 외부 사운드 파일 없이 코드로 레트로 8비트 사운드(AudioClip)를 절차적으로 생성하는 헬퍼 클래스
    /// </summary>
    public static class ProceduralAudioHelper
    {
        /// <summary>
        /// 디지털 타자기/비프음 (짧은 노이즈 + 피치 클릭)
        /// </summary>
        public static AudioClip CreateTypeSound()
        {
            int sampleRate = 44100;
            float duration = 0.04f; // 40ms로 매우 짧게
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                // 감쇠하는 노이즈와 미세한 사인파의 조합
                float envelope = Mathf.Exp(-150f * t); // 급격한 감쇠
                float noise = (Random.value * 2f - 1f) * 0.3f;
                float tone = Mathf.Sin(2f * Mathf.PI * 1200f * t) * 0.7f; // 1200Hz 고음
                samples[i] = (noise + tone) * envelope;
            }

            AudioClip clip = AudioClip.Create("TypeSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// 팝업 에러/경고음 ("띠링!" 하는 레트로 이중 비프음)
        /// </summary>
        public static AudioClip CreateWarningSound()
        {
            int sampleRate = 44100;
            float duration = 0.15f; // 150ms
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-12f * t); // 완만한 감쇠
                
                // 띠링 연출을 위해 시간에 따라 주파수가 상승하는 효과 (FM/아르페지오 느낌)
                float frequency = (t < 0.05f) ? 880f : 1200f; // 880Hz에서 1200Hz로 전환
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * t);
                
                // 약간의 스퀘어파(Square wave) 왜곡을 주어 복고풍의 쨍한 소리 연출
                float squareTone = (tone > 0) ? 0.5f : -0.5f;
                
                samples[i] = squareTone * envelope * 0.3f;
            }

            AudioClip clip = AudioClip.Create("WarningSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// 글리치 전환 효과음 (귀를 찌르는 화이트 노이즈와 전자기 간섭)
        /// </summary>
        public static AudioClip CreateGlitchSound(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            float phase = 0f;
            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float progress = t / duration;
                
                // 시간이 지날수록 피치와 볼륨이 고조되다가 컷오프되는 빌드업
                float volume = Mathf.Lerp(0.1f, 0.8f, progress);
                
                // 무작위 주파수 모듈레이션
                float freqMod = Mathf.Sin(2f * Mathf.PI * 15f * t) * 400f;
                float targetFreq = 100f + progress * 800f + freqMod;
                
                phase += 2f * Mathf.PI * targetFreq / sampleRate;
                float sine = Mathf.Sin(phase);
                
                // 전형적인 전자기기 지직거리는 노이즈 조합
                float noise = (Random.value * 2f - 1f) * 0.5f;
                
                // 주기적인 신호 드랍아웃 (지직거림)
                float dropOut = (Mathf.PingPong(t * 30f, 1f) > 0.8f) ? 0.1f : 1f;

                samples[i] = (sine * 0.4f + noise * 0.6f) * volume * dropOut;
            }

            AudioClip clip = AudioClip.Create("GlitchSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// 버튼 호버/선택용 아주 짧은 레트로 비프음 (20ms 고주파 클릭)
        /// </summary>
        public static AudioClip CreateHoverSound()
        {
            int sampleRate = 44100;
            float duration = 0.02f; // 20ms
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-200f * t); // 극히 빠른 감쇠
                float tone = Mathf.Sin(2f * Mathf.PI * 1800f * t); // 1800Hz 아주 높은 고주파음
                samples[i] = tone * envelope * 0.15f; // 작고 선명한 비프음
            }

            AudioClip clip = AudioClip.Create("HoverSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// 날카로운 시스템 경고음 (메인보드 BIOS/경고 비프 소리)
        /// </summary>
        public static AudioClip CreateBeepSound(float frequency, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float tone = Mathf.Sin(2f * Mathf.PI * frequency * t);
                // 쨍한 BIOS 비프음을 연출하기 위해 스퀘어파(Square wave)로 변환
                float squareTone = (tone > 0f) ? 0.25f : -0.25f;
                // 뒤로 갈수록 완만하게 작아지는 감쇠 적용
                float envelope = Mathf.Exp(-4f * t);
                samples[i] = squareTone * envelope;
            }

            AudioClip clip = AudioClip.Create("SystemBeepSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        /// <summary>
        /// TV 지지직 화이트 노이즈 사운드 생성
        /// </summary>
        public static AudioClip CreateStaticNoiseSound(float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                // 완전 무작위 화이트 노이즈 생성
                float noise = Random.value * 2f - 1f;
                // TV 지지직거리는 볼륨 굴곡(LFO) 연출을 위해 저주파 변조
                float t = (float)i / sampleRate;
                float lfo = 0.7f + Mathf.Sin(2f * Mathf.PI * 8f * t) * 0.2f;
                samples[i] = noise * lfo * 0.15f; // 너무 시끄럽지 않게 볼륨 제한
            }

            AudioClip clip = AudioClip.Create("TVStaticSFX", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
