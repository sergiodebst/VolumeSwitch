using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//https://stackoverflow.com/a/13139478
namespace VolumeSwitch
{
    public static class VolumneManager
    {

        #region "New implementation"
        internal enum EDataFlow
        {
            /// <summary>
            /// Audio rendering stream. Audio data flows from the application to the audio endpoint device, which renders the stream.
            /// </summary>
            eRender,
            /// <summary>
            /// Audio capture stream. Audio data flows from the audio endpoint device that captures the stream, to the application.
            /// </summary>
            eCapture,
            /// <summary>
            /// Audio rendering or capture stream. Audio data can flow either from the application to the audio endpoint device, or from the audio endpoint device to the application.
            /// </summary>
            eAll,
            /// <summary>
            /// The number of members in the EDataFlow enumeration (not counting the EDataFlow_enum_count member).
            /// </summary>
            EDataFlow_enum_count
        }

        internal enum ERole
        {
            /// <summary>
            /// Games, system notification sounds, and voice commands.
            /// </summary>
            eConsole,
            /// <summary>
            /// Music, movies, narration, and live music recording.
            /// </summary>
            eMultimedia,
            /// <summary>
            /// Voice communications (talking to another person).
            /// </summary>
            eCommunications,
            /// <summary>
            /// The number of members in the ERole enumeration (not counting the ERole_enum_count member).
            /// </summary>
            ERole_enum_count
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            //Needed in orther to keep method index for GetDefaultAudioEndpoint in the COM object
            int DummyMethod();

            /// <summary>
            /// The GetDefaultAudioEndpoint method retrieves the default audio endpoint for the specified data-flow direction and role.
            /// </summary>
            /// <param name="dataFlow">The data-flow direction for the endpoint device.</param>
            /// <param name="role">The role of the endpoint device.</param>
            /// <param name="ppDevice">Pointer to a pointer variable into which the method writes the address of the IMMDevice interface of the endpoint object for the default audio endpoint device. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the GetDefaultAudioEndpoint call fails, *ppDevice is NULL.</param>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, possible return codes include, but are not limited to, the values shown in the following table. https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdeviceenumerator-getdefaultaudioendpoint#return-value</returns>
            [PreserveSig, DispId(2)]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

            //TODO: Implement the rest if needed
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            /// <summary>
            /// The Activate method creates a COM object with the specified interface.
            /// </summary>
            /// <param name="iid">The interface identifier. This parameter is a reference to a GUID that identifies the interface that the caller requests be activated. The caller will use this interface to communicate with the COM object.</param>
            /// <param name="dwClsCtx">The execution context in which the code that manages the newly created object will run. The caller can restrict the context by setting this parameter to the bitwise OR of one or more CLSCTX enumeration values. Alternatively, the client can avoid imposing any context restrictions by specifying CLSCTX_ALL. For more information about CLSCTX, see the Windows SDK documentation.</param>
            /// <param name="pActivationParams">Set to NULL to activate an IAudioClient, IAudioEndpointVolume, IAudioMeterInformation, IAudioSessionManager, or IDeviceTopology interface on an audio endpoint device. When activating an IBaseFilter, IDirectSound, IDirectSound8, IDirectSoundCapture, or IDirectSoundCapture8 interface on the device, the caller can specify a pointer to a PROPVARIANT structure that contains stream-initialization information. For more information, see Remarks.</param>
            /// <param name="ppInterface">Pointer to a pointer variable into which the method writes the address of the interface specified by parameter iid. Through this method, the caller obtains a counted reference to the interface. The caller is responsible for releasing the interface, when it is no longer needed, by calling the interface's Release method. If the Activate call fails, *ppInterface is NULL.</param>
            /// <returns>If the method succeeds, it returns S_OK. If it fails, possible return codes include, but are not limited to, the values shown in the following table. If the method succeeds, it returns S_OK. If it fails, possible return codes include, but are not limited to, the values shown in the following table.If the method succeeds, it returns S_OK. If it fails, possible return codes include, but are not limited to, the values shown in the following table.https://learn.microsoft.com/en-us/windows/win32/api/mmdeviceapi/nf-mmdeviceapi-immdevice-activate#return-value</returns>
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

            //TODO: Implement the rest if needed
        }

        /// <summary>
        /// IAudioEndpointVolumeCallback COM interface (defined in Endpointvolume.h).
        /// </summary>
        [ComImport]
        [Guid("657804FA-D6AD-4496-8A60-352752AF4F89")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioEndpointVolumeCallback
        {
            /// <summary>
            /// Notifies the client that the volume level or muting state of the audio endpoint device has changed.
            /// </summary>
            /// <param name="notify">Pointer to the volume-notification data.</param>
            void OnNotify(IntPtr notify);
        }
        /// <summary>
        /// IAudioEndpointVolume COM interface (defined in Endpointvolume.h).
        /// </summary>
        [ComImport]
        [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioEndpointVolume
        {
            /// <summary>
            /// Registers a client's notification callback interface.
            /// </summary>
            /// <param name="notify">The client's IAudioEndpointVolumeCallback interface.</param>
            /// <returns>An HRESULT return code.</returns>
            int RegisterControlChangeNotify(IAudioEndpointVolumeCallback notify);

            /// <summary>
            /// Deletes the registration of a client's notification callback interface.
            /// </summary>
            /// <param name="notify">The client's IAudioEndpointVolumeCallback interface.</param>
            /// <returns>An HRESULT return code.</returns>
            int UnregisterControlChangeNotify(IAudioEndpointVolumeCallback notify);

            /// <summary>
            /// Gets a count of the channels in the audio stream.
            /// </summary>
            /// <param name="channelCount">The channel count.</param>
            /// <returns>An HRESULT return code.</returns>
            int GetChannelCount(out int channelCount);

            /// <summary>
            /// Sets the master volume level of the audio stream, in decibels.
            /// </summary>
            /// <param name="levelDB">The new master volume level in decibels.</param>
            /// <param name="pguidEventContext">
            /// Context value for the IAudioEndpointVolumeCallback.OnNotify method.
            /// </param>
            /// <returns>An HRESULT return code.</returns>
            int SetMasterVolumeLevel(float levelDB, [In] ref Guid pguidEventContext);

            /// <summary>
            /// Sets the master volume level, expressed as a normalized, audio-tapered value.
            /// </summary>
            /// <param name="level">The new master volume level.</param>
            /// <param name="pguidEventContext">
            /// Context value for the IAudioEndpointVolumeCallback.OnNotify method.
            /// </param>
            /// <returns>An HRESULT return code.</returns>
            int SetMasterVolumeLevelScalar(float level, [In] ref Guid pguidEventContext);

            /// <summary>
            /// Gets the master volume level of the audio stream, in decibels.
            /// </summary>
            /// <returns>The master volume level in decibels.</returns>
            float GetMasterVolumeLevel();

            /// <summary>
            /// Gets the master volume level, expressed as a normalized, audio-tapered value.
            /// </summary>
            /// <returns>The master volume level.</returns>
            float GetMasterVolumeLevelScalar();

            /// <summary>
            /// Sets the volume level, in decibels, of the specified channel of the audio stream.
            /// </summary>
            /// <param name="channel">The channel number.</param>
            /// <param name="levelDB">The new volume level in decibels.</param>
            /// <param name="pguidEventContext">
            /// Context value for the IAudioEndpointVolumeCallback.OnNotify method.
            /// </param>
            /// <returns>An HRESULT return code.</returns>
            int SetChannelVolumeLevel(uint channel, float levelDB, [In] ref Guid pguidEventContext);

            /// <summary>
            /// Sets the normalized, audio-tapered volume level of the specified channel in the audio stream.
            /// </summary>
            /// <param name="channel">The channel number.</param>
            /// <param name="level">The new volume level.</param>
            /// <param name="pguidEventContext">
            /// Context value for the IAudioEndpointVolumeCallback.OnNotify method.
            /// </param>
            /// <returns>An HRESULT return code.</returns>
            int SetChannelVolumeLevelScalar(uint channel, float level, [In] ref Guid pguidEventContext);

            /// <summary>
            /// Gets the volume level, in decibels, of the specified channel in the audio stream.
            /// </summary>
            /// <param name="channel">The channel number.</param>
            /// <returns>The channel volume level in decibels.</returns>
            float GetChannelVolumeLevel(uint channel);

            /// <summary>
            /// Gets the normalized, audio-tapered volume level of the specified channel of the audio stream.
            /// </summary>
            /// <param name="channel">The channel number.</param>
            /// <returns>The channel volume level.</returns>
            float GetChannelVolumeLevelScalar(uint channel);

            /// <summary>
            /// Sets the muting state of the audio stream.
            /// </summary>
            /// <param name="mute">The new muting state.</param>
            /// <param name="pguidEventContext">
            /// Context value for the IAudioEndpointVolumeCallback.OnNotify method.
            /// </param>
            /// <returns>An HRESULT return code.</returns>
            int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, [In] ref Guid pguidEventContext);

            /// <summary>
            /// Gets the muting state of the audio stream.
            /// </summary>
            /// <returns>The muting state.</returns>
            bool GetMute();

            /// <summary>
            /// Gets information about the current step in the volume range.
            /// </summary>
            /// <param name="stepIndex">The current step index.</param>
            /// <param name="stepCount">The number of steps in the volume range.</param>
            /// <returns>An HRESULT return code.</returns>
            int GetVolumeStepInfo(out uint stepIndex, out uint stepCount);

            /// <summary>
            /// Increases the volume level by one step.
            /// </summary>
            /// <param name="pguidEventContext">
            /// Context value for the IAudioEndpointVolumeCallback.OnNotify method.
            /// </param>
            /// <returns>An HRESULT return code.</returns>
            int VolumeStepUp([In] ref Guid pguidEventContext);

            /// <summary>
            /// Decreases the volume level by one step.
            /// </summary>
            /// <param name="pguidEventContext">
            /// Context value for the IAudioEndpointVolumeCallback.OnNotify method.
            /// </param>
            /// <returns>An HRESULT return code.</returns>
            int VolumeStepDown([In] ref Guid pguidEventContext);

            /// <summary>
            /// Queries the audio endpoint device for its hardware-supported functions.
            /// </summary>
            /// <returns>
            /// A hardware support mask that indicates the hardware capabilities of the audio endpoint device.
            /// </returns>
            int QueryHardwareSupport();

            /// <summary>
            /// Gets the volume range of the audio stream, in decibels.
            /// </summary>
            /// <param name="pflVolumeMindB">The minimum volume level.</param>
            /// <param name="pflVolumeMaxdB">The maximum volume level.</param>
            /// <param name="pflVolumeIncrementdB">The volume increment.</param>
            /// <returns>An HRESULT return code.</returns>
            int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
        }

        private static bool GetVolumeIsMuted()
        {
            using (var speakers = GetAudioDevice(EDataFlow.eRender, ERole.eMultimedia))
                return speakers.IsMuted;
        }
        public static bool GetMicrophoneIsMuted()
        {
            using (var microphone = GetMicrophone())
                return microphone.IsMuted;
        }

        private static AudioDevice GetMicrophone()
        {
            return GetAudioDevice(EDataFlow.eCapture, ERole.eCommunications);
        }

        private static AudioDevice GetAudioDevice(EDataFlow flow, ERole role)
        {
            return new AudioDevice(flow, role);
        }

        private class AudioDevice : IDisposable
        {
            public bool IsMuted
            {
                get { return _aev.GetMute(); }
                set { _aev.SetMute(value, Guid.Empty); }
            }

            private IMMDeviceEnumerator _deviceEnumerator;
            private IMMDevice _device;
            private IAudioEndpointVolume _aev;
            public AudioDevice(EDataFlow flow, ERole role)
            {
                Type MMDeviceEnumeratorType = Type.GetTypeFromCLSID(new Guid("BCDE0395-E52F-467C-8E3D-C4579291692E"));
                _deviceEnumerator = (IMMDeviceEnumerator)(Activator.CreateInstance(MMDeviceEnumeratorType));
                _deviceEnumerator.GetDefaultAudioEndpoint(flow, role, out _device);

                Guid IID_IAudioSessionManager2 = typeof(IAudioEndpointVolume).GUID;
                object o;
                _device.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                _aev = (IAudioEndpointVolume)o;
            }


            public void Dispose()
            {
                Marshal.ReleaseComObject(_aev);
                Marshal.ReleaseComObject(_device);
                Marshal.ReleaseComObject(_deviceEnumerator);
            }
        }
        #endregion

        private const int APPCOMMAND_VOLUME_MUTE = 0x80000;
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int APPCOMMAND_MICROPHONE_VOLUME_MUTE = 0x180000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        public static void ToggleMicrophoneMuteState()
        {
            using (var microphone = GetMicrophone())
                microphone.IsMuted = !microphone.IsMuted;
        }

        public static void ToggleVolumeMuteState(bool toggleMic)
        {
            SendMessageW(App.Handler, WM_APPCOMMAND, App.Handler, (IntPtr)APPCOMMAND_VOLUME_MUTE);
            if (toggleMic && GetMicrophoneIsMuted() != GetVolumeIsMuted())
                ToggleMicrophoneMuteState();
        }

        public static void VolDown(bool toggleMic)
        {
            ChangeVolumeAndToggleMicIfNeeded(APPCOMMAND_VOLUME_DOWN, toggleMic);
        }

        public static void VolUp(bool toggleMic)
        {
            ChangeVolumeAndToggleMicIfNeeded(APPCOMMAND_VOLUME_UP, toggleMic);
        }
        private static void ChangeVolumeAndToggleMicIfNeeded(int changeType, bool toggleMic)
        {
            if (toggleMic && GetVolumeIsMuted() && GetMicrophoneIsMuted())
                ToggleMicrophoneMuteState();

            SendMessageW(App.Handler, WM_APPCOMMAND, App.Handler, (IntPtr)changeType);
        }

    }
}
