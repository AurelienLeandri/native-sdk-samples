![Deezer](http://cdn-files.deezer.com/img/press/new_logo_white.jpg "Deezer") 

## UnityPlayer

UnityPlayer is a C# application using Native SDK. It displays a player UI and allow the user to load a content (track album or playlist) and select a song from the playlist.

### Features

 - C# Wrapper to create your own C# project using Native SDK
 - User authentication
 - Playing a Deezer content (track, album or playlist)
 - Nice UI using Unity engine.

### Build instructions

* Download the latest version of the [Deezer Native SDK][1]
* Unzip it and place the folder, renamed into NativeSDK, at the root of this repository, such as shown below:
```
<native-sdk-samples>
├── NativeSDK
│      ├── Bins
│      └── Include
├── UnityPlayer
└── README.md
```

### Project structure

```
<UnityPlayer>
├── Assets
│      ├── Materials
│      ├── Prefabs
│      ├── Scenes
│      │      └── Interface.scene
│      ├── Scripts <contains the C# wrapper and scripts>
│      │      ├── UI
│      │      │	  	Script for each element of the UI.
│      │      ├── Wrapper
│      │      │	  	Contains the wrapper of the functions of the C lib. Can
│      │      │	  	be used for any C# project independently.
│      │      ├── Utils
│      │      │	  	Listener interface and json parser.
│      │      ├── DataInfo
│      │      │	  	Contain the classes that represent json data.
│      │      ├── ApplicationMainScript.cs
│      │      │	  	Main controller of the application.
│      │      └── ApplicationElement.cs
│      │      	        Base class for every UI element.
│      ├── Textures
│      └── Prefabs
├── <unity folders>
└── README.md
```


### Run this sample

- Open Unity
- Go to File > Open Project
- Select the folder UnityPlayer
- After the project is loaded, go to the Project panel
- Got into the folder Scenes and click Interface.scene
- Click Play

In the application, a playlist is loaded by default. You can change the content in the input text field and load it by clicking on the "Go" button.

You can load 3 types of content:

```
album/CODE
playlist/CODE
track/CODE
```

If no track is loaded, that means the tracklist is unavailable.

### Limitations

This sample is available only for 64-bits platforms.
It is currently not possible to load and play a mix (user mix and radio).

 [1]: http://developers.deezer.com/sdk/native
