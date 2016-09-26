#!/usr/bin/python
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
    while True:
        if not input_queue.empty():
            app.process_command(input_queue.get())


def main():
    app = MyDeezerApp(True)
    app.log_connect_info()
    if len(sys.argv) != 2:
        app.argv_error()
        return 1
    app.set_song(sys.argv[1])
    app.log_command_info()
    app.start()
    process_input(app)
    return 0


if __name__ == "__main__":
    main()
