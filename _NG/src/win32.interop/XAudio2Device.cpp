// © Mike Murphy

#include "stdafx.h"
#include "XAudio2Device.h"

using namespace System;
using namespace System::Runtime::InteropServices;
using namespace EMU7800::D2D;

XAudio2Device::XAudio2Device(int freq) : m_pXAudio2(0), m_pMasterVoice(0), m_pSourceVoice(0), m_haveStarted(false)
{
    IXAudio2* pXAudio2 = NULL;
    HRESULT hr = XAudio2Create(&pXAudio2);//, 0, XAUDIO2_DEFAULT_PROCESSOR);
    if FAILED(hr)
        throw Marshal::GetExceptionForHR(hr);
    m_pXAudio2 = pXAudio2;

    IXAudio2MasteringVoice* pMasterVoice = NULL;
    hr = pXAudio2->CreateMasteringVoice(&pMasterVoice);
    if FAILED(hr)
        throw Marshal::GetExceptionForHR(hr);
    m_pMasterVoice = pMasterVoice;

    WAVEFORMATEX wfx    = {0};
    wfx.wFormatTag      = WAVE_FORMAT_PCM;
    wfx.nChannels       = 1;
    wfx.wBitsPerSample  = 8;
    wfx.nSamplesPerSec  = (UINT)freq;
    wfx.nAvgBytesPerSec = (UINT)freq;
    wfx.nBlockAlign     = 1;
    wfx.cbSize          = 0;

    IXAudio2SourceVoice* pSourceVoice;
    hr = pXAudio2->CreateSourceVoice(&pSourceVoice, (WAVEFORMATEX*)&wfx);
    if FAILED(hr)
        throw Marshal::GetExceptionForHR(hr);
    m_pSourceVoice = pSourceVoice;
}

float XAudio2Device::Volume::get()
{
    if (!m_pSourceVoice)
        return 0.0;
    float volume;
    m_pSourceVoice->GetVolume(&volume);
    return volume;
}

void XAudio2Device::Volume::set(float volume)
{
    if (!m_pSourceVoice)
        return;
    m_pSourceVoice->SetVolume(volume);
}

int XAudio2Device::BuffersQueued::get()
{
    if (!m_pSourceVoice)
        return 0;
    XAUDIO2_VOICE_STATE voiceState;
    m_pSourceVoice->GetState(&voiceState);
    return voiceState.BuffersQueued;
}

void XAudio2Device::Start()
{
    if (!m_pSourceVoice)
        return;
    HRESULT hr = m_pSourceVoice->Start(0, XAUDIO2_COMMIT_NOW);
    if FAILED(hr)
        throw Marshal::GetExceptionForHR(hr);
    m_haveStarted = true;
}

void XAudio2Device::Stop()
{
    if (!m_pSourceVoice)
        return;
    HRESULT hr = m_pSourceVoice->Stop();
    if FAILED(hr)
        throw Marshal::GetExceptionForHR(hr);
    m_haveStarted = false;
}

void XAudio2Device::SubmitBuffer(array<EMU7800::Core::BufferElement> ^buffer)
{
    if (buffer == nullptr)
        throw gcnew ArgumentNullException("buffer");
    if (buffer->Length == 0)
        return;
    if (!m_pSourceVoice)
        return;

    pin_ptr<EMU7800::Core::BufferElement> audioData = &buffer[0];

    byte* ptr = (byte*)audioData;
    for (int i = 0; i < buffer->Length * EMU7800::Core::BufferElement::SIZE; i++, ptr++)
        *ptr |= 0x80;

    XAUDIO2_BUFFER xbuffer = {0};
    xbuffer.AudioBytes     = buffer->Length;
    xbuffer.pAudioData     = (byte*)audioData;
    HRESULT hr = m_pSourceVoice->SubmitSourceBuffer(&xbuffer);
    if FAILED(hr)
        throw Marshal::GetExceptionForHR(hr);

    if (!m_haveStarted)
    {
        Start();
        m_haveStarted = true;
    }
}

XAudio2Device::~XAudio2Device()
{
    this->!XAudio2Device();
}

XAudio2Device::!XAudio2Device()
{
    if (m_pSourceVoice)
    {
        m_pSourceVoice->Stop();
        m_pSourceVoice->DestroyVoice();
        m_pSourceVoice = NULL;
    }
    if (m_pMasterVoice)
    {
        m_pMasterVoice->DestroyVoice();
        m_pMasterVoice = NULL;
    }
    SAFE_RELEASE(m_pXAudio2);
}