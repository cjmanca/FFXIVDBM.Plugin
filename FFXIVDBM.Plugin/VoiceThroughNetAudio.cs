//using FFXIVAPP.Common;
using NAudio.Wave;
using System;
using System.IO;
public class VoiceThroughNetAudio : IDisposable
{
    #region USED to Implement IDisposable
    private bool disposed = false;
    public string returnStatus { get; set; }

    public MemoryStream m_memStream = null;
    //Implement IDisposable.
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);  // GC == Garbage Collector
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                // Free other state (managed objects).
                returnStatus = null;
                if (_waveOutDevice != null)
                {
                    _waveOutDevice.Stop();
                }
                if (_volumeStream != null)
                {
                    _volumeStream.Close();
                    _volumeStream.Dispose();
                    _volumeStream = null;
                }
                if (_mainOutputStream != null)
                {
                    _mainOutputStream.Close();
                    _mainOutputStream.Dispose();
                    _mainOutputStream = null;
                }
                if (_waveOutDevice != null)
                {
                    _waveOutDevice.Dispose();
                    _waveOutDevice = null;
                }

                if (m_memStream != null)
                {
                    m_memStream.Close();
                    m_memStream.Dispose();
                    m_memStream = null;
                }
                GC.Collect();
                _IsDispose = true;
            }
            // Free your own state (unmanaged objects).
            // Set large fields to null.


            disposed = true;
        }
    }
    ~VoiceThroughNetAudio()
    {
        // Simply call Dispose(false).
        Dispose(false);
    }
    #endregion
    #region -- Declarations --
    public enum PlayBackState    // State of player
    {
        Stopped = 0,
        Playing = 1,
        Paused = 2,
    }
    public event ErrorOccuredHandle Error;     // Manage  error events
    public delegate void ErrorOccuredHandle(string FunctionName, string ErrorMessage, string ErrorStach);

    public event PlayStopHandle PlaybackStop;           // stop execution
    public delegate void PlayStopHandle(EventArgs e);

    private IWavePlayer _waveOutDevice;       // player NAudio 
    private WaveStream _mainOutputStream;     // Stream audio samples using NAudio
    private WaveChannel32 _volumeStream;      // Management properties of sound (volume, balance etc...)


    private bool _IsDispose = false;

    #endregion
    #region -- Properties --
    /// <summary>
    /// durata totale del brano 
    /// </summary>
    public TimeSpan TotalDuration
    {
        get
        {
            if (_mainOutputStream != null)
                return _mainOutputStream.TotalTime;
            else
                return new TimeSpan();
        }
    }
    /// <summary>
    /// total duration of the song format string MM: SS
    /// </summary>
    public string TotalTime
    {
        get
        {
            if (_mainOutputStream != null)
                return String.Format("{0:00}:{1:00}", (int)_mainOutputStream.TotalTime.TotalMinutes, _mainOutputStream.TotalTime.Seconds);
            else
                return "00:00";
        }
    }
    #endregion
    #region  -- Controls --
    /// <summary>
    /// Execution time of the current track
    /// </summary>
    public TimeSpan TimePosition
    {
        get
        {
            if (_mainOutputStream == null) return TimeSpan.Zero;

            return _mainOutputStream.CurrentTime;
        }
        set { _mainOutputStream.CurrentTime = value; }

    }
    /// <summary>
    /// Pause
    /// </summary>
    public void Pause()
    {
        if (_waveOutDevice == null) return;
        _waveOutDevice.Pause();
    }
    /// <summary>
    /// Play
    /// </summary>
    public void Play()
    {

        if (_waveOutDevice == null) return;

        if (_waveOutDevice.PlaybackState == PlaybackState.Playing)
        {
            return;
        }

        _waveOutDevice.Play();

    }
    /// <summary>
    /// Stop
    /// </summary>
    public void Stop()
    {
        _waveOutDevice.Stop();
    }
    /// <summary>
    /// Volume Values minimum 1.0 maximum 0.0
    /// </summary>
    public float Volume
    {
        get { return _volumeStream.Volume; }
        set
        {
            if (_volumeStream != null)
                _volumeStream.Volume = value;
        }

    }
    /// <summary>
    /// Selecting audio channel L = -1.0 R = 1.0
    /// </summary>
    public float Pan
    {
        get { return _volumeStream.Pan; }
        set
        {
            if (_volumeStream != null)
                _volumeStream.Pan = value;
        }
    }
    /// <summary>
    /// condition 
    /// </summary>
    public VoiceThroughNetAudio.PlayBackState State
    {
        get
        {
            if (_waveOutDevice != null)
                return (PlayBackState)_waveOutDevice.PlaybackState;
            else
                return VoiceThroughNetAudio.PlayBackState.Stopped;
        }
    }
    #endregion
    #region -- INTERNAL --
    public enum soundFILETypes
    {
        WAVE,
        MP3,
        AIFF
    };
    public VoiceThroughNetAudio(string FileName, Guid outputDevice)
    {
        if (!System.IO.File.Exists(FileName)) return;

        // Creating the interface class 
        //_waveOutDevice = new WaveOut();

        if (outputDevice == Guid.Empty)
        {
            _waveOutDevice = new DirectSoundOut(100);
        }
        else
        {
            LastAudioDevice = outputDevice;
            _waveOutDevice = new DirectSoundOut(outputDevice, 100);
        }

        try
        {
            // creation of Stream input from the given file
            _mainOutputStream = CreateInputStream(FileName);

            if (_mainOutputStream == null)
            {
                throw new InvalidOperationException("Unsupported file extension");
            }

        }
        catch (Exception createException)
        {
            if (Error != null)
                Error("Audio - Play - CreateInputStream", createException.Message, createException.StackTrace);
            return;
        }

        try
        {
            //Initialization
            _waveOutDevice.Init(_mainOutputStream);
        }
        catch (Exception initException)
        {
            if (Error != null)
                Error("Audio - Play - Init", initException.Message, initException.StackTrace);
            return;
        }

        // The event will stop execution --  hooked to that of class NAudio
        _waveOutDevice.PlaybackStopped += new EventHandler<StoppedEventArgs>(_waveOutDevice_PlaybackStopped);

        return;

    }

    private Guid LastAudioDevice { get; set; }

    public VoiceThroughNetAudio(MemoryStream memStream, string typAsString, Guid outputDevice)
    {
        m_memStream = memStream;

        soundFILETypes typ = soundFILETypes.WAVE;
        if (typAsString.ToUpper() == "MP3") typ = soundFILETypes.MP3;
        if (typAsString.ToUpper() == "WAV") typ = soundFILETypes.WAVE;
        if (typAsString.ToUpper() == "AIFF") typ = soundFILETypes.AIFF;
        // Creating the interface class 
        //_waveOutDevice = new WaveOut();

        if (outputDevice == Guid.Empty)
        {
            _waveOutDevice = new DirectSoundOut(100);
        }
        else
        {
            LastAudioDevice = outputDevice;
            _waveOutDevice = new DirectSoundOut(outputDevice, 100);
        }

        try
        {
            // creation of Stream input from the given file
            _mainOutputStream = CreateInputStream(memStream, typ);

            if (_mainOutputStream == null)
            {
                throw new InvalidOperationException("Unsupported file extension");
            }

        }
        catch (Exception createException)
        {
            if (Error != null)
                Error("Audio - Play - CreateInputStream", createException.Message, createException.StackTrace);
            return;
        }

        try
        {
            //Initialization
            _waveOutDevice.Init(_mainOutputStream);
        }
        catch (Exception initException)
        {
            if (Error != null)
                Error("Audio - Play - Init", initException.Message, initException.StackTrace);
            return;
        }

        // The event will stop execution --  hooked to that of class NAudio
        _waveOutDevice.PlaybackStopped += new EventHandler<StoppedEventArgs>(_waveOutDevice_PlaybackStopped);

        return;

    }
    private void _waveOutDevice_PlaybackStopped(object sender, EventArgs e)
    {
        // class will handle the event that is generated
        if (PlaybackStop != null)
            PlaybackStop(e);
    }

    // Creating the Stream Input from FileName
    private WaveStream CreateInputStream(string fileName)
    {
        WaveStream Reader = CreateReaderStream(fileName);

        if (Reader == null)
        {
            throw new InvalidOperationException("Unsupported extension");
        }
        // class is created. WaveChannel32 attached to the stream to control the execution
        _volumeStream = new WaveChannel32(Reader);
        return _volumeStream;
    }
    // Creating the Stream Input from MemoryStream
    private WaveStream CreateInputStream(MemoryStream memStream, soundFILETypes typ)
    {
        WaveStream Reader = CreateReaderStream(memStream, typ);

        if (Reader == null)
        {
            throw new InvalidOperationException("Unsupported extension");
        }
        // class is created. WaveChannel32 attached to the stream to control the execution
        _volumeStream = new WaveChannel32(Reader);
        return _volumeStream;
    }


    // Creation of the stream reading stream based on passed soundFILETypes
    private WaveStream CreateReaderStream(MemoryStream memStream, soundFILETypes typ)
    {
        WaveStream readerStream = null;
        if (typ == soundFILETypes.WAVE)
        {
            readerStream = new WaveFileReader(memStream);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
        }
        else if (typ == soundFILETypes.MP3)
        {
            readerStream = new WaveFileReader(memStream);
        }
        else if (typ == soundFILETypes.AIFF)
        {
            readerStream = new WaveFileReader(memStream);
        }
        return readerStream;
    }
    // Creation of the stream reading stream based on passed Filename

    private WaveStream CreateReaderStream(string fileName)
    {
        WaveStream readerStream = null;
        if (fileName.EndsWith(".wav", StringComparison.OrdinalIgnoreCase))
        {
            readerStream = new WaveFileReader(fileName);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
        }
        else if (fileName.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase))
        {
            readerStream = new Mp3FileReader(fileName);
        }
        else if (fileName.EndsWith(".aiff"))
        {
            readerStream = new AiffFileReader(fileName);
        }
        return readerStream;
    }
    #endregion
}