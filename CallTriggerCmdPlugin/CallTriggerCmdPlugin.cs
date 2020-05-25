using System;
using System.Linq;
using MyPhonePlugins;
using System.Collections.Generic;
using System.ServiceModel;
using Microsoft.Win32;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using _3CXWin8Phone;
using _3CXWin8Phone.ViewModel;
using MyPhoneClientChannelNet.PublicAPI;
using System.Collections.ObjectModel;
using MyPhone.Notifications;
using ProtoBuf;
using System.IO;
using System.Net;

namespace TCX.CallTriggerCmd
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Multiple, 
                     InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext=false)]
    [ErrorHandlingBehavior]
    class CallTriggerCmdPlugin : ICallTriggerService, IDisposable
    {
        static readonly Dictionary<Views, MyPhonePlugins.Views> _pluginViewToView = new Dictionary<Views, MyPhonePlugins.Views>() {
            { Views.CallHistory, MyPhonePlugins.Views.CallHistory},
            { Views.Chats, MyPhonePlugins.Views.Chats},
            { Views.Conferences, MyPhonePlugins.Views.Conferences},
            { Views.DialPad, MyPhonePlugins.Views.DialPad},
            { Views.Contacts, MyPhonePlugins.Views.Contacts},
            { Views.Presence, MyPhonePlugins.Views.Presence},
            { Views.Voicemails, MyPhonePlugins.Views.Voicemails},
        };

        readonly List<IClientCallback> _callbackChannels = new List<IClientCallback>();
        IMyPhoneCallHandler callHandler;
        IExtensionInfo extensionInfo;
        ServiceHost _serviceHost;

        static ObservableCollection<IRecordingInfo> RecordsList = new ObservableCollection<IRecordingInfo>();
        static List<string> RecordsListConfirm = new List<string>();

        public List<CallStatus> ActiveCalls 
        { 
            get
            {
                try
                {
                    List<CallStatus> activeCall = new List<CallStatus>();

                    foreach (var call in callHandler.ActiveCalls)
                    {
                        var newCall = new CallStatus();

                        fillCallStatus(call, newCall);

                        activeCall.Add(newCall);
                    }

                    return activeCall;
                }
                catch
                {
                    return null;
                }
            }
        }

        private CallState ConvertCallState(MyPhonePlugins.CallState state)
        {
            switch(state)
            {
                case MyPhonePlugins.CallState.Connected:
                    return CallState.Connected;
                case MyPhonePlugins.CallState.Dialing:
                    return CallState.Dialing;
                case MyPhonePlugins.CallState.Ended:
                    return CallState.Ended;
                case MyPhonePlugins.CallState.Ringing:
                    return CallState.Ringing;
                case MyPhonePlugins.CallState.TryingToTransfer:
                    return CallState.TryingToTransfer;
                case MyPhonePlugins.CallState.Undefined:
                    return CallState.Undefined;
                case MyPhonePlugins.CallState.WaitingForNewParty:
                    return CallState.WaitingForNewParty;
                default:
                    return CallState.Undefined;
            }
        }

        public List<UserProfileStatus> Profiles 
        {
            get
            {
                try
                {
                    return callHandler.Profiles.Select(x => new UserProfileStatus()
                    {
                        IsActive = x.IsActive,
                        ProfileId = x.ProfileId,
                        CustomName = x.CustomName,
                        Name = x.Name,
                        ExtendedStatus = x.ExtendedStatus
                    }).ToList();
                }
                catch
                {
                    return null;
                }
            }
        }

        public CallHandler ProfilesEx()
        {
            try
            {
                var _сallHandler = new CallHandler();

                foreach (var call in callHandler.ActiveCalls)
                {
                    var newCall = new CallStatus();
                    fillCallStatus(call, newCall);
                    _сallHandler.ActiveCalls.Add(newCall);
                }

                foreach (var profile in callHandler.Profiles)
                {
                    var newProfile = new UserProfileStatus();
                    fillProfiles(profile, newProfile);
                    _сallHandler.Profiles.Add(newProfile);
                }

                fillCallHandler_1(callHandler, _сallHandler);

                return _сallHandler;
            }
            catch
            {
                return null;
            }
        }

        public CallTriggerCmdPlugin(MyPhonePlugins.IMyPhoneCallHandler callHandler)
        {
            try
            {
                this.callHandler = callHandler;
                callHandler.OnCallStatusChanged += callHandler_OnCallStatusChanged;
                callHandler.OnMyPhoneStatusChanged += callHandler_OnMyPhoneStatusChanged;
                callHandler.CurrentProfileChanged += callHandler_CurrentProfileChanged;
                callHandler.ProfileExtendedStatusChanged += callHandler_ProfileExtendedStatusChanged;

            }
            catch(Exception exception)
            {
                Dispose();
                throw exception;
            }
        }

        public void StartServiceAsync()
        {
            Task.Factory.StartNew(() =>
            {
                try
                {
                    var binding = new NetNamedPipeBinding();
                    var baseAddress = new Uri(GetUserSpecificUri());

                    _serviceHost = new ServiceHost(this, baseAddress);
                    _serviceHost.AddServiceEndpoint(typeof(ICallTriggerService), binding, baseAddress);
                    _serviceHost.Open();
                }
                catch
                {
                    // Exception opening named pipe
                }
            }, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Default);
        }

        static string GetUserSpecificUri()
        {
            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\3CX"))
            {
                var registryValue = key.GetValue("CallTriggerCmdUri");
                if (registryValue != null)
                    return registryValue.ToString();

                var uri = "net.pipe://localhost/CallTriggerService/" + Guid.NewGuid().ToString();
                key.SetValue("CallTriggerCmdUri", uri, RegistryValueKind.String);
                return uri;
            }
        }

        public void Dispose()
        {
            if (_serviceHost != null)
            {
                _serviceHost.Close();
                _serviceHost = null;
            }
            callHandler.CurrentProfileChanged -= callHandler_CurrentProfileChanged;
            callHandler.ProfileExtendedStatusChanged -= callHandler_ProfileExtendedStatusChanged;
            callHandler.OnCallStatusChanged -= callHandler_OnCallStatusChanged;
            callHandler.OnMyPhoneStatusChanged -= callHandler_OnMyPhoneStatusChanged;
        }

        public void Subscribe()
        { 
            lock(_callbackChannels)
            {
                var channel = OperationContext.Current.GetCallbackChannel<IClientCallback>();
                if (!_callbackChannels.Contains(channel)) //if CallbackChannels not contain current one.
                    _callbackChannels.Add(channel);
            }
        }

        public void Unsubscribe()
        {
            lock (_callbackChannels)
            {
                var channel = OperationContext.Current.GetCallbackChannel<IClientCallback>();
                _callbackChannels.Remove(channel);
            }
        }

        public bool isInited = false;

        public CallStatus MakeCallEx(string destination, MakeCallOptions options)
        {
            try
            {
                var status = callHandler.MakeCall(destination, ConvertMakeCallOptions(options));
                if (status != null)
                {
                    var callStatus = new CallStatus();

                    fillCallStatus(status, callStatus);

                    return callStatus;
                }
            }
            catch (Exception exc)
            {
                LogHelper.Log(Environment.SpecialFolder.ApplicationData, "CallTriggerCmd.log",
                    "Error executing callHandler.MakeCall(" + destination + "): " + exc.ToString());
            }

            return null;
        }

        public ObservableCollection<IRecordingInfo> GetRecordsList()
        {
            IMyPhoneClientChannel myPhoneClientChannel = App.Channel as IMyPhoneClientChannel;

            if (myPhoneClientChannel != null && myPhoneClientChannel.IsAuthenticated)
            {
                ObservableCollection<IRecordingInfo> observableCollection = null;
                RequestGetFolder request = new RequestGetFolder
                {
                    Folder = UsersFolder.RecordingFolder
                };

                IExtensible extensible = myPhoneClientChannel.Request(request);

                if (extensible is ResponseGetFolder)
                {
                    observableCollection = new ObservableCollection<IRecordingInfo>();

                    foreach (string file in (extensible as ResponseGetFolder).Files)
                    {
                        observableCollection.Add(new RecordingInfo(file));
                    }

                    observableCollection = new ObservableCollection<IRecordingInfo>(observableCollection.OrderByDescending(i => i.RecordedDate));

                    return observableCollection;
                }
            }

            return null;
        }

        private static MyPhonePlugins.MakeCallOptions ConvertMakeCallOptions(MakeCallOptions options)
        {
            if ((options & MakeCallOptions.WithVideo) != 0)
                return MyPhonePlugins.MakeCallOptions.WithVideo;
            else
                return MyPhonePlugins.MakeCallOptions.None;
        }

        public CallStatus MakeCall(string destination)
        {
            return MakeCallEx(destination, MakeCallOptions.None);
        }

        public void DropCall(string callId)
        {
            callHandler.DropCall(callId);
        }

        public void BlindTransfer(string callId, string destination)
        {
            callHandler.BlindTransfer(callId, destination);
        }

        public CallStatus BeginTransfer(string callId, string destination)
        {
            try
            {
                var status = callHandler.BeginTransfer(callId, destination);

                if (status != null)
                {
                    var callStatus = new CallStatus();

                    fillCallStatus(status, callStatus);

                    return callStatus;
                }

                return null; // (status != null) ? status.CallID : String.Empty;
            }
            catch
            {
                return null;
            }
        }

        public void CancelTransfer(string callId)
        {
            callHandler.CancelTransfer(callId);
        }

        public void CompleteTransfer(string callId)
        {
            callHandler.CompleteTransfer(callId);
        }

        public void Activate(string callId)
        {
            ActivateEx(callId, ActivateOptions.None);
        }

        public void ActivateEx(string callId, ActivateOptions options)
        {
            callHandler.ActivateEx(callId, ConvertActivateOptions(options));
        }

        private MyPhonePlugins.ActivateOptions ConvertActivateOptions(ActivateOptions options)
        {
            if ((options & ActivateOptions.WithVideo) != 0)
                return MyPhonePlugins.ActivateOptions.WithVideo;
            else
                return MyPhonePlugins.ActivateOptions.None;
        }

        private void callHandler_OnMyPhoneStatusChanged(object sender, MyPhonePlugins.MyPhoneStatus status)
        {
            if (status == MyPhonePlugins.MyPhoneStatus.LoggedIn)
                this.extensionInfo = sender as MyPhonePlugins.IExtensionInfo;

            var onMyPhoneStatusChanged = new OnMyPhoneStatusChanged();

            try
            {
                var _sender = sender as MyPhonePlugins.IMyPhoneCallHandler;

                fillCallHandler_1(_sender, onMyPhoneStatusChanged.CallHandler);

                onMyPhoneStatusChanged.Status = status.ToString();

                foreach (var call in _sender.ActiveCalls)
                {
                    var newCall = new CallStatus();
                    fillCallStatus(call, newCall);
                    onMyPhoneStatusChanged.CallHandler.ActiveCalls.Add(newCall);
                }

                foreach (var profile in _sender.Profiles)
                {
                    var newProfile = new UserProfileStatus();
                    fillProfiles(profile, newProfile);
                    onMyPhoneStatusChanged.CallHandler.Profiles.Add(newProfile);
                }
            }
            catch
            {
                onMyPhoneStatusChanged = null;
            }

            LogHelper.Log(Environment.SpecialFolder.ApplicationData, "CallTriggerCmd.log", 
                String.Format("MyPhoneStatusChanged - Status='{0}' - Extension='{1}'", status, extensionInfo == null ? String.Empty : extensionInfo.Number));
            Callback(channel => channel.MyPhoneStatusChanged(onMyPhoneStatusChanged));
        }

        private void callHandler_OnCallStatusChanged(object sender, MyPhonePlugins.CallStatus callInfo)
        {
            LogHelper.Log(Environment.SpecialFolder.ApplicationData, "CallTriggerCmd.log", 
                String.Format("CallStatusChanged - CallID='{0}' - Incoming='{1}' - OtherPartyNumber='{2}' - State='{3}'", callInfo.CallID, callInfo.Incoming, callInfo.OtherPartyNumber, callInfo.State));

            var onCallStatusChanged = new OnCallStatusChanged();

            try
            {
                var _sender = sender as MyPhonePlugins.IMyPhoneCallHandler;

                fillCallHandler_1(_sender, onCallStatusChanged.CallHandler);

                fillCallStatus(callInfo, onCallStatusChanged.CallStatus);

                foreach (var call in _sender.ActiveCalls)
                {
                    var newCall = new CallStatus();
                    fillCallStatus(call, newCall);
                    onCallStatusChanged.CallHandler.ActiveCalls.Add(newCall);
                }

                foreach (var profile in _sender.Profiles)
                {
                    var newProfile = new UserProfileStatus();
                    fillProfiles(profile, newProfile);
                    onCallStatusChanged.CallHandler.Profiles.Add(newProfile);
                }
            }
            catch
            {
                onCallStatusChanged = null;
            }

            Callback(channel => channel.CallStatusChanged(onCallStatusChanged));
        }

        public string GetPart(string str, char start, char end)
        {
            var ind = str.IndexOf(start);

            if (ind != -1)
            {
                var len = str.IndexOf(end, ind + 1);

                if (len != -1)
                {
                    var id = str.Substring(ind + 1, len - ind - 1);

                    return id;
                }
            }

            return "";
        }

        void fillCallStatus(MyPhonePlugins.CallStatus source, CallStatus target)
        {
            target.CallID = source.CallID;
            target.Incoming = source.Incoming;
            target.IsHold = (bool)getValueFildDynamicObjectByName(source, "IsHold");
            target.IsMuted = (bool)getValueFildDynamicObjectByName(source, "IsMuted");
            target.Originator = source.Originator;
            target.OriginatorName = source.OriginatorName;
            target.OriginatorType = source.OriginatorType.ToString();
            target.OtherPartyName = source.OtherPartyName;
            target.OtherPartyNumber = source.OtherPartyNumber;
            target.State = source.State.ToString();
            target.Tag3cx = (string)getValueFildDynamicObjectByName(source, "tag3cx");

            //if (target.State == "Connected")
            //{
            //    Thread.Sleep(150);
            //}




            //var debugCount = 0;

            //if (target.State == "Connected")
            //{
            //    var sw = Stopwatch.StartNew();

            //    while (true)
            //    {
            //        debugCount++;

            //        if (RecordsList != null)
            //        {
            //            var callID = GetPart(":" + target.CallID, ':', ':');

            //            for (var i = 0; i < RecordsList.Count; i++)
            //            {
            //                var recID = GetPart(RecordsList[i].FileName, '(', ')');

            //                if (callID == recID)
            //                {
            //                    target.DisplayDate = RecordsList[i].DisplayDate;
            //                    target.FileName = RecordsList[i].FileName;

            //                    break;
            //                }
            //            }
            //        }                    

            //        var ms = sw.ElapsedMilliseconds;

            //        if (ms > 100)
            //        {
            //            break;
            //        }

            //        Thread.Yield();
            //    }
            //}




            //var recordsList = GetRecordsList();

            if (target.State == "Connected")
            {
                SearchRecordFile(target);

                if (target.FileName == null
                    && !RecordsListConfirm.Contains(target.CallID + target.OtherPartyNumber))
                {
                    Thread.Sleep(150);
                    RecordsList = GetRecordsList();

                    RecordsListConfirm.Add(target.CallID + target.OtherPartyNumber);

                    SearchRecordFile(target);
                }
            }
        }

        void SearchRecordFile(CallStatus target)
        {
            if (RecordsList != null)
            {
                var callID = GetPart(":" + target.CallID, ':', ':');

                for (var i = 0; i < RecordsList.Count; i++)
                {
                    var recID = GetPart(RecordsList[i].FileName, '(', ')');

                    if (callID == recID)
                    {
                        target.DisplayDate = RecordsList[i].DisplayDate;
                        target.FileName = RecordsList[i].FileName;
                    }
                }
            }
        }

        void fillCallHandler_1(MyPhonePlugins.IMyPhoneCallHandler source, CallHandler target)
        {
            target.FirstName = (string)getValueFildDynamicObjectByName(source, "FirstName");
            target.LastName = (string)getValueFildDynamicObjectByName(source, "LastName");
            target.MakeCallTimeout = (int)getValueFildDynamicObjectByName(source, "MakeCallTimeout");
            target.MakeCallTimeoutCtiUnsupported = (int)getValueFildDynamicObjectByName(source, "MakeCallTimeoutCtiUnsupported");
            target.Number = (string)getValueFildDynamicObjectByName(source, "Number");
            target.Status = source.Status.ToString();
        }

        void fillCallHandler_2(MyPhonePlugins.IExtensionInfo source, CallHandler target)
        {
            target.FirstName = source.FirstName;
            target.LastName = source.LastName;
            target.MakeCallTimeout = (int)getValueFildDynamicObjectByName(source, "MakeCallTimeout");
            target.MakeCallTimeoutCtiUnsupported = (int)getValueFildDynamicObjectByName(source, "MakeCallTimeoutCtiUnsupported");
            target.Number = source.Number;
            target.Status = ((MyPhonePlugins.MyPhoneStatus)getValueFildDynamicObjectByName(source, "Status")).ToString();
        }

        void fillProfiles(MyPhonePlugins.UserProfileStatus source, UserProfileStatus target)
        {
            target.IsActive = source.IsActive;
            target.ProfileId = source.ProfileId;
            target.Name = source.Name;
            target.CustomName = source.CustomName;
            target.ExtendedStatus = source.ExtendedStatus;
        }

        object getValueFildDynamicObjectByName(object source, string name)
        {
            var type = source.GetType();
            
            object val = type.InvokeMember(name, BindingFlags.GetField | BindingFlags.GetProperty, Type.DefaultBinder, source, new object[0]);

            return val;
        }

        void callHandler_ProfileExtendedStatusChanged(object sender, ProfileExtendedStatusChangedEventArgs e)
        {
            var profileExtendedStatusChanged = new ProfileExtendedStatusChanged();

            try
            {
                var _sender = sender as MyPhonePlugins.IMyPhoneCallHandler;

                fillCallHandler_1(_sender, profileExtendedStatusChanged.CallHandler);
                profileExtendedStatusChanged.ProfileId = e.ProfileId;
                profileExtendedStatusChanged.Status = e.Status;

                foreach (var call in _sender.ActiveCalls)
                {
                    var newCall = new CallStatus();
                    fillCallStatus(call, newCall);
                    profileExtendedStatusChanged.CallHandler.ActiveCalls.Add(newCall);
                }

                foreach (var profile in _sender.Profiles)
                {
                    var newProfile = new UserProfileStatus();
                    fillProfiles(profile, newProfile);
                    profileExtendedStatusChanged.CallHandler.Profiles.Add(newProfile);
                }
            }
            catch
            {
                profileExtendedStatusChanged = null;
            }

            Callback(channel => channel.ProfileExtendedStatusChanged(profileExtendedStatusChanged));
        }

        void callHandler_CurrentProfileChanged(object sender, CurrentProfileChangedEventArgs e)
        {
            var currentProfileChanged = new CurrentProfileChanged();

            try
            {
                var _sender = sender as MyPhonePlugins.IMyPhoneCallHandler;

                fillCallHandler_1(_sender, currentProfileChanged.CallHandler);
                currentProfileChanged.NewProfileId = e.NewProfileId;
                currentProfileChanged.OldProfileId = e.OldProfileId;

                foreach (var call in _sender.ActiveCalls)
                {
                    var newCall = new CallStatus();
                    fillCallStatus(call, newCall);
                    currentProfileChanged.CallHandler.ActiveCalls.Add(newCall);
                }

                foreach (var profile in _sender.Profiles)
                {
                    var newProfile = new UserProfileStatus();
                    fillProfiles(profile, newProfile);
                    currentProfileChanged.CallHandler.Profiles.Add(newProfile);
                }
            }
            catch
            {
                currentProfileChanged = null;
            }

            Callback(channel => channel.CurrentProfileChanged(currentProfileChanged));
        }

        public void SetActiveProfile(int profileId)
        {
            callHandler.SetActiveProfile(profileId);
        }

        public void SetProfileExtendedStatus(int profileId, string status)
        {
            callHandler.SetProfileExtendedStatus(profileId, status);
        }

        void Callback(Action<IClientCallback> action)
        {
            lock (_callbackChannels)
            {
                foreach (var channel in _callbackChannels.ToList())
                    try
                    {
                        action.Invoke(channel);
                    }
                    catch (CommunicationException)
                    {
                        _callbackChannels.Remove(channel);
                    }
            }
        }

        public void Hold(string callId, bool holdOn)
        {
            //try
            //{
            //    Debugger.Break();
            //}
            //catch (Exception exc)
            //{
            //    var err = exc.Message;
            //}

            callHandler.Hold(callId, holdOn);
        }

        public void Mute(string callId)
        {
            callHandler.Mute(callId);
        }

        public void SendDTMF(string callId, string dtmf)
        {
            callHandler.SendDTMF(callId, dtmf);
        }

        public void SetQueueLoginStatus(bool loggedIn)
        {
            callHandler.SetQueueLoginStatus(loggedIn);
        }

        public void Show(Views view, ShowOptions options)
        {
            callHandler.Show(_pluginViewToView[view], MyPhonePlugins.ShowOptions.None);
        }

        public string Status()
        {
            try
            {
                return callHandler.Status.ToString();
            }
            catch
            {
                return null;
            }
        }

        public void LoadRecordingFiles(RecordsRequest recordsRequest)
        {
            if (RecordsList == null)
            {
                return;
            }

            foreach (var record in recordsRequest.Records)
            {
                foreach (var recordInfo in RecordsList)
                {
                    if (record.DisplayDate == recordInfo.DisplayDate && record.FileName == recordInfo.FileName)
                    {
                        DownloadFile(recordInfo, recordsRequest.LoadPath, recordInfo.FileName);
                        break;
                    }
                    else if (record.CallerID == GetPart(recordInfo.FileName, '(', ')'))
                    {
                        var ts = System.DateTime.Now - System.DateTime.Parse(recordInfo.DisplayDate);

                        if (ts.Seconds > 2 * 24 * 60 * 60) break;

                        DownloadFile(recordInfo, recordsRequest.LoadPath, record.FileName);
                        break;
                    }
                }
            }
        }

        public void DownloadFile(IRecordingInfo file, string LoadPath, string FileName1C)
        {
            if (file != null && App.Channel != null && App.Channel.IsAuthenticated)
            {
                RequestGetFile request = new RequestGetFile
                {
                    Folder = UsersFolder.RecordingFolder,
                    FileName = file.FileName
                };

                App.Channel.BeginRequest(request, RecordingFileReceived, LoadPath + FileName1C);
            }
        }

        private void RecordingFileReceived(IAsyncResult resp)
        {

            try
            {
                ResponseFile responseFile = App.Channel.EndRequest(resp) as ResponseFile;

                byte[] content = responseFile.Content;
                File.WriteAllBytes((string)resp.AsyncState, content);
            }
            catch
            {
                //
            }
        }

        public List<Record> GetRecordingList()
        {
            var list = new List<Record>();

            RecordsList = GetRecordsList();

            if (RecordsList == null)
            {
                return list;
            }

            foreach (var recordInfo in RecordsList)
            {
                var record = new Record
                {
                    DisplayDate = recordInfo.DisplayDate,
                    FileName = recordInfo.FileName
                };

                list.Add(record);
            }

            return list;
        }

        public string GetVersionComponent()
        {
            return "2.1";
        }
    }
}
