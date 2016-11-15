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
│       ├── Bins
│       │      └── Platforms
│       ├── Linux
│       │   ├── arm
│       │   │   └── libdeezer.so
│       │   ├── i386
│       │   │   └── libdeezer.so
│       │   └── x86_64
│       │       └── libdeezer.so
│       ├── MacOSX
│       │   └── libdeezer.framework
│       │       └── Versions
│       │           	└── Current
│       │               	└── libdeezer
│       └── Windows
│           └── DLLs
│               ├── libdeezer.x64.dll
│               └── libdeezer.x86.dll
├── UnityPlayer
└── README.md
```

* This folder contains the Bins of the library depending on the platform:

```
<NativeSDK>
└── Bins
      └── Platforms
	       ├── Linux
	       │   ├── arm
	       │   │   └── libdeezer.so
	       │   ├── i386
	       │   │   └── libdeezer.so
	       │   └── x86_64
	       │       └── libdeezer.so
	       ├── MacOSX
	       │   └── libdeezer.framework
	       │       └── Versions
	       │           	└── Current
	       │               	└── libdeezer
	       └── Windows
	           └── DLLs
	               ├── libdeezer.x64.dll
	               └── libdeezer.x86.dll
```

* Copy the library corresponding to your architecture into the UnityPlayer/Assets folder. If you are running on MacOSX, rename it "libdeezer.bundle" and on Windows "libdeezer.dll".
* Open Unity
* Go to File > Open Project
* Select the folder UnityPlayer
* After the project is loaded, go to the Project panel
* Got into the folder Scenes and click Interface.scene
* Click Play


### Run this sample


In the application, a playlist is loaded by default. You can change the content in the input text field and load it by clicking on the "Go" button.

You can load 3 types of content:

```
album/CODE
playlist/CODE
track/CODE
```

If no track is loaded, that means that either the tracklist is unavailable or only for premium users.

### Limitations

* More development is necessary to display a track mix like a user mix or a radio since the tracks are known on-the-go.

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


 [1]: http://developers.deezer.com/sdk/native
