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
            app.process_command(input_queue.get())


def argv_error():
    print "Please give the content as argument like:"
    print """\t"dzmedia:///track/10287076"        (Single track example)"""
    print """\t"dzmedia:///album/607845"          (Album example)"""
    print """\t"dzmedia:///playlist/1363560485"   (Playlist example)"""
    print """\t"dzradio:///radio-223"             (Radio example)"""
    print """\t"dzradio:///user-743548285"        (User Mix example)"""


def main():
    app = MyDeezerApp(True)
    app.log_connect_info()
    if len(sys.argv) != 2:
        argv_error()
        return 1
    app.load_content(sys.argv[1])
    app.log_command_info()
    process_input(app)
    return 0


if __name__ == "__main__":
    main()
