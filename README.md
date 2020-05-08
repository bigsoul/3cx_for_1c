# 3cx_for_1c
Обработка и и плагин для интеграции телефонии 3CX в 1С:Предприятие 8

Папки:
 - CallTriggerCmdPlugin - плагин, который загружается в 3cx phone for windows.
 - CallTriggerCmdServiceProvider - описание формата обмена между CallTriggerCmdPlugin и V8AddInLibrary.
 - References - интеграционная компонента для создания плагинов 3cx phone for windows.
 - V8AddInLibrary - com-объект для подключения к 1С:Предприятие.
 - install - готовая сборка для инсталяции плагина.
 
 Файлы:
 - Телефон.epf - обработка, реализующая интерфейс sip-фона аналогичный стандартному интерфейсу 3cx phone for windows. Реализует программный интерфейс для управления всеми возможностями телефона.
 - ТестПрограммногоИнтерфейса.epf - обработка, реализующая графический интерфейс для тестирования программного интерфейса Телефон.epf.

Как работает ?
 CallTriggerCmdPlugin.dll загружается в память sip-фона 3cx phone for windows и управляет им, а так же получает события с через MyPhoneCRMIntegration.dll. V8AddInLibrary.dll загружается в память 1С:Предприятия как внешняя компонента и принимает команды, уведомляет о событиях через "ВнешнееСобытие". CallTriggerCmdPlugin и V8AddInLibrary связаны между собой через WFC используя описание протокола взаимодействия из CallTriggerCmdServiceProvider.dll. 
  Телефон.epf загружается в глобальную переменную на клиенте и далее два сценария:
  - Просто открывает форму и работает как sip-фон с похожим интерфейсом (естественно не полным) на 3cx phone for windows.
  - Форма не открывается. Программист использую программный интерфейс управляет телефоном, например создав собственные элементы управления.

Установка:
 1. Закрыть клиенты 3cx phone for windows.
 2. Закрыть клиенты 1С:Предприятие.
 3. Скачать install на локальный диск.
 4. Запустить bat
 
 Описание программного интерфейса смотрите непосредственно в коде обработки Телефон.epf в комментариях. 
 
 Телефон.epf - написан полностью на колбэках (ОписаниеОповещения) в том числе, вызовы com-компаненты.
 
 Проблемы:
  - Со стороны кода 1С минимальные.
  - Со стороны кода C# - основная проблема в том, что код писал специалист 1С. Знатокам C# комнями не кидать. Код на C# минимальный, лучше перепешите если нужно, если нет, используйте так. Работает стабильно.
  
  Используется на реальном внедрении больше 3 лет, 70 активных пользователей sip-фона через программный интерфейс в базе.
