# UnitySkyboxCreator
A useful tool to generate a skybox material from a camera placed in a scene.

Takes 6 pictures from the location of the camera to construct a cube map, and then packages it into a skybox material which can be dragged into the environment settings.

## To Install

You need to first gain access to its scoped registry, then install the package from this registry

### Load registry (will make updates easier)
1) Navigate to Edit > Project Settings
2) Click on package manager tab, go to Scoped Registries section, hit + to add a new registry
3) Fill in the following info:

```
Name: AdamBebko
URL: https://registry.npmjs.org
Scope(s): com.adambebko
```

### Install Package
Next in Window > Package manager, select "my registries" from the dropdown

Under the Adam Bebko registry, select the package and install the most recent version

For more information on scoped registries: https://docs.unity3d.com/Manual/upm-scoped.html

### To Update Package

In Window > Package manager, select "my registries" from the dropdown

Under the Adam Bebko registry, select the package and update to the most recent version


