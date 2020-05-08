using System;
using System.Reflection;
using System.Runtime.InteropServices;
using TCX.CallTriggerCmd;
using V8.AddIn;

[ComVisible(true)]
[Guid("adbd4ca6-6ad7-4d6f-9dfc-d6d2f07c0dab")] // произвольный Guid-идентификатор Вашей компоненты
[ProgId("AddIn.SIPConnector")] // это имя COM-объекта, по которому Вы будете ее подключать
public class Stub : LanguageExtenderAddIn
{
    public Stub() : base(typeof(SIPConnector), 1000) { }
}
   