# VicServoPriceWatch

VicServoPriceWatch is a .NET 10 solution for monitoring Victorian servo fuel prices. It contains a Windows desktop application, shared application logic, and a .NET MAUI Android application.

## Projects

- `VicServoPriceWatch` — Windows desktop application targeting Windows and using Windows Forms.
- `VicServoPriceWatch.Core` — shared .NET library containing functionality used by the applications.
- `VicServoPriceWatch.Android` — .NET MAUI Android application targeting `net10.0-android`.

## Requirements

- Visual Studio with the required .NET desktop and .NET MAUI workloads installed
- .NET 10 SDK

The Android deployment instructions below apply only to the `VicServoPriceWatch.Android` project.

## Run the Windows application

Open the solution in Visual Studio, select `VicServoPriceWatch` as the startup project, and press **F5**. The Windows project can be built and run independently of the Android project.

## Run the Android application on a phone

1. Enable **Developer options** on the phone by tapping **Build number** seven times in **Settings → About phone**.
2. Enable **USB debugging** in **Developer options**.
3. Connect the phone to the computer and accept the USB debugging prompt.
4. Open the solution in Visual Studio.
5. Select `VicServoPriceWatch.Android` as the startup project.
6. Select the phone as the debug target.
7. Press **F5** to run with the debugger, or **Ctrl+F5** to run without it.

Visual Studio builds and installs the app on the phone. Fast Deployment is normal for this development workflow. The app remains installed after Visual Studio stops running it.

## Create an APK for direct installation

An APK can be copied to an Android phone and installed directly. From the repository directory, run:

```powershell
dotnet publish .\VicServoPriceWatch.Android\VicServoPriceWatch.Android.csproj `
  -f net10.0-android `
  -c Release `
  -p:AndroidPackageFormats=apk `
  -p:AndroidFastDeploymentType=None
```

The APK is generated in:

```text
VicServoPriceWatch.Android\bin\Release\net10.0-android\publish\
```

Copy the `.apk` file to the phone and open it to install. Android may require permission to install apps from that source.

## Create an Android App Bundle

An `.aab` file is intended for publishing through Google Play and cannot normally be installed directly on a phone. Use the Visual Studio publish workflow or publish with the Android package format set to `aab` when preparing a store release.
