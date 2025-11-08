# Game1 Project Instructions for AI Assistants

## Table of Contents
- [Project Overview](#project-overview)
- [Architecture](#architecture)
- [Key Systems](#key-systems)
- [Coding Standards](#coding-standards)
- [Development Guidelines](#development-guidelines)
- [Troubleshooting](#troubleshooting)

## Project Overview

**Game1** is a Unity 2D game project built on the **OSK Framework**, a custom game development framework that provides:
- Scene management with async loading
- Component-based architecture
- UI management system
- Object pooling
- Centralized update loop management

### Tech Stack
- **Unity 2022.3+** (Unity 2D)
- **C#** (.NET Standard 2.1)
- **OSK Framework** (Custom)
- **DOTween** (Animations)
- **UniTask** (Async operations)
- **Odin Inspector** (Editor tools)

## Architecture

### Core Framework (OSK)
The project is built around the OSK Framework with these main components:

#### Main Hub (`Main` class)
```csharp
Main.Director   // Scene management
Main.UI         // UI management  
Main.Pool       // Object pooling
Main.Mono       // Update loop management
```

#### Component Registration Pattern
```csharp
// In Awake() method
Main.Mono.Register(this);                              // For IUpdate interface
SingletonManager.Instance.RegisterScene(this);        // For singleton access
```

### Scene Flow
```
Splash Scene (Bootstrap) → MainMenu Scene → Game Scene
```

## Key Systems

### 1. Scene Management
- **Builder Pattern**: `SceneLoadBuilder` for fluent scene loading
- **Async Loading**: Non-blocking with progress tracking
- **Fake Duration**: Smooth UI transitions

```csharp
Main.Director
    .LoadScene(new DataScene() { sceneName = "Game", loadMode = ELoadMode.Single })
    .Async(true)
    .FakeDuration(2f)
    .OnStart(() => Main.UI.Open<LoadingUI>())
    .OnComplete(() => Main.UI.Get<LoadingUI>().Hide())
    .Build();
```

### 2. Character System
#### Player Architecture
- **PlayerControl**: Main controller (composition pattern)
- **PlayerMovement**: Movement handling with normalized vectors
- **PlayerWeapon**: Weapon system integration
- **PlayerRotate**: Mouse-based sprite flipping
- **PlayerInput**: Interface-based input (`IPlayerInput`)

#### Enemy Architecture
- **EnemyControl**: Main enemy controller
- **EnemyMovement**: Movement behaviors
- **EnemyAttack**: Strategy pattern for different fire types

### 3. Weapon System (Strategy Pattern)
```csharp
public interface IWeaponFire
{
    void Fire(GameObject bulletPrefab, Transform target, Transform shootPoint, float damage, LayerMask targetLayer);
}

// Implementations:
// - SingleFire: Basic single shot
// - BurstFire: Multiple rapid shots  
// - CircleFire: 360-degree circular shooting
```

### 4. Factory System
```csharp
// Enemy creation with object pooling
IEnemy enemy = EnemyFactory.Spawn(EEnemyType.Basic, spawnPosition);
```

### 5. UI System
- **View Pattern**: UI classes extend `View` base class
- **TransitionUI**: For loading screens and transitions
- **RootUI**: Centralized UI management

## Coding Standards

### Framework Integration
1. **Always register components** in `Awake()`:
   ```csharp
   void Awake()
   {
       Main.Mono.Register(this);  // For update loops
       SingletonManager.Instance.RegisterScene(this);  // For singleton access
   }
   ```

2. **Use OSK update interfaces**:
   ```csharp
   public class MyClass : MonoBehaviour, IUpdate, IFixedUpdate
   {
       public void Tick(float deltaTime) { }
       public void FixedTick(float fixedDeltaTime) { }
   }
   ```

3. **Access framework modules** through Main:
   ```csharp
   Main.UI.Open<SomeUI>();
   Main.Pool.Spawn(poolKey, prefab);
   Main.Director.LoadScene(...);
   ```

### Design Patterns
1. **Prefer Interfaces over Inheritance**:
   ```csharp
   // Good: Strategy pattern
   public interface IMovement
   {
       void Move(Transform transform, Vector3 target, float deltaTime);
   }
   
   // Avoid: Deep inheritance hierarchies
   ```

2. **Use Factory Pattern** for object creation:
   ```csharp
   // Good: Centralized creation with pooling
   EnemyFactory.Spawn(EEnemyType.Basic, position);
   
   // Avoid: Direct Instantiate calls
   ```

3. **Component Composition** over inheritance:
   ```csharp
   // Good: PlayerControl manages separate components
   [SerializeField] private PlayerMovement movement;
   [SerializeField] private PlayerWeapon weapon;
   
   // Avoid: Monolithic player class
   ```

### Performance Guidelines
1. **Cache references** instead of repeated lookups:
   ```csharp
   // Good: Cache in Awake/Start
   private Transform playerTransform;
   void Awake() { playerTransform = FindPlayer(); }
   
   // Avoid: Repeated FindGameObjectWithTag in Update
   ```

2. **Use object pooling** for frequently created objects:
   ```csharp
   // Good: Use framework pooling
   Main.Pool.Spawn(KEY_POOL.DEFAULT, bulletPrefab);
   
   // Avoid: Direct Instantiate/Destroy
   ```

## Development Guidelines

### Adding New Features
1. **Follow existing patterns** in the codebase
2. **Create interfaces** for extensible systems
3. **Use OSK framework systems** where applicable
4. **Add proper error handling** and null checks
5. **Consider performance implications** (pooling, caching)

### File Organization
```
Assets/_Project/Scripts/
├── GamePlay/Scripts/
│   ├── Characters/Player/     # Player components
│   ├── Characters/Enemies/    # Enemy components  
│   ├── Weapon/               # Weapon system
│   ├── Spawner/              # Factory & spawning
│   ├── Managers/             # Game managers
│   ├── Interfaces.cs         # Shared interfaces
│   └── Enums.cs             # Shared enums
├── MainMenu/Scripts/         # Menu systems
│   ├── UI/                  # Menu UI components
│   └── Managers/            # Menu managers
└── Splash/Scripts/           # Bootstrap & loading
```

### Common Issues to Avoid
1. **Player Reference Issues**: Don't use `FindGameObjectWithTag("Player")` without null checks
2. **Performance Issues**: Don't create Vector3 objects every frame
3. **Pattern Inconsistency**: Ensure all managers follow OSK patterns
4. **Factory Errors**: Check method names (e.g., `Spawn` vs `Create`)

## Troubleshooting

### Common Problems
1. **NullReferenceException on Player**: Usually from unsafe `FindGameObjectWithTag`
2. **Components not updating**: Check if `Main.Mono.Register()` was called
3. **Singleton not found**: Verify `SingletonManager.Instance.RegisterScene()` was called
4. **Scene loading issues**: Check scene names in Build Settings

### Debug Tips
1. Use `OSKLogger.Log()` for framework-aware logging
2. Check component registration in Main's component list
3. Verify singleton registration in SingletonManager
4. Use Odin Inspector attributes for better debugging

## AI Assistant Guidelines

When helping with this project:
1. **Understand the OSK Framework** - it's central to everything
2. **Follow existing patterns** - don't suggest breaking architectural consistency
3. **Consider performance** - suggest pooling and caching where appropriate
4. **Provide complete solutions** - include proper registration and error handling
5. **Explain trade-offs** - help understand why certain patterns are used

Feel free to ask about:
- OSK Framework specifics
- Architectural decisions
- Performance optimizations  
- Best practice implementations
- Integration with existing systems