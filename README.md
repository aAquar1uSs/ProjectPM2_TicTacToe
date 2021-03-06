# **Многопользовательская сетевая модифицированная игра Tic-tac-toe**
Пользователь взаимодействует с системой через консольное приложение-клиент. В свою очередь, клиент взаимодействует с сервером по протоколу НТТР.
Одновременно запускается только одна инстанция сервера и неограниченное количество клиентов.

## Терминология
Игра крестики-нолики.

Раунд – один этап игры крестики-нолики. Раунд считается завершенным, когда одна из сторон зачеркивает свой цвет, составляющий сплошной ряд (горизонтально, вертикально, диагонально), или один из игроков сдается, или по таймауту хода(по умолчанию - 20 секунд). Если игрок не походил, победа автоматом засчитывается последнему игроку, который сделал ход.

Серия – серия раундов между двумя противниками. Можно считать, что серия – это общая сессия(комната) для двух игроков. Серия заканчивается когда один из игроков выходит, или по таймауту, если ни один игрок не проявляет активности в рамках сессии (по умолчанию - 2 минуты).

## Модификация игры
Каждый игрок имеет набор цифр от 1 до 9 и свой цвет для ходов (например игрок 1 – красные ходы, игрок 2 - зеленые). Первый игрок ставит на поле цифру, заняв пустую клеточку. Следующий игрок может занять любой цифрой пустую клеточку или перебить ход игрока, поставив на уже занятую ячейку большее число. Выигрывает тот, кто первым закроет ряд. Поставленное на поле число изменять нельзя.

## Регистрация
Регистрация состоять из Логина и Пароля для аккаунта, где Логин уникален внутри сервера. Пароль не может быть пустым и простым – минимум 6 символов. После 3 неудачных попыток логина есть временная блокировка. Регистрации переживають перезапуск сервера.

## Настройка таймаутов
Пользователь может настроить:
- таймаут подключения в комнату(сессию).
- таймаут активности в комнате(сессии).
- таймаут начала раунда.
- таймаут хода в раунде.

**Настройки учитываются только если пользователь создает комнату!**

## Режим игры
Есть три режима игры:
- Тренировочный – игра с ботом.
- Приватный – вход в конкретную комнату с помощью кода (пользователь создает приватную игру - получает ключ (идентификатор игры), передает ключ своему коллеге (противнику), который заходит по ключу в сессию и тогда начинается отсчет активности).
- Публичный – серия раундов со случайным противником.

## Статистика
Все результаты успешных раундов сохраняються для расчетов статистики.

### Общая статистика
Общая статистика строиться в разрезе всех пользователей (Leaderboard). Общая статистика является публичной и доступна для просмотра, даже для гостей (незарегистрированных пользователей). Пользователь учитывается в общей статистике только когда имеет более 10 раундов.

Mожно посмотреть топ игроков по:
- количеству выигрышей.
- количеству проигрышей.
- процента выигрышей.
- количеству комнат.
- общем времени в игре.

### Личная статистика
Пользователь может просматривать личную статистику: количество выигрышей и проигрышей, изменение отношения выигрышей к проигрышам во времени, проведенное время в игре, статистика используемых ходов.
