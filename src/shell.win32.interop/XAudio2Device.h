// © Mike Murphy

#pragma once

namespace EMU7800 { namespace D2D {

public ref class XAudio2Device
{
private:
    IXAudio2 *m_pXAudio2;
    IXAudio2MasteringVoice *m_pMasterVoice;
    IXAudio2SourceVoice *m_pSourceVoice;
    bool m_haveStarted;

public:
    XAudio2Device(int freq);
    ~XAudio2Device();
    !XAudio2Device();

    property float Volume { float get(); void set(float volume); }
    property int BuffersQueued { int get(); }

    void Start();
    void Stop();
    void SubmitBuffer(array<EMU7800::Core::BufferElement> ^buffer);
};

} }