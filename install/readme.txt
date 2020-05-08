Вариант установки: ручной
1. Закрыть процесс клиента 3CXPhone и все процессы 1С.

2. Скопировать файлы: 
	   V8AddInLibrary.dll 
       CallTriggerCmdPlugin.dll 
       CallTriggerCmdServiceProvider.dll 
       MyPhoneCRMIntegration.dll 
в каталог "C:\ProgramData\3CXPhone for Windows\PhoneApp".

3. Открыть командную строку и перейти в каталог "C:\Windows\Microsoft.NET\Framework\v4.0.30319". Выполнить команду "regasm.exe <ПУТЬ_К_ФАЙЛУ_V8AddInLibrary.dll> /tlb/codebase"

4. Экспортировать записи реестра из AddIn.SIPConnector.reg в систему.