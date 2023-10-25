# Narupa Unity Plugin

A set of libraries for creating Narupa applications in Unity.

The easiest way to use these libraries is to add them as a submodule to your 
Unity3D project.

```
cd my-narupa-unity-project
mkdir -p Assets/Plugins 
git submodule add git@github.com:IRL2/NarupaUnityPlugin.git Assets/Plugins/Narupa
git submodule update --init 
```

Whenever you need to update the Narupa plugin, you can just run

```
git submodule update
```
