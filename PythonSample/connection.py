#!/usr/bin/python
# coding: utf8


"""
    Deezer ``connection`` module for NativeSDK
    ==========================================

    Manage connection operations and the session info.

    This is a part of the Python wrapper for the NativeSDK. This module wraps
    the deezer-connect functions into several python classes. The calls to the
    C lib are done using ctypes.

    Globals
    -------

    libdeezer: The lib object loaded to perform the function calls to the C SDK
    *_func types: C func types declared to translate python functions to
        callbacks when required. See section Callback types below.

    Content summary
    ---------------

    The class used to manage connection is the Connection class. The others
    describe C enums to be used in callbacks (see below) and logs.

    Callback types
    --------------

    A bunch of this module's functions use callbacks to react to some
    connection events or to process some data. you are free to pass your funcs
    as callbacks, they are then translated to C functions and passed to the SDK
    functions. Here is a description of their parameters:

        dz_connect_on_event_cb:
            Called after connection activation and when the connection state
            changes. The callback must take 3 parameters:
            -   The connection handle (same as Connection.connect_handle)
            -   An event object used to get the event that has been caught.
                In your callback, use the static method get_event to convert the
                event object to a ConnectionEvent index.
            -   A user_data that is an object you can pass through some
                functions and that can be manipulated by the callback.

        dz_activity_operation_cb:
            Can be set in some functions of Connection class to be called after
            the operation. The callback must take 4 parameters:
            -   A delegate that is the context object to store and change info
                in the callback.
            -   An operation_userdata that is the object you can pass to the
                calling function
            -   The error status used to get the index of the error enum
            -   An event object used to get the event that has been caught

        dz_connect_crash_reporting_delegate:
            Takes nothing an returns a boolean.
            Use this to call your own crash reporting system. If left to None,
            the SDK will use its own crash reporting system.

"""

from ctypes import *
import platform

import sys

# TODO: Move to sdk import file, taking the path as parameters
lib_name = u'libdeezer.so'
if platform.system() == u'Darwin':
    lib_name = u'libdeezer'
if platform.system() == u'Windows':
    lib_name = u'libdeezer.'
    lib_name += u'x64.dll' if sys.maxsize > 2**32 else u'x86.dll'
libdeezer = cdll.LoadLibrary(lib_name)
p_type = c_uint64 if sys.maxsize > 2**32 else c_uint32

dz_on_event_cb_func = CFUNCTYPE(c_int, p_type, c_void_p, c_void_p)
dz_connect_crash_reporting_delegate_func = CFUNCTYPE(c_bool)
dz_activity_operation_cb_func = CFUNCTYPE(c_int, c_void_p, c_void_p, p_type, p_type)

libdeezer.dz_connect_new.restype = p_type
libdeezer.dz_connect_get_device_id.argtypes = [p_type]
libdeezer.dz_connect_debug_log_disable.argtypes = [p_type]
libdeezer.dz_connect_activate.argtypes = [p_type, py_object]
libdeezer.dz_connect_cache_path_set.argtypes = [p_type, c_void_p, py_object, c_char_p]
libdeezer.dz_connect_set_access_token.argtypes = [p_type, c_void_p, py_object, c_char_p]
libdeezer.dz_connect_offline_mode.argtypes = [p_type, c_void_p, py_object, c_bool]
libdeezer.dz_connect_deactivate.argtypes = [p_type, c_void_p, py_object]
libdeezer.dz_player_new.argtypes = [p_type]
libdeezer.dz_player_new.restype = p_type
libdeezer.dz_player_activate.argtypes = [p_type, py_object]
libdeezer.dz_player_set_event_cb.argtypes = [p_type, dz_on_event_cb_func]
libdeezer.dz_player_load.argtypes = [p_type, c_void_p, py_object, c_char_p]
libdeezer.dz_player_play.argtypes = [p_type, c_void_p, py_object, c_int, c_int]
libdeezer.dz_player_deactivate.argtypes = [p_type, c_void_p, c_void_p]
libdeezer.dz_player_event_get_type.argtypes = [c_void_p]
libdeezer.dz_player_event_get_type.restype = c_int


# TODO: Check if it works without pack
class DZConnectConfiguration(Structure):
    """Contains the connection info used by the sdk."""
    _fields_ = [(u'app_id', c_char_p),
                (u'product_id', c_char_p),
                (u'product_build_id', c_char_p),
                (u'user_profile_path', c_char_p),
                (u'dz_connect_on_event_cb', dz_on_event_cb_func),
                (u'anonymous_blob', c_void_p),
                (u'dz_connect_crash_reporting_delegate', dz_connect_crash_reporting_delegate_func)]


class ConnectionInitFailedError(Exception):
    def __init__(self, value):
        self.value = value

    def __str__(self):
        return repr(self.value)


class ConnectionRequestFailedError(Exception):
    def __init__(self, value):
        self.value = value

    def __str__(self):
        return repr(self.value)


class ConnectionActivationError(Exception):
    def __init__(self, value):
        self.value = value

    def __str__(self):
        return repr(self.value)


class ConnectionEvent:
    """Defines values associated to connection events.
        In the on_event_callbacks you define, you can convert the event object
        to an integer corresponding to a value of this class using get_event

        Warning: If you happen to change the values, make sure they correspond
        to the values of the corresponding C enum
    """

    def __init__(self):
        pass

    (
        UNKNOWN,
        USER_OFFLINE_AVAILABLE,
        USER_ACCESS_TOKEN_OK,
        USER_ACCESS_TOKEN_FAILED,
        USER_LOGIN_OK,
        USER_LOGIN_FAIL_NETWORK_ERROR,
        USER_LOGIN_FAIL_BAD_CREDENTIALS,
        USER_LOGIN_FAIL_USER_INFO,
        USER_LOGIN_FAIL_OFFLINE_MODE,
        USER_NEW_OPTIONS,
        ADVERTISEMENT_START,
        ADVERTISEMENT_STOP
    ) = range(0, 12)


class StreamingMode:
    """Defines values associated to the streaming mode

    Warning: If you happen to change the values, make sure they correspond
    to the values of the corresponding C enum
    """

    def __init__(self):
        pass

    (
        UNKNOWN,
        ON_DEMAND,
        RADIO
    ) = range(0, 3)


class Connection:
    """Manage and initiate connection to the API.

        Represents a connection to the API. Stores connection info, event
        callbacks and wrappers to the NativeSDK.

        Attributes:
            app_id                   The ID of the application
            product_id               The name of your application
            product_build_id         The version number
            user_profile_path        The cache path of the user. Deprecated.
            dz_connect_on_event_cb   The event listener to connection operations
            anonymous_blob           Deprecated
            dz_connect_crash_reporting_delegate     The error callback
            active                   True when connection has been activated
            connect_handle           The ID used for all operations initialized
                                     after given connection info
    """

    def __init__(self, app_id=u'', product_id=u'', product_build_id=u'',
                 user_profile_path=u'/var/tmp/dzrcache_NDK_SAMPLE', dz_connect_on_event_cb=None, anonymous_blob=None,
                 dz_connect_crash_reporting_delegate=None):
        """
        :param app_id: The ID of the application
        :param product_id: The name of your application
        :param product_build_id: The version number
        :param user_profile_path: The cache path of the user. Deprecated.
        :param dz_connect_on_event_cb: The event listener to connection
            operations
        :param anonymous_blob: Deprecated
        :param dz_connect_crash_reporting_delegate: The error callback
        :type app_id: unicode
        :type product_id: unicode
        :type product_build_id: unicode
        :type user_profile_path: unicode
        :type dz_connect_on_event_cb: function
        :type dz_connect_crash_reporting_delegate: function
        """
        self.app_id = app_id
        self.product_id = product_id
        self.product_build_id = product_build_id
        self.user_profile_path = user_profile_path
        self.dz_connect_on_event_cb = dz_on_event_cb_func(dz_connect_on_event_cb)
        self.anonymous_blob = anonymous_blob
        self.dz_connect_crash_reporting_delegate = dz_connect_crash_reporting_delegate_func(
            dz_connect_crash_reporting_delegate)
        self.connect_handle = 0
        self.active = False

    def init_handle(self):
        """Initialize connection info and return the connection handler

            Build a DZConnectConfiguration object that wraps
            dz_connect_configuration from the C SDK and fill it with the
            connection instance info. To be called after initializing instance.
        """
        config = DZConnectConfiguration(c_char_p(self.app_id),
                                        c_char_p(self.product_id),
                                        c_char_p(self.product_build_id),
                                        c_char_p(self.user_profile_path),
                                        self.dz_connect_on_event_cb,
                                        c_void_p(self.anonymous_blob),
                                        self.dz_connect_crash_reporting_delegate)
        self.connect_handle = libdeezer.dz_connect_new(byref(config))
        if not self.connect_handle:
            raise ConnectionInitFailedError(u'Connection handle failed to initialize. Check connection info you gave.')

    def set_event_cb(self, callback):
        """
        Set dz_connect_on_event_cb, a callback called after each state change.
        See "callbacks" section in the module documentation.

        :param callback: The event callback to give.
        :type callback: function
        """
        self.dz_connect_on_event_cb = dz_on_event_cb_func(callback)

    def get_device_id(self):
        """
        :return: The device ID for logs
        """
        return libdeezer.dz_connect_get_device_id(self.connect_handle)

    def debug_log_disable(self):
        """
        Mute all API logs for readability's sake
        """
        if libdeezer.dz_connect_debug_log_disable(self.connect_handle):
            raise ConnectionRequestFailedError(u'debug_log_disable: Request failed.')

    def activate(self, user_data=None):
        """Launch the connection. Call this after init_handle.

        Calls self.dz_connect_on_event_cb after activation. You can provide any
        object you want through user_data as long as it is managed by this
        callback.

        :param user_data: An object you want to pass to dz_connect_on_event_cb.
        :type user_data: The type of the object you want to manipulate
        """
        delegate = py_object(user_data) if user_data else c_void_p(0)
        if libdeezer.dz_connect_activate(self.connect_handle, delegate):
            raise ConnectionActivationError(u'Failed to activate connection. Check your network connection.')
        self.active = True

    def cache_path_set(self, user_cache_path, activity_operation_cb=None, operation_userdata=None):
        """Set the cache path for debug purposes and logs.

        Cache will be stored in the specified path. Calls
        activity_operation_cb after the operation.

        :param user_cache_path: The desired path
        :param activity_operation_cb: The callback to this function.
        :param operation_userdata: An object you want to pass to
            activity_operation_cb.
        :type user_cache_path: str
        :type activity_operation_cb: dz_activity_operation_cb_func
        :type operation_userdata: The type of the object you want to manipulate
        """
        delegate = py_object(operation_userdata) if operation_userdata else c_void_p(0)
        cb = activity_operation_cb if activity_operation_cb else c_void_p(0)
        if libdeezer.dz_connect_cache_path_set(self.connect_handle, cb, delegate,
                                               c_char_p(user_cache_path)):
            raise ConnectionRequestFailedError(
                u'cache_path_set: Request failed. Check connection and/or path validity.')

    def set_access_token(self, user_access_token, activity_operation_cb=None, operation_user_data=None):
        """
        Set the user access token given by OAuth process.
        Mandatory to allow connection.

        :param user_access_token: The token given by OAuth 2 process.
            Refer to the API documentation.
        :param activity_operation_cb: The callback to this function.
        :param operation_user_data: An object you want to pass to
            activity_operation_cb.
        :type user_access_token: unicode
        :type activity_operation_cb: dz_activity_operation_cb_func
        :type operation_user_data: The type of the object you want to manipulate
        """
        delegate = py_object(operation_user_data) if operation_user_data else c_void_p(0)
        cb = activity_operation_cb if activity_operation_cb else c_void_p(0)
        if libdeezer.dz_connect_set_access_token(self.connect_handle, cb, delegate,
                                                 c_char_p(user_access_token)):
            raise ConnectionRequestFailedError(u'set_access_token: Request failed. Check access token or update it.')

    def connect_offline_mode(self, activity_operation_cb=None, operation_user_data=None, offline_mode_forced=False):
        """Force offline mode in lib.

        Calling this function is mandatory to force user login.

        :param activity_operation_cb: The callback of the operation.
        :param operation_user_data: An object you want to pass to
            activity_operation_cb.
        :param offline_mode_forced: Force offline mode. Leave to False
            if just to allow connection.
        :type activity_operation_cb: dz_activity_operation_cb_func
        :type operation_user_data: The type of the object you want to manipulate
        :type offline_mode_forced: bool
        """
        delegate = py_object(operation_user_data) if operation_user_data else c_void_p(0)
        cb = activity_operation_cb if activity_operation_cb else c_void_p(0)
        if libdeezer.dz_connect_offline_mode(self.connect_handle, cb, delegate, c_bool(offline_mode_forced)):
            raise ConnectionRequestFailedError(
                u'connect_offline_mode: Request failed. Check connection and callbacks if used.')

    def shutdown(self):
        """Deactivate connection associated to the handle."""
        if self.connect_handle:
            libdeezer.dz_connect_deactivate(self.connect_handle, c_void_p(0), None)
            self.active = False

    @staticmethod
    def get_event(event_obj):
        """Get the event value from the event_obj given by the SDK."""
        return int(libdeezer.dz_player_event_get_type(c_void_p(event_obj)))
