#!/usr/bin/python
import time

from player import *
from connection import *


class MyDeezerApp(object):
    """A simple deezer application using NativeSDK

    Initialize a connection and a player, then load and play a song.

    Attributes:
        connection  A Connection instance to store connection info
        player      A Player instance to store the player's data
        debug_mode  When True displays event and API logs
    """

    def __init__(self, debug_mode=False):
        self.debug_mode = debug_mode
        # Identifiers
        self.user_access_token = "frOxVGZdmtWbW1pzgHfMPzaMSNNCyVvbRdmtfrzu8IHfFbzsCev"  # SET your user access token
        self.your_application_id = "190262"  # SET your application id
        self.your_application_name = "PythonSampleApp"  # SET your application name
        self.your_application_version = "00001"  # SET your application version
        if platform.system() == 'Windows':
            self.user_cache_path = "c:\\dzr\\dzrcache_NDK_SAMPLE"  # SET the user cache path, the path must exist
        else:
            self.user_cache_path = "/var/tmp/dzrcache_NDK_SAMPLE"  # SET the user cache path, the path must exist
        self.connection = Connection(self.your_application_id, self.your_application_name,
                                     self.your_application_version, self.user_cache_path, 0, 0, 0)
        self.player = None
        self.player_cb = dz_on_event_cb_func(self.player_event_callback)
        self.cache_path_set_cb = dz_activity_operation_cb_func(self.operation_cb)
        self._initialize_connection()
        self._activate_connection()
        self._initialize_player()

    def _initialize_connection(self):
        """
        Set up connection
        :param event_callback: The event listener triggered when the connection
            state change.
        :param debug_mode: Set to true to mute API and callback logs.
        """
        self.connection.set_event_cb(self.connection_event_callback)
        self.connection.init_handle()
        if not self.debug_mode:
            self.connection.debug_log_disable()
        else:
            print "Device ID:", self.connection.get_device_id()

    def _activate_connection(self):
        """
        Activate the connection. Must be used after initialization.
        :param user_access_token: The token given by OAuth 2 process.
            Refer to the API documentation.
        """
        self.connection.activate(self)
        self.connection.cache_path_set(self.connection.user_profile_path, activity_operation_cb=self.cache_path_set_cb, operation_userdata=self)
        self.connection.set_access_token(self.user_access_token)
        self.connection.connect_offline_mode()

    def _initialize_player(self):
        """
        Set up the player
        :param event_callback: The event listener triggered when the connection
            state change.
        """
        self.player = Player(self.connection)
        self.player.set_event_cb(self.player_cb)

    def _activate_player(self):
        """
        Activate the player. Must be used after calling initialize_player
        Sets the track that will be played using start_player.
        :param track: The track to be played.
        """
        self.player.activate(self)

    def set_song(self, track):
        """
        Load the current track and play it.
        """
        self.player.track = track

    def start(self):
        self._activate_player()
        while self.connection.active and self.player.active:
            time.sleep(1)

    def log(self, message):
        """
        Print a log message unless debug_mode is False
        :param message: The message to display
        """
        if self.debug_mode:
            print message

    # We set the callback for player events, to print various logs and listen to events
    @staticmethod
    def player_event_callback(handle, event, delegate):
        app = cast(delegate, py_object).value
        event_names = [
            'UNKNOWN',
            'LIMITATION_FORCED_PAUSE',
            'QUEUELIST_LOADED',
            'QUEUELIST_TRACK_NO_RIGHT',
            'QUEUELIST_TRACK_NOT_AVAILABLE_OFFLINE',
            'QUEUELIST_TRACK_RIGHTS_AFTER_AUDIOADS',
            'QUEUELIST_SKIP_NO_RIGHT',
            'QUEUELIST_TRACK_SELECTED',
            'QUEUELIST_NEED_NATURAL_NEXT',
            'MEDIASTREAM_DATA_READY',
            'MEDIASTREAM_DATA_READY_AFTER_SEEK',
            'RENDER_TRACK_START_FAILURE',
            'RENDER_TRACK_START',
            'RENDER_TRACK_END',
            'RENDER_TRACK_PAUSED',
            'RENDER_TRACK_SEEKING',
            'RENDER_TRACK_UNDERFLOW',
            'RENDER_TRACK_RESUMED',
            'RENDER_TRACK_REMOVED'
        ]
        streaming_mode = c_int()
        idx = c_int()
        event_type = Player.get_event(event)
        # Print track info after the track is loaded and selected
        if event_type == PlayerEvent.QUEUELIST_TRACK_SELECTED:
            can_pause_unpause = c_bool()
            can_seek = c_bool()
            no_skip_allowed = c_int()
            is_preview = libdeezer.dz_player_event_track_selected_is_preview(c_void_p(event))
            libdeezer.dz_player_event_track_selected_rights(
                c_void_p(event),
                byref(can_pause_unpause),
                byref(can_seek),
                byref(no_skip_allowed)
            )
            selected_dz_api_info = libdeezer.dz_player_event_track_selected_dzapiinfo(c_void_p(event))
            next_dz_api_info = libdeezer.dz_player_event_track_selected_next_track_dzapiinfo(c_void_p(event))
            app.log("==== PLAYER_EVENT ==== {0} for idx: {1} - is_preview: {2}"
                    .format(event_names[event_type], idx.value, is_preview))
            app.log("\tcan_pause_unpause: {0} - can_seek: {1}"
                    .format(can_pause_unpause.value, can_seek.value))
            if selected_dz_api_info:
                app.log("\tnow:{0}".format(selected_dz_api_info))
            if next_dz_api_info:
                app.log("\tnext:{0}".format(next_dz_api_info))
            app.player.nb_tracks_played += 1
            return 0
        app.log("==== PLAYER_EVENT ==== {0} for idx: {1}".format(event_names[event_type], idx.value))
        # Will stop execution after the track is finished
        if event_type == PlayerEvent.RENDER_TRACK_END:
            app.log("\tnb_track_to_play: {0}\tnb_track_played: {1}"
                    .format(app.player.nb_tracks_to_play, app.player.nb_tracks_played))
            if app.player.nb_tracks_played != -1 and app.player.nb_tracks_to_play == app.player.nb_tracks_played:
                app.player.shutdown()
            else:
                app.player.launch_play()
        if event_type == PlayerEvent.QUEUELIST_NEED_NATURAL_NEXT:
            app.player.launch_play()
        return 0

    # We set the connection callback to launch the player after connection is established
    @staticmethod
    def connection_event_callback(handle, event, delegate):
        app = cast(delegate, py_object).value
        event_names = [
            'UNKNOWN',
            'USER_OFFLINE_AVAILABLE',
            'USER_ACCESS_TOKEN_OK',
            'USER_ACCESS_TOKEN_FAILED',
            'USER_LOGIN_OK',
            'USER_LOGIN_FAIL_NETWORK_ERROR',
            'USER_LOGIN_FAIL_BAD_CREDENTIALS',
            'USER_LOGIN_FAIL_USER_INFO',
            'USER_LOGIN_FAIL_OFFLINE_MODE',
            'USER_NEW_OPTIONS',
            'ADVERTISEMENT_START',
            'ADVERTISEMENT_STOP'
        ]
        event_type = Connection.get_event(event)
        app.log("++++ CONNECT_EVENT ++++ {0}".format(event_names[event_type]))
        # After User is authenticated we can start the player
        if event_type == ConnectionEvent.USER_LOGIN_OK:
            app.player.launch_play()
        return 0

    @staticmethod
    def operation_cb(delegate, operation_userdata, status, result):
        print "This is an example of an activity_operation_cb"
        return 0

