using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace TCX.CallTriggerCmd
{
    [DataContract]
    public class CurrentProfileChanged
    {
        public CurrentProfileChanged()
        {
            CallHandler = new CallHandler();
        }

        [DataMember]
        public CallHandler CallHandler;

        [DataMember]
        public int NewProfileId;

        [DataMember]
        public int OldProfileId;
    }

    [DataContract]
    public class ProfileExtendedStatusChanged
    {
        public ProfileExtendedStatusChanged()
        {
            CallHandler = new CallHandler();
        }

        [DataMember]
        public CallHandler CallHandler;

        [DataMember]
        public int ProfileId;

        [DataMember]
        public string Status;
    }

    [DataContract]
    public class OnMyPhoneStatusChanged
    {
        public OnMyPhoneStatusChanged()
        {
            CallHandler = new CallHandler();
        }

        [DataMember]
        public CallHandler CallHandler;

        [DataMember]
        public string Status;
    }

    [DataContract]
    public class OnCallStatusChanged
    {
        public OnCallStatusChanged()
        {
            CallHandler = new CallHandler();
            CallStatus = new CallStatus();
        }

        [DataMember]
        public CallHandler CallHandler;

        [DataMember]
        public CallStatus CallStatus;
    }

    [DataContract]
    public class CallHandler
    {
        public CallHandler()
        {
            ActiveCalls = new List<CallStatus>();
            Profiles = new List<UserProfileStatus>();
        }

        [DataMember]
        public List<CallStatus> ActiveCalls;

        [DataMember]
        public List<UserProfileStatus> Profiles;

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public int MakeCallTimeout { get; set; }

        [DataMember]
        public int MakeCallTimeoutCtiUnsupported { get; set; }

        [DataMember]
        public string Number { get; set; }

        [DataMember]
        public string Status { get; set; }
    }

    [DataContract]
    public class CallStatus
    {
        [DataMember]
        public string CallID { get; set; }

        [DataMember]
        public bool Incoming { get; set; }

        [DataMember]
        public bool IsHold { get; set; }

        [DataMember]
        public bool IsMuted { get; set; }

        [DataMember]
        public string Originator { get; set; }

        [DataMember]
        public string OriginatorName { get; set; }

        [DataMember]
        public string OriginatorType { get; set; }

        [DataMember]
        public string OtherPartyName { get; set; }

        [DataMember]
        public string OtherPartyNumber { get; set; }

        [DataMember]
        public string State { get; set; }

        [DataMember]
        public string Tag3cx { get; set; }

        [DataMember]
        public string DisplayDate { get; set; }

        [DataMember]
        public string FileName { get; set; }
    }

    [DataContract]
    public class UserProfileStatus
    {
        [DataMember]
        public bool IsActive { get; set; }

        [DataMember]
        public int ProfileId { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string CustomName { get; set; }

        [DataMember]
        public string ExtendedStatus { get; set; }
    };

    [DataContract]
    public class RecordsRequest
    {
        [DataMember]
        public string LoadPath { get; set; }

        [DataMember]
        public List<Record> Records { get; set; }
    }

    [DataContract]
    public class Record
    {
        [DataMember]
        public string DisplayDate { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string CallerID { get; set; }
    }

    public interface IClientCallback 
    {
        [OperationContract]
        void CurrentProfileChanged(CurrentProfileChanged currentProfileChanged);

        [OperationContract]
        void ProfileExtendedStatusChanged(ProfileExtendedStatusChanged profileExtendedStatusChanged);

        [OperationContract]
        void CallStatusChanged(OnCallStatusChanged onCallStatusChanged);

        [OperationContract]
        void MyPhoneStatusChanged(OnMyPhoneStatusChanged onMyPhoneStatusChanged);
    };

    public enum CallState
    {
        /// <summary>
        /// Undefined state. Such calls are not valid
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Incoming call is ringing on user's phone. Call is not yet answered by the user
        /// </summary>
        Ringing = 1,
        /// <summary>
        /// Outbound call initiated from user's phone. Call is not yet answered by remote party 
        /// </summary>
        Dialing = 2,
        /// <summary>
        /// Call is established. User is talking with other party
        /// </summary>
        Connected = 3,
        /// <summary>
        /// Call is rerouted. User waits for the answer form new party
        /// </summary>
        WaitingForNewParty = 4,
        /// <summary>
        /// User is transferring call to new destination. After successful transfer user will be disconnected from the call.
        /// </summary>
        TryingToTransfer = 5,
        /// <summary>
        /// Call has been disconnected
        /// </summary>
        Ended = 6
    }

    [Flags]
    public enum MakeCallOptions
    {
        /// <summary>
        /// No options
        /// </summary>
        None = 0,
        /// <summary>
        ///  Make video call
        /// </summary>
        WithVideo = 1
    };

    [Flags]
    public enum ActivateOptions
    {
        /// <summary>
        /// No options
        /// </summary>
        None = 0,
        /// <summary>
        ///  Answer with video
        /// </summary>
        WithVideo = 1
    };

    /// <summary>
    /// Additional options for show
    /// </summary>
    [Flags]
    public enum ShowOptions
    {
        /// <summary>
        /// No options
        /// </summary>
        None = 0
    }

    /// <summary>
    /// View to show
    /// </summary>
    public enum Views
    {
        /// <summary>
        /// Dialpad
        /// </summary>
        DialPad,
        /// <summary>
        /// Presence
        /// </summary>
        Presence,
        /// <summary>
        /// Contacts
        /// </summary>
        Contacts,
        /// <summary>
        /// Call history
        /// </summary>
        CallHistory,
        /// <summary>
        /// Voicemails
        /// </summary>
        Voicemails,
        /// <summary>
        /// Conferences
        /// </summary>
        Conferences,
        /// <summary>
        /// Chat
        /// </summary>
        Chats
    }
     
    [ServiceContract(CallbackContract = typeof(IClientCallback), SessionMode = SessionMode.Required)]
    public interface ICallTriggerService
    {
        List<CallStatus> ActiveCalls 
        {
            [OperationContract]
            get; 
        }

        List<UserProfileStatus> Profiles 
        {
            [OperationContract]
            get; 
        }

        [OperationContract]
        string Status();

        [OperationContract]
        CallHandler ProfilesEx();

        [OperationContract]
        void Subscribe();

        [OperationContract]
        void Unsubscribe();

        [OperationContract]
        CallStatus MakeCall(string destination);

        [OperationContract]
        CallStatus MakeCallEx(string destination, MakeCallOptions options);

        [OperationContract]
        void DropCall(string callId);
        
        [OperationContract]
        void BlindTransfer(string callId, string destination);
        
        [OperationContract]
        CallStatus BeginTransfer(string callId, string destination);
        
        [OperationContract]
        void CancelTransfer(string callId);
        
        [OperationContract]
        void CompleteTransfer(string callId);
        
        [OperationContract]
        void Activate(string callId);

        [OperationContract]
        void ActivateEx(string callId, ActivateOptions options);

        [OperationContract]
        void SetActiveProfile(int profileId);

        [OperationContract]
        void SetProfileExtendedStatus(int profileId, string status);

        [OperationContract]
        void Hold(string callId, bool holdOn);

        [OperationContract]
        void Mute(string callId);

        [OperationContract]
        void SendDTMF(string callId, string dtmf);

        [OperationContract]
        void SetQueueLoginStatus(bool loggedIn);

        [OperationContract]
        void Show(Views view, ShowOptions options);

        [OperationContract]
        void LoadRecordingFiles(RecordsRequest recordsRequest);

        [OperationContract]
        List<Record> GetRecordingList();

        [OperationContract]
        string GetVersionComponent();
    }
}
