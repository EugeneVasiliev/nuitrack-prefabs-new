**Main**

Nuitrack Unity Package (tutorials inside)  
Look actual version in ProjectSettings/ProjectVersion.txt

**Namespace convention**

All scripts and shaders must be nested in the name space of the corresponding location directory, with the root **NuitrackSDK**.  
Common and frequently used components from _NuitrackSDK/Nuitrack_ can be located in the _NuitrackSDK_ root.

At the same time, they need to be structured according to their meaning, for example, the scripts associated with the skeleton should be attributed to `NuitrackSDK.Skeleton`, or with frames in `NuitrackSDK.Frame`

The paths of the scripts should be clear and consistent. Spaces are skipped.

- Example script: _Assets/NuitrackSDK/VicoVRCalibration.BackTextureCreator.cs_  
Will have the namespace: `NuitrackSDK.VicoVRCalibration`


- Example shader: _Assets/NuitrackSDK/NuitrackDemos/Shaders/CustomMesh.shader_  
It will have the title: `Shader "NuitrackSDK/NuitrackDemos/CustomMesh"`
