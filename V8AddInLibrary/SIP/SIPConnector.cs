using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.IO;
using System.Runtime.Serialization;
using Microsoft.Win32;
using V8.AddIn;
using System.Runtime.Serialization.Json;
using System.Diagnostics;

namespace TCX.CallTriggerCmd
{
    public class SIPConnector
    {
        public static ICallTriggerService Service;
        public static DuplexChannelFactory<ICallTriggerService> ChannelFactory;

        public static bool Inited;

        public SIPConnector()
        {
            InstanceName = "SIPConnector";

            try
            {
                InitService();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
        }

        static void InitService()
        {
            //try
            //{
            //    Debugger.Break();
            //}
            //catch (Exception exc)
            //{
            //    var err = exc.Message;
            //}

            if (Inited)
            {
                try
                {
                    Service.Subscribe();
                    return;
                } catch { }
            }

            var binding = new NetNamedPipeBinding();
            var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\3CX");
            var uri = key.GetValue("CallTriggerCmdUri");
            if (uri == null)
                throw new Exception("User specific 3CXPhone CallTrigger uri is not found");
             
            var address = new EndpointAddress(uri.ToString());

            ChannelFactory = new DuplexChannelFactory<ICallTriggerService>(
                new ServiceCallback(), binding, address);

            ChannelFactory.Closed += new EventHandler(Closed);
            ChannelFactory.Faulted += new EventHandler(Faulted);

            Service = ChannelFactory.CreateChannel();
            Service.Subscribe();

            Inited = true;
        }

        static void Closed(object sender, EventArgs e)
        {
            V8Context.CreateV8Context().AsyncEvent.ExternalEvent("Debug", "Closed", "Closed");
            Inited = false;
            InitService();
        }
        static void Faulted(object sender, EventArgs e)
        {
            V8Context.CreateV8Context().AsyncEvent.ExternalEvent("Debug", "Faulted", "Faulted");
            Inited = false;
            InitService();
        }

        static string convertToJSON(object source, Type type)
        {
            DataContractJsonSerializer jsonFormatter = new DataContractJsonSerializer(type);

            var ms = new MemoryStream();

            jsonFormatter.WriteObject(ms, source);

            string result = Encoding.UTF8.GetString((ms as MemoryStream).ToArray());

            return result;
        }

        //static object convertFromJSON(object source, Type type)
        //{
        //    User deserializedUser = new User();
        //    MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));
        //    DataContractJsonSerializer ser = new DataContractJsonSerializer(deserializedUser.GetType());
        //    deserializedUser = ser.ReadObject(ms) as User;
        //    ms.Close();
        //    return deserializedUser;
        //}

        static T convertFromJSON<T>(string source)
        {
            var ser = new DataContractJsonSerializer(typeof(T));
            var str = GenerateStreamFromString(source);
            var obj = (T)ser.ReadObject(str);

            return obj;
        }

        public static Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;

            return stream;
        }

        private static string normalize(string input)
        {
            // Strip letters for tel: protocol
            if (input.StartsWith("tel:"))
                input = input.Substring(4);

            StringBuilder sb = new StringBuilder();
            foreach (char c in input)
            {
                if (Char.IsLetter(c))
                {
                    switch (new string(c, 1).ToUpper())
                    {
                        case "A": // fall down
                        case "B": // fall down
                        case "C": sb.Append('2'); break;
                        case "D": // fall down
                        case "E": // fall down
                        case "F": sb.Append('3'); break;
                        case "G": // fall down
                        case "H": // fall down
                        case "I": sb.Append('4'); break;
                        case "J": // fall down
                        case "K": // fall down
                        case "L": sb.Append('5'); break;
                        case "M": // fall down
                        case "N": // fall down
                        case "O": sb.Append('6'); break;
                        case "P": // fall down
                        case "Q": // fall down
                        case "R": // fall down
                        case "S": sb.Append('7'); break;
                        case "T": // fall down
                        case "U": // fall down
                        case "V": sb.Append('8'); break;
                        case "W": // fall down
                        case "X": // fall down
                        case "Y": // fall down
                        case "Z": sb.Append('9'); break;
                    }
                }
                else if (Char.IsDigit(c) || c == '+' || c == '#' || c == '*')
                    sb.Append(c);
            }
            return sb.ToString();
        }

        [Alias("Инициализировано")]
        public bool InitedState()
        {
            return Inited;
        }

        [Alias("Инициализировать")]
        public void Init()
        {
            //try
            //{
            //    

            //}
            //catch (Exception exc)
            //{
            //    var err = exc.Message;
            //}

            InitService(); 
        }

        [Alias("ИмяЭкземпляра")]
        public string InstanceName { get; private set; }

        [Alias("Позвонить")]
        public object MakeCall(string Distantion, bool Video)
        {
            try
            {
                return _MakeCall(Distantion, Video);
            }
            catch
            {
                InitService();
                return _MakeCall(Distantion, Video);
            }
        }

        public string _MakeCall(string Distantion, bool Video)
        {
            CallStatus callStatus;

            if (!Video)
            {
                callStatus = Service.MakeCall(Distantion);
            }
            else
            {
                callStatus = Service.MakeCallEx(Distantion, MakeCallOptions.WithVideo);
            }

            return convertToJSON(callStatus, typeof(CallStatus));
        }

        [Alias("Завершить")]
        public void DropCall(string callId)
        {
            try
            {
                _DropCall(callId);
            }
            catch
            {
                InitService();
                _DropCall(callId);
            }
        }

        public void _DropCall(string callId)
        {
            Service.DropCall(callId);
        }

        [Alias("Удержать")]
        public void Hold(string callId, bool holdOn)
        {
            try
            {
                _Hold(callId, holdOn);
            }
            catch
            {
                InitService();
                _Hold(callId, holdOn);
            }
        }

        public void _Hold(string callId, bool holdOn)
        {
            Service.Hold(callId, holdOn);
        }

        [Alias("ПеревестиКонсультативно")]
        public object BeginTransfer(string callId, string destination)
        {
            try
            {
                return _BeginTransfer(callId, destination);
            }
            catch
            {
                InitService();
                return _BeginTransfer(callId, destination);
            }
        }

        public string _BeginTransfer(string callId, string destination)
        {
            CallStatus callStatus = Service.BeginTransfer(callId, destination);

            return convertToJSON(callStatus, typeof(CallStatus));
        }

        [Alias("ПеревестиВСлепую")]
        public void BlindTransfer(string callId, string destination)
        {
            try
            {
                _BlindTransfer(callId, destination);
            }
            catch
            {
                InitService();
                _BlindTransfer(callId, destination);
            }
        }

        public void _BlindTransfer(string callId, string destination)
        {
            Service.BlindTransfer(callId, destination);
        }

        [Alias("Ответить")]
        public void Activate(string callId, bool Video)
        {
            try
            {
                _Activate(callId, Video);
            }
            catch
            {
                InitService();
                _Activate(callId, Video);
            }
        }

        public void _Activate(string callId, bool Video)
        {
            if (!Video)
            {
                Service.Activate(callId);
            }
            else
            {
                Service.ActivateEx(callId, ActivateOptions.WithVideo);
            }
        }

        [Alias("ОтменитьПеревод")]
        public void CancelTransfer(string callId)
        {
            try
            {
                _CancelTransfer(callId);
            }
            catch
            {
                InitService();
                _CancelTransfer(callId);
            }
        }

        public void _CancelTransfer(string callId)
        {
            Service.CancelTransfer(callId);
        }
         
        [Alias("ЗавершитьПеревод")]
        public void CompleteTransfer(string callId)
        {
            try
            {
                _CompleteTransfer(callId);
            }
            catch
            {
                InitService();
                _CompleteTransfer(callId);
            }
        }

        public void _CompleteTransfer(string callId)
        {
            Service.CompleteTransfer(callId);
        }

        [Alias("Микрофон")]
        public void Mute(string callId)
        {
            try
            {
                _Mute(callId);
            }
            catch
            {
                InitService();
                _Mute(callId);
            }
        }

        public void _Mute(string callId)
        {
            Service.Mute(callId);
        }

        [Alias("ОтправитьDTMF")]
        public void SendDTMF(string callId, string dtmf)
        {
            try
            {
                _SendDTMF(callId, dtmf);
            }
            catch
            {
                InitService();
                _SendDTMF(callId, dtmf);
            }
        }

        public void _SendDTMF(string callId, string dtmf)
        {
            Service.SendDTMF(callId, dtmf);
        }

        [Alias("УстановитьАктивныйПрофиль")]
        public void SetActiveProfile(int profileId)
        {
            try
            {
                _SetActiveProfile(profileId);
            }
            catch
            {
                InitService();
                _SetActiveProfile(profileId);
            }
        }

        public void _SetActiveProfile(int profileId)
        {
            Service.SetActiveProfile(profileId);
        }

        [Alias("УстановитьРасширенныйСтатусПрофиля")]
        public void SetProfileExtendedStatus(int profileId, string status)
        {
            try
            {
                _SetProfileExtendedStatus(profileId, status);
            }
            catch
            {
                InitService();
                _SetProfileExtendedStatus(profileId, status);
            }
        }

        public void _SetProfileExtendedStatus(int profileId, string status)
        {
            Service.SetProfileExtendedStatus(profileId, status);
        }

        [Alias("УстановитьСтатусАвторизации")]
        public void SetQueueLoginStatus(bool loggedIn)
        {
            try
            {
                _SetQueueLoginStatus(loggedIn);
            }
            catch
            {
                InitService();
                _SetQueueLoginStatus(loggedIn);
            }
        }

        public void _SetQueueLoginStatus(bool loggedIn)
        {
            Service.SetQueueLoginStatus(loggedIn);
        }

        [Alias("АктивныеВызовы")]
        public object ActiveCalls()
        {
            try
            {
                return _ActiveCalls();
            }
            catch
            {
                InitService();
                return _ActiveCalls();
            }
        }

        public string _ActiveCalls()
        {
            return convertToJSON(Service.ActiveCalls, typeof(List<CallStatus>));
        }

        [Alias("ПрофилиПользователей")]
        public object Profiles()
        {
            try
            {
                return _Profiles();
            }
            catch
            {
                InitService();
                return _Profiles();
            }
        }

        public string _Profiles()
        {
            return convertToJSON(Service.Profiles, typeof(List<UserProfileStatus>));
        }

        [Alias("ПрофилиПользователейРасширенный")]
        public object ProfilesEx()
        {
            try
            {
                return _ProfilesEx();
            }
            catch
            {
                InitService();
                return _ProfilesEx();
            }
        }

        public object _ProfilesEx()
        {
            return convertToJSON(Service.ProfilesEx(), typeof(CallHandler));
        }

        [Alias("Статус")]
        public object Status()
        {
            try
            {
                return _Status();
            }
            catch
            {
                InitService();
                return _Status();
            }
        }

        public object _Status()
        {
            return convertToJSON(Service.Status(), typeof(String));
        }

        [Alias("ЗагрузитьФайлыЗаписей")]
        public void LoadRecordingFiles(string recordsRequest)
        {
            try
            {
                _LoadRecordingFiles(recordsRequest);
            }
            catch
            {
                InitService();
                _LoadRecordingFiles(recordsRequest);
            }
        }

        public void _LoadRecordingFiles(string recordsRequest)
        {
            var tmp = convertFromJSON<RecordsRequest>(recordsRequest);
            Service.LoadRecordingFiles(tmp);
        }

        [Alias("ПолучитьСписокЗаписей")]
        public object GetRecordingList()
        {
            try
            {
                return _GetRecordingList();
            }
            catch
            {
                InitService();
                return _GetRecordingList();
            }
        }

        public object _GetRecordingList()
        {
            return convertToJSON(Service.GetRecordingList(), typeof(List<Record>));
        }

        [Alias("ПолучитьВерсиюКомпоненты")]
        public object GetVersionComponent()
        {
            string verPlugin;
            string verCOM = "2.2";

            try
            {
                verPlugin = _GetVersionComponent();
            }
            catch
            {
                InitService();
                verPlugin = _GetVersionComponent();
            }

            if (verPlugin == verCOM)
            {
                return verPlugin;
            }
            else
            {
                return "0";
            }
        }

        public string _GetVersionComponent()
        {
            return Service.GetVersionComponent();
        }

        class ServiceCallback : IClientCallback
        {

            public void CurrentProfileChanged(CurrentProfileChanged currentProfileChanged)
            {
                V8Context.CreateV8Context().AsyncEvent.ExternalEvent("3CXPhonePlugin", "CurrentProfileChanged", convertToJSON(currentProfileChanged, typeof(CurrentProfileChanged)));
            }

            public void ProfileExtendedStatusChanged(ProfileExtendedStatusChanged profileExtendedStatusChanged)
            {
                V8Context.CreateV8Context().AsyncEvent.ExternalEvent("3CXPhonePlugin", "ProfileExtendedStatusChanged", convertToJSON(profileExtendedStatusChanged, typeof(ProfileExtendedStatusChanged)));
            }

            public void CallStatusChanged(OnCallStatusChanged onCallStatusChanged)
            {
                V8Context.CreateV8Context().AsyncEvent.ExternalEvent("3CXPhonePlugin", "OnCallStatusChanged", convertToJSON(onCallStatusChanged, typeof(OnCallStatusChanged)));
            }

            public void MyPhoneStatusChanged(OnMyPhoneStatusChanged onMyPhoneStatusChanged)
            {
                V8Context.CreateV8Context().AsyncEvent.ExternalEvent("3CXPhonePlugin", "OnMyPhoneStatusChanged", convertToJSON(onMyPhoneStatusChanged, typeof(OnMyPhoneStatusChanged)));
            }
        };
    }
}
