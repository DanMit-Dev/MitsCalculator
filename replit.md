# MITS Calculator

## Overview

MITS Calculator is a standalone desktop application built for Linux that focuses on providing a comprehensive mathematical computing experience. The project follows a "local-first" philosophy, ensuring all computation and data storage happens locally without requiring internet connectivity. The application is designed around three core principles: Personal Mathematics (privacy and customization), Living Mathematics (interactive exploration), and Universal Mathematics (accessibility and extensibility through plugins).

## User Preferences

Preferred communication style: Simple, everyday language.

## System Architecture

### Desktop Application Framework
- **Core Technology**: .NET 8 with Avalonia UI framework
- **Programming Language**: C# for type safety and cross-platform compatibility
- **UI Architecture**: XAML-based declarative UI with Avalonia.Themes.Fluent for modern styling
- **Target Platform**: Linux desktop (with cross-platform potential)

### Mathematical Computing Engine
- **Primary Math Library**: MathNet.Numerics for core arithmetic, complex numbers, matrices, and unit conversions
- **Expression Parsing**: NCalc for mathematical expression evaluation and Computer Algebra System (CAS) functionality
- **Performance**: GPU acceleration planned for complex algebra and matrix calculations

### Data Persistence
- **Database**: SQLite for local data storage (user_data.db)
- **Data Access**: Microsoft.Data.Sqlite for database interactions
- **Storage Schema**: 
  - History table for calculation records
  - Variables table for user-defined variables and functions
  - Formulas table for saved mathematical expressions
  - Settings table for UI preferences and configuration
  - Projects table for user workspace management

### Visualization and Plotting
- **2D Plotting**: OxyPlot.Avalonia for statistical and mathematical plots
- **3D Graphics**: Helix Toolkit planned for 3D function visualization and simulations
- **Rendering**: Hardware-accelerated graphics through Avalonia's OpenGL/Vulkan support

### Application Design Patterns
- **Architecture**: MVVM (Model-View-ViewModel) pattern typical for Avalonia applications
- **Data Flow**: Local-first with no external API dependencies for core functionality
- **Plugin System**: Extensible architecture planned for community contributions
- **Privacy**: Complete offline operation with local data encryption

## External Dependencies

### Core Framework Dependencies
- **Avalonia UI Suite**: 
  - Avalonia (11.3.4) - Cross-platform UI framework
  - Avalonia.Desktop - Desktop platform support
  - Avalonia.Themes.Fluent - Modern UI theming
- **.NET Runtime**: .NET 8.0 framework

### Mathematical Libraries
- **MathNet.Numerics** (5.0.0) - Core mathematical operations and algorithms
- **NCalc** (1.3.8) - Mathematical expression parser and evaluator

### Data Storage
- **Microsoft.Data.Sqlite** (9.0.8) - SQLite database connectivity
- **SQLite Runtime**: Embedded database engine for local data persistence

### Visualization
- **OxyPlot.Avalonia** (2.1.0) - 2D plotting and charting capabilities

### Platform Integration
- **Native Libraries**: Platform-specific libraries for Linux desktop integration
- **Graphics Acceleration**: OpenGL/Vulkan support through Avalonia's rendering pipeline
- **Hardware Access**: GPU acceleration libraries for mathematical computations (planned)

### Build and Runtime Support
- **MicroCom.Runtime** - COM interop support
- **HarfBuzzSharp** - Text shaping and font rendering
- **SkiaSharp** - 2D graphics rendering engine