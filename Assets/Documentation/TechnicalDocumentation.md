# Техническая документация проекта «?»

> **Файл:** `Docs/TechnicalDocumentation.md`

## Оглавление

- [1. Введение](#1-введение)
- [2. Структура проекта](#2-структура-проекта)
- [3. Сцены и игровой поток](#3-сцены-и-игровой-поток)
- [4. Префабы и компоненты](#4-префабы-и-компоненты)
- [5. ScriptableObject‑ы и конфигурации](#5-scriptableobject‑ы-и-конфигурации)
- [6. Расширения редактора](#6-расширения-редактора)
- [7. Автоматизация](#7-автоматизация)
- [8. Поддержание актуальности](#8-поддержание-актуальности)

---

## 1. Введение

- **Название проекта:** ?
- **Платформа:** iOS / Android (мобильные устройства)
- **Сложность:** Казуал
- **Примерный срок реализации:** ?

### 1.1 Цели проекта

- Сделать привлекательную мобильную игру
- Создать сбалансированный геймплей
- Прощупать рынок и протестировать концепцию
- Организовать процессы внутри команды
- Определить реальные возможности при доступных ресурсах

### 1.2 Цель игры

На каждом этапе (лесные уровни и лабиринты) игрок должен спасать фей, избегать монстров и преодолевать препятствия (деревья, камни, водные преграды).

### 1.3 Аудитория и тематика

- **Основная ЦА:** любители простых аркад всех возрастов
- **Сегменты ЦА:** подростки и взрослые, ищущие короткие игровые сессии, семейные игроки
- **Тема:** Волшебный лес и магия природы

---

## 2. Структура проекта

| Папка         | Описание                                         | Примеры файлов и папок                                                                                     |
|---------------|--------------------------------------------------|------------------------------------------------------------------------------------------------------------|
| `Data`        | ScriptableObject‑ы (настройки арен и лабиринтов) | `Data/Arenas/Arena1.asset`, `Data/Labyrinths/Labyrinth1.asset`                                            |
| `Environment` | Текстуры, PNG-файлы и анимации объектов          | `Environment/Player/PNG/Idle/06.png0001.png`, `Environment/Player/Animations/Idle/Idle.anim`              |
| `Materials`   | ???                                              | `???`, `???`                                                                                                |
| `Prefabs`     | Игровые префабы                                  | `Prefabs/Player/Player.prefab`, `Prefabs/Enemy/Goblin/Enemy.prefab`, `Prefabs/Fairy/FairyF/FairyF.prefab` |
| `Scenes`      | Сцены проекта                                    | `Scenes/Menu.unity`, `Scenes/Arenas/Arena1.unity`, `Scenes/Labyrinths/Labyrinth1.unity`                   |
| `Scripts`     | C#‑скрипты                                       | `Scripts/Player/PlayerController.cs`, `Scripts/GameManagers/ArenaManager.cs`                              |
| `Textures`    | ???                                              | `Textures/???`, `Textures/???`                                                                             |
| `Tilemaps`    | Тайловые карты лабиринтов                        | `Tilemaps/Grass/Grass1.asset`, `Tilemaps/Palette/Grass.prefab`                                            |

---

## 3. Сцены и игровой поток

### 3.1 Menu

- **Файл:** `Scenes/Menu.unity`
- **Описание:** Начальный экран с фоном волшебного леса, кнопками `Играть`, `Опции`, `Выход`.
- **Ключевые объекты:**
  - `Canvas` (UI элементы)
  - `MenuManager` (скрипт для навигации)

### 3.2 ArenaX (1–5)

- **Файлы:** `Scenes/Arena1.unity` … `Scenes/Arena5.unity`
- **Описание:** Открытая зона (квадрат), где лис собирает фей и убегает от монстров.
- **Объекты и компоненты:**
  - Префабы фей (двигающиеся)
  - Префабы монстров (преследующие, элементали и др.)
  - `ArenaManager` (спавн, контроль победы/поражения)
  - Препятствия (деревья, кусты)

### 3.3 LabyrinthX (1–N)

- **Файлы:** `Scenes/Labyrinth1.unity` … `Scenes/LabyrinthN.unity`
- **Описание:** Запутанный лабиринт на память; игрок может просматривать мини‑карту.
- **Объекты и компоненты:**
  - `Tilemap` (лайаут лабиринта)
  - `LabyrinthManager` (логика ловушек, проверки правильного пути)

### 3.4 Игровой поток

> > **В работе**

---

## 4. Префабы и компоненты

| Префаб              | Описание                             | Ключевые компоненты                                                                           |
|---------------------|--------------------------------------|-----------------------------------------------------------------------------------------------|
| `Player.prefab`     | Лис Spark                            | Rigidbody2D, PlayerController, PlayerStatsHandler, DrawingManager, CapsuleCollider2D, Animator |
| `FairyF.prefab`     | Фея (жен.)                           | FairyController, CircleCollider2D                                                             |
| `FairyM.prefab`     | Фея (муж.)                           | FairyController, CircleCollider2D                                                             |
| `Enemy.prefab`      | Преследующий враг (череп, тролль)    | EnemyController, EnemyStatsHandler                                                            |
| `Enemy.prefab`      | Стреляющий враг (гоблин)             | EnemyController, EnemyStatsHandler                                                            |
| `Obstacle.prefab`   | Препятствия (дерево1, дерево2, куст) | BoxCollider2D                                                                                 |

---

## 5. ScriptableObject‑ы и конфигурации

| Конфиг                                | Тип              | Описание            | Публичные поля                                                                                                                    |
|---------------------------------------|------------------|---------------------|-----------------------------------------------------------------------------------------------------------------------------------|
| `Arena1.asset` `ArenaN.asset`         | ScriptableObject | Настройки арены     | Survival time, Enemy count, Enemy prefabs, Fairy count, Fairy prefabs, Obstacle types, obstacle min. distance, plant trees edge |
| `Labyrinth1.asset` `LabyrinthN.asset` | ScriptableObject | Настройки лабиринта | Rows, cols, cell size, time limit, Camera position/rotation/size                                                                  |

---

## 6. Расширения редактора

- **Scripts/Editor/SkillSymbolEditor.cs**  
  Кастомный инспектор для настройки шаблонов символов и силы заряда.

- **Scripts/Editor/FairySpawnEditor.cs**  
  Визуальный редактор параметров волн и спауна фей.

- **Scripts/Editor/LabyrinthMapPreview.cs**  
  Отображение мини‑карты и настройки плотности ловушек.

---

## 7. Автоматизация

- **Editor/ExportSettings.cs**  
  Экспортирует все ScriptableObject в `Docs/Export/*.json` для QA и анализа.

- **Tests/DocsValidation.cs**  
  Юнит‑тест проверяет, что каждая ScriptableObject имеет запись в MD.

- **CI (.github/workflows/docs.yml)**

```yaml
name: Docs Validation
on: [push]
jobs:
  validate:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v2
      - name: Validate Docs
        run: dotnet test Tests/DocsValidation.csproj
