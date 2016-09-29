#!/usr/bin/env python
# coding: utf8

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
        """
            Can be used to pass a context to store various info and pass them
            to your callbacks
        """
        def __init__(self):
            self.nb_track_played = 0
            self.dz_content_url = ""
            self.dz_index_in_queue_list = 0
            self.repeat_mode = 0
            self.is_shuffle_mode = False
            self.connect_handle = 0
            self.player_handle = 0
            self.streaming_mode = 0

    def __init__(self, debug_mode=False):
        self.debug_mode = debug_mode
        # Identifiers
        self.user_access_token = u"frS28iNAoq3J4UiShfcAa18AoXO0jAutv64zDJXoxjmV3IGBVtp"  # SET your user access token
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
        self.cache_path_set_cb = dz_activity_operation_cb_func()
        if not self.debug_mode:
            self.connection.debug_log_disable()
        else:
            print u"Device ID:", self.connection.get_device_id()
        self.player = Player(self, self.connection.handle)
        self.player.set_event_cb(self.player_cb)
        self.connection.cache_path_set(self.connection.user_profile_path, activity_operation_cb=self.cache_path_set_cb,
                                       operation_userdata=self)
        self.connection.set_access_token(self.user_access_token)
        self.connection.set_offline_mode(False)
        self.context.player_handle = self.player.handle
        self.context.connect_handle = self.connection.handle
        self.dz_player_deactivate_cb = dz_activity_operation_cb_func(self.player_on_deactivate_cb)
        self.dz_connect_deactivate_cb = dz_activity_operation_cb_func(self.connection_on_deactivate_cb)

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
            print "---- Deezer NativeSDK version: {}".format(Connection.get_build_id())
            print "---- Application ID: {}".format(self.your_application_id)
            print "---- Product ID: {}".format(self.your_application_name)

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

    def playback_start_stop(self):
        if not self.player.is_playing:
            if self.context.streaming_mode == ConnectionStreamingMode.ON_DEMAND:
                self.player.play(command=PlayerCommand.START_TRACKLIST, index=PlayerIndex.CURRENT)
        elif self.context.streaming_mode == ConnectionStreamingMode.RADIO:
            self.player.play(command=PlayerCommand.START_TRACKLIST, index=PlayerIndex.CURRENT)
        else:
            self.log("STOP => {}".format(self.player.current_content))
            self.player.stop()

    def playback_play_pause(self):
        if self.player.is_playing:
            self.log("PAUSE track n° {} of => {}".format(self.context.nb_track_played, self.context.dz_content_url))
            self.player.pause()
        else:
            self.log("RESUME track n° {} of => {}".format(self.context.nb_track_played, self.context.dz_content_url))
            self.player.resume()

    def playback_next(self):
        self.log("NEXT => {}".format(self.context.dz_content_url))
        self.player.play(command=PlayerCommand.START_TRACKLIST, index=PlayerIndex.NEXT)

    def playback_previous(self):
        self.log("PREVIOUS => {}".format(self.context.dz_content_url))
        self.player.play(command=PlayerCommand.START_TRACKLIST, index=PlayerIndex.PREVIOUS)

    def playback_toggle_repeat(self):
        self.context.repeat_mode += 1
        if self.context.repeat_mode > PlayerRepeatMode.ALL:
            self.context.repeat_mode = PlayerRepeatMode.OFF
        self.log("REPEAT mode => {}".format(self.context.repeat_mode))
        self.player.set_repeat_mode(self.context.repeat_mode)

    def playback_toggle_random(self):
        self.context.is_shuffle_mode = not self.context.is_shuffle_mode
        self.log("SHUFFLE mode => {}".format("ON" if self.context.is_shuffle_mode else "OFF"))
        self.player.enable_shuffle_mode(self.context.is_shuffle_mode)

    def load_content(self, content):
        self.log("LOAD => {}".format(self.context.dz_content_url))
        self.context.dz_content_url = content
        self.player.load(content)

    def shutdown(self):
        if self.context.player_handle:
            self.log("SHUTDOWN PLAYER - player_handle = {}".format(self.context.player_handle))
            self.player.shutdown(activity_operation_cb=self.dz_player_deactivate_cb,
                                 operation_user_data=self)
        elif self.context.connect_handle:
            self.log("SHUTDOWN CONNECTION - connect_handle = {}".format(self.context.connect_handle))
            self.connection.shutdown(activity_operation_cb=self.dz_connect_deactivate_cb,
                                     operation_user_data=self)

    # We set the callback for player events, to print various logs and listen to events
    @staticmethod
    def player_event_callback(handle, event, userdata):
        # We retrieve our deezer app
        streaming_mode = ConnectionStreamingMode.UNKNOWN
        app = cast(userdata, py_object).value
        event_type = Player.get_event(event)
        idx = 0
        if Player.get_queuelist_context(event, streaming_mode, idx):
            streaming_mode = ConnectionStreamingMode.ON_DEMAND
            idx = PlayerIndex.INVALID
        # Update the streaming mode if relevant
        if streaming_mode != ConnectionStreamingMode.UNKNOWN:
            app.context.streaming_mode = streaming_mode
            app.context.dz_index_in_queue_list = idx
        # Print track info after the track is loaded and selected
        if event_type == PlayerEvent.QUEUELIST_TRACK_SELECTED:
            can_pause_unpause = c_bool()
            can_seek = c_bool()
            no_skip_allowed = c_int()
            is_preview = Player.is_selected_track_preview(event)
            Player.event_track_selected_rights(event, can_pause_unpause, can_seek, no_skip_allowed)
            selected_dz_api_info = Player.event_track_selected_dzapiinfo(event)
            next_dz_api_info = Player.event_track_selected_next_track_dzapiinfo(event)
            app.log(u"==== PLAYER_EVENT ==== {0} - is_preview: {1}"
                    .format(PlayerEvent.event_name(event_type), is_preview))
            app.log(u"\tcan_pause_unpause: {0} - can_seek: {1}"
                    .format(can_pause_unpause.value, can_seek.value))
            if selected_dz_api_info:
                app.log(u"\tnow:{0}".format(selected_dz_api_info))
            if next_dz_api_info:
                app.log(u"\tnext:{0}".format(next_dz_api_info))
            return 0
        app.log(u"==== PLAYER_EVENT ==== {0}".format(PlayerEvent.event_name(event_type)))
        if event_type == PlayerEvent.QUEUELIST_LOADED:
            app.player.play()
        return 0

    # We set the connection callback to launch the player after connection is established
    @staticmethod
    def connection_event_callback(handle, event, userdata):
        # We retrieve our deezerApp
        app = cast(userdata, py_object).value
        event_type = Connection.get_event(event)
        app.log(u"++++ CONNECT_EVENT ++++ {0}".format(ConnectionEvent.event_name(event_type)))
        # After User is authenticated we can start the player
        if event_type == ConnectionEvent.USER_LOGIN_OK:
            app.player.load(app.context.dz_content_url)
        if event_type == ConnectionEvent.USER_LOGIN_FAIL_USER_INFO:
            app.shutdown()
        return 0

    @staticmethod
    def player_on_deactivate_cb(delegate, operation_userdata, status, result):
        app = cast(operation_userdata, py_object).value
        app.player.active = False
        app.context.player_handle = 0
        app.log("Player deactivated")
        if app.context.connect_handle:
            app.log("SHUTDOWN CONNECTION - connect_handle = {}".format(app.context.connect_handle))
            app.connection.shutdown(activity_operation_cb=app.dz_connect_deactivate_cb,
                                    operation_user_data=app)
        return 0

    @staticmethod
    def connection_on_deactivate_cb(delegate, operation_userdata, status, result):
        app = cast(operation_userdata, py_object).value
        if app.context.connect_handle:
            app.connection.active = False
            app.context.connect_handle = 0
        app.log("Connection deactivated")
        return 0
