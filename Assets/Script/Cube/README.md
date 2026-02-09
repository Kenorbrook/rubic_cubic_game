# 2D Rubik's Cube - MVVM Architecture

## Описание
Реализация 2D кубика Рубика с 5 видимыми сторонами (передняя, правая, левая, верхняя, нижняя) по архитектуре MVVM.

## Структура проекта

Все скрипты находятся в папке `Assets/Script/Cube/` и организованы по MVVM архитектуре:

### Model (Модель данных)
- **CubeColor.cs** - Enum цветов кубика
- **CubeSide.cs** - Enum сторон кубика
- **RotationDirection.cs** - Enum направлений вращения
- **RowColumnRotationType.cs** - Enum типа вращения (строка/столбец)
- **RowColumnRotation.cs** - Данные о вращении строки/столбца
- **CubeFaceModel.cs** - Модель одной грани 3x3
- **RubikCubeModel.cs** - Основная модель кубика с логикой вращения

### ViewModel (Логика управления)
- **RubikCubeViewModel.cs** - ViewModel для управления состоянием кубика
- **RotationCommand.cs** - Команда вращения грани

### View (Отображение)
- **CubeCellView.cs** - Отображение одной ячейки грани
- **CubeFaceView.cs** - Отображение одной грани (9 ячеек)
- **RubikCubeView.cs** - Главный компонент отображения всех 5 граней

### Animation (Анимация)
- **FaceRotationAnimator.cs** - Анимация вращения грани
- **RowColumnAnimator.cs** - Анимация вращения строк/столбцов
- **CellColorAnimator.cs** - Анимация изменения цвета ячейки

### Input (Управление)
- **SwipeDetector.cs** - Детектор свайпов для мобильных устройств
- **CubeInputController.cs** - Контроллер для вращения целых граней
- **FrontFaceInputController.cs** - Контроллер для вращения строк/столбцов на передней грани

### Helpers
- **CubeColorHelper.cs** - Конвертация CubeColor в Unity Color

## Требования

### Input System
Проект использует **New Input System**. Убедитесь, что настройки правильные:

1. Откройте **Edit → Project Settings → Player**
2. В разделе **Other Settings** найдите **Active Input Handling**
3. Установите **Both** или **Input System Package (New)**
4. Unity попросит перезапуститься - согласитесь

**Важно:** Если вы видите ошибки компиляции после изменения настроек, просто перезапустите Unity.

## Настройка в Unity

### 1. Создание UI структуры

Создайте следующую иерархию в сцене:

```
Canvas
├── RubikCube (GameObject)
│   ├── RubikCubeView (Component)
│   ├── SwipeDetector (Component)
│   ├── CubeInputController (Component - для вращения граней)
│   ├── FrontFaceInputController (Component - для вращения строк/столбцов)
│   │
│   ├── FrontFace (GameObject с RectTransform)
│   │   ├── CubeFaceView (Component)
│   │   ├── FaceRotationAnimator (Component)
│   │   ├── RowColumnAnimator (Component)
│   │   └── Grid (GameObject с GridLayoutGroup)
│   │       ├── Cell_0_0 (Image + CubeCellView + CellColorAnimator)
│   │       ├── Cell_0_1 (Image + CubeCellView + CellColorAnimator)
│   │       └── ... (всего 9 ячеек)
│   │
│   ├── RightFace (аналогично FrontFace)
│   ├── LeftFace (аналогично FrontFace)
│   ├── TopFace (аналогично FrontFace)
│   └── BottomFace (аналогично FrontFace)
│
├── ShuffleButton (Button)
└── ResetButton (Button)
```

**Важно для Grid:**
- Grid - это GameObject с компонентом **GridLayoutGroup**
- Настройте GridLayoutGroup: Cell Size, Spacing, Constraint = Fixed Column Count (3)

### 2. Настройка компонентов

#### RubikCubeView
- Перетащите все 5 CubeFaceView в соответствующие поля
- Назначьте CubeInputController
- Назначьте кнопки Shuffle и Reset

#### CubeFaceView
- Установите Side (Front/Right/Left/Top/Bottom)
- Перетащите все 9 CubeCellView в массив Cells (в порядке: row 0, row 1, row 2)
- Назначьте FaceRotationAnimator
- Назначьте RowColumnAnimator

#### FaceRotationAnimator
- Настройте Rotation Duration (рекомендуется 0.3 секунды)
- Настройте Animation Curve для плавности

#### RowColumnAnimator
- Настройте Rotation Duration (рекомендуется 0.3 секунды)
- Настройте Animation Curve для плавности

#### CellColorAnimator
- Настройте Color Transition Duration (рекомендуется 0.2 секунды)

#### SwipeDetector
- Min Swipe Distance: 50 пикселей
- Max Swipe Time: 1 секунда

#### CubeInputController (для вращения граней)
- Назначьте SwipeDetector
- Назначьте RubikCubeView

#### FrontFaceInputController (для вращения строк/столбцов)
- Назначьте SwipeDetector
- Назначьте RubikCubeView
- Назначьте Front Face Rect (RectTransform передней грани)
- Установите Cell Size (размер одной ячейки, например 100)

### 3. Настройка ячеек

Каждая ячейка должна иметь:
- **Image** компонент (для отображения цвета)
- **CubeCellView** компонент
- **CellColorAnimator** компонент

### 4. Настройка коллайдеров (опционально для CubeInputController)

**Что такое "грань"?**
Грань - это GameObject типа **FrontFace**, **RightFace**, **LeftFace**, **TopFace**, **BottomFace** в вашей иерархии.

**Когда нужны коллайдеры?**
Если вы используете **CubeInputController** для вращения целых граней (не строк/столбцов), то каждому GameObject грани нужно добавить:
- **BoxCollider2D** или **PolygonCollider2D** (2D коллайдер)
- Это нужно для определения, на какой грани был сделан свайп

**Пример:** 
```
FrontFace (GameObject)
├── BoxCollider2D (покрывает всю грань)
├── CubeFaceView
├── FaceRotationAnimator
└── Grid...
```

**Примечание:** Для **FrontFaceInputController** (вращение строк/столбцов) коллайдеры НЕ нужны - он использует RectTransform для определения позиции свайпа.

## Использование

### Управление свайпами

#### Вращение строк и столбцов на передней грани (FrontFaceInputController):
- **Свайп влево/вправо** на любой строке → вращает эту строку
  - Вправо = по часовой стрелке (строка движется вправо)
  - Влево = против часовой стрелки (строка движется влево)
- **Свайп вверх/вниз** на любом столбце → вращает этот столбец
  - Вниз = по часовой стрелке (столбец движется вниз)
  - Вверх = против часовой стрелки (столбец движется вверх)

#### Вращение целых граней (CubeInputController):
- **Передняя грань**: Свайп влево/вправо для вращения
- **Правая грань**: Свайп вверх/вниз для вращения
- **Левая грань**: Свайп вверх/вниз для вращения
- **Верхняя грань**: Свайп влево/вправо для вращения
- **Нижняя грань**: Свайп влево/вправо для вращения

### API

```csharp
// Получить ViewModel
RubikCubeViewModel viewModel = rubikCubeView.GetViewModel();

// Перемешать кубик
viewModel.ShuffleCube(20); // 20 случайных ходов

// Сбросить кубик
viewModel.ResetCube();

// Проверить, решен ли кубик
bool isSolved = viewModel.IsCubeSolved();

// Попытаться повернуть грань
bool success = viewModel.TryRotateFace(CubeSide.Front, RotationDirection.Clockwise);

// Попытаться повернуть строку (0, 1, или 2)
bool success = viewModel.TryRotateRow(0, RotationDirection.Clockwise);

// Попытаться повернуть столбец (0, 1, или 2)
bool success = viewModel.TryRotateColumn(1, RotationDirection.CounterClockwise);
```

### События ViewModel

```csharp
viewModel.OnRotationStarted += (side, direction) => { /* ... */ };
viewModel.OnRotationCompleted += () => { /* ... */ };
viewModel.OnFaceUpdated += (side) => { /* ... */ };
viewModel.OnCubeSolved += () => { /* ... */ };
```

## Особенности реализации

1. **Блокировка одновременных вращений**: Система автоматически блокирует новые вращения во время анимации
2. **Подписка на изменения**: Все грани подписаны на изменения передней грани
3. **Анимации**: Плавные анимации вращения и изменения цвета
4. **Мобильная поддержка**: Полная поддержка тач-управления и свайпов
5. **MVVM архитектура**: Четкое разделение Model, View и ViewModel

## Расширение функционала

### Добавление сохранения состояния

Можно интегрировать с существующим SaveManager:

```csharp
// В RubikCubeModel добавить сериализацию
[Serializable]
public class CubeState
{
    public Dictionary<CubeSide, CubeColor[,]> FaceStates;
}

// Сохранение
string json = JsonUtility.ToJson(GetCubeState());
PlayerPrefs.SetString("CubeState", json);

// Загрузка
CubeState state = JsonUtility.FromJson<CubeState>(PlayerPrefs.GetString("CubeState"));
LoadCubeState(state);
```

### Добавление звуков

```csharp
// В RubikCubeView
private void HandleRotationStarted(CubeSide side, RotationDirection direction)
{
    AudioManager.PlaySound("RotateSound");
    // ... остальной код
}
```

### Добавление счетчика ходов

```csharp
// В RubikCubeViewModel
private int _moveCount;

public void TryRotateFace(CubeSide side, RotationDirection direction)
{
    if (base.TryRotateFace(side, direction))
    {
        _moveCount++;
        OnMoveCountChanged?.Invoke(_moveCount);
    }
}
```

## Тестирование

1. Запустите сцену
2. Используйте мышь для тестирования свайпов (работает как тач)
3. Нажмите Shuffle для перемешивания
4. Нажмите Reset для сброса
5. Попробуйте решить кубик!

## Производительность

- Все вращения выполняются асинхронно через корутины
- Анимации оптимизированы с использованием AnimationCurve
- Минимальное количество Update() вызовов
- Эффективное использование событий вместо постоянных проверок
