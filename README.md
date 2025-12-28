# 🎨 AttributeKit

<div align="center">

**A professional collection of Unity inspector attributes to supercharge your workflow**

[![Unity](https://img.shields.io/badge/Unity-2020.3+-black.svg?style=flat&logo=unity)](https://unity.com/)
[![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat)](LICENSE)
[![Made with Love](https://img.shields.io/badge/Made%20with-♥-red.svg?style=flat)]()

*Transform your Unity Inspector into a powerful, organized, and beautiful interface*

</div>

---

## 📋 Table of Contents

- [✨ Features](#-features)
- [🚀 Installation](#-installation)
- [📚 Attributes](#-attributes)
  - [🔘 InspectorButton](#-inspectorbutton)
  - [🔲 InlineButton](#-inlinebutton)
  - [🔳 InlineButtons](#-inlinebuttons)
  - [⏱️ TimeDecomposer](#️-timedecomposer)
  - [👁️ ConditionalDisplay](#️-conditionaldisplay)
  - [🆔 UniqueId](#-uniqueid)
  - [📦 Expandable](#-expandable)
  - [🔒 ReadOnly](#-readonly)
  - [📦 BoxGroup](#-boxgroup)
- [💡 Quick Examples](#-quick-examples)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)

---

## ✨ Features

- 🎯 **Inspector Buttons** - Execute methods directly from the Inspector
- 🔲 **Inline Buttons** - Add buttons next to any field for quick actions
- ⏰ **Time Decomposition** - Break down time values into readable units
- 🔍 **Conditional Display** - Show/hide fields based on conditions
- 🔑 **Unique ID Generation** - Generate and manage unique identifiers
- 📂 **Expandable ScriptableObjects** - Edit SO references inline
- 🔐 **Read-Only Fields** - Prevent accidental modifications
- 📊 **Box Grouping** - Organize fields into visual groups
- 🎨 **Professional Design** - Clean, modern inspector interface
- 🚀 **Zero Dependencies** - Pure C# implementation
- 📖 **Well Documented** - Comprehensive XML documentation

---

## 🚀 Installation

### Option 1: Unity Package Manager (Git URL)
1. Open Unity Package Manager (`Window > Package Manager`)
2. Click `+` and select `Add package from git URL`
3. Enter: `https://github.com/MuhammetInceee/AttributeKit.git`

### Option 2: Manual Installation
1. Download the latest release
2. Extract to your `Assets` folder
3. Start using the attributes!

---

## 📚 Attributes

### 🔘 InspectorButton

Execute methods directly from the Inspector with customizable buttons.

**Features:**
- ✅ Method invocation from Inspector
- ✅ Custom button labels
- ✅ Auto dirty marking
- ✅ Coroutine support
- ✅ Parameter validation
- ✅ Error handling

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    [InspectorButton("Reset Player")]
    private void ResetPlayerData()
    {
        Debug.Log("Player data reset!");
    }

    [InspectorButton(markDirty: true)]
    private void GenerateLevel()
    {
        // Level generation logic
    }

    [InspectorButton]
    private IEnumerator StartGameSequence()
    {
        yield return new WaitForSeconds(1f);
        Debug.Log("Game started!");
    }
}
```

**Inspector Preview:**
```
┌─────────────────────────────────┐
│ [Reset Player]                   │
│ [Generate Level]                 │
│ [Start Game Sequence]            │
└─────────────────────────────────┘
```

---

### 🔲 InlineButton

Add a button next to any field for quick method invocation - perfect for single button actions.

**Features:**
- ✅ Button next to field
- ✅ Custom labels and widths
- ✅ Private method support
- ✅ Auto dirty marking
- ✅ Coroutine support
- ✅ Minimal inspector space

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Simple reset button
    [InlineButton("ResetHealth")]
    public int health = 100;

    // Custom label and width
    [InlineButton("Randomize", "🎲", 40f)]
    public int score = 0;

    // With larger button
    [InlineButton("LoadData", "Load from File", 100f)]
    public string dataPath = "";

    private void ResetHealth()
    {
        health = 100;
    }

    private void Randomize()
    {
        score = Random.Range(0, 1000);
    }

    private void LoadData()
    {
        // Load logic
    }
}
```

**Inspector Preview:**
```
Health: [100      ] [Reset Health]
Score:  [0        ] [🎲]
Data Path: [     ] [Load from File]
```

---

### 🔳 InlineButtons

Add **multiple buttons** next to a field using a simple string array syntax - perfect for increment/decrement or multiple actions.

**Features:**
- ✅ Multiple buttons on one field
- ✅ Simple string array syntax
- ✅ Format: `"MethodName|Label|Width"`
- ✅ Label and width optional
- ✅ Clean, compact layout
- ✅ All InlineButton features

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Simple - method names only
    [InlineButtons("Double", "Halve", "Clear")]
    public float value = 10f;

    // With custom labels
    [InlineButtons("Increment|+", "Decrement|-", "Reset|↻")]
    public int counter = 0;

    // With labels and widths
    [InlineButtons("Increment|+|30", "Decrement|-|30", "Reset|Reset|50")]
    public int score = 0;

    // Methods
    private void Increment() => counter++;
    private void Decrement() => counter--;
    private void Reset() => counter = 0;

    private void Double() => value *= 2;
    private void Halve() => value /= 2;
    private void Clear() => value = 0f;
}
```

**Inspector Preview:**
```
Value:   [10.0    ] [Double] [Halve] [Clear]
Counter: [0       ] [+] [-] [↻]
Score:   [0       ] [+] [-] [Reset]
```

**Comparison:**
```csharp
// OLD WAY (complex, causes issues)
[InlineButton("Inc", "+", 30f)]
[InlineButton("Dec", "-", 30f)]
[InlineButton("Reset", "↻", 30f)]
public int counter = 0;

// NEW WAY (simple, reliable) ✨
[InlineButtons("Inc|+|30", "Dec|-|30", "Reset|↻|30")]
public int counter = 0;
```

---

### ⏱️ TimeDecomposer

Display time values as separate units (months, days, hours, minutes, seconds).

**Features:**
- ✅ 5 time units (Month, Day, Hour, Minute, Second)
- ✅ Default: Minutes and Seconds
- ✅ Custom unit selection
- ✅ Automatic overflow handling
- ✅ float/int support
- ✅ Responsive UI

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Default: Minutes and Seconds
    [TimeDecomposer]
    public float cooldownTime = 90f;

    // Hours, Minutes, Seconds
    [TimeDecomposer(TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second)]
    public float eventDuration = 3665f;

    // All units
    [TimeDecomposer(TimeUnit.Month | TimeUnit.Day | TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second)]
    public float totalPlayTime = 5000000f;
}
```

**Inspector Preview:**
```
Cooldown Time:     [1] Min  [30] Sec
Event Duration:    [1] Hr   [1] Min   [5] Sec
Total Play Time:   [1] Mo   [27] D    [18] Hr  [53] Min  [20] Sec
```

---

### 👁️ ConditionalDisplay

Show or hide fields based on conditions with powerful comparison operators.

**Features:**
- ✅ 6 comparison types (==, !=, >, <, >=, <=)
- ✅ Multiple data types (bool, int, float, enum, string)
- ✅ Property and method support
- ✅ Nested field support
- ✅ Reflection caching
- ✅ Zero performance impact when hidden

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    public bool isEnabled;
    public int playerLevel = 1;
    public WeaponType weaponType;

    // Simple bool check
    [ConditionalDisplay("isEnabled")]
    public float damage = 10f;

    // Numeric comparison
    [ConditionalDisplay("playerLevel", ComparisonType.GreaterOrEqual, 5)]
    public GameObject advancedWeapon;

    // Enum comparison
    [ConditionalDisplay("weaponType", ComparisonType.Equals, WeaponType.Sword)]
    public float swordDamage = 15f;

    // Method support
    private bool CanUseSpecialAbility() => playerLevel >= 5;

    [ConditionalDisplay("CanUseSpecialAbility")]
    public GameObject specialAbilityEffect;
}
```

**Inspector Preview:**
```
Is Enabled: ☑
Damage: 10.0                    ← Visible when isEnabled = true

Player Level: 7
Advanced Weapon: [GameObject]   ← Visible when level >= 5

Weapon Type: Sword
Sword Damage: 15.0              ← Visible when type = Sword
```

---

### 🆔 UniqueId

Generate and manage unique identifiers with multiple generation strategies.

**Features:**
- ✅ 5 generation strategies (GUID, Short, Timestamp, Sequential)
- ✅ Case formatting (Upper, Lower, Default)
- ✅ Prefix/Suffix support
- ✅ Auto-generation option
- ✅ Copy to clipboard
- ✅ Regenerate with confirmation

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Default GUID
    [UniqueId]
    public string entityId;

    // Auto-generate on creation
    [UniqueId(autoGenerate: true)]
    public string sessionId;

    // Short ID, uppercase
    [UniqueId(IdGenerationType.ShortID, IdCaseFormat.Upper)]
    public string playerId;

    // With prefix
    [UniqueId(IdGenerationType.Timestamp, "EVENT_")]
    public string eventId;
}
```

**Inspector Preview:**
```
Entity ID:   550e8400-e29b-41d4-a716-446655440000  [Copy] [Regenerate]
Session ID:  a3f8c9d2e1b4f5a6                      [Copy] [Regenerate]
Player ID:   A3F8C9D2                               [Copy] [Regenerate]
Event ID:    EVENT_20250115143052_a3f8              [Copy] [Regenerate]
```

---

### 📦 Expandable

Display ScriptableObject references inline without opening separate windows.

**Features:**
- ✅ Inline SO editing
- ✅ Foldable sections
- ✅ Create/Delete buttons
- ✅ 3 header styles
- ✅ Nested SO support
- ✅ Array/List support
- ✅ Type filtering

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Basic expandable
    [Expandable]
    public ItemData itemData;

    // With create button
    [Expandable(showCreateButton: true)]
    public WeaponData weaponData;

    // Box style with buttons
    [Expandable(ExpandableHeaderStyle.Box, showCreateButton: true, showDeleteButton: true)]
    public PlayerProfile profile;

    // Works with arrays
    [Expandable(showCreateButton: true)]
    public ItemData[] inventory;
}
```

**Inspector Preview:**
```
▼ Item Data: HealthPotion        [Delete]
┌─────────────────────────────────────┐
│  Item Name: "Health Potion"         │
│  Item Value: 50                     │
│  Item Icon: [Sprite]                │
│  Description: "Restores 100 HP"     │
└─────────────────────────────────────┘
```

---

### 🔒 ReadOnly

Make fields read-only in the Inspector to prevent accidental modifications.

**Features:**
- ✅ 3 modes (Always, PlayMode only, EditMode only)
- ✅ Visual indicator option
- ✅ Custom tooltips
- ✅ Array/List support
- ✅ All field types supported
- ✅ Code access still works

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Always read-only
    [ReadOnly]
    public int currentScore = 100;

    // Read-only in play mode only
    [ReadOnly(ReadOnlyMode.OnlyInPlayMode)]
    public float maxHealth = 100f;

    // With visual indicator
    [ReadOnly(ReadOnlyMode.Always, showIndicator: true)]
    public Vector3 currentPosition;

    // Custom tooltip
    [ReadOnly("This value is calculated automatically")]
    public float damageMultiplier = 1.5f;

    void Update()
    {
        // Can still modify through code!
        currentScore += 10;
        currentPosition = transform.position;
    }
}
```

**Inspector Preview:**
```
Current Score: 100                     [grayed out, not editable]
Max Health: 100.0                      [editable in edit mode]
█ Current Position: (0, 0, 0)          [orange bar + grayed out]
Damage Multiplier: 1.5                 [tooltip on hover]
```

---

### 📦 BoxGroup

Organize fields into visual groups with customizable boxes.

**Features:**
- ✅ Field grouping by ID
- ✅ 4 box styles
- ✅ Foldable groups
- ✅ Custom colors
- ✅ Order control
- ✅ Custom titles
- ✅ Pro/Light skin support

**Usage:**
```csharp
using AttributeKit;
using UnityEngine;

public class Example : MonoBehaviour
{
    // Basic grouping
    [BoxGroup("Stats")]
    public int health = 100;

    [BoxGroup("Stats")]
    public int mana = 50;

    [BoxGroup("Stats")]
    public float stamina = 100f;

    // Custom title and style
    [BoxGroup("Weapons", "Player Weapons", BoxStyle.Rounded)]
    public GameObject primaryWeapon;

    [BoxGroup("Weapons", "Player Weapons", BoxStyle.Rounded)]
    public GameObject secondaryWeapon;

    // Foldable group
    [BoxGroup("Audio", foldable: true)]
    public AudioClip walkSound;

    [BoxGroup("Audio", foldable: true)]
    public float volume = 0.8f;

    // Ungrouped fields display normally
    public string playerName = "Player";
}
```

**Inspector Preview:**
```
┌─────────────────────────────────┐
│ Stats                            │
│   Health: 100                    │
│   Mana: 50                       │
│   Stamina: 100.0                 │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ Player Weapons                   │
│   Primary Weapon: [None]         │
│   Secondary Weapon: [None]       │
└─────────────────────────────────┘

┌─────────────────────────────────┐
│ ▼ Audio                          │
│   Walk Sound: [AudioClip]        │
│   Volume: 0.8                    │
└─────────────────────────────────┘

Player Name: "Player"
```

---

## 💡 Quick Examples

### Complete Player Controller
```csharp
using AttributeKit;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [BoxGroup("Stats", "Player Stats")]
    [ReadOnly(ReadOnlyMode.OnlyInPlayMode, showIndicator: true)]
    public int currentHealth = 100;

    [BoxGroup("Stats", "Player Stats")]
    [ReadOnly(ReadOnlyMode.OnlyInPlayMode, showIndicator: true)]
    public int currentMana = 50;

    [BoxGroup("Settings", "Player Settings", foldable: true)]
    public float moveSpeed = 5f;

    [BoxGroup("Settings", "Player Settings", foldable: true)]
    public float jumpHeight = 2f;

    [BoxGroup("Equipment", foldable: true)]
    [Expandable(showCreateButton: true)]
    public WeaponData equippedWeapon;

    [BoxGroup("Equipment", foldable: true)]
    [Expandable(showCreateButton: true)]
    public ArmorData equippedArmor;

    public bool isInCombat;

    [ConditionalDisplay("isInCombat")]
    [ReadOnly]
    public float combatTime;

    [TimeDecomposer(TimeUnit.Minute | TimeUnit.Second)]
    public float cooldownDuration = 30f;

    [UniqueId(autoGenerate: true)]
    public string playerId;

    [InspectorButton("Restore Full Health")]
    private void RestoreHealth()
    {
        currentHealth = 100;
        Debug.Log("Health restored!");
    }

    [InspectorButton("Take Damage")]
    private void TakeDamage()
    {
        currentHealth -= 10;
        Debug.Log($"Took damage! Health: {currentHealth}");
    }
}
```

### Game Settings ScriptableObject
```csharp
using AttributeKit;
using UnityEngine;

[CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Settings")]
public class GameSettings : ScriptableObject
{
    [BoxGroup("Graphics", "Graphics Settings", BoxStyle.Rounded, foldable: true)]
    public int resolutionWidth = 1920;

    [BoxGroup("Graphics", "Graphics Settings", BoxStyle.Rounded, foldable: true)]
    public int resolutionHeight = 1080;

    [BoxGroup("Graphics", "Graphics Settings", BoxStyle.Rounded, foldable: true)]
    public bool fullscreen = true;

    [BoxGroup("Gameplay", "Gameplay Settings", BoxStyle.Default, foldable: true)]
    public float difficulty = 1.0f;

    [BoxGroup("Gameplay", "Gameplay Settings", BoxStyle.Default, foldable: true)]
    [ConditionalDisplay("difficulty", ComparisonType.GreaterOrEqual, 2f)]
    public bool hardModeFeatures = false;

    [BoxGroup("Time", "Time Settings", foldable: true)]
    [TimeDecomposer(TimeUnit.Hour | TimeUnit.Minute | TimeUnit.Second)]
    public float dayDuration = 86400f;

    [UniqueId(IdGenerationType.ShortID, IdCaseFormat.Upper, autoGenerate: true)]
    [ReadOnly]
    public string settingsId;

    [InspectorButton("Reset to Defaults")]
    private void ResetSettings()
    {
        resolutionWidth = 1920;
        resolutionHeight = 1080;
        fullscreen = true;
        difficulty = 1.0f;
        Debug.Log("Settings reset to defaults!");
    }
}
```

---

## 🎯 Best Practices

### Performance
- ✅ ConditionalDisplay uses reflection caching
- ✅ BoxGroup caches field information on enable
- ✅ ReadOnly has zero runtime cost
- ✅ All attributes are editor-only

### Organization
- 📁 Use BoxGroup for related fields
- 🏷️ Use descriptive GroupIds and titles
- 📊 Leverage Order parameter for logical flow
- 🎨 Combine attributes for maximum effect

### Workflow
- 🔘 Use InspectorButton for common debug actions
- 🔒 Use ReadOnly for calculated/runtime values
- 👁️ Use ConditionalDisplay to reduce clutter
- 📦 Use Expandable for nested configurations

---

## 🏗️ Architecture

```
AttributeKit/
├── InspectorButtonAttribute/
│   ├── InspectorButtonAttribute.cs
│   └── InspectorButtonAttributeDrawer.cs
├── InlineButtonAttribute/
│   ├── InlineButtonAttribute.cs
│   ├── InlineButtonAttributeDrawer.cs
│   ├── InlineButtonsAttribute.cs
│   ├── InlineButtonsAttributeDrawer.cs
│   └── InlineButtonAttributeExample.cs
├── TimeDecomposerAttribute/
│   ├── TimeDecomposerAttribute.cs
│   └── TimeDecomposerAttributeDrawer.cs
├── ConditionalDisplayAttribute/
│   ├── ConditionalDisplayAttribute.cs
│   └── ConditionalDisplayAttributeDrawer.cs
├── UniqueIdAttribute/
│   ├── UniqueIdAttribute.cs
│   └── UniqueIdAttributeDrawer.cs
├── ExpandableAttribute/
│   ├── ExpandableAttribute.cs
│   └── ExpandableAttributeDrawer.cs
├── ReadOnlyAttribute/
│   ├── ReadOnlyAttribute.cs
│   └── ReadOnlyAttributeDrawer.cs
└── BoxGroupAttribute/
    ├── BoxGroupAttribute.cs
    └── BoxGroupAttributeDrawer.cs
```

---

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Guidelines
- Follow existing code style
- Add XML documentation
- Include usage examples
- Test with Unity 2020.3+
- Keep attributes editor-only

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 Support

If you find this useful, please ⭐ star the repository!

For issues and feature requests, please use the [GitHub Issues](https://github.com/MuhammetInceee/AttributeKit/issues) page.

---

<div align="center">

**Made with Unity 🎮**

*Happy Coding!* 🚀

</div>
