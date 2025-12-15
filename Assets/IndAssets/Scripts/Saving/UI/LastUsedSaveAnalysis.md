# 最近使用存档的分析

## 当前实现方式

### 方法一：通过 SaveTime 判断（已实现）

在 `PvSaveListUI.cs` 中，我们通过比较所有存档的 `SaveTime` 字段来确定最近使用的存档：

```csharp
private PvSaveDetails DetermineLastUsedSave(List<PvSaveDetails> saveDetailsList)
{
    // 按 SaveTime 降序排序，最新的存档就是最近使用的
    var sortedList = saveDetailsList.OrderByDescending(s => 
    {
        if (DateTime.TryParse(s.SaveTime, out DateTime saveTime))
        {
            return saveTime;
        }
        return DateTime.MinValue;
    }).ToList();
    
    return sortedList.FirstOrDefault();
}
```

**优点：**
- 不需要修改现有文件
- 实现简单
- 基于实际的保存时间，数据准确

**缺点：**
- 如果用户加载了存档但没有保存，无法追踪
- 只能判断最近保存的存档，不一定是最近加载的存档

### 方法二：在 PvSaveManager 中追踪当前加载的存档（需要修改现有文件）

如果需要更精确地追踪"最近使用的存档"（包括加载但未保存的情况），可以考虑以下方案：

#### 方案 2.1：在 PvSaveManager 中添加字段

在 `PvSaveManager` 中添加：
```csharp
private string _lastLoadedSaveGuid; // 存储最近加载的存档 GUID

public string LastLoadedSaveGuid => _lastLoadedSaveGuid;
```

在 `LoadGameByGuidAsync` 和 `LoadGameAsync` 方法中更新：
```csharp
_lastLoadedSaveGuid = saveFolderGuid; // 或从 slotName 获取 GUID
```

#### 方案 2.2：在 PvSaveDetails 中添加 LastLoadTime 字段

在 `PvSaveDetails` 中添加：
```csharp
[SerializeField] private string lastLoadTime; // 最后加载时间

public string LastLoadTime
{
    get => lastLoadTime;
    set => lastLoadTime = value;
}
```

在加载存档时更新这个字段。

#### 方案 2.3：使用 PlayerPrefs 存储

在加载存档时，将 GUID 保存到 PlayerPrefs：
```csharp
PlayerPrefs.SetString("LastUsedSaveGuid", saveFolderGuid);
PlayerPrefs.Save();
```

在 UI 中读取：
```csharp
string lastUsedGuid = PlayerPrefs.GetString("LastUsedSaveGuid", "");
```

**优点：**
- 可以追踪加载操作，不仅仅是保存操作
- 数据持久化，即使游戏重启也能记住

**缺点：**
- 需要修改现有文件（方案 2.1 和 2.2）
- PlayerPrefs 方案不需要修改现有文件，但需要额外的存储逻辑

## 推荐方案

**当前推荐使用方法一（通过 SaveTime 判断）**，因为：
1. 不需要修改现有文件
2. 对于大多数游戏场景，最近保存的存档通常就是最近使用的存档
3. 实现简单，维护成本低

如果未来需要更精确的追踪（包括加载但未保存的情况），可以考虑使用方案 2.3（PlayerPrefs），因为它不需要修改现有的存档系统文件。

## 使用示例

在 `PvSaveListUI` 中，最近使用的存档会被自动高亮显示：

```csharp
bool isLastUsed = _lastUsedSave != null && 
                  saveDetails.SaveFolderGuid == _lastUsedSave.SaveFolderGuid;
saveSlotUI.Initialize(saveDetails, isLastUsed, OnSaveSlotSelected);
```

UI 组件会通过 `highlightImage` 来显示高亮效果，表示这是最近使用的存档。
