# Quick Fix for .NET SDK PATH Issue

## The Problem

When you run `dotnet --version` from `C:\WINDOWS\system32` or any PowerShell window, you get:
```
No .NET SDKs were found.
```

This happens because the PATH environment variable doesn't include the .NET SDK location in your current PowerShell session.

## ✅ Immediate Fix (Current Session)

Run this in your PowerShell window:

```powershell
$env:DOTNET_ROOT = "C:\Program Files\dotnet"
$env:PATH = "C:\Program Files\dotnet;" + $env:PATH
dotnet --version
```

This will fix the PATH **for your current PowerShell session only**.

## ✅ Permanent Fix (All Future Sessions)

The PATH has already been added to your User environment variables, but you need to:

1. **Close ALL PowerShell windows** (including this one)
2. **Open a NEW PowerShell window**
3. **Test**: `dotnet --version`

It should work! ✅

## 🔍 Verify PATH is Set

To check if PATH is configured:

```powershell
[System.Environment]::GetEnvironmentVariable("PATH", "User") -split ';' | Where-Object { $_ -like "*dotnet*" }
```

You should see: `C:\Program Files\dotnet`

## 🚀 Quick Fix Script

I've created `fix-dotnet-path.ps1` that you can run:

```powershell
.\fix-dotnet-path.ps1
```

This will:
- Verify .NET SDK is installed
- Add to User PATH if missing
- Fix the current session
- Test that it works

## 📝 Why This Happens

- PowerShell sessions inherit PATH from when they were opened
- If you opened PowerShell **before** the PATH was added, it won't have it
- **Solution**: Close and reopen PowerShell, or fix the current session with the commands above

## ✅ Summary

**For current session:**
```powershell
$env:PATH = "C:\Program Files\dotnet;" + $env:PATH
```

**For all future sessions:**
- Close PowerShell
- Open new PowerShell window
- It will work automatically!

The PATH is already configured in your system - you just need a fresh PowerShell session to pick it up.
