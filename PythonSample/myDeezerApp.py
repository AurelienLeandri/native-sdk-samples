#!/usr/bin/python
# coding: utf8
import inspect

from wrapper.deezer_connect import *
from wrapper.deezer_player import *


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
        self.user_access_token = u"fr8VDJgNPRCk6k55W8sEM1hX4iqDlFUXw3FAU04AK3Hbq4PU9Xt"  # SET your user access token
        self.your_application_id = u"190262"  # SET your application id
        self.your_application_name = u"PythonSampleApp"  # SET your application name
        self.your_application_version = u"00001"  # SET your application version
        if platform.system() == u'Windows':
            self.user_cache_path = u"c:\\dzr\\dzrcache_NDK_SAMPLE"  # SET the user cache path, the path must exist
        else:
            self.user_cache_path = u"/var/tmp/dzrcache_NDK_SAMPLE"  # SET the user cache path, the path must exist
        self.context = self.AppContext()
        self.connection = Connection(self, self.your_application_id, self.your_application_name,
                                     self.your_application_version, self.user_cache_path,
                                     self.connection_event_callback, 0, 0)
        self.player = None
        self.player_cb = dz_on_event_cb_func(self.player_event_callback)
        self.cache_path_set_cb = dz_activity_operation_cb_func(self.operation_cb)
        if not self.debug_mode:
            self.connection.debug_log_disable()
        else:
            print u"Device ID:", self.connection.get_device_id()
        self.player = Player(self, self.connection)
        self.player.set_event_cb(self.player_cb)
        self.connection.cache_path_set(self.connection.user_profile_path, activity_operation_cb=self.cache_path_set_cb,
                                       operation_userdata=self)
        self.connection.set_access_token(self.user_access_token)
        self.connection.set_offline_mode(False)

    def log(self, message):
        """
        Print a log message unless debug_mode is False
        :param message: The message to display
        """
        if self.debug_mode:
            print message

    def log_connect_info(self):
        """Print connection info"""
        if self.debug_mode:
            print "---- Deezer NativeSDK version: {}".format(libdeezer.dz_connect_get_build_id())
            print "---- Application ID: {}".format(self.your_application_id)
            print "---- Product ID: {}".format(self.your_application_name)

    # TODO: Hid that in connection
    # TODO: Put all init ant activate in connection and player constructors

    def set_song(self, track):
        """
        Load the current track and play it.
        """
        # TODO: remove track from player
        self.context.dz_content_url = track
        self.player.track = track

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

    # We set the callback for player events, to print various logs and listen to events
    @staticmethod
    def player_event_callback(handle, event, delegate):
        # We retrieve our deezer app
        app = cast(delegate, py_object).value
        # TODO: idx use ??
        idx = c_int()
        event_type = Player.get_event(event)
        # Print track info after the track is loaded and selected
        if event_type == PlayerEvent.QUEUELIST_TRACK_SELECTED:
            can_pause_unpause = c_bool()
            can_seek = c_bool()
            no_skip_allowed = c_int()
            # TODO wrap these libdeezer calls
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
                    .format(PlayerEvent.event_name(event_type), idx.value, is_preview))
            app.log(u"\tcan_pause_unpause: {0} - can_seek: {1}"
                    .format(can_pause_unpause.value, can_seek.value))
            if selected_dz_api_info:
                app.log(u"\tnow:{0}".format(selected_dz_api_info))
            if next_dz_api_info:
                app.log(u"\tnext:{0}".format(next_dz_api_info))
            return 0
        app.log(u"==== PLAYER_EVENT ==== {0} for idx: {1}".format(PlayerEvent.event_name(event_type), idx.value))
        # Will stop execution after the track is finished
        # TODO: when to stop execution if no nb_track_to_play ?
        if event_type == PlayerEvent.RENDER_TRACK_END:
            context.player.launch_play()
        return 0

    # We set the connection callback to launch the player after connection is established
    @staticmethod
    def connection_event_callback(handle, event, delegate):
        # We retrieve our deezerApp
        app = cast(delegate, py_object).value
        event_type = Connection.get_event(event)
        app.log(u"++++ CONNECT_EVENT ++++ {0}".format(ConnectionEvent.event_name(event_type)))
        # After User is authenticated we can start the player
        if event_type == ConnectionEvent.USER_LOGIN_OK:
            app.player.launch_play()
        if event_type == ConnectionEvent.USER_LOGIN_FAIL_USER_INFO:
            exit(1)
        return 0

    # This is an example of a usable operation callback
    @staticmethod
    def operation_cb(delegate, operation_userdata, status, result):
        return 0

