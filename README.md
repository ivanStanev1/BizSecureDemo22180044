# BizSecureDemo 22180044
*Курс:* Информационна сигурност в бизнес приложенията – УНСС

Този проект демонстрира основни механизми за идентифициране и коригиране на уязвимости в ASP.NET Core MVC среда.

## Технологичен стек
* *Framework:* .NET 8 / ASP.NET Core MVC
* *ORM:* Entity Framework Core
* *Database:* Microsoft SQL Server (LocalDB)
* *Authentication:* Cookie-based Authentication

## Реализирани защити (Сигурност)

### 1. Фикс на IDOR (Insecure Direct Object Reference)
В OrdersController е добавена логика, която проверява дали UserId на поръчката съвпада с ID на текущо логнатия потребител.

### 2. Защита от Stored XSS
Използва се *Razor View Engine*, който автоматично кодира данни в изгледите (напр. @Model.Title).

### 3. Brute Force защита (Rate Limiting)
Конфигурирана политика за *Account Lockout* (5 грешни опита = заключване).

## Структура на проекта
* *Controllers/* – Логика и проверки за достъп.
* *Data/* – Контекст на базата данни.
* *Models/* – Потребители и Поръчки.
* *Views/* – Защитен интерфейс.
