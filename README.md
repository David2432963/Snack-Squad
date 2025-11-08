# Game1 - Unity 2D Game Project

A Unity 2D game built with the OSK Framework, featuring component-based architecture, advanced scene management, and modular systems.

## 🚀 Quick Start

### Prerequisites
- Unity 2022.3 or later
- .NET Standard 2.1

### Project Structure
```
Assets/_Project/
├── 🎬 Scenes/           # Game scenes (Splash, MainMenu, Game)
├── 🎮 Scripts/          # All game code
│   ├── GamePlay/        # Core gameplay systems
│   ├── MainMenu/        # Menu management
│   └── Splash/          # Bootstrap and loading
├── 🎨 Resources/        # Runtime-loaded assets
├── 🎵 AudioClip/       # Audio files
└── 🖼️  Sprites/         # 2D graphics

Plugins/
├── OSK-Framework/       # Custom game framework
├── DOTween/            # Animation library
├── UniTask/            # Async operations
└── Odin Inspector/     # Editor tools
```

## 🏗️ Architecture

### OSK Framework Integration
This project is built around the **OSK Framework**, providing:
- **Scene Management**: Async loading with progress tracking
- **Component System**: Centralized update loop management  
- **UI Management**: View-based architecture
- **Object Pooling**: Performance-optimized object creation
- **Singleton Management**: Scene-specific singleton handling

### Key Systems
1. **Character System**: Composition-based Player/Enemy architecture
2. **Weapon System**: Strategy pattern for different firing modes
3. **Spawning System**: Factory pattern with object pooling
4. **UI System**: View classes with transition management
5. **Scene Flow**: Bootstrap → MainMenu → Game

## 🛠️ Development Guidelines

### Framework Usage
```csharp
// Component registration (in Awake)
Main.Mono.Register(this);                              // For IUpdate interface
SingletonManager.Instance.RegisterScene(this);        // For singleton access

// System access
Main.Director.LoadScene(...);                          // Scene management
Main.UI.Open<SomeUI>();                               // UI management
Main.Pool.Spawn(key, prefab);                        // Object pooling
```

### Design Patterns
- **Component Pattern**: Player/Enemy use composition over inheritance
- **Factory Pattern**: `EnemyFactory` for centralized enemy creation
- **Strategy Pattern**: `IWeaponFire` for different weapon behaviors
- **Builder Pattern**: `SceneLoadBuilder` for fluent scene loading
- **Singleton Pattern**: `SingletonManager` for scene-specific singletons

### Best Practices
- Use interfaces for extensible systems (`IWeaponFire`, `IPlayerInput`)
- Register components with OSK framework in `Awake()`
- Prefer object pooling over `Instantiate/Destroy`
- Cache references instead of repeated `Find` calls
- Follow existing architectural patterns

## 🎮 Game Flow

```
Splash Scene (Bootstrap)
    ↓ (Automatic after 0.1s or 'L' key)
Loading UI
    ↓
MainMenu Scene
    ↓ (Start Game button)
Loading UI  
    ↓
Game Scene (Gameplay)
```

## 📚 For AI Assistants

See [`PROJECT_INSTRUCTIONS.md`](PROJECT_INSTRUCTIONS.md) for detailed architectural guidelines and development instructions.

Key files to understand:
- `.cursorrules` - AI assistant configuration
- `PROJECT_INSTRUCTIONS.md` - Detailed development guidelines
- `Assets/Plugins/OSK-Framework/` - Core framework code

## 🤝 Contributing

When adding new features:
1. Follow existing OSK Framework patterns
2. Use interfaces for extensibility
3. Integrate with existing systems (pooling, scene management, UI)
4. Add proper error handling and null checks
5. Consider performance implications

## 🐛 Common Issues

- **NullReferenceException**: Usually from unsafe `FindGameObjectWithTag` usage
- **Components not updating**: Check `Main.Mono.Register()` was called
- **Singleton not found**: Verify `SingletonManager.Instance.RegisterScene()` 
- **Scene loading fails**: Check scene names exist in Build Settings

## 📄 License

[Add your license information here]