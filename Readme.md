# LogisticsSystem

A modular logistics management system built in .NET, consisting of:

- **LogisticsSystem.Core** – domain logic and business entities  
- **LogisticsSystem.Core.Tests** – unit tests for the core module  
- **LogisticsSystem.UI** – WPF user interface  

## 🧱 Architecture

The solution follows a clean modular structure:

```
LogisticsSystem/
├── LogisticsSystem.Core
├── LogisticsSystem.Core.Tests
└── LogisticsSystem.UI
```

- **Core** contains domain models (e.g., `SparePart`)
- **Tests** validate business logic using unit tests
- **UI** is a WPF application providing the presentation layer

## 🚀 Getting Started

### Requirements
- .NET 10 SDK  
- Visual Studio 2026 or JetBrains Rider  

### Build
```
dotnet build
```

### Run UI
```
cd LogisticsSystem.UI
dotnet run
```

### Run Tests
```
cd LogisticsSystem.Core.Tests
dotnet test
```

## 📦 Future Plans
- Expand domain model  
- Add database layer  
- Add API module  
- Improve UI  

## 📄 License
To be added before making the repository public.
