# 🖥️ IconToggler

**IconToggler** is a Windows tray application that allows you to easily show and hide desktop icons.

## 📌 Features
- ✅ **Quick access via tray icon**
- ✅ **Double-click on the tray icon** → Toggle desktop icons on/off
- ✅ **Automatically hides icons** on program startup
- ✅ **Restores icons** when the program is closed or Windows is shut down
- ✅ **Embedded icon**, so shortcuts automatically display the same icon

---

## 🚀 Installation & Usage

### **1️⃣ Clone Repository & Set Up Project**
If you have Git installed, you can clone the repository directly:

```sh
git clone https://github.com/VanSnitza/IconToggler.git
cd IconToggler
```

If you don’t use Git, download the project as a ZIP file and extract it to a directory of your choice.

### **2️⃣ Open the Project in Visual Studio**
1. Start **Visual Studio**.
2. Open the project using **"Open a project or solution"** (`IconToggler.sln`).

### **3️⃣ Build a Release Version**
1. Select **"Release"** as the configuration.
2. Press **`Ctrl + Shift + B`** or go to **"Build → Build Solution"**.
3. The **compiled EXE** can be found in:
   ```
   bin\Release\IconToggler.exe
   ```

### **4️⃣ Run the Program**
1. Execute `IconToggler.exe`.
2. The **tray icon will appear in the taskbar**.
3. **Double-click the tray icon** → Toggles desktop icons on/off.
4. **Right-click the tray icon** → Select “Exit” to close the program.

---

## 🛠 Standalone EXE without .NET Dependencies
If you want the EXE to run **without requiring .NET Framework**:

```sh
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

The final EXE will be located in:
```
bin\Release\net472\win-x64\publish\IconToggler.exe
```

---

## 🔧 Developer Setup
- **Programming Language**: C# (.NET Framework 4.7.2)
- **Dependencies**: Windows Forms, System.Drawing
- **Development Tools**: Visual Studio 2019/2022

---

## 💡 Future Enhancements
- [ ] **Hotkey support** (e.g., `Ctrl + Alt + D` to toggle icons)
- [ ] **Settings menu for custom configurations**
- [ ] **Auto-start with Windows**

---

## 🤝 Contributors
This project was developed with assistance from [ChatGPT](https://openai.com/chatgpt). Feel free to contribute and improve it! 🚀

---

## 📜 License
MIT License – Feel free to use & contribute! 🎉
