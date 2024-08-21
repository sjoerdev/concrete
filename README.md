# Concrete
Modern .NET 8 based game engine.

## Features

- unity inspired structure
- component based architecture
- simple imgui based editor
- simple opengl renderer
- supports any model file

## Example
```csharp
var testScene = new Scene();
SceneManager.LoadScene(testScene);

var cameraObject = GameObject.Create();
cameraObject.AddComponent<Camera>();
cameraObject.name = "Camera";

var modelObject = GameObject.Create();
modelObject.AddComponent<ModelRenderer>().modelPath = "res/models/cesium.glb";
modelObject.name = "Cesium Model";

var lightObject = GameObject.Create();
lightObject.AddComponent<DirectionalLight>();
lightObject.transform.localEulerAngles = new Vector3(20, 135, 0);
lightObject.name = "Directional Light";
```

## Gallery
<img src="https://github.com/user-attachments/assets/8a026e3f-5bf2-4f8d-b691-8d16b5d475fe" width="800"/>
<img src="https://github.com/user-attachments/assets/43502d68-cab4-4b07-a745-cc4643c0a764" width="800"/>
<img src="https://github.com/user-attachments/assets/27b4653a-58cc-441a-a272-9b4cd1bc7700" width="800"/>
