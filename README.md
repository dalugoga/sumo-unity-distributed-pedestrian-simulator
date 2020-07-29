# Enabling Pedestrian Safety Research in SUMO Through Externally Modelled Agents in Unity3D
This project was developed through courses of Advanced Methods of Modeling and Simulation and Social Simulation and Complex Analysis Systems from the Doctoral Program in Informatics Engineering at FEUP.

The goal was to facilitate the integration and implementation of pedestrian models in the traffic simulator SUMO. To achieve this, the SUMO simulation was recreated in Unity3D. This way new model can be implemented and verified in Unity, while keeping the great SUMO traffic simulation as it's backbone.

Currently it is possible to sync SUMO to Unity3D and extarnally simulate pedestrians in Unity with the Social Forces Model, and have that simualtion be mirrored in SUMO, while preserving all the pedestrian-vehicle interactions.

## Instalation
### Step #1: Install Sumo
Follow all the instruction given in this guide: [[https://sumo.dlr.de/docs/Installing/Windows_Build.html](https://sumo.dlr.de/docs/Installing/Windows_Build.html)]

Don't forget to install all the additional libraries and python modules.

### Step #2: Install Unity3D
The latest Unity3D version should work, but to be sure download version 2019.2.10f1.

### Step #3: Clone this repository

## Running the Simulator
### Step 1: Run SUMO
Open command line in the root of the project and run 

```shell
python "SUMO Network\runner.py"
```
After the SUMO GUI window opens, press the play button or Ctrl + A.

### Step 2: Run Unity3D scene.
Open the Unity3D project in Unity editor and select the scene [SUMO Perfect Backup](SSASC%20-%20SUMO%20Unity%20Scene/Assets/Scenes/Sumo%20Perfect%20Backup.unity).
Press play and SUMO and Unity should sync up automatically.
