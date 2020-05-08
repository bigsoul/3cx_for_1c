SET path=%~dp0
SET target="C:\ProgramData\3CXPhone for Windows\PhoneApp\"
copy %path%CallTriggerCmdPlugin.dll %target%CallTriggerCmdPlugin.dll
copy %path%CallTriggerCmdServiceProvider.dll %target%CallTriggerCmdServiceProvider.dll
copy %path%MyPhoneCRMIntegration.dll %target%MyPhoneCRMIntegration.dll
copy %path%V8AddInLibrary.dll %target%V8AddInLibrary.dll
%path%AddIn.SIPConnector.reg
C:\Windows\Microsoft.NET\Framework\v4.0.30319\regasm.exe "C:\ProgramData\3CXPhone for Windows\PhoneApp\V8AddInLibrary.dll" /tlb /codebase"
cmd /k