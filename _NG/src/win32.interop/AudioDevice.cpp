// © Mike Murphy

#include "stdafx.h"
#include "AudioDevice.h"

using namespace EMU7800::D2D::Interop;

void AudioDevice::Open()
{
    WAVEFORMATEX wfx    = {0};
    wfx.wFormatTag      = WAVE_FORMAT_PCM;
    wfx.nChannels       = 1;
    wfx.wBitsPerSample  = 8;
    wfx.nSamplesPerSec  = m_frequency;
    wfx.nAvgBytesPerSec = m_frequency;
    wfx.nBlockAlign     = 1;
    wfx.cbSize          = 0;

    HWAVEOUT hwo;
    m_mmResult = waveOutOpen((LPHWAVEOUT)&hwo, WAVE_MAPPER, &wfx, NULL, NULL, CALLBACK_NULL);
    if (m_mmResult)
        return;

    m_hwo = hwo;

    HANDLE hHeap = GetProcessHeap();
    SIZE_T dwBytes = m_queueLength * m_bufferSizeInBytes;
    m_pBufferStorage = (byte*)HeapAlloc(hHeap, 0, dwBytes);
    if (m_pBufferStorage)
    {
        ZeroMemory(m_pBufferStorage, dwBytes);
        byte* ptr = m_pBufferStorage;
        for (UINT i = 0; i < m_queueLength; i++)
        {
            LPWAVEHDR pwh = (LPWAVEHDR)ptr;
            pwh->dwFlags = WHDR_DONE;
            ptr += m_bufferSizeInBytes;
        }
    }
    else
    {
        m_mmResult = E_OUTOFMEMORY;
    }
}

void AudioDevice::Write(array<byte>^ buffer)
{
    if (m_mmResult || !m_hwo || !m_pwhFree || BuffersQueued >= (int)m_queueLength)
        return;

    LPWAVEHDR pwh = m_pwhFree;
    m_pwhFree = NULL;

    m_mmResult = waveOutUnprepareHeader(m_hwo, pwh, sizeof(WAVEHDR));
    if (m_mmResult)
        return;

    pwh->dwBufferLength = m_bufferPayloadSizeInBytes;
    pwh->dwFlags = 0;
    pwh->lpData = (LPSTR)pwh + sizeof(WAVEHDR);
    pin_ptr<byte> pData = &buffer[0];
    CopyMemory(pwh->lpData, pData, m_bufferPayloadSizeInBytes);

    m_mmResult = waveOutPrepareHeader(m_hwo, pwh, sizeof(WAVEHDR));
    if (m_mmResult)
        return;

    m_mmResult = waveOutWrite(m_hwo, pwh, sizeof(WAVEHDR));
}

int AudioDevice::BuffersQueued::get()
{
    if (m_mmResult || !m_hwo)
        return -1;

    int queued = 0;

    byte* ptr = m_pBufferStorage;
    for (UINT i = 0; i < m_queueLength; i++)
    {
        LPWAVEHDR pwh = (LPWAVEHDR)ptr;
        if (pwh->dwFlags & WHDR_DONE)
        {
            if (!m_pwhFree)
                m_pwhFree = (LPWAVEHDR)(m_pBufferStorage + i * m_bufferSizeInBytes);
        }
        else
        {
            queued++;
        }
        ptr += m_bufferSizeInBytes;
    }

    return queued;
}

int AudioDevice::SubmitBuffer(array<byte>^ buffer)
{
    if (m_mmResult)
        return m_mmResult;
    if (buffer->Length != (int)m_bufferPayloadSizeInBytes)
        return E_INVALIDARG;

    if (!m_hwo)
    {
        Open();
        if (m_mmResult)
            return m_mmResult;
    }

    Write(buffer);

    return m_mmResult;
}

AudioDevice::AudioDevice(int frequency, int bufferSizeInBytes, int queueLength)
{
    m_mmResult = 0;
    m_hwo = 0;
    m_pBufferStorage = NULL;
    m_pwhFree = NULL;

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
    m_bufferSizeInBytes = sizeof(WAVEHDR) + (UINT)bufferSizeInBytes;
    m_queueLength = (UINT)queueLength;
}

AudioDevice::~AudioDevice()
{
    this->!AudioDevice();
}

AudioDevice::!AudioDevice()
{
    if (m_hwo)
    {
        waveOutReset(m_hwo);
        waveOutClose(m_hwo);
        m_hwo = NULL;
    }
    if (m_pBufferStorage)
    {
        HANDLE hHeap = GetProcessHeap();
        HeapFree(hHeap, 0, m_pBufferStorage);
        m_pBufferStorage = NULL;
    }
}

UINT AudioDevice::GetWaveOutVolume()
{
    DWORD dwVolume;
    waveOutGetVolume(m_hwo, &dwVolume);
    return dwVolume;
}

void AudioDevice::SetWaveOutVolume(UINT dwVolume)
{
    waveOutSetVolume(m_hwo, dwVolume);
}