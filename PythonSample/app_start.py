#!/usr/bin/env python
# coding: utf8

import Queue
import threading
from myDeezerApp import *


def add_input(input_queue):
    while True:
        user_input = sys.stdin.readline()
        input_queue.put(user_input)


def process_input(app):
    input_queue = Queue.Queue()
    input_thread = threading.Thread(target=add_input, args=(input_queue,))
    input_thread.daemon = True
    input_thread.start()
    while app.connection.active or app.player.active:
        if not input_queue.empty():
            command = input_queue.get()
            if len(command) != 2 or command[0] not in "PSQ+-?R":
                print "INVALID COMMAND"
                log_command_info()
            else:
                app.process_command(command)


def argv_error():
    print "Please give the content as argument like:"
    print """\t"dzmedia:///track/10287076"        (Single track example)"""
    print """\t"dzmedia:///album/607845"          (Album example)"""
    print """\t"dzmedia:///playlist/1363560485"   (Playlist example)"""
    print """\t"dzradio:///radio-220"             (Radio example)"""  # TODO: check for radio
    print """\t"dzradio:///user-743548285"        (User Mix example)"""  # TODO: check for user


def log_connect_info(app):
    if app.debug_mode:
        print "---- Deezer NativeSDK version: {}".format(Connection.get_build_id())
        print "---- Application ID: {}".format(app.your_application_id)
        print "---- Product ID: {}".format(app.your_application_name)


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


def main():
    app = MyDeezerApp(True)
    log_connect_info(app)
    if len(sys.argv) != 2:
        argv_error()
        return 1
    app.load_content(sys.argv[1])
    log_command_info()
    process_input(app)
    return 0


if __name__ == "__main__":
    main()
