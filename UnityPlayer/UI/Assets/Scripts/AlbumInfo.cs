using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

[Serializable]
public class AlbumInfo {
	public string title;
	public string cover_small;
	public string cover_xl;
	public ArtistInfo artist;
	public List<TrackInfo> tracks = new List<TrackInfo>();
}
