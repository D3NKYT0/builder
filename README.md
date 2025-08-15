# PDL Builder (L2JCore Builder)

A Windows WPF application for building and managing update patches for L2JCore server files. This tool helps developers and server administrators create efficient update packages with file integrity verification and compression.

## 🚀 Features

- **File Tree Management**: Visual tree view of files and folders with modification tracking
- **Hash Verification**: CRC32 hash calculation for file integrity
- **Update Package Creation**: Generate compressed update packages with XML manifest
- **Progress Tracking**: Real-time progress monitoring during build operations
- **File Synchronization**: Compare and sync files between different versions
- **Critical Update Marking**: Mark files as critical for immediate updates
- **Multi-language Support**: Interface available in multiple languages
- **Modern UI**: Clean and intuitive WPF interface with custom styling

## 📋 Requirements

- **Operating System**: Windows 7/8/10/11
- **.NET Framework**: 4.5 or higher
- **RAM**: Minimum 512MB (2GB recommended)
- **Disk Space**: At least 100MB free space

## 🛠️ Installation

1. **Download**: Get the latest release from the releases page
2. **Extract**: Extract the ZIP file to your desired location
3. **Run**: Execute `L2JCore Builder.exe`

### Building from Source

```bash
# Clone the repository
git clone [repository-url]

# Open the solution in Visual Studio
# Build the solution (Ctrl+Shift+B)
# Run the application (F5)
```

## 📖 Usage

### Basic Workflow

1. **Set Patch Path**: Click "Set Patch Path" to select the folder containing your update files
2. **Set Output Path**: Click "Set Output Path" to specify where the update package will be created
3. **Review Files**: The application will scan and display all files in a tree structure
4. **Configure Options**:
   - Mark files as "Critical" for immediate updates
   - Enable "Check Hash" for file integrity verification
   - Use "Quick Update" for faster processing
5. **Build Update**: Click "Build Update" to create the update package

### File Status Indicators

- 🟢 **Green**: New files
- 🔴 **Red**: Deleted files  
- 🔵 **Blue**: Modified files
- ⚪ **White**: Unchanged files

### Advanced Features

#### Synchronization
- Use the "Sync" feature to compare two folder structures
- Automatically detect file changes between versions
- Generate differential update packages

#### Hash Verification
- CRC32 hash calculation for all files
- Ensures file integrity during updates
- Prevents corruption during transfer

#### Compression
- Built-in ZIP compression using Ionic.Zip library
- Reduces update package size significantly
- Supports password protection (if needed)

## 🏗️ Project Structure

```
UpdateBuilder/
├── Models/                 # Data models
│   ├── FileModel.cs       # File information model
│   ├── FolderModel.cs     # Folder structure model
│   ├── UpdateInfoModel.cs # Update manifest model
│   └── ModifyType.cs      # File modification types
├── ViewModels/            # MVVM view models
│   ├── MainWindowViewModel.cs
│   └── Items/            # Item-specific view models
├── Views/                 # WPF user interface
│   └── MainWindow.xaml   # Main application window
├── Utils/                # Utility classes
│   ├── PatchWorker.cs    # Core update building logic
│   ├── Logger.cs         # Logging functionality
│   └── Crc32.cs          # Hash calculation
├── ZIPLib/               # Compression library
└── Resource/             # Application resources
```

## 🔧 Configuration

### Settings
The application saves user preferences including:
- Last used patch path
- Last used output path
- UI preferences
- Build options

### Logging
- All operations are logged for debugging
- Log entries include timestamps and operation details
- Logs can be cleared using the "Clear Log" button

## 🐛 Troubleshooting

### Common Issues

**Application won't start**
- Ensure .NET Framework 4.5+ is installed
- Check Windows compatibility settings
- Verify file permissions

**Build fails**
- Check available disk space
- Ensure source folder is accessible
- Verify file permissions on output directory

**Slow performance**
- Close other applications
- Reduce folder size being processed
- Check available RAM

### Error Messages

- **"Folder not found"**: Verify the patch path exists and is accessible
- **"Access denied"**: Check file permissions and close any open files
- **"Out of memory"**: Reduce the number of files or increase available RAM

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Development Guidelines

- Follow C# coding conventions
- Use MVVM pattern for new features
- Add appropriate error handling
- Include XML documentation for public methods
- Test thoroughly before submitting

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- **Ionic.Zip**: For compression functionality
- **WPF Community**: For UI components and styling
- **L2JCore Community**: For feedback and testing

## 📞 Support

- **Issues**: Report bugs and feature requests via GitHub Issues
- **Discussions**: Join community discussions for help and ideas
- **Documentation**: Check the wiki for detailed guides

---

**Version**: 1.0.0  
**Last Updated**: December 2024  
**Maintainer**: L2JCore Development Team
