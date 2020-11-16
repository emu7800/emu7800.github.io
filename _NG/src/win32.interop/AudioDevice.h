// © Mike Murphy

#pragma once

namespace EMU7800 { namespace D2D { namespace Interop {

/// <summary>
/// Class responsible for PCM sound playback.
/// </summary>
public ref class AudioDevice sealed
{
private:
    UINT m_frequency;
    UINT m_bufferPayloadSizeInBytes;
    UINT m_queueLength;

    MMRESULT m_mmResult;
    HWAVEOUT m_hwo;
    byte* m_pBufferStorage;
    UINT m_bufferSizeInBytes;
    LPWAVEHDR m_pwhFree;

    void Open();
    void Write(array<byte>^ buffer);

public:
    /// <summary>
    /// A non-zero and platform-specific value if the instance has transitioned to the error state.
    /// </summary>
    property int ErrorCode { int get() { return m_mmResult; } }

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
    int SubmitBuffer(array<byte>^ buffer);

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
    ~AudioDevice();

    /// <summary>
    /// Terminates the lifetime of this instance of <c>AudioDevice</c>.
    /// </summary>
    !AudioDevice();

    UINT GetWaveOutVolume();
    void SetWaveOutVolume(UINT dwVolume);
};

} } }