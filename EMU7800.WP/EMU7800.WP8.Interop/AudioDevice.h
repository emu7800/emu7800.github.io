// © Mike Murphy

#pragma once

using namespace Microsoft::WRL;
using namespace Platform;

namespace EMU7800 { namespace WP8 { namespace Interop {

/// <summary>
/// Class responsible for PCM sound playback.
/// </summary>
public ref class AudioDevice sealed
{
private:
    UINT m_frequency;
    UINT m_bufferPayloadSizeInBytes;
    UINT m_queueLength;

    HRESULT m_hr;
    byte* m_pBufferStorage;
    int m_currentBufferPosition;
    ComPtr<IXAudio2> m_pXAudio2;
    IXAudio2MasteringVoice* m_pMasterVoice;
    IXAudio2SourceVoice* m_pSourceVoice;
    XAUDIO2_VOICE_STATE m_sourceVoiceState;
    XAUDIO2_BUFFER m_xBuffer;

    void Open();
    void Write(const Array<uint8>^ buffer);

public:
    /// <summary>
    /// A non-zero and platform-specific value if the instance has transitioned to the error state.
    /// </summary>
    property int ErrorCode { int get() { return m_hr; } }

    /// <summary>
    /// Returns the number of sound buffers yet to be processed.
    /// Returns <c>-1</c> if the instance has transitioned to the error state or if <c>SubmitBuffer</c> has yet to be called on the instance.
    /// </summary>
    property int BuffersQueued { int get(); }

    /// <summary>
    /// Enqueues the specified buffer of PCM sound data to the underlying sound playback mechanism.
    /// Returns <c>E_INVALIDARG</c> if buffer length does not match <c>bufferSizeInBytes</c> provided to the constructor.
    /// Returns the value of <c>ErrorCode</c>, which will be a non-zero and platform-specific value if the instance has transitioned to the error state.
    /// Once transitioned to the error state, sound playback ceases to function.
    /// </summary>
    int SubmitBuffer(const Array<uint8>^ buffer);

    /// <summary>
    /// Creates an instance of <c>AudioDevice</c> with the specified parameters.
    /// </summary>
    /// <param name="frequency">Must be in the range [0, *).</param>
    /// <param name="bufferSizeInBytes">Must be in the range [0, 1024].</param>
    /// <param name="queueLength">Must be in the range [0, 16].</param>
    AudioDevice(int frequency, int bufferSizeInBytes, int queueLength);

    /// <summary>
    /// Terminates the lifetime of this instance of <c>AudioDevice</c>.
    /// </summary>
    virtual ~AudioDevice();

    float GetSourceVoiceVolume();
    void SetSourceVoiceVolume(float volume);
};

} } }