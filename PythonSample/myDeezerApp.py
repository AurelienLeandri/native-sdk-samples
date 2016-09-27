#!/usr/bin/python
# coding: utf8
import inspect
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
    class AppContext(object):
        def __init__(self):
            self.nb_track_played = 0
            self.is_playing = False
            self.dz_content_url = ""
            self.dz_index_in_queue_list = 0
            self.repeat_mode = 0
            self.is_shuffle_mode = False

    def __init__(self, debug_mode=False):
        self.debug_mode = debug_mode
        # Identifiers
        self.user_access_token = u"frglBrm4qzcaC1IspVEFPXAnVe4APOmWk7kNPCjwg0Hw3uWM0bO"  # SET your user access token
        self.your_application_id = u"190262"  # SET your application id
        self.your_application_name = u"PythonSampleApp"  # SET your application name
        self.your_application_version = u"00001"  # SET your application version
        if platform.system() == u'Windows':
            self.user_cache_path = u"c:\\dzr\\dzrcache_NDK_SAMPLE"  # SET the user cache path, the path must exist
        else:
            self.user_cache_path = u"/var/tmp/dzrcache_NDK_SAMPLE"  # SET the user cache path, the path must exist
        self.context = self.AppContext()
        self.connection = Connection(self.your_application_id, self.your_application_name,
                                     self.your_application_version, self.user_cache_path,
                                     self.connection_event_callback, 0, 0)
        self.player = None
        self.player_cb = dz_on_event_cb_func(self.player_event_callback)
        self.cache_path_set_cb = dz_activity_operation_cb_func(self.operation_cb)
        self._initialize_connection()
        self._initialize_player()
        self._activate_connection()

    def _initialize_connection(self):
        """Set up connection"""
        if not self.debug_mode:
            self.connection.debug_log_disable()
        else:
            print u"Device ID:", self.connection.get_device_id()

    def log_connect_info(self):
        """Print connection info"""
        if self.debug_mode:
            print "---- Deezer NativeSDK version: {}".format(libdeezer.dz_connect_get_build_id())
            print "---- Application ID: {}".format(self.your_application_id)
            print "---- Product ID: {}".format(self.your_application_name)
            print "---- Product BUILD ID: {}".format(self.your_application_version)
            print "---- User Profile Path: {}".format(self.your_application_version)

    def _activate_connection(self):
        """
        Activate the connection. Must be used after initialization.
        """
        self.connection.activate(self)
        self.connection.cache_path_set(self.connection.user_profile_path, activity_operation_cb=self.cache_path_set_cb,
                                       operation_userdata=self)
        self.connection.set_access_token(self.user_access_token)
        self.connection.connect_offline_mode()

    def _initialize_player(self):
        """
        Set up the player
        """
        self.player = Player(self.connection)
        self.player.set_event_cb(self.player_cb)

    def _activate_player(self):
        """
        Activate the player. Must be used after calling initialize_player
        Sets the track that will be played using start_player.
        """
        self.player.activate(self)

    def set_song(self, track):
        """
        Load the current track and play it.
        """
        # TODO: remove track from player
        self.context.dz_content_url = track
        self.player.track = track

    def log(self, message):
        """
        Print a log message unless debug_mode is False
        :param message: The message to display
        """
        frame_record = inspect.stack()[1]
        frame = frame_record[0]
        info = inspect.getframeinfo(frame)
        if self.debug_mode:
            print message

    def process_command(self, command):
        c = ''.join(command.splitlines())
        call = {
            'S': self.playback_start_stop,
            'P': self.playback_play_pause,
            '+': self.playback_next,
            '-': self.playback_previous,
            'R': self.playback_toggle_repeat,
            '?': self.playback_toggle_random,
            'Q': self.shutdown
        }
        if c not in call.keys():
            print "Invalid command, try again"
            self.log_command_info()
            return
        call.get(c)()

    @staticmethod
    def log_command_info():
        print "######### MENU #########"
        print "- Please enter keys for command -"
        print "\tS : START/STOP"
        print "\tP : PLAY/PAUSE"
        print "\t+ : NEXT"
        print "\t- : PREVIOUS"
        print "\tR : NEXT REPEAT MODE"
        print "\t? : TOGGLE SHUFFLE MODE"
        print "\tQ : QUIT"
        print "########################"

    # TODO: Streaming mode enum
    # TODO: Index enum
    # TODO: Add player and connect handles to context class
    def playback_start_stop(self):
        if not self.context.is_playing:
            if self.context.streaming_mode == ConnectionStreamingMode.ONDEMAND:
                # TODO: Check arguments for play
                self.player.play(self.context.player_handle, None, None, PlayerCommand.START_TRACKLIST,
                                 PlayerIndex.IN_QUEUELIST_CURRENT)
        elif self.context.streaming_mode == ConnectionStreamingMode.RADIO:
            self.player.play(self.context.player_handle, None, None, PlayerCommand.START_TRACKLIST,
                             PlayerIndex.IN_QUEUELIST_CURRENT)
        else:
            self.log("STOP => {}".format(self.context.dz_content_url))
            # TODO: Add function stop
            self.player.stop(self.context.player_handle, None, None)

    def playback_play_pause(self):
        if self.context.is_playing:
            self.log("PAUSE track n° {} of => {}".format(self.context.nb_track_played, self.context.dz_content_url))
            # TODO: Add function pause
            self.player.pause(self.context.player_handle, None, None)
        else:
            self.log("RESUME track n° {} of => {}".format(self.context.nb_track_played, self.context.dz_content_url))
            # TODO: Add function resume
            self.player.resume(self.context.player_handle, None, None)

    def playback_next(self):
        self.log("NEXT => {}".format(self.context.dz_content_url))
        self.player.play(self.context.player_handle, None, None, PlayerCommand.START_TRACKLIST,
                         PlayerIndex.IN_QUEUELIST_NEXT)

    def playback_previous(self):
        self.log("PREVIOUS => {}".format(self.context.dz_content_url))
        self.player.play(self.context.player_handle, None, None, PlayerCommand.START_TRACKLIST,
                         PlayerIndex.IN_QUEUELIST_PREVIOUS)

    def playback_toggle_repeat(self):
        self.context.repeat_mode += 1
        if self.context.repeat_mode > PlayerRepeatMode.ALL:
            self.context.repeat_mode = PlayerRepeatMode.OFF
        self.log("REPEAT mode => {}".format(self.context.repeat_mode))
        # TODO: Add function set_repeat_mode
        self.player.set_repeat_mode(self.context.player_handle, None, None, self.context.repeat_mode)

    def playback_toggle_random(self):
        self.context.is_shuffle_mode = not self.context.is_shuffle_mode
        self.log("SHUFFLE mode => {}".format("ON" if self.context.is_shuffle_mode else "OFF"))
        # TODO: Add function enable_shuffle_mode
        self.player.enable_shuffle_mode(self.context.player_handle, None, None, self.context.is_shuffle_mode)

    def load_content(self):
        self.log("LOAD => {}".format(self.context.dz_content_url))
        self.player.load(self.context.player_handle, None, None, self.context.dz_content_url)

    # TODO: Add arguments to player and connection shutdown functions
    def shutdown(self):
        self.log("SHUTDOWN (1/2) - dzplayer = {}".format(self.context.player_handle))
        if self.context.player_handle:
            self.player.shutdown()
        self.log("SHUTDOWN (2/2) - dzconnect = {}".format(self.context.connect_handle))
        if self.context.connect_handle:
            self.connection.shutdown()

    def start(self):
        self._activate_player()

    # We set the callback for player events, to print various logs and listen to events
    @staticmethod
    def player_event_callback(handle, event, delegate):
        # We retrieve our deezer app
        app = cast(delegate, py_object).value
        event_names = [
            u'UNKNOWN',
            u'LIMITATION_FORCED_PAUSE',
            u'QUEUELIST_LOADED',
            u'QUEUELIST_TRACK_NO_RIGHT',
            u'QUEUELIST_TRACK_NOT_AVAILABLE_OFFLINE',
            u'QUEUELIST_TRACK_RIGHTS_AFTER_AUDIOADS',
            u'QUEUELIST_SKIP_NO_RIGHT',
            u'QUEUELIST_TRACK_SELECTED',
            u'QUEUELIST_NEED_NATURAL_NEXT',
            u'MEDIASTREAM_DATA_READY',
            u'MEDIASTREAM_DATA_READY_AFTER_SEEK',
            u'RENDER_TRACK_START_FAILURE',
            u'RENDER_TRACK_START',
            u'RENDER_TRACK_END',
            u'RENDER_TRACK_PAUSED',
            u'RENDER_TRACK_SEEKING',
            u'RENDER_TRACK_UNDERFLOW',
            u'RENDER_TRACK_RESUMED',
            u'RENDER_TRACK_REMOVED'
        ]
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
            app.log(u"==== PLAYER_EVENT ==== {0} for idx: {1} - is_preview: {2}"
                    .format(event_names[event_type], idx.value, is_preview))
            app.log(u"\tcan_pause_unpause: {0} - can_seek: {1}"
                    .format(can_pause_unpause.value, can_seek.value))
            if selected_dz_api_info:
                app.log(u"\tnow:{0}".format(selected_dz_api_info))
            if next_dz_api_info:
                app.log(u"\tnext:{0}".format(next_dz_api_info))
            app.player.nb_tracks_played += 1
            return 0
        app.log(u"==== PLAYER_EVENT ==== {0} for idx: {1}".format(event_names[event_type], idx.value))
        # Will stop execution after the track is finished
        if event_type == PlayerEvent.RENDER_TRACK_END:
            app.log(u"\tnb_track_to_play: {0}\tnb_track_played: {1}"
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
        # We retrieve our deezerApp
        app = cast(delegate, py_object).value
        event_names = [
            u'UNKNOWN',
            u'USER_OFFLINE_AVAILABLE',
            u'USER_ACCESS_TOKEN_OK',
            u'USER_ACCESS_TOKEN_FAILED',
            u'USER_LOGIN_OK',
            u'USER_LOGIN_FAIL_NETWORK_ERROR',
            u'USER_LOGIN_FAIL_BAD_CREDENTIALS',
            u'USER_LOGIN_FAIL_USER_INFO',
            u'USER_LOGIN_FAIL_OFFLINE_MODE',
            u'USER_NEW_OPTIONS',
            u'ADVERTISEMENT_START',
            u'ADVERTISEMENT_STOP'
        ]
        event_type = Connection.get_event(event)
        app.log(u"++++ CONNECT_EVENT ++++ {0}".format(event_names[event_type]))
        # After User is authenticated we can start the player
        if event_type == ConnectionEvent.USER_LOGIN_OK:
            app.player.launch_play()
        if event_type == ConnectionEvent.USER_LOGIN_FAIL_USER_INFO:
            exit(1)
        return 0

    @staticmethod
    def operation_cb(delegate, operation_userdata, status, result):
        app = cast(delegate, py_object).value
        app2 = cast(operation_userdata, py_object).value
        app.log(u"This is an example of")
        app2.log(u"an activity operation callback.")
        return 0

