<p align="center"><img width="80%" src="https://github.com/user-attachments/assets/6fadccdb-3a27-4170-afbc-98725970b0e5"></p>

<h3 align="center">Modern C# game engine with a Unity like api and structure.</h3>

<div align="center">

<img style="display: inline-block" alt="lines" src="https://sloc.xyz/github/sjoerdev/concrete/?lower=true" />
<img style="display: inline-block" alt="stars" src="https://img.shields.io/github/stars/sjoerdev/concrete?style=flat" />
<img style="display: inline-block" alt="version" src="https://img.shields.io/github/v/release/sjoerdev/concrete?include_prereleases" />
<img style="display: inline-block" alt="license" src="https://img.shields.io/badge/license-MIT-blue.svg" />

</div>

## Features

- unity inspired structure
- component based architecture
- powerful imgui based editor
- lightweight opengl renderer
- skinned mesh rendering
- complete gltf support

## Scripting
```csharp
var scene = new Scene();
LoadScene(scene);

var cameraObject = scene.AddGameObject();
cameraObject.AddComponent<Camera>();
cameraObject.name = "Main Camera";

var lightObject = scene.AddGameObject();
lightObject.AddComponent<DirectionalLight>();
lightObject.transform.localEulerAngles = new Vector3(20, 135, 0);
lightObject.name = "Directional Light";
```

## Editor

<img alt="editor" src="https://github.com/user-attachments/assets/3ba95a9a-f89a-439c-b82a-4b5c0f80b174" />

## Usage Requirements:
- The .NET 10 SDK ([Download](https://dotnet.microsoft.com/en-us/download))
- Visual C++ Redistributable ([Download](https://aka.ms/vc14/vc_redist.x64.exe))

## Building:

Download .NET 10: https://dotnet.microsoft.com/en-us/download

Building for Windows:

``dotnet publish ./Engine/Editor/Editor.csproj -o ./Build/Windows -r win-x64 -c release --sc true``

Building for Linux:

``dotnet publish ./Engine/Editor/Editor.csproj -o ./Build/Linux -r linux-x64 -c release --sc true``
