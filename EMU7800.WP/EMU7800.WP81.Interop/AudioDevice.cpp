// © Mike Murphy

#include "pch.h"
#include "AudioDevice.h"

using namespace EMU7800::WP8::Interop;

void AudioDevice::Open()
{
    m_hr = XAudio2Create(&m_pXAudio2);
    if FAILED(m_hr)
        return;

    m_hr = m_pXAudio2->CreateMasteringVoice(&m_pMasterVoice);
    if FAILED(m_hr)
        return;

    WAVEFORMATEX wfx    = {0};
    wfx.wFormatTag      = WAVE_FORMAT_PCM;
    wfx.nChannels       = 1;
    wfx.wBitsPerSample  = 8;
    wfx.nSamplesPerSec  = m_frequency;
    wfx.nAvgBytesPerSec = m_frequency;
    wfx.nBlockAlign     = 1;
    wfx.cbSize          = 0;

    m_hr = m_pXAudio2->CreateSourceVoice(&m_pSourceVoice, (LPWAVEFORMATEX)&wfx);
    if FAILED(m_hr)
        return;

    m_pMasterVoice->SetVolume(1.0f);
    m_pSourceVoice->SetVolume(1.0f);

    m_hr = m_pSourceVoice->Start(0, XAUDIO2_COMMIT_NOW);
    if FAILED(m_hr)
        return;

    ZeroMemory(&m_sourceVoiceState, sizeof(XAUDIO2_VOICE_STATE));
    ZeroMemory(&m_xBuffer, sizeof(XAUDIO2_BUFFER));

    HANDLE hHeap = GetProcessHeap();
    SIZE_T dwBytes = m_queueLength * m_bufferPayloadSizeInBytes;
    m_pBufferStorage = (byte*)HeapAlloc(hHeap, 0, dwBytes);
    if (m_pBufferStorage)
        ZeroMemory(m_pBufferStorage, dwBytes);
    else
        m_hr = E_OUTOFMEMORY;

    m_currentBufferPosition = 0;
}

void AudioDevice::Write(const Array<uint8>^ buffer)
{
    if (FAILED(m_hr) || !m_pSourceVoice || BuffersQueued >= (int)m_queueLength)
        return;

    byte* pData = m_pBufferStorage + m_currentBufferPosition * m_bufferPayloadSizeInBytes;
    m_currentBufferPosition++;
    m_currentBufferPosition %= m_queueLength;

    CopyMemory(pData, buffer->Data, m_bufferPayloadSizeInBytes);
    m_xBuffer.AudioBytes = m_bufferPayloadSizeInBytes;
    m_xBuffer.pAudioData = pData;

    m_hr = m_pSourceVoice->SubmitSourceBuffer(&m_xBuffer);
}

int AudioDevice::BuffersQueued::get()
{
    if (FAILED(m_hr) || !m_pSourceVoice)
        return -1;

    m_pSourceVoice->GetState(&m_sourceVoiceState, XAUDIO2_VOICE_NOSAMPLESPLAYED);
    return m_sourceVoiceState.BuffersQueued;
}

int AudioDevice::SubmitBuffer(const Array<uint8>^ buffer)
{
    if FAILED(m_hr)
        return m_hr;
    if (buffer->Length != m_bufferPayloadSizeInBytes)
        return E_INVALIDARG;

    if (!m_pXAudio2)
    {
        Open();
        if FAILED(m_hr)
            return m_hr;
    }

    Write(buffer);

    return m_hr;
}

AudioDevice::AudioDevice(int frequency, int bufferSizeInBytes, int queueLength)
{
    m_hr = 0;
    m_pMasterVoice = NULL;
    m_pSourceVoice = NULL;

    if (frequency < 0)
        frequency = 0;

    if (bufferSizeInBytes < 0)
        bufferSizeInBytes = 0;
    else if (bufferSizeInBytes > 0x400)
        bufferSizeInBytes = 0x400;

    if (queueLength < 0)
        queueLength = 0;
    else if (queueLength > 0x10)
        queueLength = 0x10;

    m_frequency = (UINT)frequency;
    m_bufferPayloadSizeInBytes = (UINT)bufferSizeInBytes;
    m_queueLength = (UINT)queueLength;
}

AudioDevice::~AudioDevice()
{
    if (m_pSourceVoice)
    {
        m_pSourceVoice->Stop();
        m_pSourceVoice->DestroyVoice();
        m_pSourceVoice = nullptr;
    }
    if (m_pMasterVoice)
    {
        m_pMasterVoice->DestroyVoice();
        m_pMasterVoice = nullptr;
    }
    if (m_pXAudio2)
    {
        m_pXAudio2 = nullptr;
    }
    if (m_pBufferStorage)
    {
        HANDLE hHeap = GetProcessHeap();
        HeapFree(hHeap, 0, m_pBufferStorage);
        m_pBufferStorage = NULL;
    }
}

float AudioDevice::GetSourceVoiceVolume()
{
    if (!m_pSourceVoice)
        return 0.0;

    float volume;
    m_pSourceVoice->GetVolume(&volume);
    return volume;
}

void AudioDevice::SetSourceVoiceVolume(float volume)
{
    if (!m_pSourceVoice)
        return;
    m_pSourceVoice->SetVolume(volume, XAUDIO2_COMMIT_ALL);
}