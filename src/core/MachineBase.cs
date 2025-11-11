/*
/*
 * MachineBase.cs
 *
 * Abstraction of an emulated machine.
 *
 * Copyright © 2003, 2004 Mike Murphy
 *
 */
using System;
using System.IO;
using System.Reflection;
using EMU7800.Core.Extensions;

namespace EMU7800.Core;

public abstract class MachineBase
{
    public static readonly MachineBase Default = new MachineUnknown();

    #region Fields

    readonly int _VisiblePitch, _Scanlines;

    #endregion

    #region Public Properties

    /// <summary>
    /// The machine's Central Processing Unit.
    /// </summary>
    public M6502 CPU { get; protected set; } = M6502.Default;

    /// <summary>
    /// The machine's Address Space.
    /// </summary>
    public AddressSpace Mem { get; protected set; } = AddressSpace.Default;

    /// <summary>
    /// The machine's Peripheral Interface Adaptor device.
    /// </summary>
    public PIA PIA { get; protected set; } = PIA.Default;

    /// <summary>
    /// The current frame buffer.
    /// </summary>
    public FrameBuffer FrameBuffer { get; init; }

    /// <summary>
    /// The game cart inserted into the machine.
    /// </summary>
    public Cart Cart { get; protected set; } = Cart.Default;

    /// <summary>
    /// Reports whether the machine has been halted due to an internal condition or error.
    /// </summary>
    public bool MachineHalt { get; protected set; }

    /// <summary>
    /// The machine input state.
    /// </summary>
    public InputState InputState { get; } = new();

    /// <summary>
    /// The current frame number.
    /// </summary>
    public long FrameNumber { get; protected set; }

    /// <summary>
    /// The first scanline that is visible.
    /// </summary>
    public int FirstScanline { get; }

    /// <summary>
    /// Frame rate.
    /// </summary>
    public int FrameHZ
    {
        get => field < 1 ? 1 : field;
        set => field = value < 1 ? 1 : value;
    }

    /// <summary>
    /// Number of sound samples per second.
    /// </summary>
    public int SoundSampleFrequency { get; } = 1;

    /// <summary>
    /// The color palette for the configured machine.
    /// </summary>
    public ReadOnlyMemory<uint> Palette { get; }

    /// <summary>
    /// Dumps CPU registers to the log when NOP instructions are encountered.
    /// </summary>
    public bool NOPRegisterDumping { get; set; }

    /// <summary>
    /// The configured logger sink.
    /// </summary>
    public ILogger Logger
    {
        get => field ?? NullLogger.Default;
        set => field = value;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Creates an instance of the specified machine.
    /// </summary>
    /// <param name="machineType"></param>
    /// <param name="cart"></param>
    /// <param name="bios">7800 BIOS, optional.</param>
    /// <param name="p1">Left controller, optional.</param>
    /// <param name="p2">Right controller, optional.</param>
    /// <param name="logger"></param>
    public static MachineBase Create(MachineType machineType, Cart cart, Bios7800 bios, Controller p1, Controller p2, ILogger logger)
    {
        MachineBase m =
            MachineTypeUtil.Is2600NTSC(machineType) ? new Machine2600NTSC(cart, logger) :
            MachineTypeUtil.Is2600PAL(machineType)  ? new Machine2600PAL(cart, logger) :
            MachineTypeUtil.Is7800NTSC(machineType) ? new Machine7800NTSC(cart, bios, logger) :
            MachineTypeUtil.Is7800PAL(machineType)  ? new Machine7800PAL(cart, bios, logger) :
            throw new ArgumentException("Unexpected MachineType: " + machineType);

        m.InputState.LeftControllerJack = p1;
        m.InputState.RightControllerJack = p2;

        m.Reset();

        return m;
    }

    /// <summary>
    /// Deserialize a <see cref="MachineBase"/> from the specified stream.
    /// </summary>
    /// <param name="binaryReader"/>
    /// <exception cref="SerializationException"/>
    public static MachineBase Deserialize(BinaryReader binaryReader)
    {
        var context = new DeserializationContext(binaryReader);
        return context.ReadMachine();
    }

    /// <summary>
    /// Resets the state of the machine.
    /// </summary>
    public virtual void Reset()
    {
        Logger.WriteLine($"Machine {this}  reset ({FrameHZ} HZ  {_Scanlines} scanlines)");
        FrameNumber = 0;
        MachineHalt = false;
        InputState.ClearAllInput();
    }

    /// <summary>
    /// Computes the next machine frame.
    /// </summary>
    public virtual void ComputeNextFrame()
    {
        if (MachineHalt)
            return;

        InputState.CaptureInputState();

        FrameNumber++;

        FrameBuffer.SoundBuffer.Span.Clear();
    }

    /// <summary>
    /// Serialize the state of the machine to the specified stream.
    /// </summary>
    /// <param name="binaryWriter"/>
    /// <exception cref="SerializationException"/>
    public void Serialize(BinaryWriter binaryWriter)
    {
        var context = new SerializationContext(binaryWriter);
        context.Write(this);
    }

    #endregion

    #region Constructors

    protected MachineBase(ILogger logger, int scanLines, int firstScanline, int fHZ, int soundSampleFreq, ReadOnlyMemory<uint> palette, int vPitch)
    {
        ArgumentException.ThrowIf(soundSampleFreq <= 0, "must be a positive integer", nameof(soundSampleFreq));

        Logger = logger;
        _Scanlines = scanLines;
        FirstScanline = firstScanline;
        FrameHZ = fHZ;
        SoundSampleFrequency = soundSampleFreq;
        Palette = palette;
        _VisiblePitch = vPitch;
        FrameBuffer = new(_VisiblePitch, _Scanlines);
    }

    #endregion

    #region Serialization Members

    protected MachineBase(DeserializationContext input, ReadOnlyMemory<uint> palette)
    {
        ArgumentException.ThrowIf(palette.Length != 0x100, "palette incorrect size, must be 256", nameof(palette));

        input.CheckVersion(1);
        MachineHalt = input.ReadBoolean();
        FrameHZ = input.ReadInt32();
        _VisiblePitch = input.ReadInt32();
        _Scanlines = input.ReadInt32();
        FirstScanline = input.ReadInt32();
        SoundSampleFrequency = input.ReadInt32();
        NOPRegisterDumping = input.ReadBoolean();
        InputState = input.ReadInputState();

        Palette = palette;
        FrameBuffer = new(_VisiblePitch, _Scanlines);
    }

    public virtual void GetObjectData(SerializationContext output)
    {
        output.WriteVersion(1);
        output.Write(MachineHalt);
        output.Write(FrameHZ);
        output.Write(_VisiblePitch);
        output.Write(_Scanlines);
        output.Write(FirstScanline);
        output.Write(SoundSampleFrequency);
        output.Write(NOPRegisterDumping);
        output.Write(InputState);
    }

    #endregion

    class MachineUnknown() : MachineBase(NullLogger.Default, 100, 1, 1, 1, ReadOnlyMemory<uint>.Empty, 1)
    {
    }
}